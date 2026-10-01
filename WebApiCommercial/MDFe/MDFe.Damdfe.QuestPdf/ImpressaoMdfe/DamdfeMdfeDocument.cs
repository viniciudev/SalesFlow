using System.Globalization;
using BarcodeStandard;
using DFe.Utils;
using MDFe.Classes.Flags;
using MDFe.Classes.Informacoes;
using MDFe.Classes.Retorno;
using MDFe.Classes.Retorno.MDFeRetRecepcao;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SaveTypes = BarcodeStandard.SaveTypes;

namespace MDFe.Damdfe.QuestPdf.ImpressaoMdfe;

/// <summary>
/// DAMDFe — Documento Auxiliar do Manifesto Eletrônico de Documentos, em PDF,
/// pelo QuestPDF. Espelha o <c>DanfeNfeDocument</c> do DANFE de NF-e: recebe o
/// XML já gravado, o logo da empresa e devolve os bytes do PDF.
///
/// Duas diferenças de desenho em relação ao DANFE de NF-e, de propósito:
///
/// <list type="number">
///   <item>o XML pode chegar como <c>mdfeProc</c> (manifesto + protocolo, que é o
///   que fica gravado depois da autorização) ou como <c>MDFe</c> solto. Os dois
///   são aceitos: sem isso a impressão de um manifesto autorizado falharia
///   justamente no único caso em que ela é chamada.</item>
///   <item>a Arial é <b>embutida e registrada</b> no <c>FontManager</c>, e não
///   pedida por nome ao sistema — em container Linux não há garantia de fonte
///   instalada, e o layout sairia com métricas diferentes das do papel.</item>
/// </list>
///
/// A tarja (<c>CANCELADO</c>/<c>ENCERRADO</c>) vem por parâmetro e não do XML: o
/// documento continua sendo um manifesto autorizado depois do cancelamento, e o
/// XML não muda por causa disso. Quem sabe o estado é o banco.
/// </summary>
public class DamdfeMdfeDocument : IDocument
{
    private readonly byte[]? _logo;
    private readonly string? _tarja;

    private MDFe.Classes.Informacoes.MDFe _mdfe = null!;
    private MDFeInfProtMDFe? _protocolo;

    /// <summary>
    /// O nome que o SkiaSharp vai procurar depois do registro. É o nome interno
    /// da própria Arial.ttf — trocar por outro nome aqui faria o registro passar
    /// e o texto cair na fonte padrão sem avisar.
    /// </summary>
    private const string Fonte = "Arial";

    private static float FontePequena => 6.5f;
    private static float FonteNormal => 8f;
    private static float FonteTitulo => 12f;

    static DamdfeMdfeDocument()
    {
        RegistrarFonteEmbutida();
    }

    /// <param name="xml">XML do manifesto: <c>mdfeProc</c> (autorizado) ou <c>MDFe</c>.</param>
    /// <param name="logo">Logo da empresa, opcional.</param>
    /// <param name="tarja">Texto da tarja (<c>CANCELADO</c>/<c>ENCERRADO</c>), ou nulo no manifesto vigente.</param>
    public DamdfeMdfeDocument(string xml, byte[]? logo, string? tarja = null)
    {
        _logo = logo;
        _tarja = string.IsNullOrWhiteSpace(tarja) ? null : tarja!.Trim().ToUpperInvariant();
        CarregarXml(xml);
    }

    /// <summary>
    /// Registra a Arial embutida. Silencioso de propósito quando o recurso não
    /// está no assembly: o QuestPDF cai na fonte padrão e o documento ainda
    /// imprime — falhar a impressão do manifesto por causa de fonte seria pior
    /// do que imprimir com outra.
    /// </summary>
    private static void RegistrarFonteEmbutida()
    {
        try
        {
            var assembly = typeof(DamdfeMdfeDocument).Assembly;

            var nome = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("Arial.ttf", StringComparison.OrdinalIgnoreCase));

            if (nome == null)
                return;

            using var stream = assembly.GetManifestResourceStream(nome);
            if (stream == null)
                return;

            QuestPDF.Drawing.FontManager.RegisterFont(stream);
        }
        catch (Exception)
        {
            // Fonte é apresentação, não conteúdo.
        }
    }

    private void CarregarXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("O XML do manifesto está vazio.");

        try
        {
            var proc = FuncoesXml.XmlStringParaClasse<MDFeProcMDFe>(xml);
            if (proc?.MDFe != null)
            {
                _mdfe = ComVersaoDoDocumento(proc.MDFe);
                _protocolo = proc.ProtMDFe?.InfProt;
                return;
            }
        }
        catch (Exception)
        {
            // Não é um procMDFe: tenta o MDFe solto abaixo.
        }

        try
        {
            _mdfe = ComVersaoDoDocumento(
                FuncoesXml.XmlStringParaClasse<MDFe.Classes.Informacoes.MDFe>(xml));
            _protocolo = null;
        }
        catch (Exception)
        {
            throw new ArgumentException("Verifique se o XML do manifesto está correto.");
        }
    }

    /// <summary>
    /// Propaga a versão do leiaute declarada pelo documento para o <c>ide</c>
    /// (<c>infMDFe/@versao</c> → <c>ide/@versao</c>).
    ///
    /// Sem isto a impressão quebra: o <c>XmlSerializer</c> cria o <c>MDFeIde</c>
    /// pelo construtor privado de serialização, que não recebe a versão, e os
    /// proxies de data caem no singleton <c>MDFeConfiguracao.Instancia</c> — que
    /// numa aplicação web nunca é configurado. O <c>ProxyDhEmi</c> lido no
    /// cabeçalho lançaria "Versão Inválida para MDF-e" ao montar o PDF.
    /// </summary>
    private static MDFe.Classes.Informacoes.MDFe ComVersaoDoDocumento(MDFe.Classes.Informacoes.MDFe documento)
    {
        documento.InfMDFe.Ide.VersaoLayout = documento.InfMDFe.Versao;
        return documento;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public byte[] GerarPdfBytes() => this.GeneratePdf();

    // ------------------------------------------------------------------
    // Acesso ao leiaute
    // ------------------------------------------------------------------

    private MDFeInfMDFe Inf => _mdfe.InfMDFe;

    private string Chave => (Inf.Id ?? string.Empty).StartsWith("MDFe", StringComparison.Ordinal)
        ? Inf.Id!.Substring(4)
        : Inf.Id ?? string.Empty;

    /// <summary>
    /// O modal rodoviário. <c>infModal</c> é <c>xs:any</c> no schema, então o
    /// grupo pode ser <c>rodo</c>, <c>aereo</c>, <c>aquav</c>… — as seções que
    /// dependem de <c>rodo</c> só aparecem quando ele está lá, em vez de estourar
    /// um cast.
    /// </summary>
    private MDFeRodo? Rodo => Inf.InfModal?.Modal as MDFeRodo;

    private static string Dinheiro(decimal valor) => valor.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));

    private static string Quantidade(decimal valor) => valor.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));

    private static string Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? "-" : valor!.Trim();

    /// <summary>
    /// Texto do <c>tpEmit</c>. O enum do leiaute tem um terceiro valor
    /// (transportador que emite CT-e globalizado) que o sistema ainda não emite —
    /// por isso o <c>default</c> devolve o número cru em vez de cair em "carga
    /// própria", que seria uma informação errada no papel.
    /// </summary>
    private static string DescricaoTipoEmitente(MDFeTipoEmitente tipo)
    {
        return tipo switch
        {
            MDFeTipoEmitente.PrestadorServicoDeTransporte => "1 - PRESTADOR DE SERVIÇO DE TRANSPORTE",
            MDFeTipoEmitente.TransportadorCargaPropria => "2 - TRANSPORTADOR DE CARGA PRÓPRIA",
            MDFeTipoEmitente.PrestadorServicoDeTransporteCTeGlobalizado => "3 - PRESTADOR DE SERV. DE TRANSPORTE (CT-e GLOBALIZADO)",
            _ => ((int)tipo).ToString()
        };
    }

    // ------------------------------------------------------------------
    // Composição
    // ------------------------------------------------------------------

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(0.5f, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontFamily(Fonte).FontSize(FonteNormal).LineHeight(1.3f));
            page.Header().Element(Cabecalho);
            page.Content().Element(Conteudo);
            page.Footer().Element(Rodape);
        });
    }

    private void Cabecalho(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(2.6f, Unit.Centimetre).Column(c =>
                {
                    if (_logo != null)
                        c.Item().MaxWidth(70).MaxHeight(70).Image(_logo);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text("DAMDFe").FontSize(FonteTitulo + 6).Bold();
                    c.Item().AlignCenter().Text("Documento Auxiliar do Manifesto Eletrônico de Documentos")
                        .FontSize(FontePequena);
                    c.Item().AlignCenter().Text("MODAL RODOVIÁRIO DE CARGAS").FontSize(FontePequena).Bold();
                });

                row.ConstantItem(3.6f, Unit.Centimetre).Column(c =>
                {
                    c.Item().AlignRight().Text($"Nº {Inf.Ide.NMDF:D9}").FontSize(FonteNormal).Bold();
                    c.Item().AlignRight().Text($"Série {Inf.Ide.Serie:D3}").FontSize(FontePequena);
                    c.Item().AlignRight().Text($"Emissão {Inf.Ide.ProxyDhEmi}").FontSize(FontePequena);
                    c.Item().AlignRight().Text($"Folha 1/1").FontSize(FontePequena);
                });
            });

            column.Item().PaddingTop(3);

            // Faixa da chave + código de barras. O código de barras é o que o
            // fiscal lê no posto; a chave legível fica ao lado para conferência
            // quando a leitura falha.
            column.Item().Border(0.5f).Row(row =>
            {
                row.RelativeItem(7).Column(c =>
                {
                    c.Item().PaddingLeft(2).Text("CHAVE DE ACESSO").FontSize(FontePequena - 1);
                    c.Item().PaddingLeft(2).Text(Chave).FontSize(FonteNormal).Bold();
                });
                row.ConstantItem(3.2f, Unit.Centimetre).Padding(1).AlignRight().Image(CodigoDeBarras());
            });

            // Protocolo de autorização: é o que dá validade ao manifesto na
            // fiscalização, então fica no cabeçalho junto da chave.
            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(row =>
            {
                Celula(row.RelativeItem(3), "PROTOCOLO DE AUTORIZAÇÃO", Texto(_protocolo?.NProt));
                Celula(row.RelativeItem(3), "DATA/HORA DE AUTORIZAÇÃO",
                    _protocolo == null || _protocolo.DhRecbto == default ? "-" : _protocolo.DhRecbto.ToString("dd/MM/yyyy HH:mm:ss"));
                Celula(row.RelativeItem(2), "TIPO DO EMITENTE", DescricaoTipoEmitente(Inf.Ide.TpEmit));
                Celula(row.RelativeItem(2), "AMBIENTE",
                    Inf.Ide.TpAmb == DFe.Classes.Flags.TipoAmbiente.Homologacao ? "HOMOLOGAÇÃO" : "PRODUÇÃO");
            });

            if (_tarja != null)
            {
                // Tarja em vermelho porque é assim que o cancelamento é lido de
                // longe, e em tarja de faixa inteira para não depender de a
                // impressão sair colorida: em preto e branco ela ainda é uma
                // faixa com moldura grossa no meio do documento.
                column.Item().PaddingTop(4);
                column.Item().Border(2).BorderColor(Colors.Red.Medium).Padding(3)
                    .AlignCenter().Text(_tarja).FontSize(FonteTitulo + 8).Bold().FontColor(Colors.Red.Medium);
            }
        });
    }

    private void Conteudo(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(4);

            column.Item().Element(BlocoEmitente);
            column.Item().Element(BlocoPercurso);
            column.Item().Element(BlocoMunicipiosDeCarregamento);
            column.Item().Element(BlocoDocumentos);
            column.Item().Element(BlocoComboio);
            column.Item().Element(BlocoProdutoETotais);
            column.Item().Element(BlocoSeguro);
            column.Item().Element(BlocoInformacoesAdicionais);
        });
    }

    private void BlocoEmitente(IContainer container)
    {
        var emit = Inf.Emit;
        var end = emit?.EnderEmit;

        container.Border(0.5f).Column(column =>
        {
            column.Item().Element(TituloSecao("EMITENTE"));
            column.Item().Row(row =>
            {
                Celula(row.RelativeItem(4), "NOME / RAZÃO SOCIAL", Texto(emit?.XNome));
                Celula(row.RelativeItem(2), "CNPJ / CPF", Texto(emit?.CNPJ ?? emit?.CPF));
                Celula(row.RelativeItem(1), "IE", Texto(emit?.IE));
            });
            column.Item().Row(row =>
            {
                Celula(row.RelativeItem(4), "ENDEREÇO", Texto(end?.XLgr));
                Celula(row.RelativeItem(1), "Nº", Texto(end?.Nro));
                Celula(row.RelativeItem(2), "BAIRRO", Texto(end?.XBairro));
                Celula(row.RelativeItem(2), "MUNICÍPIO", Texto(end?.XMun));
                Celula(row.RelativeItem(1), "UF", end?.ProxyUF ?? "-");
            });
        });
    }

    private void BlocoPercurso(IContainer container)
    {
        var ide = Inf.Ide;
        var percursos = ide.InfPercurso ?? new List<MDFeInfPercurso>();

        container.Border(0.5f).Column(column =>
        {
            column.Item().Element(TituloSecao("PERCURSO"));
            column.Item().Row(row =>
            {
                Celula(row.RelativeItem(2), "UF DE CARREGAMENTO", ide.ProxyUFIni);
                Celula(row.RelativeItem(2), "UF DE DESCARREGAMENTO", ide.ProxyUFFim);
                Celula(row.RelativeItem(1), "MODAL", "1 - RODOVIÁRIO");
                Celula(row.RelativeItem(2), "INÍCIO DA VIAGEM",
                    ide.DhIniViagem.HasValue ? ide.ProxyDhIniViagem : "-");
                Celula(row.RelativeItem(3), "UF(s) DO PERCURSO",
                    percursos.Count == 0 ? "-" : string.Join(", ", percursos.Select(p => p.ProxyUFPer)));
            });
        });
    }

    private void BlocoMunicipiosDeCarregamento(IContainer container)
    {
        var municipios = Inf.Ide.InfMunCarrega ?? new List<MDFeInfMunCarrega>();

        container.Border(0.5f).Column(column =>
        {
            column.Item().Element(TituloSecao("MUNICÍPIO(S) DE CARREGAMENTO"));
            column.Item().Row(row =>
            {
                if (municipios.Count == 0)
                {
                    Celula(row.RelativeItem(), "MUNICÍPIO", "-");
                    return;
                }

                foreach (var m in municipios)
                    Celula(row.RelativeItem(), "MUNICÍPIO / CÓD. IBGE", $"{m.XMunCarrega} — {m.CMunCarrega}");
            });
        });
    }

    private void BlocoDocumentos(IContainer container)
    {
        var descargas = Inf.InfDoc?.InfMunDescarga ?? new List<MDFeInfMunDescarga>();

        container.Border(0.5f).Column(column =>
        {
            column.Item().Element(TituloSecao("DOCUMENTOS FISCAIS VINCULADOS — POR MUNICÍPIO DE DESCARGA"));

            if (descargas.Count == 0)
            {
                column.Item().Padding(2).Text("Nenhum documento vinculado.").FontSize(FontePequena);
                return;
            }

            foreach (var descarga in descargas)
            {
                column.Item().PaddingTop(2).PaddingLeft(2).Text(
                    $"{Texto(descarga.XMunDescarga)} — IBGE {Texto(descarga.CMunDescarga)}")
                    .FontSize(FontePequena).Bold();

                var chaves = (descarga.InfNFe ?? new List<MDFeInfNFe>()).Select(n => n.ChNFe ?? string.Empty)
                    .Concat((descarga.InfCTe ?? new List<MDFeInfCTe>()).Select(c => c.ChCTe ?? string.Empty))
                    .Concat((descarga.InfMdFeTransps ?? new List<MDFeInfMDFeTransp>()).Select(m => m.ChMDFe ?? string.Empty))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                if (chaves.Count == 0)
                {
                    column.Item().PaddingLeft(4).Text("— sem chaves neste município").FontSize(FontePequena);
                    continue;
                }

                foreach (var chave in chaves)
                    column.Item().PaddingLeft(4).Text(chave).FontSize(FontePequena).FontFamily(Fonts.CourierNew);
            }
        });
    }

    private void BlocoComboio(IContainer container)
    {
        var tracao = Rodo?.VeicTracao;
        var reboques = Rodo?.VeicReboque ?? new List<MDFeVeicReboque>();

        container.Border(0.5f).Column(column =>
        {
            column.Item().Element(TituloSecao("COMBOIO"));

            if (Rodo == null)
            {
                column.Item().Padding(2).Text("Modal não rodoviário: sem dados de comboio.").FontSize(FontePequena);
                return;
            }

            column.Item().Row(row =>
            {
                Celula(row.RelativeItem(2), "PLACA DE TRAÇÃO", Texto(tracao?.Placa));
                Celula(row.RelativeItem(2), "RENAVAM", Texto(tracao?.RENAVAM));
                Celula(row.RelativeItem(2), "TARA (KG)", tracao?.Tara?.ToString() ?? "-");
                Celula(row.RelativeItem(2), "CAPACIDADE (KG)", tracao?.CapKG?.ToString() ?? "-");
                Celula(row.RelativeItem(2), "TIPO DE RODADO", tracao == null ? "-" : tracao.TpRod.ToString());
                Celula(row.RelativeItem(2), "CARROCERIA", tracao == null ? "-" : tracao.TpCar.ToString());
                Celula(row.RelativeItem(1), "UF", tracao?.ProxyUF ?? "-");
            });

            foreach (var reboque in reboques)
            {
                column.Item().Row(row =>
                {
                    Celula(row.RelativeItem(2), "PLACA DO REBOQUE", Texto(reboque.Placa));
                    Celula(row.RelativeItem(2), "RENAVAM", Texto(reboque.RENAVAM));
                    Celula(row.RelativeItem(2), "TARA (KG)", reboque.Tara?.ToString() ?? "-");
                    Celula(row.RelativeItem(2), "CAPACIDADE (KG)", reboque.CapKG?.ToString() ?? "-");
                    Celula(row.RelativeItem(2), "CARROCERIA", reboque.TpCar.ToString());
                    Celula(row.RelativeItem(2), "UF", reboque.ProxyUF ?? "-");
                    Celula(row.RelativeItem(1), "TIPO", "REBOQUE");
                });
            }

            // Condutores: o MOC exige que a DAMDFe os identifique — é a
            // informação que a fiscalização confere na abordagem.
            var condutores = tracao?.Condutor ?? new List<MDFeCondutor>();
            column.Item().Row(row =>
            {
                if (condutores.Count == 0)
                {
                    Celula(row.RelativeItem(), "CONDUTOR", "-");
                    return;
                }

                foreach (var condutor in condutores)
                    Celula(row.RelativeItem(), "CONDUTOR / CPF", $"{Texto(condutor.XNome)} — {Texto(condutor.CPF)}");
            });

            var proprietario = tracao?.Prop;
            if (proprietario != null)
            {
                column.Item().Row(row =>
                {
                    Celula(row.RelativeItem(3), "PROPRIETÁRIO DO VEÍCULO (TERCEIRO)", Texto(proprietario.XNome));
                    Celula(row.RelativeItem(2), "CNPJ / CPF", Texto(proprietario.CNPJ ?? proprietario.CPF));
                    Celula(row.RelativeItem(2), "RNTRC", Texto(proprietario.RNTRC));
                    Celula(row.RelativeItem(1), "UF", proprietario.ProxyUF ?? "-");
                });
            }
        });
    }

    private void BlocoProdutoETotais(IContainer container)
    {
        var produto = Inf.ProdPred;
        var tot = Inf.Tot;

        container.Border(0.5f).Column(column =>
        {
            column.Item().Element(TituloSecao("PRODUTO PREDOMINANTE E TOTAIS DA CARGA"));
            column.Item().Row(row =>
            {
                Celula(row.RelativeItem(3), "PRODUTO PREDOMINANTE", Texto(produto?.XProd));
                Celula(row.RelativeItem(2), "TIPO DE CARGA", produto == null ? "-" : produto.TpCarga.ToString());
                Celula(row.RelativeItem(2), "NCM", Texto(produto?.Ncm));
                Celula(row.RelativeItem(1), "NF-e", tot?.QNFe?.ToString() ?? "-");
                Celula(row.RelativeItem(1), "CT-e", tot?.QCTe?.ToString() ?? "-");
                Celula(row.RelativeItem(1), "MDF-e", tot?.QMDFe?.ToString() ?? "-");
            });
            column.Item().Row(row =>
            {
                Celula(row.RelativeItem(3), "VALOR TOTAL DA CARGA (R$)", tot == null ? "-" : Dinheiro(tot.vCarga));
                Celula(row.RelativeItem(3), "QUANTIDADE DA CARGA", tot == null ? "-" : Quantidade(tot.QCarga));
                Celula(row.RelativeItem(2), "UNIDADE DE MEDIDA", tot == null ? "-" : tot.CUnid.ToString());
                Celula(row.RelativeItem(2), "CIOT / RNTRC",
                    $"{Texto(Rodo?.CIOT)} / {Texto(Rodo?.RNTRC)}");
            });
        });
    }

    private void BlocoSeguro(IContainer container)
    {
        var seguros = Inf.Seg ?? new List<MDFeSeg>();

        container.Border(0.5f).Column(column =>
        {
            column.Item().Element(TituloSecao("SEGURO DA CARGA"));

            if (seguros.Count == 0)
            {
                column.Item().Padding(2).Text("Sem seguro informado no manifesto.").FontSize(FontePequena);
                return;
            }

            foreach (var seguro in seguros)
            {
                column.Item().Row(row =>
                {
                    Celula(row.RelativeItem(2), "RESPONSÁVEL",
                        seguro.InfResp == null ? "-" : seguro.InfResp.RespSeg.ToString());
                    Celula(row.RelativeItem(3), "SEGURADORA", Texto(seguro.InfSeg?.XSeg));
                    Celula(row.RelativeItem(2), "CNPJ", Texto(seguro.InfSeg?.CNPJ));
                    Celula(row.RelativeItem(2), "APÓLICE", Texto(seguro.NApol));
                    Celula(row.RelativeItem(4), "AVERBAÇÕES",
                        seguro.NAver == null || seguro.NAver.Count == 0
                            ? "-"
                            : string.Join(", ", seguro.NAver));
                });
            }
        });
    }

    private void BlocoInformacoesAdicionais(IContainer container)
    {
        var adicional = Inf.InfAdic;
        var qrCodigo = _mdfe.InfMDFeSupl?.QrCodMDFe;

        container.Border(0.5f).Row(row =>
        {
            row.RelativeItem(4).Column(column =>
            {
                column.Item().Element(TituloSecao("INFORMAÇÕES ADICIONAIS"));
                column.Item().PaddingLeft(2).PaddingRight(2).PaddingBottom(2)
                    .DefaultTextStyle(t => t.FontSize(FontePequena))
                    .Text(text =>
                {
                    text.Span("Fisco: ").Bold();
                    text.Span(Texto(adicional?.InfAdFisco) + "\n");
                    text.Span("Complementares: ").Bold();
                    text.Span(Texto(adicional?.InfCpl));
                });
            });

            // QR Code: o MOC manda imprimi-lo na DAMDFe. Sem ele (manifesto
            // antigo, gravado antes do infMDFeSupl entrar) a célula sai vazia em
            // vez de derrubar a impressão inteira.
            row.ConstantItem(2.8f, Unit.Centimetre).BorderLeft(0.5f).Padding(2).Column(column =>
            {
                var imagem = ImagemQrCode(qrCodigo);
                if (imagem.Length > 0)
                    column.Item().AlignCenter().Image(imagem).FitArea();
                else
                    column.Item().AlignCenter().Text("QR Code indisponível").FontSize(FontePequena - 1);
            });
        });
    }

    private void Rodape(IContainer container)
    {
        container.AlignCenter().DefaultTextStyle(t => t.FontSize(FontePequena - 1)).Text(text =>
        {
            text.Span("DAMDFe gerada pelo ServiceBOX — ");
            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            text.Span("   |   Página ");
            text.CurrentPageNumber();
            text.Span(" de ");
            text.TotalPages();
        });
    }

    // ------------------------------------------------------------------
    // Peças reutilizadas
    // ------------------------------------------------------------------

    /// <summary>Faixa de título de seção, com fundo cinza.</summary>
    private static Action<IContainer> TituloSecao(string titulo)
    {
        return container => container.Background(Colors.Grey.Lighten2).PaddingLeft(2).PaddingVertical(1)
            .Text(titulo).FontSize(FontePequena).Bold();
    }

    /// <summary>
    /// Célula rótulo/valor com as bordas internas que separam as colunas do
    /// bloco. A borda é aplicada aqui, e não no bloco, porque cada linha de
    /// células tem contagem diferente — é o mesmo desenho das grades do DANFE.
    /// </summary>
    private static void Celula(IContainer container, string rotulo, string valor)
    {
        container.BorderLeft(0.5f).PaddingLeft(2).PaddingRight(2).PaddingVertical(1).Column(column =>
        {
            column.Item().Text(rotulo).FontSize(FontePequena - 1);
            column.Item().Text(string.IsNullOrWhiteSpace(valor) ? "-" : valor).FontSize(FontePequena).Bold();
        });
    }

    private byte[] CodigoDeBarras()
    {
        var barcode = new Barcode();
        barcode.Encode(BarcodeStandard.Type.Code128, Chave, 400, 40);
        return barcode.GetImageData(SaveTypes.Png);
    }

    /// <summary>
    /// QR Code do <c>infMDFeSupl</c>. Devolve vazio quando não há o que codificar
    /// — a impressão segue sem ele, em vez de estourar.
    /// </summary>
    private static byte[] ImagemQrCode(string? conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            return Array.Empty<byte>();

        try
        {
            using var gerador = new QRCoder.QRCodeGenerator();
            var dados = gerador.CreateQrCode(conteudo, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qr = new QRCoder.PngByteQRCode(dados);
            return qr.GetGraphic(20);
        }
        catch (Exception)
        {
            return Array.Empty<byte>();
        }
    }
}
