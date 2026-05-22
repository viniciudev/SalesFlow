using System.Text;
using BarcodeStandard;
using DFe.Classes.Flags;
using DFe.Utils;
using NFe.Classes;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Transporte;
using NFe.Danfe.Base.NFe;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SaveTypes = BarcodeStandard.SaveTypes;

namespace NFe.Danfe.QuestPdf.ImpressaoNfe;

public class DanfeNfeDocument : IDocument
{
    private readonly byte[]? _logo;
    private nfeProc? _nfeProc;
    private NFe.Classes.NFe? _nfe;
    private ConfiguracaoDanfeNfe _configuracao;
    private string _desenvolvedor;

    private static string FontFamily => "Times New Roman";
    private static float FontSizeSmall => 7f;
    private static float FontSizeNormal => 8f;
    private static float FontSizeTitle => 12f;

    public DanfeNfeDocument(string xml, byte[]? logo, ConfiguracaoDanfeNfe? configuracao = null, string desenvolvedor = "")
    {
        _logo = logo;
        _configuracao = configuracao ?? new ConfiguracaoDanfeNfe();
        _desenvolvedor = desenvolvedor;
        CarregarXml(xml);
    }

    private void CarregarXml(string xml)
    {
        try
        {
            _nfeProc = null;
            _nfe = null;
            _nfeProc = FuncoesXml.XmlStringParaClasse<nfeProc>(xml);
            _nfe = _nfeProc.NFe;
        }
        catch (Exception)
        {
            try
            {
                NFe.Classes.NFe nfe = FuncoesXml.XmlStringParaClasse<NFe.Classes.NFe>(xml);
                _nfe = nfe;
                _nfeProc = null;
            }
            catch (Exception)
            {
                throw new ArgumentException("Verifique se o XML está correto.");
            }
        }
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginLeft(0.5f, Unit.Centimetre);
            page.MarginRight(0.5f, Unit.Centimetre);
            page.MarginTop(0.5f, Unit.Centimetre);
            page.MarginBottom(0.5f, Unit.Centimetre);
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
                row.ConstantItem(3f, Unit.Centimetre).Column(c =>
                {
                    if (_logo != null)
                        c.Item().MaxWidth(70).MaxHeight(70).Image(_logo);
                });
                row.RelativeItem().Column(c =>
                {
                    var titulo = DeveExibirMensagemContingencia() || DeveExibirMensagemHomologacao()
                        ? GetMensagemAmbiente() : "DANFE";
                    c.Item().AlignCenter().Text(titulo).FontSize(FontSizeTitle + 4).FontFamily(FontFamily).Bold();
                    c.Item().AlignCenter().Text("Documento Auxiliar da Nota Fiscal Eletrônica").FontSize(FontSizeSmall).FontFamily(FontFamily);
                    c.Item().PaddingTop(2);
                    c.Item().AlignCenter().Text("0 - ENTRADA                                                        1 - SAÍDA")
                        .FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
                });
            });

            column.Item().PaddingTop(2);

            column.Item().Border(0.5f).Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().BorderBottom(0.5f).PaddingLeft(2).Text("CHAVE DE ACESSO").FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    c.Item().PaddingLeft(2).Text(_nfe.infNFe.Id.Substring(3)).FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
                });
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().PaddingLeft(2).Text($"Natureza da operação: {_nfe.infNFe.ide.natOp}").FontSize(FontSizeNormal).FontFamily(FontFamily);
                });
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    var proto = DeveExibirDadosProtocolo()
                        ? $"Protocolo de autorização: {_nfeProc.protNFe.infProt.nProt} - {_nfeProc.protNFe.infProt.dhRecbto:dd/MM/yyyy HH:mm:ss}"
                        : "Protocolo de autorização: NFe sem Autorização de Uso da SEFAZ";
                    c.Item().PaddingLeft(2).Text(proto).FontSize(FontSizeNormal).FontFamily(FontFamily);
                });
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(r =>
            {
                r.RelativeItem(5).Column(c =>
                    c.Item().PaddingLeft(2).Text($"Inscrição Estadual: {_nfe.infNFe.emit.IE}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(5).Column(c =>
                    c.Item().PaddingLeft(2).Text("Inscrição Estadual do Substituto Tributário:").FontSize(FontSizeNormal).FontFamily(FontFamily));
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(r =>
                r.RelativeItem().Column(c =>
                    c.Item().PaddingLeft(2).Text($"CNPJ: {FormatarCnpj(_nfe.infNFe.emit.CNPJ)}").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold()));

            column.Item().PaddingTop(5);
        });
    }

    private void Conteudo(IContainer container)
    {
        container.Column(column =>
        {
            // DESTINATÁRIO / REMETENTE
            column.Item().Border(0.5f).PaddingLeft(2).Text("DESTINATÁRIO / REMETENTE").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();

            column.Item().Border(0.5f).Row(r =>
            {
                r.RelativeItem(6).Column(c => c.Item().PaddingLeft(2).Text($"Nome / Razão Social: {_nfe.infNFe.dest?.xNome ?? "NÃO IDENTIFICADO"}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(2).Column(c => c.Item().PaddingLeft(2).Text($"CNPJ / CPF: {_nfe.infNFe.dest?.CNPJ ?? _nfe.infNFe.dest?.CPF ?? ""}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(2).Column(c => c.Item().PaddingLeft(2).Text($"Data Emissão: {_nfe.infNFe.ide.dhEmi:dd/MM/yyyy}").FontSize(FontSizeNormal).FontFamily(FontFamily));
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(r =>
            {
                r.RelativeItem(6).Column(c => c.Item().PaddingLeft(2).Text($"Endereço: {ObterEnderecoDestinatario()}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(2).Column(c => c.Item().PaddingLeft(2).Text($"Bairro: {_nfe.infNFe.dest?.enderDest?.xBairro ?? ""}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(1).Column(c => c.Item().PaddingLeft(2).Text($"CEP: {_nfe.infNFe.dest?.enderDest?.CEP ?? ""}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(1).Column(c => c.Item().PaddingLeft(2).Text($"Data Saída: {(_nfe.infNFe.ide.dhSaiEnt.HasValue ? _nfe.infNFe.ide.dhSaiEnt.Value.ToString("dd/MM/yyyy") : "---")}").FontSize(FontSizeNormal).FontFamily(FontFamily));
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(r =>
            {
                r.RelativeItem(4).Column(c => c.Item().PaddingLeft(2).Text($"Município: {_nfe.infNFe.dest?.enderDest?.xMun ?? ""}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(1).Column(c => c.Item().PaddingLeft(2).Text($"UF: {_nfe.infNFe.dest?.enderDest?.UF ?? ""}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(2).Column(c => c.Item().PaddingLeft(2).Text($"Fone: {_nfe.infNFe.dest?.enderDest?.fone?.ToString() ?? ""}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                r.RelativeItem(3).Column(c => c.Item().PaddingLeft(2).Text($"IE: {_nfe.infNFe.dest?.IE ?? ""}").FontSize(FontSizeNormal).FontFamily(FontFamily));
            });

            // FATURA / DUPLICATAS
            if (_nfe.infNFe.cobr != null)
            {
                column.Item().PaddingTop(5);
                column.Item().Border(0.5f).PaddingLeft(2).Text("FATURA / DUPLICATAS").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
                if (_nfe.infNFe.cobr.fat != null)
                {
                    column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                    {
                        rr.RelativeItem(3).Column(c => c.Item().PaddingLeft(2).Text($"Número: {_nfe.infNFe.cobr.fat.nFat}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                        rr.RelativeItem(3).Column(c => c.Item().PaddingLeft(2).Text($"Valor Original: {_nfe.infNFe.cobr.fat.vOrig:N2}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                        rr.RelativeItem(3).Column(c => c.Item().PaddingLeft(2).Text($"Valor Desconto: {_nfe.infNFe.cobr.fat.vDesc:N2}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                        rr.RelativeItem(3).Column(c => c.Item().PaddingLeft(2).Text($"Valor Líquido: {_nfe.infNFe.cobr.fat.vLiq:N2}").FontSize(FontSizeNormal).FontFamily(FontFamily));
                    });
                }
                if (_nfe.infNFe.cobr.dup != null && _nfe.infNFe.cobr.dup.Count > 0)
                {
                    column.Item().BorderLeft(0.5f).BorderRight(0.5f).Row(rr =>
                    {
                        rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("Número").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                        rr.RelativeItem(4).BorderRight(0.5f).PaddingLeft(2).Text("Vencimento").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                        rr.RelativeItem(5).PaddingLeft(2).Text("Valor").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                    });
                    foreach (var dup in _nfe.infNFe.cobr.dup)
                    {
                        column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                        {
                            rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(dup.nDup).FontSize(FontSizeSmall).FontFamily(FontFamily);
                            rr.RelativeItem(4).BorderRight(0.5f).PaddingLeft(2).Text(dup.dVenc.Value. ToString("dd/MM/yyyy")).FontSize(FontSizeSmall).FontFamily(FontFamily);
                            rr.RelativeItem(5).PaddingLeft(2).Text(dup.vDup.ToString("N2") ?? "").FontSize(FontSizeSmall).FontFamily(FontFamily);
                        });
                    }
                }
            }

            // CÁLCULO DO IMPOSTO
            column.Item().PaddingTop(5);
            column.Item().Border(0.5f).PaddingLeft(2).Text("CÁLCULO DO IMPOSTO").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();

            var icmsTot = _nfe.infNFe.total.ICMSTot;

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).Row(rr =>
            {
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("BASE DE CÁLCULO DO ICMS").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("VALOR DO ICMS").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("BASE CÁLC. ICMS ST").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(3).PaddingLeft(2).Text("VALOR DO ICMS ST").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
            });
            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
            {
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vBC.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vICMS.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vBCST.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
                rr.RelativeItem(3).PaddingLeft(2).Text(icmsTot.vST.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).Row(rr =>
            {
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("VALOR DO FRETE").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("VALOR DO SEGURO").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("DESCONTO").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(3).PaddingLeft(2).Text("OUTRAS DESPESAS").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
            });
            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
            {
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vFrete.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily);
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vSeg.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily);
                rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vDesc.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily);
                rr.RelativeItem(3).PaddingLeft(2).Text(icmsTot.vOutro.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily);
            });

            column.Item().BorderLeft(0.5f).BorderRight(0.5f).Row(rr =>
            {
                rr.RelativeItem(4).BorderRight(0.5f).PaddingLeft(2).Text("VALOR DO IPI").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(4).BorderRight(0.5f).PaddingLeft(2).Text("VALOR TOTAL DOS PRODUTOS").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
                rr.RelativeItem(4).PaddingLeft(2).Text("VALOR TOTAL DA NOTA").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily);
            });
            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
            {
                rr.RelativeItem(4).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vIPI.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily);
                rr.RelativeItem(4).BorderRight(0.5f).PaddingLeft(2).Text(icmsTot.vProd.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily);
                rr.RelativeItem(4).PaddingLeft(2).Text(icmsTot.vNF.ToString("N2")).FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
            });

            // TRANSPORTE
            column.Item().PaddingTop(5);
            column.Item().Border(0.5f).PaddingLeft(2).Text("TRANSPORTADOR / VOLUMES TRANSPORTADOS").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();

            var transp = _nfe.infNFe.transp;
            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
            {
                rr.RelativeItem(2).BorderRight(0.5f).Column(c => c.Item().PaddingLeft(2).Text($"Modalidade do Frete: {ObterModalidadeFrete(transp.modFrete)}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                rr.RelativeItem(4).BorderRight(0.5f).Column(c =>
                {
                    var tn = transp.transporta?.xNome ?? "";
                    var tc = transp.transporta?.CNPJ ?? transp.transporta?.CPF ?? "";
                    c.Item().PaddingLeft(2).Text($"Transportador: {tn}  CNPJ/CPF: {tc}").FontSize(FontSizeSmall).FontFamily(FontFamily);
                });
                rr.RelativeItem(3).BorderRight(0.5f).Column(c => c.Item().PaddingLeft(2).Text($"Placa: {transp.veicTransp?.placa ?? ""}  UF: {transp.veicTransp?.UF ?? ""}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                rr.RelativeItem(3).Column(c => c.Item().PaddingLeft(2).Text($"IE Transportador: {transp.transporta?.IE ?? ""}").FontSize(FontSizeSmall).FontFamily(FontFamily));
            });

            if (transp.transporta != null && !string.IsNullOrEmpty(transp.transporta.xEnder))
            {
                column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                {
                    rr.RelativeItem(4).BorderRight(0.5f).Column(c => c.Item().PaddingLeft(2).Text($"Endereço: {transp.transporta.xEnder}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                    rr.RelativeItem(3).BorderRight(0.5f).Column(c => c.Item().PaddingLeft(2).Text($"Município: {transp.transporta.xMun ?? ""}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                    rr.RelativeItem(2).Column(c => c.Item().PaddingLeft(2).Text($"UF: {transp.transporta.UF ?? ""}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                });
            }

            if (transp.vol != null && transp.vol.Count > 0)
            {
                column.Item().BorderLeft(0.5f).BorderRight(0.5f).Row(rr =>
                {
                    rr.RelativeItem(2).BorderRight(0.5f).PaddingLeft(2).Text("Qtd").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                    rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("Espécie").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                    rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text("Marca").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                    rr.RelativeItem(2).BorderRight(0.5f).PaddingLeft(2).Text("Numeração").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                    rr.RelativeItem(2).PaddingLeft(2).Text("Peso Bruto / Líquido").FontSize(FontSizeSmall).FontFamily(FontFamily).Bold();
                });
                foreach (var vol in transp.vol)
                {
                    column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                    {
                        rr.RelativeItem(2).BorderRight(0.5f).PaddingLeft(2).Text(vol.qVol?.ToString() ?? "").FontSize(FontSizeSmall).FontFamily(FontFamily);
                        rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(vol.esp ?? "").FontSize(FontSizeSmall).FontFamily(FontFamily);
                        rr.RelativeItem(3).BorderRight(0.5f).PaddingLeft(2).Text(vol.marca ?? "").FontSize(FontSizeSmall).FontFamily(FontFamily);
                        rr.RelativeItem(2).BorderRight(0.5f).PaddingLeft(2).Text(vol.nVol ?? "").FontSize(FontSizeSmall).FontFamily(FontFamily);
                        rr.RelativeItem(2).PaddingLeft(2).Text($"Bruto: {vol.pesoB?.ToString() ?? ""}  Líq: {vol.pesoL?.ToString() ?? ""}").FontSize(FontSizeSmall).FontFamily(FontFamily);
                    });
                }
            }

            // DADOS DO PRODUTO / SERVIÇO
            // Grid de 12 colunas: CÓD(0.7) + DESC(4.0) + NCM(0.8) + CST(0.6) + CFOP(0.6) + UN(0.4) + QTD(0.8) + VL UN(1.0) + VL TOT(1.0) + BC ICMS(0.9) + VL ICMS(0.9) + VL IPI(0.6) = 12.3
            column.Item().PaddingTop(5);
            column.Item().Border(0.5f).PaddingLeft(2).Text("DADOS DO PRODUTO / SERVIÇO").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();

            // Header linha 1: rótulos principais
            column.Item().BorderLeft(0.5f).BorderRight(0.5f).Row(rr =>
            {
                rr.RelativeItem(0.7f).BorderRight(0.5f).PaddingLeft(1).Text("CÓD").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(4.0f).BorderRight(0.5f).PaddingLeft(1).Text("DESCRIÇÃO DO PRODUTO / SERVIÇO").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.8f).BorderRight(0.5f).PaddingLeft(1).Text("NCM/SH").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.6f).BorderRight(0.5f).PaddingLeft(1).Text("CST").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.6f).BorderRight(0.5f).PaddingLeft(1).Text("CFOP").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.4f).BorderRight(0.5f).PaddingLeft(1).Text("UN").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.8f).BorderRight(0.5f).PaddingLeft(1).Text("QTD").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(1.0f).BorderRight(0.5f).PaddingLeft(1).Text("VL UNIT").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(1.0f).BorderRight(0.5f).PaddingLeft(1).Text("VL TOTAL").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.9f).BorderRight(0.5f).PaddingLeft(1).Text("BC ICMS").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.9f).BorderRight(0.5f).PaddingLeft(1).Text("VL ICMS").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.6f).PaddingLeft(1).Text("VL IPI").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
            });

            // Header linha 2: sub-rótulos (alíquotas + pedido)
            column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
            {
                rr.RelativeItem(0.7f).BorderRight(0.5f);
                rr.RelativeItem(4.0f).BorderRight(0.5f);
                rr.RelativeItem(0.8f).BorderRight(0.5f);
                rr.RelativeItem(0.6f).BorderRight(0.5f);
                rr.RelativeItem(0.6f).BorderRight(0.5f);
                rr.RelativeItem(0.4f).BorderRight(0.5f);
                rr.RelativeItem(0.8f).BorderRight(0.5f);
                rr.RelativeItem(1.0f).BorderRight(0.5f);
                rr.RelativeItem(1.0f).BorderRight(0.5f);
                rr.RelativeItem(0.9f).BorderRight(0.5f).PaddingLeft(1).Text("ALÍQ").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.9f).BorderRight(0.5f).PaddingLeft(1).Text("ALÍQ IPI").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
                rr.RelativeItem(0.6f).PaddingLeft(1).Text("PEDIDO").FontSize(FontSizeSmall - 0.5f).FontFamily(FontFamily).Bold();
            });

            // Itens (2 linhas por item, alinhadas à mesma grid de 12 colunas)
            foreach (var det in _nfe.infNFe.det)
            {
                var icms = det.imposto.ICMS;
                var ipi = det.imposto.IPI;
                var icmsVbc = ObterValorIcmsBc(icms);
                var icmsVicms = ObterValorIcms(icms);
                var icmsPicms = ObterAliquotaIcms(icms);
                var ipiVipi = ObterValorIpi(ipi);
                var ipiPipi = ObterAliquotaIpi(ipi);
                var cst = ObterCst(icms);

                // Linha 1 do item: dados principais
                column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                {
                    rr.RelativeItem(0.7f).BorderRight(0.5f).PaddingLeft(1).Text(det.prod.cProd).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(4.0f).BorderRight(0.5f).PaddingLeft(1).Text(det.prod.xProd).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.8f).BorderRight(0.5f).PaddingLeft(1).Text(det.prod.NCM).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.6f).BorderRight(0.5f).PaddingLeft(1).Text(cst).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.6f).BorderRight(0.5f).PaddingLeft(1).Text(det.prod.CFOP.ToString()).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.4f).BorderRight(0.5f).PaddingLeft(1).Text(det.prod.uCom).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.8f).BorderRight(0.5f).AlignRight().PaddingRight(1).Text(det.prod.qCom.ToString("N3")).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(1.0f).BorderRight(0.5f).AlignRight().PaddingRight(1).Text(det.prod.vUnCom.ToString("N2")).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(1.0f).BorderRight(0.5f).AlignRight().PaddingRight(1).Text(det.prod.vProd.ToString("N2")).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.9f).BorderRight(0.5f).AlignRight().PaddingRight(1).Text(icmsVbc?.ToString("N2") ?? "").FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.9f).BorderRight(0.5f).AlignRight().PaddingRight(1).Text(icmsVicms?.ToString("N2") ?? "").FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.6f).AlignRight().PaddingRight(1).Text(ipiVipi?.ToString("N2") ?? "").FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                });

                // Linha 2 do item: alíquotas e pedido (alinhado à mesma grid)
                column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                {
                    rr.RelativeItem(0.7f).BorderRight(0.5f);
                    rr.RelativeItem(4.0f).BorderRight(0.5f);
                    rr.RelativeItem(0.8f).BorderRight(0.5f);
                    rr.RelativeItem(0.6f).BorderRight(0.5f);
                    rr.RelativeItem(0.6f).BorderRight(0.5f);
                    rr.RelativeItem(0.4f).BorderRight(0.5f);
                    rr.RelativeItem(0.8f).BorderRight(0.5f);
                    rr.RelativeItem(1.0f).BorderRight(0.5f);
                    rr.RelativeItem(1.0f).BorderRight(0.5f);
                    rr.RelativeItem(0.9f).BorderRight(0.5f).AlignRight().PaddingRight(1).Text(icmsPicms?.ToString("N2") ?? "").FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.9f).BorderRight(0.5f).AlignRight().PaddingRight(1).Text(ipiPipi?.ToString("N2") ?? "").FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                    rr.RelativeItem(0.6f).PaddingLeft(1).Text(det.prod.nItemPed).FontSize(FontSizeSmall - 1).FontFamily(FontFamily);
                });
            }

            // ISSQN
            if (_nfe.infNFe.total.ISSQNtot != null)
            {
                column.Item().PaddingTop(5);
                column.Item().Border(0.5f).PaddingLeft(2).Text("CÁLCULO DO ISSQN").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();
                var issqn = _nfe.infNFe.total.ISSQNtot;
                column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                {
                    rr.RelativeItem(3).BorderRight(0.5f).Column(c => c.Item().PaddingLeft(2).Text($"BASE CÁLCULO: {issqn.vBC:N2}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                    rr.RelativeItem(3).BorderRight(0.5f).Column(c => c.Item().PaddingLeft(2).Text($"VALOR ISSQN: {issqn.vISS:N2}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                    //rr.RelativeItem(3).BorderRight(0.5f).Column(c => c.Item().PaddingLeft(2).Text($"ALÍQUOTA: {issqn.vAliq:N2}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                    //rr.RelativeItem(3).Column(c => c.Item().PaddingLeft(2).Text($"CÓD. SERVIÇO: {issqn.cServico}").FontSize(FontSizeSmall).FontFamily(FontFamily));
                });
            }

            // DADOS ADICIONAIS
            if (_nfe.infNFe.infAdic != null)
            {
                column.Item().PaddingTop(5);
                column.Item().Border(0.5f).PaddingLeft(2).Text("DADOS ADICIONAIS").FontSize(FontSizeNormal).FontFamily(FontFamily).Bold();

                if (!string.IsNullOrEmpty(_nfe.infNFe.infAdic.infCpl))
                {
                    column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                        rr.RelativeItem().Column(c => c.Item().PaddingLeft(2).Text($"Inf. Complementares: {_nfe.infNFe.infAdic.infCpl}").FontSize(FontSizeSmall).FontFamily(FontFamily)));
                }

                if (_nfe.infNFe.infAdic.obsCont != null)
                    foreach (var obs in _nfe.infNFe.infAdic.obsCont)
                        column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                            rr.RelativeItem().Column(c => c.Item().PaddingLeft(2).Text($"Obs: {obs.xTexto}").FontSize(FontSizeSmall).FontFamily(FontFamily)));

                if (_nfe.infNFe.infAdic.obsFisco != null)
                    foreach (var obs in _nfe.infNFe.infAdic.obsFisco)
                        column.Item().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.5f).Row(rr =>
                            rr.RelativeItem().Column(c => c.Item().PaddingLeft(2).Text($"Obs Fisco: {obs.xTexto}").FontSize(FontSizeSmall).FontFamily(FontFamily)));
            }

            // RESUMO DO CANHOTO
            if (_configuracao.ExibirResumoCanhoto)
            {
                column.Item().PaddingTop(5);
                var resumo = string.IsNullOrEmpty(_configuracao.ResumoCanhoto)
                    ? $"Emissão: {_nfe.infNFe.ide.dhEmi:dd/MM/yyyy} Dest/Reme: {_nfe.infNFe.dest?.xNome ?? ""} Valor Total: {icmsTot.vNF:N2}"
                    : _configuracao.ResumoCanhoto;
                column.Item().Border(0.5f).Row(rr => rr.RelativeItem().Column(c => c.Item().PaddingLeft(2).Text($"RESUMO DO CANHOTO: {resumo}").FontSize(FontSizeSmall).FontFamily(FontFamily)));
            }

            // CONSULTA AUTENTICIDADE
            column.Item().PaddingTop(5);
            column.Item().Border(0.5f).Row(rr => rr.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text("Consulta de autenticidade no portal nacional da NF-e").FontSize(FontSizeSmall).FontFamily(FontFamily);
                c.Item().AlignCenter().Text("www.nfe.fazenda.gov.br/portal ou no site da Sefaz autorizadora").FontSize(FontSizeSmall).FontFamily(FontFamily);
            }));
        });
    }

    private void Rodape(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(3);
            column.Item().Border(0.5f).Row(row =>
            {
                row.ConstantItem(5.5f, Unit.Centimetre).Column(c =>
                {
                    var barcodeBytes = GerarCodigoBarras(_nfe.infNFe.Id.Substring(3));
                    if (barcodeBytes != null && barcodeBytes.Length > 0)
                        c.Item().MaxWidth(5f, Unit.Centimetre).Image(barcodeBytes);
                    c.Item().AlignCenter().Text(_nfe.infNFe.Id.Substring(3)).FontSize(FontSizeSmall).FontFamily(FontFamily);
                });
                row.RelativeItem().Column(c =>
                {
                    if (!string.IsNullOrEmpty(_desenvolvedor))
                        c.Item().PaddingLeft(2).PaddingTop(2).Text($"Desenvolvedor: {_desenvolvedor}").FontSize(FontSizeSmall).FontFamily(FontFamily);
                    c.Item().PaddingLeft(2).PaddingTop(2).Text($"Data e hora da impressão: {(_configuracao.DataHoraImpressao ?? DateTime.Now):dd/MM/yyyy HH:mm:ss}").FontSize(FontSizeSmall).FontFamily(FontFamily);
                });
            });
        });
    }

    private bool DeveExibirMensagemHomologacao() => _nfe.infNFe.ide.tpAmb == TipoAmbiente.Homologacao;

    private bool DeveExibirMensagemContingencia() => _nfe.infNFe.ide.tpEmis != TipoEmissao.teNormal && _nfeProc == null;

    private string GetMensagemAmbiente()
    {
        if (_nfe.infNFe.ide.tpAmb == TipoAmbiente.Homologacao)
            return DeveExibirMensagemContingencia() ? "DANFE - HOMOLOGAÇÃO EM CONTINGÊNCIA" : "DANFE - HOMOLOGAÇÃO";
        if (_configuracao.DocumentoCancelado) return "DANFE - NF-e CANCELADA";
        return "DANFE";
    }

    private bool DeveExibirDadosProtocolo() => _nfeProc?.protNFe?.infProt != null;

    private string ObterEnderecoDestinatario()
    {
        var ender = _nfe.infNFe.dest?.enderDest;
        if (ender == null) return "";
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(ender.xLgr)) sb.Append(ender.xLgr);
        if (!string.IsNullOrEmpty(ender.nro)) sb.Append($", {ender.nro}");
        if (!string.IsNullOrEmpty(ender.xCpl)) sb.Append($" - {ender.xCpl}");
        return sb.ToString();
    }

    private string ObterModalidadeFrete(ModalidadeFrete? modFrete) => modFrete switch
    {
        ModalidadeFrete.mfContaEmitenteOumfContaRemetente => "0 - Por conta do emitente",
        ModalidadeFrete.mfContaDestinatario => "1 - Por conta do destinatário",
        ModalidadeFrete.mfContaTerceiros => "2 - Por conta de terceiros",
        ModalidadeFrete.mfProprioContaRemente => "3 - Próprio por conta do remetente",
        ModalidadeFrete.mfProprioContaDestinatario => "4 - Próprio por conta do destinatário",
        ModalidadeFrete.mfSemFrete => "9 - Sem frete",
        _ => ""
    };

    private decimal? ObterValorIcmsBc(NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.ICMS icms)
    {
        var p = icms?.TipoICMS?.GetType().GetProperty("vBC");
        return p?.GetValue(icms.TipoICMS) as decimal?;
    }

    private decimal? ObterValorIcms(NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.ICMS icms)
    {
        var p = icms?.TipoICMS?.GetType().GetProperty("vICMS");
        return p?.GetValue(icms.TipoICMS) as decimal?;
    }

    private decimal? ObterAliquotaIcms(NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.ICMS icms)
    {
        var p = icms?.TipoICMS?.GetType().GetProperty("pICMS");
        return p?.GetValue(icms.TipoICMS) as decimal?;
    }

    private decimal? ObterValorIpi(NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.IPI ipi)
    {
        if (ipi?.TipoIPI is IPITrib ipiTrib)
            return ipiTrib.vIPI;
        return null;
    }

    private decimal? ObterAliquotaIpi(NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.IPI ipi)
    {
        if (ipi?.TipoIPI is IPITrib ipiTrib)
            return ipiTrib.pIPI;
        return null;
    }

    private string ObterCst(NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.ICMS icms)
    {
        if (icms?.TipoICMS == null) return "";
        var tipo = icms.TipoICMS;
        var cstProp = tipo.GetType().GetProperty("CST");
        if (cstProp != null)
        {
            var v = cstProp.GetValue(tipo);
            if (v != null) return ((int)v).ToString("D2");
        }
        var csosnProp = tipo.GetType().GetProperty("CSOSN");
        if (csosnProp != null)
        {
            var v = csosnProp.GetValue(tipo);
            if (v != null) return ((int)v).ToString("D3");
        }
        return "";
    }

    private byte[]? GerarCodigoBarras(string codigo)
    {
        try
        {
            var barcode = new Barcode();
            barcode.Encode(BarcodeStandard.Type.Code128, codigo, 400, 40);
            return barcode.GetImageData(SaveTypes.Png);
        }
        catch { return null; }
    }

    private string FormatarCnpj(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14) return cnpj ?? "";
        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }

    public byte[] GerarPdfBytes() => this.GeneratePdf();
}
