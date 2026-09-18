#nullable enable
using Model;
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.NFSe.Nacional.Common.Model;
using OpenAC.Net.NFSe.Nacional.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service
{
    /// <summary>
    /// Monta a DPS (Declaração de Prestação de Serviços) do padrão Nacional a partir das
    /// nossas entidades.
    ///
    /// É uma classe separada de propósito: é a única parte da emissão que pode ser
    /// verificada OFFLINE — dá para montar a DPS e validar contra os XSDs sem certificado
    /// e sem rede. Mantendo-a isolada, dá para exercitá-la em um harness e provar que o
    /// XML está correto antes de qualquer tentativa de transmissão.
    /// </summary>
    public static class DpsBuilder
    {
        /// <summary>
        /// Versão do leiaute. Constante nomeada para não virar literal espalhado: quando o
        /// SEFIN publicar uma versão nova, muda-se aqui e nos XSDs vendorizados.
        /// </summary>
        public const VersaoNFSe Versao = VersaoNFSe.Ve101;

        /// <summary>
        /// Versão da aplicação que assina a DPS (tag versaoAplicacao). O SEFIN exige uma
        /// string nesse formato; não é a versão da nossa API.
        /// </summary>
        private const string VersaoAplicacao = "SalesFlow-1.0";

        /// <summary>
        /// Monta a DPS. <paramref name="numeroDps"/> já deve estar RESERVADO e persistido
        /// pela camada de emissão: o id da DPS precisa ser o mesmo entre tentativas, senão
        /// uma retentativa depois de uma falha de rede vira uma segunda nota.
        /// </summary>
        public static Dps Construir(
            ServiceInvoice fatura,
            FiscalConfiguration config,
            string numeroDps)
        {
            if (fatura == null) throw new ArgumentNullException(nameof(fatura));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var prestador = config.Emitente
                ?? throw new InvalidOperationException("FiscalConfiguration sem Emitente preenchido.");
            var cliente = fatura.Client
                ?? throw new InvalidOperationException("NFS-e sem tomador (Client) carregado.");

            var itens = fatura.ServiceInvoiceItems?.ToList() ?? new List<ServiceInvoiceItem>();
            if (itens.Count == 0)
                throw new InvalidOperationException("NFS-e sem itens: não há serviço a declarar.");

            var codMunIBGE = ExigirDigitos(fatura.CodMunIBGE ?? config.CodMunIBGE, "código do município (IBGE)");
            var inscricaoFederal = ExigirDigitos(prestador.Cnpj ?? prestador.Cpf, "CNPJ/CPF do emitente");
            var serie = (config.NumeracaoDocumentos?.Dps?.Serie ?? "1").PadLeft(5, '0');

            // O primeiro item define o serviço da DPS. A NFS-e copia TODOS os itens da OS e
            // o SEFIN comporta um único grupo <serv>, então o serviço declarado é o do
            // primeiro item e a discriminação abaixo concatena os demais.
            var primeiro = itens[0];

            var dps = new Dps
            {
                Versao = Versao,
                Informacoes = new InfDps
                {
                    Id = MontarIdDps(codMunIBGE, TipoInscricaoFederal(prestador), inscricaoFederal, serie, numeroDps),
                    TipoAmbiente = fatura.TipoAmbiente == AmbienteEnum.Producao
                        ? DFeTipoAmbiente.Producao
                        : DFeTipoAmbiente.Homologacao,
                    DhEmissao = new DateTimeOffset(fatura.DhEmissao ?? DateTime.Now),
                    // IBSCBS = new RTCInfoIBSCBS
                    // {
                    //     
                    // }
                    VersaoAplicacao = VersaoAplicacao,
                    Serie = serie,
                    NumeroDps = numeroDps,
                    Competencia = fatura.DataCompetencia.Date,
                    TipoEmitente = EmitenteDps.Prestador,
                    LocalidadeEmitente = codMunIBGE,
                    Prestador = MontarPrestador(prestador, config),
                    Tomador = MontarTomador(cliente),
                    Servico = MontarServico(primeiro, itens, codMunIBGE),

                    // O grupo IBSCBS (reforma tributária) é minOccurs="0" no
                    // tiposComplexos_v1.01.xsd e o nosso cadastro de serviço não tem os
                    // códigos de classificação que ele exige. Omitir é válido no leiaute.
                    Valores = MontarValores(itens)
                }
            };

            return dps;
        }

        /// <summary>
        /// Id da DPS a partir das nossas entidades. Existe separado de
        /// <see cref="Construir"/> para que a emissão possa gravar o id na fatura ANTES de
        /// transmitir — o id precisa sobreviver a uma falha de rede.
        ///
        /// Extrai exatamente os mesmos dados que <see cref="Construir"/> usa no id, para os
        /// dois não divergirem.
        /// </summary>
        public static string MontarIdDps(ServiceInvoice fatura, FiscalConfiguration config, string numeroDps)
        {
            if (fatura == null) throw new ArgumentNullException(nameof(fatura));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var prestador = config.Emitente
                ?? throw new InvalidOperationException("FiscalConfiguration sem Emitente preenchido.");

            return MontarIdDps(
                ExigirDigitos(fatura.CodMunIBGE ?? config.CodMunIBGE, "código do município (IBGE)"),
                TipoInscricaoFederal(prestador),
                ExigirDigitos(prestador.Cnpj ?? prestador.Cpf, "CNPJ/CPF do emitente"),
                config.NumeracaoDocumentos?.Dps?.Serie ?? "1",
                numeroDps);
        }

        /// <summary>
        /// Id da DPS: "DPS" + cLocEmi(7) + tipoInsc(1) + inscricaoFederal(14) + serie(5) + numero(15).
        ///
        /// Montado À MÃO de propósito. A biblioteca também monta o id, mas o faz com
        /// interpolação de formatador ({Serie:D5}) sobre campos que ela mesma declara como
        /// string — e string não implementa IFormattable, então os formatadores são
        /// ignorados EM SILÊNCIO e o id sai curto. Montar aqui é seguro porque
        /// Dps.Assinar() só preenche o id quando ele está vazio.
        /// </summary>
        public static string MontarIdDps(
            string codMunIBGE, int tipoInscricaoFederal, string inscricaoFederal, string serie, string numeroDps)
        {
            if (string.IsNullOrWhiteSpace(codMunIBGE))
                throw new InvalidOperationException("Id da DPS: código do município (IBGE) não informado.");
            if (string.IsNullOrWhiteSpace(inscricaoFederal))
                throw new InvalidOperationException("Id da DPS: CNPJ/CPF do emitente não informado.");

            return "DPS"
                + codMunIBGE.PadLeft(7, '0')
                + tipoInscricaoFederal
                + inscricaoFederal.PadLeft(14, '0')
                + serie.PadLeft(5, '0')
                + numeroDps.PadLeft(15, '0');
        }

        /// <summary>1 = CNPJ, 2 = CPF (tipoInscricaoFederal do leiaute).</summary>
        private static int TipoInscricaoFederal(Emitente e)
            => !string.IsNullOrWhiteSpace(e.Cnpj) ? 2 : 1;

        private static PrestadorDps MontarPrestador(Emitente e, FiscalConfiguration config)
        {
            var p = new PrestadorDps
            {
                // Sem e-mail no cadastro do emitente (Contato só guarda telefone) — o campo
                // é opcional no leiaute, então vai vazio em vez de inventado.
                InscricaoMunicipal = SomenteDigitos(e.InscricaoMunicipal),
                Nome = e.RazaoSocial ?? e.Fantasia,
                Telefone = SomenteDigitos(e.EmitenteContato?.Telefone),
                // Qualificado: "RegimeTributario" é o NOSSO tipo e o do OpenAC ao mesmo tempo.
                Regime = new OpenAC.Net.NFSe.Nacional.Common.Model.RegimeTributario
                {
                    OptanteSimplesNacional = ResolverSimplesNacional(config.Emitente?.RegimeTributario),
                    RegimeEspecial = RegimeEspecial.Nenhum
                },
                Email = config?.Emitente?.EmitenteContato.Email
            };

            if (!string.IsNullOrWhiteSpace(e.Cnpj)) p.CNPJ = SomenteDigitos(e.Cnpj);
            else p.CPF = SomenteDigitos(e.Cpf);

            return p;
        }

        /// <summary>
        /// 1 = Não optante, 2 = Optante MEI, 3 = Optante ME/EPP.
        ///
        /// O campo explícito vence. Quando é nulo, derivamos do CRT: o CRT diz o REGIME
        /// (1=Simples, 2=Simples excesso, 3=Normal, 4=MEI), mas não distingue MEI de
        /// ME/EPP — então tratamos 1|2 como ME/EPP, 4 como MEI e o resto como não optante.
        /// Documentado em vez de adivinhado em silêncio.
        /// </summary>
        // Totalmente qualificado: "RegimeTributario" existe nos nossos modelos E no OpenAC,
        // e o using das duas torna o nome curto ambíguo.
        private static OptanteSimplesNacional ResolverSimplesNacional(Model.Registrations.RegimeTributario? regime)
        {
            if (regime?.OpcaoSimplesNacional is int explicito)
            {
                return explicito switch
                {
                    2 => OptanteSimplesNacional.OptanteMEI,
                    3 => OptanteSimplesNacional.OptanteMEEPP,
                    _ => OptanteSimplesNacional.NaoOptante
                };
            }

            return regime?.Crt switch
            {
                "1" or "2" => OptanteSimplesNacional.OptanteMEEPP,
                "4" => OptanteSimplesNacional.OptanteMEI,
                _ => OptanteSimplesNacional.NaoOptante
            };
        }

        private static InfoPessoaNFSe MontarTomador(Client c)
        {
            var doc = SomenteDigitos(c.Document);
            var pessoa = new InfoPessoaNFSe
            {
                Nome = c.Name,
                Email = string.IsNullOrWhiteSpace(c.Email) ? null : c.Email,
                Telefone = SomenteDigitos(c.CellPhone),
                Endereco = new EnderecoNFSe
                {
                    Logradouro = c.Address,
                    Numero = c.Numero,
                    Complemento = c.Complemento,
                    Bairro = c.Bairro,
                    Municipio = MontarMunicipioTomador(c)
                }
            };

            // TipoPessoa: 'F' = física, 'J' = jurídica. A decisão pelo tipo declarado (e não
            // pelo tamanho do documento) evita mandar CNPJ no campo CPF quando o cadastro
            // está com o documento incompleto.
            if (string.Equals(c.TipoPessoa, "J", StringComparison.OrdinalIgnoreCase))
                pessoa.CNPJ = doc;
            else
                pessoa.CPF = doc;

            return pessoa;
        }

        /// <summary>
        /// Município do tomador: nacional quando temos o código IBGE, exterior quando temos
        /// só o país. O leiaute exige um dos dois grupos — sem nenhum, o tomador fica sem
        /// endereço e a DPS é rejeitada.
        /// </summary>
        private static IMunicipio MontarMunicipioTomador(Client c)
        {
            var cod = SomenteDigitos(c.CodMunicipioIbge);
            if (!string.IsNullOrWhiteSpace(cod))
            {
                return new MunicipioNacional
                {
                    CodMunicipio = cod.PadLeft(7, '0'),
                    CEP = SomenteDigitos(c.ZipCode)
                };
            }

            return new MunicipioExterior
            {
                CodigoPais = string.IsNullOrWhiteSpace(c.CodPais) ? "1058" : SomenteDigitos(c.CodPais),
                EnderecoPostal = SomenteDigitos(c.ZipCode),
                Cidade = c.Municipio,
                EstadoProvincia = c.Uf
            };
        }

        private static ServicoNFSe MontarServico(ServiceInvoiceItem primeiro, List<ServiceInvoiceItem> itens,
            string codMunEmitente)
        {
            var cadastro = primeiro.ServiceProvided;
            var servico = new ServicoNFSe
            {
                Localidade = new LocalidadeNFSe
                {
                    // Onde o serviço foi prestado. TCLocPrest é uma xs:choice OBRIGATÓRIA
                    // (cLocPrestacao | cPaisPrestacao): sem nenhum dos dois o grupo sai vazio
                    // e o schema rejeita a DPS inteira — por isso o fallback não é opcional.
                    // Sem o código no cadastro do serviço, assume o município do emitente, que
                    // é onde o ISS é devido na regra geral (LC 116/2003, art. 3º).
                    CodMunicipioPrestacao = SomenteDigitos(cadastro?.LocationCode) ?? codMunEmitente
                },
                Informacoes = new InformacoesServico
                {
                    CodTributacaoNacional = SomenteDigitos(cadastro?.NationalTaxCode),
                    CodTributacaoMunicipio = string.IsNullOrWhiteSpace(cadastro?.MunicipalTaxCode)
                        ? null
                        : SomenteDigitos(cadastro.MunicipalTaxCode),
                    CodNBS = string.IsNullOrWhiteSpace(cadastro?.NbsCode) ? null : SomenteDigitos(cadastro.NbsCode),
                    CodInterno = NormalizarCodigoInterno(cadastro?.InternalContributorCode),
                    Descricao = Discriminar(itens)
                }
            };

            AplicarTipoEspecial(servico, cadastro);

            return servico;
        }

        /// <summary>
        /// Discriminação dos serviços. A DPS tem um único campo de descrição, então quando a
        /// OS tem mais de um item juntamos as descrições com a quantidade e o valor de cada
        /// um — a nota precisa refletir o que foi efetivamente prestado, e emitir só a
        /// descrição do primeiro item esconderia os demais.
        /// </summary>
        private static string Discriminar(List<ServiceInvoiceItem> itens)
        {
            if (itens.Count == 1)
                return DescricaoDoItem(itens[0]);

            return string.Join(" | ", itens.Select(DescricaoDoItem));
        }

        private static string DescricaoDoItem(ServiceInvoiceItem i)
        {
            var texto = string.IsNullOrWhiteSpace(i.Description) ? i.ServiceProvided?.Description : i.Description;
            if (string.IsNullOrWhiteSpace(texto)) texto = i.ServiceProvided?.Name ?? "Serviço prestado";

            var quantidade = i.Quantity.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

            // "2 x Consultoria em TI" — a quantidade só aparece quando não é 1, para não
            // poluir a descrição do caso mais comum.
            var prefixo = i.Quantity == 1m ? string.Empty : $"{quantidade} x ";

            return prefixo + texto;
        }

        /// <summary>
        /// Subgrupos opcionais do grupo <c>serv</c> (obra / evento / comércio exterior),
        /// derivados do <see cref="ServiceSpecialType"/> do serviço cadastrado.
        ///
        /// Cada grupo tem exigências próprias e um grupo emitido pela metade é rejeitado
        /// pelo schema, derrubando a emissão inteira. Por isso, quando o cadastro declara o
        /// tipo especial mas não tem os dados que o grupo exige, lançamos com o campo que
        /// falta em vez de omitir o grupo em silêncio — omitir mudaria o tratamento fiscal
        /// da nota sem que ninguém percebesse.
        /// </summary>
        private static void AplicarTipoEspecial(ServicoNFSe servico, ServiceProvided? cadastro)
        {
            switch (cadastro?.SpecialType)
            {
                case ServiceSpecialType.Construction:
                    AplicarObra(servico, cadastro);
                    break;

                case ServiceSpecialType.Event:
                    AplicarEvento(servico, cadastro);
                    break;

                case ServiceSpecialType.ForeignTrade:
                    AplicarComercioExterior(servico, cadastro);
                    break;
            }
        }

        /// <summary>
        /// <c>TCInfoObra</c> é uma <c>xs:choice</c>: exatamente UM entre <c>cObra</c>
        /// (CNO/CEI), <c>cCIB</c> e <c>end</c>. Preencher mais de um invalida o XML — e é
        /// fácil fazer isso sem perceber, porque o cadastro do serviço guarda os dois
        /// códigos. Priorizamos o CNO, que é o identificador usual da obra.
        /// </summary>
        private static void AplicarObra(ServicoNFSe servico, ServiceProvided cadastro)
        {
            var codObra = SomenteDigitos(cadastro.ConstructionCode);
            var codCib = SomenteDigitos(cadastro.CibCode);

            var obra = new ObraNFSe
            {
                InscricaoImobiliaria = SomenteDigitos(cadastro.PropertyRegistry)
            };

            if (!string.IsNullOrWhiteSpace(codObra))
            {
                obra.CodObra = codObra;
            }
            else if (!string.IsNullOrWhiteSpace(codCib))
            {
                // TSCodCIB exige exatamente 8 dígitos — conferimos aqui para o erro sair
                // legível, em vez de virar XmlSchemaException depois da DPS assinada.
                if (codCib.Length != 8)
                    throw new InvalidOperationException(
                        $"DPS: CIB da obra deve ter 8 dígitos (recebido \"{codCib}\").");

                obra.CodigoCIB = codCib;
            }
            else
            {
                throw new InvalidOperationException(
                    "DPS: serviço do tipo obra sem CNO/CEI nem CIB no cadastro — um dos dois é obrigatório.");
            }

            servico.Obra = obra;
        }

        /// <summary>
        /// <c>TCAtvEvento</c> exige <c>xNome</c>, <c>dtIni</c>, <c>dtFim</c> e, ao final,
        /// exatamente UM entre <c>idAtvEvt</c> e <c>end</c>. Não montamos endereço de
        /// evento, então o identificador é obrigatório.
        ///
        /// As datas não têm default: um <c>DateTime.MinValue</c> viraria <c>0001-01-01</c>
        /// no XML, que o schema aceita como data e o SEFIN rejeita como fato.
        /// </summary>
        private static void AplicarEvento(ServicoNFSe servico, ServiceProvided cadastro)
        {
            if (string.IsNullOrWhiteSpace(cadastro.EventName))
                throw new InvalidOperationException("DPS: serviço do tipo evento sem nome do evento (xNome).");

            if (!cadastro.EventStartDate.HasValue || !cadastro.EventEndDate.HasValue)
                throw new InvalidOperationException(
                    "DPS: serviço do tipo evento sem data de início e/ou de fim (dtIni/dtFim).");

            if (string.IsNullOrWhiteSpace(cadastro.EventIdentifier))
                throw new InvalidOperationException(
                    "DPS: serviço do tipo evento sem identificador (idAtvEvt) — obrigatório, "
                    + "já que a DPS não leva o endereço do evento.");

            servico.Evento = new EventoServicoNFSe
            {
                Descricao = cadastro.EventName,
                DataInicio = cadastro.EventStartDate.Value,
                DataFim = cadastro.EventEndDate.Value,
                IdEvento = cadastro.EventIdentifier
            };
        }

        private static void AplicarComercioExterior(ServicoNFSe servico, ServiceProvided cadastro)
        {
            if (!int.TryParse(cadastro.CurrencyCode, out var moeda))
                throw new InvalidOperationException(
                    "DPS: serviço de comércio exterior sem código de moeda (tpMoeda).");

            if (!cadastro.ForeignValue.HasValue)
                throw new InvalidOperationException(
                    "DPS: serviço de comércio exterior sem valor em moeda estrangeira (vServMoeda).");

            servico.ServicoExterior = new ServicoExterior
            {
                // Os dois enums abaixo são não-anuláveis no leiaute. Quando o cadastro não
                // tem o código, vale o default (0 = "Desconhecido"), que é exatamente o
                // valor que o leiaute prevê para "não informado".
                Modo = ParseEnum<ModoPrestacao>(cadastro.ServiceMode) ?? default,
                Vinculo = ParseEnum<VinculoPrestador>(cadastro.ServiceLink) ?? default,
                CodMoeda = moeda,
                ValorServico = cadastro.ForeignValue.Value
            };
        }

        private static ValoresDps MontarValores(List<ServiceInvoiceItem> itens)
        {
            // Os totais são RECALCULADOS dos itens aqui, e não lidos de fatura.TotalValue /
            // IssqnValue / itens[i].TotalPrice. Duas razões:
            //
            // 1. TotalPrice de item é preenchido pela camada de serviço, não pelos totais
            //    consolidados — depender dele cria um acoplamento invisível onde esquecer de
            //    preenchê-lo faz o ISSQN sumir da DPS em silêncio (a nota sai sem imposto).
            // 2. ServiceTotals é a fonte única do cálculo. Recalcular aqui garante que a DPS
            //    declarada é exatamente o que a OS registrou, sem depender de ordem de chamadas.
            var t = ServiceTotals.ForInvoice(itens);

            var valores = new ValoresDps
            {
                // vServ é o valor BRUTO (soma dos itens antes do desconto). O desconto vai em
                // grupo próprio — mandar o líquido aqui E o desconto separado contaria o
                // desconto duas vezes na base do ISS.
                ValoresServico = new ValoresServico { Valor = t.Subtotal }
            };

            if (t.DiscountValue > 0)
            {
                // Só o desconto INCONDICIONAL é modelado (LC 116/2003, art. 7º). O condicional
                // existe no leiaute mas depende de evento posterior, que não temos como comprovar.
                valores.ValoresDesconto = new ValoresDesconto
                {
                    ValorIncodicional = t.DiscountValue
                };
            }

            // tribMun é OBRIGATÓRIO no leiaute e, dentro dele, tribISSQN e tpRetISSQN também.
            // Por isso o grupo é sempre montado — nunca condicionado a "tem imposto?".
            var municipal = new TributoMunicipal
            {
                ISSQN = TributoISSQN.OperacaoTributavel,

                // Retenção é por nota: o tomador retém quando QUALQUER item manda reter.
                // Declarar "não retido" com ISS retido apurado seria contraditório perante o fisco.
                TipoRetencaoISSQN = t.IssqnRetidoValue > 0
                    ? TipoRetencaoISSQN.RetidoTomador
                    : TipoRetencaoISSQN.NaoRetido
            };

            // Alíquota só é declarada quando há ISS apurado. Como os itens podem ter alíquotas
            // diferentes, declaramos a alíquota efetiva ponderada pela base: é o único número
            // que, aplicado à base total, reproduz o ISS total.
            if (t.BaseTotal > 0 && t.IssqnValue > 0)
            {
                municipal.Aliquota = Math.Round(t.IssqnValue / t.BaseTotal * 100m, 2, MidpointRounding.AwayFromZero);
            }

            // Retenções federais: o leiaute carrega VALOR para IRRF/CSLL/CP. PIS/COFINS têm
            // grupo próprio (piscofins) que exige CST — não emitimos sem a classificação.
            var porItem = itens.Select(ServiceTotals.ForItem).ToList();
            var ir = porItem.Sum(i => i.Ir);
            var csll = porItem.Sum(i => i.Csll);
            var inss = porItem.Sum(i => i.Inss);

            var tributos = new TributosNFSe
            {
                Municipal = municipal,
                Total = new TotalTributos
                {
                    // totTrib é obrigatório e é um xs:choice: exige UM dos quatro filhos.
                    // indTotTrib = 0 significa "não informar valor estimado" (Decreto
                    // 8.264/2014) — que é a verdade: não calculamos o total aproximado da
                    // Lei 12.741/2012. Preferimos declarar zero a inventar um percentual.
                    IndicadorTotal = 0
                }
            };

            if (ir > 0 || csll > 0 || inss > 0)
            {
                tributos.Federal = new TributoFederal
                {
                    ValorIRRF = ir > 0 ? ir : null,
                    ValorCSLL = csll > 0 ? csll : null,
                    ValorCP = inss > 0 ? inss : null
                    
                };
            }

            valores.Tributos = tributos;

            return valores;
        }

        private static T? ParseEnum<T>(string? valor) where T : struct, Enum
            => Enum.TryParse<T>(valor, out var v) ? v : null;

        /// <summary>
        /// Normaliza o código interno do contribuinte para o que o leiaute aceita:
        /// <c>[a-zA-Z0-9]{1,20}</c>. O cadastro costuma guardar códigos legíveis com hífen
        /// ("INT-001"), que o XSD REJEITA — a DPS seria recusada por causa de um traço.
        /// Devolve nulo quando não sobra nada, porque o campo é opcional.
        /// </summary>
        private static string? NormalizarCodigoInterno(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            var limpo = new string(codigo.Where(char.IsLetterOrDigit).ToArray());
            if (limpo.Length == 0)
                return null;

            return limpo.Length > 20 ? limpo.Substring(0, 20) : limpo;
        }

        /// <summary>
        /// Remove tudo que não é dígito. Os campos do leiaute são numéricos e os nossos
        /// cadastros guardam documentos e códigos formatados (pontos, traços, barras).
        /// </summary>
        public static string? SomenteDigitos(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            var digitos = new string(texto.Where(char.IsDigit).ToArray());
            if (digitos.Length == 0)
                return null;

            return digitos;
        }

        /// <summary>Variante que exige valor — usada nos campos sem os quais a DPS não é montável.</summary>
        private static string ExigirDigitos(string? texto, string campo)
        {
            var digitos = SomenteDigitos(texto);
            if (string.IsNullOrWhiteSpace(digitos))
                throw new InvalidOperationException($"DPS: {campo} não informado.");

            return digitos;
        }
    }
}
