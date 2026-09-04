using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Service
{
	public interface IPurchaseXmlImportService
	{
		Task<XmlImportResultDto> ImportarXmlAsync(Stream fileStream, string fileName, int tenantId);
	}

	public class PurchaseXmlImportService : IPurchaseXmlImportService
	{
		private const string NfeNamespace = "http://www.portalfiscal.inf.br/nfe";
		private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
		private const int ChaveLength = 44;
		private static readonly XNamespace Ns = XNamespace.Get(NfeNamespace);

		private readonly ContextBase _dbContext;
		private readonly IProviderService _providerService;
		private readonly IProductService _productService;

		public PurchaseXmlImportService(ContextBase dbContext, IProviderService providerService, IProductService productService)
		{
			_dbContext = dbContext;
			_providerService = providerService;
			_productService = productService;
		}

		public async Task<XmlImportResultDto> ImportarXmlAsync(Stream fileStream, string fileName, int tenantId)
		{
			var resultado = new XmlImportResultDto();

			// ---------- 1. Validacao fisica ----------
			if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException("Envie um arquivo com extensao .xml.");

			if (fileStream == null || fileStream.Length <= 0)
				throw new ArgumentException("O arquivo enviado esta vazio.");

			if (fileStream.Length > MaxFileSize)
				throw new ArgumentException("O arquivo excede o tamanho maximo de 10 MB.");

			// ---------- 2. Leitura segura (anti-XXE: DTD bloqueado) ----------
			string xml;
			try
			{
				using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
				xml = await reader.ReadToEndAsync();
			}
			catch (Exception ex)
			{
				throw new ArgumentException("Nao foi possivel ler o arquivo XML.", ex);
			}

			if (string.IsNullOrWhiteSpace(xml))
				throw new ArgumentException("O arquivo XML esta vazio.");

			// ---------- 3. Deteccao da raiz e validacao contra XSD ----------
			string raiz;
			try
			{
				raiz = ObterNomeDaRaiz(xml);
			}
			catch (XmlException ex)
			{
				return ComErro(resultado, $"O arquivo nao e um XML valido: {ex.Message}");
			}

			string schemaFile = raiz switch
			{
				"NFe" => "nfe_v4.00.xsd",
				"nfeProc" => "procNFe_v4.00.xsd",
				_ => null
			};

			if (schemaFile == null)
				return ComErro(resultado, "O XML enviado nao e uma NF-e (raiz esperada: NFe ou nfeProc).");

			if (!ValidarContraXsd(xml, schemaFile, resultado.Erros))
				return resultado;

			// ---------- 4. Parse ----------
			XDocument doc;
			try
			{
				doc = XDocument.Parse(xml, LoadOptions.None);
			}
			catch (XmlException ex)
			{
				return ComErro(resultado, $"O arquivo nao e um XML valido: {ex.Message}");
			}

			var infNFe = raiz == "nfeProc"
				? doc.Root?.Descendants(Ns + "infNFe").FirstOrDefault()
				: doc.Root?.Element(Ns + "infNFe");

			if (infNFe == null)
				return ComErro(resultado, "Nao foi encontrado o elemento infNFe no XML.");

			ExtrairDados(infNFe, tenantId, resultado, out var itens);

			if (resultado.Erros.Count > 0)
				return resultado;

			// Aviso quando a chave ja existe no sistema (nao bloqueia)
			if (!string.IsNullOrWhiteSpace(resultado.ChaveNfe))
			{
				bool chaveExiste = await _dbContext.Set<Purchase>().AnyAsync(x =>
					x.IdCompany == tenantId && x.ChaveNfe == resultado.ChaveNfe);
				if (chaveExiste)
					resultado.Avisos.Add("Ja existe uma compra com esta chave de NF-e. Verifique antes de salvar.");
			}

			// ---------- 5. Persistencia idempotente (fornecedor e produtos) ----------
			await PersistirCadastrosAsync(tenantId, resultado, itens);

			if (resultado.Erros.Count > 0)
				return resultado;

			// ---------- 6. Montar itens de retorno ----------
			resultado.Itens = itens.Select(i => new PurchaseItemImportDto
			{
				ProdutoId = i.ProdutoId,
				CodigoProduto = i.Codigo,
				DescricaoProduto = i.Descricao,
				Quantidade = i.Quantidade,
				ValorUnitario = i.ValorUnitario,
				Desconto = i.Desconto,
				ValorTotal = i.ValorTotal,
				ProdutoCriado = i.ProdutoCriado,
				Unidade = i.Unidade,
				QuantidadeXml = i.QuantidadeXml,
				ValorUnitarioXml = i.ValorUnitarioXml,
				FatorConversao = i.FatorConversao
			}).ToList();

			return resultado;
		}

		// =====================================================================
		// Etapas internas
		// =====================================================================

		private static string ObterNomeDaRaiz(string xml)
		{
			using var sr = new StringReader(xml);
			var settings = new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Prohibit,
				XmlResolver = null,
				IgnoreWhitespace = true,
				IgnoreComments = true
			};
			using var xr = XmlReader.Create(sr, settings);
			xr.MoveToContent();
			return xr.NodeType == XmlNodeType.Element ? xr.LocalName : null;
		}

		private static bool ValidarContraXsd(string xml, string xsdFileName, List<string> erros)
		{
			try
			{
				string schemaDir = Path.Combine(AppContext.BaseDirectory, "NFSchemas");
				string xsdPath = Path.Combine(schemaDir, xsdFileName);

				if (!System.IO.File.Exists(xsdPath))
				{
					erros.Add($"Schema de validacao nao encontrado ({xsdFileName}).");
					return false;
				}

				var schemas = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
				schemas.Add(null, xsdPath);

				var settings = new XmlReaderSettings
				{
					ValidationType = ValidationType.Schema,
					Schemas = schemas,
					DtdProcessing = DtdProcessing.Prohibit,
					XmlResolver = null
				};

				List<string> falhas = new List<string>();
				settings.ValidationEventHandler += (_, e) =>
				{
					if (e.Severity == XmlSeverityType.Error)
						falhas.Add($"{e.Message} (linha {e.Exception?.LineNumber}, pos {e.Exception?.LinePosition})");
				};

				using var sr = new StringReader(xml);
				using var reader = XmlReader.Create(sr, settings);
				while (reader.Read()) { }

				if (falhas.Count > 0)
				{
					erros.Add($"XML invalido conforme schema ({falhas.Count} erro(s)). Primeiro erro: {falhas[0]}");
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				erros.Add($"Nao foi possivel validar o XML contra o schema: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Lê os dados fiscais do infNFe e preenche Compra/Fornecedor/Chave/avisos.
		/// Os itens retornados ainda não possuem ProdutoId (definido na persistência).
		/// </summary>
		private static void ExtrairDados(XElement infNFe, int tenantId, XmlImportResultDto resultado, out List<ItemInfo> itens)
		{
			itens = new List<ItemInfo>();

			var ide = infNFe.Element(Ns + "ide");
			var emit = infNFe.Element(Ns + "emit");
			var ender = emit?.Element(Ns + "enderEmit");

			if (emit == null)
			{
				resultado.Erros.Add("Emitente (emit) nao encontrado no XML.");
				return;
			}

			// ---------- Ambiente / homologacao ----------
			string tpAmb = Valor(ide, "tpAmb");
			if (tpAmb == "2")
			{
				resultado.IsHomologacao = true;
				resultado.Avisos.Add("Nota fiscal emitida em ambiente de HOMOLOGACAO (teste). Confira os dados antes de salvar.");
			}

			// ---------- Fornecedor ----------
			string documento = SoDigitos(Valor(emit, "CNPJ"));
			if (string.IsNullOrEmpty(documento))
				documento = SoDigitos(Valor(emit, "CPF"));

			string nome = Valor(emit, "xNome");
			if (string.IsNullOrEmpty(documento))
				resultado.Erros.Add("CNPJ/CPF do emitente ausente no XML.");
			if (string.IsNullOrEmpty(nome))
				resultado.Erros.Add("Razao social do emitente (xNome) ausente no XML.");

			var fornecedor = new ProviderDto
			{
				Id = 0,
				Nome = nome,
				RazaoSocial = nome,
				NomeFantasia = string.IsNullOrWhiteSpace(Valor(emit, "xFant")) ? nome : Valor(emit, "xFant"),
				Cnpj = documento,
				InscricaoEstadual = Valor(emit, "IE"),
				Logradouro = Valor(ender, "xLgr"),
				Numero = int.TryParse(Valor(ender, "nro"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int num) ? num : 0,
				Bairro = Valor(ender, "xBairro"),
				Cidade = Valor(ender, "xMun"),
				Uf = Valor(ender, "UF"),
				Cep = SoDigitos(Valor(ender, "CEP")),
				Complemento = Valor(ender, "xCpl")
			};
			resultado.Fornecedor = fornecedor;

			// ---------- Chave / dados da compra ----------
			string chave = DeterminarChave(infNFe, ide, emit, documento, resultado.Avisos);
			resultado.ChaveNfe = chave;
			if (chave.Length != ChaveLength)
				resultado.Erros.Add("Nao foi possivel obter a chave de 44 digitos da NF-e.");

			DateTime dataEmissao = DateTime.Today;
			string dhEmi = Valor(ide, "dhEmi");
			if (DateTimeOffset.TryParse(dhEmi, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dh))
				dataEmissao = dh.Date;

			var total = infNFe.Element(Ns + "total");
			var icmsTot = total?.Element(Ns + "ICMSTot");

			resultado.Compra = new PurchaseImportDto
			{
				Id = 0,
				IdCompany = tenantId,
				DataEntrada = dataEmissao.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				DataCompra = dataEmissao.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				ChaveNfe = chave,
				FornecedorId = 0,
				ValorTotal = LerDecimal(icmsTot, "vNF") ?? 0,
				Serie = Valor(ide, "serie"),
				Numero = Valor(ide, "nNF"),
				Custos = ExtrairCustos(total)
			};

			// Resumo legivel dos custos (gravado na Observacao da compra ao salvar)
			if (resultado.Compra.Custos != null)
				resultado.Compra.Observacao = MontarObservacaoCustos(resultado.Compra, resultado.IsHomologacao);

			// ---------- Itens ----------
			var dets = infNFe.Elements(Ns + "det").ToList();
			if (dets.Count == 0)
				resultado.Erros.Add("A nota nao possui itens (det/prod).");

			var codigosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			decimal somaItens = 0;

			foreach (var det in dets)
			{
				var prod = det.Element(Ns + "prod");
				if (prod == null)
				{
					resultado.Erros.Add("Item da nota sem o grupo prod.");
					continue;
				}

				string codigo = Valor(prod, "cProd");
				string descricao = Valor(prod, "xProd");
				decimal qCom = LerDecimal(prod, "qCom") ?? 0;
				decimal vUnCom = LerDecimal(prod, "vUnCom") ?? 0;
				decimal desconto = LerDecimal(prod, "vDesc") ?? 0;
				string unidadeCom = NormalizarUnidade(Valor(prod, "uCom"));
				string unidadeTrib = NormalizarUnidade(Valor(prod, "uTrib"));
				decimal qTrib = LerDecimal(prod, "qTrib") ?? 0;

				if (string.IsNullOrWhiteSpace(codigo))
				{
					resultado.Erros.Add("Item sem codigo do produto (cProd).");
					continue;
				}

				if (string.IsNullOrWhiteSpace(descricao))
					descricao = codigo;

				// Aviso de arredondamento quando a nota usa mais casas que o modelo
				if (CasasDecimais(Valor(prod, "qCom")) > 3)
					resultado.Avisos.Add($"Item '{codigo}': quantidade com mais de 3 casas decimais foi arredondada.");
				if (CasasDecimais(Valor(prod, "vUnCom")) > 4)
					resultado.Avisos.Add($"Item '{codigo}': valor unitario com mais de 4 casas decimais foi arredondado.");

				// Origem do XML preservada para reexibir/reeditar o fator ao abrir a compra
				var novoItem = new ItemInfo
				{
					Codigo = codigo,
					Descricao = descricao,
					Ncm = Valor(prod, "NCM"),
					Cest = Valor(prod, "CEST"),
					Unidade = string.IsNullOrEmpty(unidadeCom) ? null : unidadeCom,
					QuantidadeXml = qCom,
					ValorUnitarioXml = vUnCom,
					UnidadeTrib = string.IsNullOrEmpty(unidadeTrib) ? null : unidadeTrib,
					QuantidadeTrib = qTrib > 0 ? qTrib : (decimal?)null,
					Desconto = desconto
				};

				// Valores de trabalho: ja arredondados para o modelo (antes da conversao por unidade)
				novoItem.Quantidade = Math.Round(qCom, 3, MidpointRounding.AwayFromZero);
				novoItem.ValorUnitario = Math.Round(vUnCom, 4, MidpointRounding.AwayFromZero);

				decimal subtotal = Math.Round(novoItem.Quantidade * novoItem.ValorUnitario, 2, MidpointRounding.AwayFromZero);
				if (novoItem.Desconto > subtotal)
				{
					resultado.Avisos.Add($"Item '{codigo}': desconto maior que o subtotal foi limitado ao valor do item.");
					novoItem.Desconto = subtotal;
				}
				novoItem.Desconto = Math.Round(novoItem.Desconto, 2, MidpointRounding.AwayFromZero);
				novoItem.ValorTotal = subtotal - novoItem.Desconto;

				if (codigosVistos.Contains(codigo))
					resultado.Avisos.Add($"O codigo de produto '{codigo}' aparece mais de uma vez na nota (itens mantidos separados).");
				codigosVistos.Add(codigo);

				itens.Add(novoItem);

				somaItens += novoItem.ValorTotal;
			}

			// Divergencia entre soma dos itens e total da nota (frete/seguro/etc nao importados)
			decimal totalNota = resultado.Compra?.ValorTotal ?? 0;
			if (Math.Abs(somaItens - totalNota) > 0.02m)
				resultado.Avisos.Add($"Soma dos itens (R$ {somaItens.ToString("N2", CultureInfo.InvariantCulture)}) difere do total da nota (R$ {totalNota.ToString("N2", CultureInfo.InvariantCulture)}). Verifique frete/seguro/outros.");
		}

		private static string DeterminarChave(XElement infNFe, XElement ide, XElement emit, string documento, List<string> avisos)
		{
			string idAttr = infNFe.Attribute("Id")?.Value?.Trim() ?? string.Empty;
			if (idAttr.StartsWith("NFe", StringComparison.OrdinalIgnoreCase))
				idAttr = idAttr.Substring(3);

			string digitos = SoDigitos(idAttr);
			if (digitos.Length == ChaveLength)
				return digitos;

			// Monta a chave a partir das partes (e calcula o DV) quando o atributo nao vem preenchido
			if (digitos.Length > 0)
				avisos.Add("Atributo Id do infNFe incompleto; a chave foi reconstruida a partir dos campos da nota.");

			string aamm = string.Empty;
			if (DateTimeOffset.TryParse(Valor(ide, "dhEmi"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dh))
				aamm = dh.ToString("yyMM", CultureInfo.InvariantCulture);

			string cnf = SoDigitos(Valor(ide, "cNF")).PadLeft(8, '0');
			string cUf = SoDigitos(Valor(ide, "cUF")).PadLeft(2, '0');
			string mod = SoDigitos(Valor(ide, "mod")).PadLeft(2, '0');
			string serie = SoDigitos(Valor(ide, "serie")).PadLeft(3, '0');
			string nnf = SoDigitos(Valor(ide, "nNF")).PadLeft(9, '0');
			string tpEmis = SoDigitos(Valor(ide, "tpEmis"));
			if (string.IsNullOrEmpty(tpEmis))
				tpEmis = "1";

			string chave43 = cUf + aamm + documento + mod + serie + nnf + tpEmis + cnf;
			if (chave43.Length == ChaveLength - 1 && chave43.All(char.IsDigit))
				return chave43 + CalcularDigitoVerificador(chave43);

			return string.Empty;
		}

		private static int CalcularDigitoVerificador(string chave43)
		{
			int soma = 0;
			int peso = 2;
			for (int i = chave43.Length - 1; i >= 0; i--)
			{
				soma += (chave43[i] - '0') * peso;
				peso++;
				if (peso > 9)
					peso = 2;
			}
			int resto = soma % 11;
			return resto < 2 ? 0 : 11 - resto;
		}

		/// <summary>
		/// Cria (ou reutiliza) o fornecedor e os produtos da nota dentro de uma transacao.
		/// Preenche resultado.Fornecedor/Produtos e os ProdutoId dos itens.
		/// </summary>
		private async Task PersistirCadastrosAsync(int tenantId, XmlImportResultDto resultado, List<ItemInfo> itens)
		{
			await using var tx = await _dbContext.Database.BeginTransactionAsync();
			try
			{
				string cnpj = SoDigitos(resultado.Fornecedor?.Cnpj);

				var provider = await _dbContext.Set<Provider>()
					.AsNoTracking()
					.FirstOrDefaultAsync(p => p.IdCompany == tenantId && p.cnpj == cnpj);

				if (provider == null)
				{
					provider = new Provider
					{
						IdCompany = tenantId,
						nome = resultado.Fornecedor.Nome ?? string.Empty,
						razaoSocial = resultado.Fornecedor.RazaoSocial ?? string.Empty,
						nomeFantasia = resultado.Fornecedor.NomeFantasia ?? string.Empty,
						cnpj = cnpj,
						inscricaoEstadual = resultado.Fornecedor.InscricaoEstadual ?? string.Empty,
						telefone = resultado.Fornecedor.Telefone ?? string.Empty,
						email = resultado.Fornecedor.Email ?? string.Empty,
						logradouro = resultado.Fornecedor.Logradouro ?? string.Empty,
						numero = resultado.Fornecedor.Numero,
						bairro = resultado.Fornecedor.Bairro ?? string.Empty,
						cidade = resultado.Fornecedor.Cidade ?? string.Empty,
						uf = resultado.Fornecedor.Uf ?? string.Empty,
						cep = resultado.Fornecedor.Cep ?? string.Empty,
						complemento = resultado.Fornecedor.Complemento ?? string.Empty,
						idcnae = 0,
						nomecnae = string.Empty
					};

					await _providerService.Create(provider);
					resultado.Avisos.Add($"Fornecedor '{resultado.Fornecedor.Nome}' criado automaticamente a partir do XML.");
				}

				resultado.Fornecedor.Id = provider.Id;
				resultado.Compra.FornecedorId = provider.Id;

				var produtosPorCodigo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				var produtosRetornados = new Dictionary<int, ProductImportDto>();

				// Peso (kg por unidade) de cada produto resolvido - usado p/ fator de conversao
				var pesoPorProduto = new Dictionary<int, decimal?>();

				foreach (var item in itens)
				{
					if (produtosPorCodigo.TryGetValue(item.Codigo, out int idExistente))
					{
						item.ProdutoId = idExistente;
						continue;
					}

					var product = await _dbContext.Set<Product>()
						.AsNoTracking()
						.FirstOrDefaultAsync(p => p.IdCompany == tenantId
							&& (p.Reference == item.Codigo || p.Code == item.Codigo));

					bool criado = false;
					if (product == null)
					{
						await _productService.SaveProduct(new ProductCreateModelDto
						{
							Id = 0,
							Name = item.Descricao,
							Description = item.Descricao,
							Quantity = 0, // estoque inicial 0; o lancamento de entrada vem do SaveWithItems
							Value = 0,
							Code = item.Codigo,
							Reference = item.Codigo,
							CostPrice = item.ValorUnitario,
							Ncm = item.Ncm ?? string.Empty,
							Cest = string.IsNullOrWhiteSpace(item.Cest) ? null : item.Cest
						}, tenantId);

						product = await _dbContext.Set<Product>()
							.AsNoTracking()
							.FirstOrDefaultAsync(p => p.IdCompany == tenantId
								&& (p.Reference == item.Codigo || p.Code == item.Codigo));

						if (product == null)
						{
							resultado.Erros.Add($"Nao foi possivel criar o produto de codigo '{item.Codigo}'.");
							await tx.RollbackAsync();
							return;
						}

						criado = true;
						resultado.Avisos.Add($"Produto '{item.Descricao}' (codigo {item.Codigo}) criado automaticamente a partir do XML.");
					}

					item.ProdutoId = product.Id;
					item.ProdutoCriado = criado;
					produtosPorCodigo[item.Codigo] = product.Id;

					if (!produtosRetornados.ContainsKey(product.Id))
					{
						pesoPorProduto[product.Id] = product.PesoUnitario;
						produtosRetornados[product.Id] = new ProductImportDto
						{
							Id = product.Id,
							IdCompany = tenantId,
							Name = product.Name,
							Code = product.Code,
							Reference = product.Reference,
							Ncm = product.Ncm,
							Cest = product.Cest,
							CostPrice = product.CostPrice,
							PesoUnitario = product.PesoUnitario,
							CriadoNaImportacao = criado
						};
					}
				}

				// ---------- Fator de conversao (depende do peso cadastrado no produto) ----------
				foreach (var item in itens)
				{
					if (item.ProdutoId == null)
						continue;

					pesoPorProduto.TryGetValue(item.ProdutoId.Value, out decimal? peso);
					AplicarConversao(item, peso, resultado.Avisos);
				}

				resultado.Produtos = produtosRetornados.Values.ToList();
				await tx.CommitAsync();
			}
			catch
			{
				await tx.RollbackAsync();
				throw;
			}
		}

		// =====================================================================
		// Helpers
		// =====================================================================

		private static XmlImportResultDto ComErro(XmlImportResultDto resultado, string mensagem)
		{
			resultado.Erros.Add(mensagem);
			return resultado;
		}

		private static string Valor(XElement parent, string local)
		{
			return parent?.Element(Ns + local)?.Value?.Trim() ?? string.Empty;
		}

		private static string SoDigitos(string valor)
		{
			return string.IsNullOrWhiteSpace(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());
		}

		private static decimal? LerDecimal(XElement parent, string local)
		{
			string valor = Valor(parent, local);
			if (string.IsNullOrWhiteSpace(valor))
				return null;
			return decimal.TryParse(valor, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal d) ? d : (decimal?)null;
		}

		private static int CasasDecimais(string valor)
		{
			if (string.IsNullOrWhiteSpace(valor))
				return 0;
			int idx = valor.IndexOf('.');
			if (idx < 0)
				return 0;
			string frac = valor.Substring(idx + 1).TrimEnd('0');
			return frac.Length;
		}

		// =====================================================================
		// Fator de conversao de unidade
		// =====================================================================

		private static string NormalizarUnidade(string unidade)
		{
			if (string.IsNullOrWhiteSpace(unidade))
				return string.Empty;
			return unidade.Trim().ToUpperInvariant();
		}

		private static bool EhUnidadeContagem(string unidadeNormalizada)
		{
			return unidadeNormalizada == "UN" || unidadeNormalizada == "UNID"
				|| unidadeNormalizada == "PC" || unidadeNormalizada == "PÇ";
		}

		private static bool EhUnidadeMassa(string unidadeNormalizada)
		{
			return unidadeNormalizada == "KG" || unidadeNormalizada == "KGM"
				|| unidadeNormalizada == "G" || unidadeNormalizada == "TON"
				|| unidadeNormalizada == "T";
		}

		/// <summary>
		/// Quantas unidades do sistema existem em 1 unidade do XML, a partir do peso
		/// (em kg) de cada unidade cadastrado no produto. Sem peso cadastrado retorna null.
		/// </summary>
		private static decimal? FatorPorPeso(string unidadeNormalizada, decimal? pesoUnitarioKg)
		{
			if (pesoUnitarioKg == null || pesoUnitarioKg.Value <= 0)
				return null;

			decimal peso = pesoUnitarioKg.Value;
			switch (unidadeNormalizada)
			{
				case "KG":
				case "KGM":
					return 1m / peso;            // 1 kg contem (1/peso) unidades
				case "G":
					return 1m / (peso * 1000m);  // 1 g = 0,001 kg
				case "TON":
				case "T":
					return 1000m / peso;         // 1 tonelada = 1000 kg
				default:
					return null;
			}
		}

		/// <summary>
		/// Conversao declarada pela propria nota (uTrib de contagem com qTrib &gt; 0).
		/// Ex.: qCom=500 (KG) corresponde a qTrib=1000 (UN) -&gt; fator = 1000/500 = 2.
		/// </summary>
		private static decimal? FatorPorConversaoDaNota(ItemInfo item)
		{
			if (item.QuantidadeXml <= 0 || item.QuantidadeTrib == null || item.QuantidadeTrib.Value <= 0)
				return null;
			if (!EhUnidadeContagem(NormalizarUnidade(item.UnidadeTrib)))
				return null;
			return item.QuantidadeTrib.Value / item.QuantidadeXml;
		}

		/// <summary>
		/// Aplica o fator de conversao (multiplicador) de um item:
		/// Quantidade = qCom * fator e ValorUnitario = vUnCom / fator (preserva o total em R$).
		/// Fator default 1. So converte quando ha base fisica: peso cadastrado no produto
		/// (unidade de massa) ou conversao declarada na propria nota (uTrib/qTrib).
		/// </summary>
		private static void AplicarConversao(ItemInfo item, decimal? pesoUnitarioKg, List<string> avisos)
		{
			string unidade = NormalizarUnidade(item.Unidade);

			// Unidade de contagem (ou ausente) nao exige conversao
			if (string.IsNullOrEmpty(unidade) || EhUnidadeContagem(unidade))
			{
				item.FatorConversao = 1;
				return;
			}

			decimal? fator = null;
			if (EhUnidadeMassa(unidade))
			{
				fator = FatorPorPeso(unidade, pesoUnitarioKg);        // 1) peso por unidade no cadastro
				if (fator == null)
					fator = FatorPorConversaoDaNota(item);            // 2) conversao declarada na propria nota
			}

			if (fator == null || fator.Value <= 0)
			{
				item.FatorConversao = 1;
				string motivo = EhUnidadeMassa(unidade)
					? "porque o produto nao possui 'Peso por unidade' no cadastro (informe-o no produto para conversao automatica)"
					: "porque a unidade nao e reconhecida para conversao";
				avisos.Add($"Item '{item.Codigo}': unidade '{item.Unidade}' mantida como veio no XML ({motivo}).");
				return;
			}

			decimal fatorFinal = Math.Round(fator.Value, 6, MidpointRounding.AwayFromZero);
			item.FatorConversao = fatorFinal;
			if (fatorFinal == 1)
				return;

			item.Quantidade = Math.Round(item.QuantidadeXml * fatorFinal, 3, MidpointRounding.AwayFromZero);
			item.ValorUnitario = Math.Round(item.ValorUnitarioXml / fatorFinal, 4, MidpointRounding.AwayFromZero);
			item.ValorTotal = Math.Round(item.Quantidade * item.ValorUnitario - item.Desconto, 2, MidpointRounding.AwayFromZero);

			avisos.Add($"Item '{item.Codigo}': unidade '{item.Unidade}' convertida para a unidade de contagem do produto (fator {fatorFinal.ToString("0.######", CultureInfo.InvariantCulture)}).");
		}

		// =====================================================================
		// Destaque de custos (totais da NF-e)
		// =====================================================================

		private static decimal LerTotal(XElement parent, string local)
		{
			return LerDecimal(parent, local) ?? 0;
		}

		/// <summary>
		/// Le total/ICMSTot + total/IBSCBSTot e preenche PurchaseCostsDto.
		/// IBS/CBS ficam aninhados (gIBS/vIBS, gCBS/vCBS). Valores do ICMSTot sem coluna
		/// propria (vOutro, vII, vFCP, vST, vFCPST, vICMSDeson, ...) vao para OutrosCustos.
		/// </summary>
		private static PurchaseCostsDto ExtrairCustos(XElement total)
		{
			if (total == null)
				return null;

			var icmsTot = total.Element(Ns + "ICMSTot");
			var ibscbsTot = total.Element(Ns + "IBSCBSTot");

			var custos = new PurchaseCostsDto
			{
				BaseCalculoICMS = LerTotal(icmsTot, "vBC"),
				ValorICMS = LerTotal(icmsTot, "vICMS"),
				ValorProdutos = LerTotal(icmsTot, "vProd"),
				ValorFrete = LerTotal(icmsTot, "vFrete"),
				ValorSeguro = LerTotal(icmsTot, "vSeg"),
				ValorDesconto = LerTotal(icmsTot, "vDesc"),
				ValorIPI = LerTotal(icmsTot, "vIPI"),
				ValorPIS = LerTotal(icmsTot, "vPIS"),
				ValorCOFINS = LerTotal(icmsTot, "vCOFINS"),
				ValorTotal = LerTotal(icmsTot, "vNF"),
				ValorTotalTributos = LerTotal(icmsTot, "vTotTrib"),
				BaseCalculoIBSCBS = LerTotal(ibscbsTot, "vBCIBSCBS")
			};

			if (ibscbsTot != null)
			{
				custos.ValorIBS = LerTotal(ibscbsTot.Element(Ns + "gIBS"), "vIBS");
				custos.ValorCBS = LerTotal(ibscbsTot.Element(Ns + "gCBS"), "vCBS");
			}

			var comColuna = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"vBC", "vICMS", "vProd", "vFrete", "vSeg", "vDesc",
				"vIPI", "vPIS", "vCOFINS", "vNF", "vTotTrib"
			};
			if (icmsTot != null)
				CapturarExtras(icmsTot, custos, comColuna);

			var comColunaIbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"vBCIBSCBS", "gIBS", "gCBS"
			};
			if (ibscbsTot != null)
				CapturarExtras(ibscbsTot, custos, comColunaIbs);

			return custos;
		}

		private static void CapturarExtras(XElement parent, PurchaseCostsDto custos, ISet<string> comColuna)
		{
			foreach (var el in parent.Elements())
			{
				if (comColuna.Contains(el.Name.LocalName))
					continue;
				if (el.Elements().Any()) // grupo com filhos (ex.: vIPIDevol, gMono) - sem valor simples
					continue;
				if (decimal.TryParse(el.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal valor) && valor > 0)
					custos.OutrosCustos[el.Name.LocalName] = Math.Round(valor, 2, MidpointRounding.AwayFromZero);
			}
		}

		/// <summary>
		/// Resumo legivel dos custos para a Observacao da compra.
		/// </summary>
		private static string MontarObservacaoCustos(PurchaseImportDto compra, bool homologacao)
		{
			var c = compra.Custos;
			if (c == null)
				return null;

			var sb = new StringBuilder();
			string titulo = $"NF-e {compra.Serie}/{compra.Numero} (chave {compra.ChaveNfe})";
			if (homologacao)
				titulo += " [HOMOLOGACAO]";
			sb.AppendLine(titulo);

			void Linha(string nome, decimal valor)
			{
				if (valor > 0)
					sb.AppendLine($"{nome}: {Moeda(valor)}");
			}

			Linha("Produtos (vProd)", c.ValorProdutos);
			Linha("Frete (vFrete)", c.ValorFrete);
			Linha("Seguro (vSeg)", c.ValorSeguro);
			Linha("Desconto (vDesc)", c.ValorDesconto);
			Linha("IPI (vIPI)", c.ValorIPI);
			Linha("PIS (vPIS)", c.ValorPIS);
			Linha("COFINS (vCOFINS)", c.ValorCOFINS);
			Linha("ICMS (vICMS)", c.ValorICMS);
			Linha("IBS (vIBS)", c.ValorIBS);
			Linha("CBS (vCBS)", c.ValorCBS);
			Linha("Base de calculo ICMS (vBC)", c.BaseCalculoICMS);
			Linha("Base de calculo IBS/CBS (vBCIBSCBS)", c.BaseCalculoIBSCBS);
			foreach (var extra in c.OutrosCustos)
				Linha($"Outros - {extra.Key}", extra.Value);

			sb.AppendLine($"Total NF-e (vNF): {Moeda(c.ValorTotal)}");
			if (c.ValorTotalTributos > 0)
				sb.AppendLine($"Total estimado de tributos (vTotTrib): {Moeda(c.ValorTotalTributos)}");

			return sb.ToString().TrimEnd();
		}

		private static string Moeda(decimal valor)
		{
			return "R$ " + valor.ToString("N2", CultureInfo.InvariantCulture);
		}

		private class ItemInfo
		{
			public string Codigo { get; set; }
			public string Descricao { get; set; }
			public string Ncm { get; set; }
			public string Cest { get; set; }

			/// <summary>Quantidade na unidade do sistema (ja convertida pelo fator).</summary>
			public decimal Quantidade { get; set; }

			/// <summary>Valor unitario na unidade do sistema (ja convertido pelo fator).</summary>
			public decimal ValorUnitario { get; set; }
			public decimal Desconto { get; set; }
			public decimal ValorTotal { get; set; }

			// ===== FATOR DE CONVERSAO (origem da linha no XML) =====

			/// <summary>uCom da nota (ex.: KG).</summary>
			public string Unidade { get; set; }

			/// <summary>qCom original (sem conversao).</summary>
			public decimal QuantidadeXml { get; set; }

			/// <summary>vUnCom original (sem conversao).</summary>
			public decimal ValorUnitarioXml { get; set; }

			/// <summary>uTrib da nota (conversao declarada pelo proprio emitente).</summary>
			public string UnidadeTrib { get; set; }

			/// <summary>qTrib da nota.</summary>
			public decimal? QuantidadeTrib { get; set; }

			/// <summary>Fator multiplicador aplicado (Quantidade = qCom * fator). Preenchido na persistencia.</summary>
			public decimal? FatorConversao { get; set; }

			public int? ProdutoId { get; set; }
			public bool ProdutoCriado { get; set; }
		}
	}
}
