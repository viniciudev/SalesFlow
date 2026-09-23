#nullable enable
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Model.DTO;
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.NFSe.Nacional;
using OpenAC.Net.NFSe.Nacional.DANFSe.PDFSharp;
using OpenAC.Net.NFSe.Nacional.DANFSe.PDFSharp.Configuracao;
using OpenAC.Net.NFSe.Nacional.Common;
using OpenAC.Net.NFSe.Nacional.Common.Model;
using OpenAC.Net.NFSe.Nacional.Webservice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OpenAC.Net.NFSe.Nacional.Common.Types;

namespace Service
{
    /// <summary>
    /// Fronteira única com o SEFIN (NFS-e padrão Nacional).
    ///
    /// Nenhum tipo do OpenAC atravessa esta interface: o retorno é o POCO
    /// <see cref="NfseEmissaoResult"/>. Assim a biblioteca de emissão fica trocável e os
    /// services/controllers não passam a depender dela.
    ///
    /// Depende APENAS de <see cref="IWebHostEnvironment"/> — nunca de
    /// <c>IServiceInvoiceService</c>, que por sua vez depende desta interface (ciclo de DI).
    /// </summary>
    public interface INfseService
    {
        /// <summary>
        /// Diz se a empresa tem certificado A1 configurado, ou seja, se a emissão pode
        /// ser TRANSMITIDA. Sem certificado a NFS-e é apenas registrada localmente — é
        /// essa a mesma regra que a UI usa para mostrar o aviso antes de emitir.
        /// </summary>
        bool TemCertificadoConfigurado(FiscalConfiguration? config);

        /// <summary>
        /// Id da DPS (45 caracteres) que SERÁ transmitido. Exposto separadamente para que
        /// o chamador possa gravar o id na fatura ANTES da transmissão: se a resposta se
        /// perder no caminho, o id gravado é o que permite consultar no SEFIN se a DPS
        /// chegou — sem ele, a única saída seria emitir de novo e duplicar a nota.
        /// </summary>
        string MontarIdDps(ServiceInvoice fatura, FiscalConfiguration config, string numeroDps);

        /// <summary>
        /// Monta, assina e transmite a DPS. NÃO lança: qualquer falha (montagem,
        /// certificado, rede, rejeição do SEFIN) volta em <see cref="NfseEmissaoResult"/>
        /// com <c>Sucesso = false</c> e o motivo preenchido, para que a fatura nunca perca
        /// estado por causa de uma transmissão que deu errado.
        /// </summary>
        Task<NfseEmissaoResult> EmitirAsync(ServiceInvoice fatura, FiscalConfiguration config);

        /// <summary>DANFSe (PDF) de uma NFS-e já autorizada, obtido pela chave de acesso.</summary>
        Task<byte[]> ObterDanfseAsync(ServiceInvoice fatura, FiscalConfiguration config);

        Task<bool?> CancelarNfse(ServiceInvoice fatura, FiscalConfiguration config, string cancelReason);
    }

    public class NfseService : INfseService
    {
        /// <summary>
        /// Teto do motivo de falha gravado em <c>ServiceInvoice.ErrorMessage</c>. A coluna
        /// é <c>text</c>, mas o motivo também vai no corpo da LISTAGEM paginada: sem teto,
        /// um HTML de proxy (502) viraria centenas de KB por linha.
        /// </summary>
        private const int MaxMotivoLength = 600;

        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Usado para registrar o status HTTP e o CORPO da resposta do ADN. Sem o corpo
        /// não dá para distinguir um 503 do próprio ADN de um 503 de gateway/WAF na
        /// frente dele — que é justamente a dúvida sobre o DANFSe.
        /// </summary>
        private readonly ILogger<NfseService> _logger;

        /// <summary>
        /// Cliente HTTP só para BAIXAR o certificado, quando ele é guardado como URL.
        /// A transmissão em si usa o cliente interno do OpenAC (que anexa o certificado
        /// no handler), então este não é reutilizado por ela.
        /// </summary>
        private static readonly HttpClient _downloadHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public NfseService(IWebHostEnvironment environment, ILogger<NfseService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public bool TemCertificadoConfigurado(FiscalConfiguration? config)
            => !string.IsNullOrWhiteSpace(config?.CertificadoDigital?.Arquivo);

        public string MontarIdDps(ServiceInvoice fatura, FiscalConfiguration config, string numeroDps)
            => DpsBuilder.MontarIdDps(fatura, config, numeroDps);

        public async Task<NfseEmissaoResult> EmitirAsync(ServiceInvoice fatura, FiscalConfiguration config)
        {
            // Id já reservado pelo chamador quando existir; mantém o valor anterior numa
            // retentativa em vez de zerar o campo.
            var resultado = new NfseEmissaoResult { IdDps = fatura.IdDPS };

            // Declarado FORA do try de propósito: se a falha acontecer depois da montagem
            // (certificado, rede), este XML é o único registro do que foi construído — e o
            // catch abaixo precisa dele.
            string? xmlConstruido = null;

            try
            {
                var dps = DpsBuilder.Construir(fatura, config, fatura.NumeroDPS.ToString());
                resultado.IdDps = dps.Informacoes.Id;

                // Xml do documento ainda SEM assinatura. EnviarAsync assina por dentro
                // (Dps.Assinar) e devolve o XML assinado em resposta.XmlEnvio, que passa a
                // ser o valor gravado.
                xmlConstruido = dps.GetXml();

                var certificado = await ResolverCertificadoAsync(config.CertificadoDigital!.Arquivo!);

                var open = new OpenNFSeNacional();
                fatura.TipoAmbiente = config.Ambiente;
                AplicarConfiguracao(open.Configuracoes, fatura, certificado, config.CertificadoDigital.Senha);

                var resposta = await open.EnviarAsync(dps);

                resultado.RequestPayloadJson = resposta.JsonEnvio;
                resultado.XmlEnvio = resposta.XmlEnvio ?? xmlConstruido;
                resultado.ResponseJson = resposta.JsonRetorno;
                resultado.IdDps = resposta.Resultado?.IdDps ?? resultado.IdDps;
                resultado.ChaveAcesso = resposta.Resultado?.ChaveAcesso;
                resultado.XmlNfse = resposta.Resultado?.XmlNFSe;

                // HTTP 2xx NÃO basta. A autorização é provada pela CHAVE DE ACESSO: um 200
                // cujo corpo não desserializou (proxy devolveu HTML) volta com
                // Resultado = null, e marcar isso como emitido criaria uma nota que não
                // existe no SEFIN. Exigir os dois é o que torna o sucesso confiável.
                var autorizada = resposta.Sucesso && !string.IsNullOrWhiteSpace(resultado.ChaveAcesso);

                resultado.Sucesso = autorizada;
                if (!autorizada)
                    resultado.MensagemErro = DescreverErros(resposta.Resultado, resposta.JsonRetorno);
            }
            catch (Exception ex)
            {
                // Falha de rede, certificado inválido/expirado, XmlSchemaException, DPS
                // malformada. Todas viram soft-fail com motivo — o chamador decide o que
                // fazer, mas a fatura nunca é perdida.
                resultado.Sucesso = false;
                resultado.ChaveAcesso = null;
                resultado.XmlNfse = null;
                resultado.XmlEnvio = xmlConstruido;
                resultado.MensagemErro = DescreverExcecao(ex);
            }

            return resultado;
        }
        public async Task<bool?> CancelarNfse(ServiceInvoice fatura, FiscalConfiguration config, string cancelReason)
        {
            // As mensagens abaixo sao do dominio do CANCELAMENTO. As identicas em
            // ObterDanfseAsync falam de download de DANFSe — nao copiar de la.
            if (string.IsNullOrWhiteSpace(fatura.ChaveAcesso))
                throw new InvalidOperationException(
                    "NFS-e sem chave de acesso: só é possível cancelar uma nota emitida.");

            if (!TemCertificadoConfigurado(config))
                throw new InvalidOperationException(
                    "Empresa sem certificado digital configurado: o cancelamento é transmitido ao SEFIN e exige autenticação.");

            var certificado = await ResolverCertificadoAsync(config.CertificadoDigital!.Arquivo!);

            var open = new OpenNFSeNacional();
          

            var evento = new PedidoRegistroEvento
            {
                
                // Mesma versao da DPS. NAO e detalhe: ValidarSchema faz
                // Configuracao.Arquivos.VersaoSchema = evento.Versao e GetSchema monta
                // pedRegEvento_v{versao}.xsd — a versao do DOCUMENTO e que escolhe o XSD,
                // entao o VersaoSchema da config e sobrescrito aqui. O schema 1.00 exige
                // Id PRE[0-9]{59} (chave + tipoEvento + nPedRegEvento) e o elemento
                // nPedRegEvento, que este modelo nao emite: o Id daqui tem 56 digitos.
                Versao = DpsBuilder.Versao,
                Informacoes = new InfPedReg
                {
                    
                    Id = "PRE" + fatura.ChaveAcesso + TipoEventoCod.Cancelamento,
                    // Da FATURA, igual ao DpsBuilder e ao cfg.WebServices.Ambiente. Fixo em
                    // Producao, um cancelamento de homologacao seria POSTado na URL de
                    // homologacao carregando <tpAmb>1</tpAmb> — ambiente incoerente.
                    TipoAmbiente = config.Ambiente == AmbienteEnum.Producao
                        ? DFeTipoAmbiente.Producao
                        : DFeTipoAmbiente.Homologacao,
                    DhEvento = DateTime.Now,
                    ChNFSe = fatura.ChaveAcesso,
                    // `Emitente?.` — a config pode existir com certificado e sem emitente.
                    // SomenteDigitos devolve null com CNPJ vazio, e a lib omite o elemento,
                    // que e obrigatorio (choice CNPJAutor|CPFAutor) => erro de schema.
                    CNPJAutor = DpsBuilder.SomenteDigitos(config.Emitente?.Cnpj), 
                    Evento = new EventoCancelamento
                    {
                        
                        CodMotivo = MotivoCancelamento.ErroEmissao,
                        // Descricao NAO se atribui: em 1.01 o xDesc e enumeracao de valor
                        // unico ("Cancelamento de NFS-e") e a lib ja traz esse default.
                        // Sobrescrever com a justificativa viola a enumeracao.
                        // A justificativa vai em xMotivo (TSMotivo, 15 a 255 caracteres).
                        Motivo = cancelReason
                        
                    },
                    
                }
            };
            try
            {
                AplicarConfiguracao(open.Configuracoes, fatura, certificado, config.CertificadoDigital.Senha);
                var retornoEvento= await open.EnviarEventoAsync(evento);

                // HTTP 2xx NAO basta, mesma razao do EmitirAsync: um 200 cujo corpo nao
                // desserializou (proxy devolveu HTML) volta com Resultado = null, e o
                // SEFIN tambem responde 200 com a rejeicao em `erros`. Marcar a fatura
                // como Cancelada sem conferir isso registraria um cancelamento que nao
                // existe no SEFIN.
                return retornoEvento.Sucesso
                       && retornoEvento.Resultado != null
                       && retornoEvento.Resultado.Erros.Count == 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
 
        }

        public async Task<byte[]> ObterDanfseAsync(ServiceInvoice fatura, FiscalConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(fatura.ChaveAcesso))
                throw new InvalidOperationException(
                    "NFS-e sem chave de acesso: não há DANFSe a baixar.");

            if (!TemCertificadoConfigurado(config))
                throw new InvalidOperationException(
                    "Empresa sem certificado digital configurado: o download do DANFSe exige autenticação.");

            // Layout da chave (TSChaveNFSe no XSD oficial):
            //   6 dígitos (IBGE) + 14 alfanuméricos (inscrição federal) + 30 dígitos = 50.
            // Os 14 alfanuméricos são o CNPJ alfanumérico do layout 1.01 — NÃO exigir
            // só dígitos: o schema 1.00 usava [0-9]{50}, o 1.01 aceita [0-9A-Z].
            if (!ChaveAcessoValida(fatura.ChaveAcesso))
                throw new InvalidOperationException(
                    $"Chave de acesso inválida: esperado o padrão TSChaveNFSe "
                    + $"[0-9]{{6}}[0-9A-Z]{{14}}[0-9]{{30}}, mas veio \"{fatura.ChaveAcesso}\" "
                    + $"({fatura.ChaveAcesso.Length} caracteres).");

            var certificado = await ResolverCertificadoAsync(config.CertificadoDigital!.Arquivo!);

            var open = new OpenNFSeNacional();
            fatura.TipoAmbiente=config.Ambiente;
            AplicarConfiguracao(open.Configuracoes, fatura, certificado, config.CertificadoDigital.Senha);
            return await GerarDanfseFallbackAsync(fatura, config);
            // Passe 1 — ADN. O download é GET {base}/danfse/{chave} contra o
            // adn.nfse.gov.br (ver NacionalWebservice.DownloadDANFSeAsync).
            // 503 NÃO é retentado aqui de propósito: ele cai no fallback local, que é
            // determinístico e não depende do ADN voltar.
            // for (var tentativa = 0; ; tentativa++)
            // {
            //     try
            //     {
            //         return await open.DownloadDANFSeAsync(fatura.ChaveAcesso);
            //     }
            //     catch (HttpRequestException ex) when (
            //         TentativaRecuperavel(ex.StatusCode)
            //         && ex.StatusCode != HttpStatusCode.ServiceUnavailable
            //         && tentativa < EsperasDanfse.Length)
            //     {
            //         await Task.Delay(EsperasDanfse[tentativa]);
            //     }
            //     catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            //     {
            //         _logger.LogWarning(
            //             ex,
            //             "ADN respondeu 503 no DANFSe da NFS-e {ChaveAcesso} (ambiente {Ambiente}). "
            //             + "Gerando o PDF localmente a partir do XML autorizado.",
            //             fatura.ChaveAcesso, config.Ambiente);
            //
            //         // O corpo do 503 não chega até aqui: EnsureSuccessStatusCode descarta
            //         // o conteúdo ao lançar. Sem ele não dá para saber se o 503 é do próprio
            //         // ADN ou de um gateway/WAF na frente dele — por isso a sonda.
            //         await RegistrarDiagnosticoAdnAsync(open, fatura, certificado, config.CertificadoDigital.Senha);
            //
            //         return await GerarDanfseFallbackAsync(fatura, config);
            //     }
            // }
        }

        /// <summary>
        /// Gera o DANFSe (PDF) LOCALMENTE, sem rede, a partir do XML autorizado já
        /// persistido em <see cref="ServiceInvoice.XmlNfse"/>.
        ///
        /// É o fallback do 503 do ADN. Usa o gerador do pacote
        /// <c>OpenAC.Net.NFSe.Nacional.DANFSe.PDFSharp</c>, que é offline (fontes e
        /// catálogo IBGE embutidos) — não é layout nosso.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Quando não há XML autorizado na fatura. Sem XML não existe DANFSe a gerar, e a
        /// mensagem diz exatamente isso em vez de devolver um PDF vazio ou um erro opaco.
        /// </exception>
        public Task<byte[]> GerarDanfseFallbackAsync(ServiceInvoice fatura, FiscalConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(fatura.XmlNfse))
                throw new InvalidOperationException(
                    $"Não é possível gerar o DANFSe localmente para a NFS-e {fatura.ChaveAcesso}: "
                    + "esta fatura não tem XML autorizado persistido (coluna XmlNfse) e o download "
                    + "no ADN falhou. Sem o XML não há como montar o PDF — transmita ou consulte a "
                    + "nota para obter o XML e tente novamente.");

            var nota = DesserializarNfse(fatura.XmlNfse)
                ?? throw new InvalidOperationException(
                    $"O XML autorizado da NFS-e {fatura.ChaveAcesso} não pôde ser desserializado "
                    + "como NotaFiscalServico; o DANFSe não pode ser gerado localmente.");

            var cfg = new DANFSeNacionalConfig
            {
                // Homologacao marca o PDF como teste. Vem da FATURA, mesma fonte do
                // <tpAmb> da DPS — nunca da config, pelo mesmo motivo de AplicarConfiguracao.
                Homologacao = fatura.TipoAmbiente != AmbienteEnum.Producao,
                ExibirQRCode = true,
                ExibirCanhoto = true,
                Cancelada = fatura.Status == ServiceInvoiceStatus.Cancelado
            };

            return Task.FromResult(OpenDANFSeNacional.GerarPDF(nota, cfg));
        }

        /// <summary>
        /// Repete o GET do DANFSe só para CAPTURAR status e corpo da resposta — que a
        /// biblioteca descarta ao lançar (<c>EnsureSuccessStatusCode</c> não guarda o
        /// conteúdo). É o que permite distinguir um 503 do próprio ADN de um 503 de
        /// gateway/WAF na frente dele.
        ///
        /// A URL vem da tabela da própria biblioteca (<see cref="NFSeServiceManager"/>),
        /// não é montada aqui. Best-effort: qualquer falha da sonda é logada e engolida —
        /// ela existe para diagnosticar, nunca para derrubar a geração do PDF.
        /// </summary>
        private async Task RegistrarDiagnosticoAdnAsync(
            OpenNFSeNacional open, ServiceInvoice fatura, byte[] certificado, string? senha)
        {
            try
            {
                var webservices = open.Configuracoes.WebServices;
                var url = NFSeServiceManager.Instance.Services[webservices.CodigoMunicipio]
                              [webservices.Ambiente][TipoUrl.DownloadDanfse]
                          + "/danfse/" + fatura.ChaveAcesso;

                using var cert = X509CertificateLoader.LoadPkcs12(certificado, senha ?? string.Empty);
                using var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(cert);

                using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
                using var resposta = await http.GetAsync(url);

                var corpo = await resposta.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "Sonda DANFSe: GET {Url} -> HTTP {Status} ({Motivo}), content-type {ContentType}. Corpo: {Corpo}",
                    url, (int)resposta.StatusCode, resposta.ReasonPhrase,
                    resposta.Content.Headers.ContentType?.ToString() ?? "(sem content-type)",
                    Truncar(corpo));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex, "Sonda DANFSe não pôde ser concluída para a NFS-e {ChaveAcesso}.",
                    fatura.ChaveAcesso);
            }
        }

        /// <summary>
        /// Desserializa o XML da NFS-e autorizada (coluna <c>ServiceInvoice.XmlNfse</c>)
        /// no modelo <see cref="NotaFiscalServico"/> do OpenAC.
        ///
        /// É exatamente o que a própria biblioteca faz em <c>RespostaEnvioDps.NFSe</c>
        /// (<c>DFeDocument&lt;NotaFiscalServico&gt;.Load(XmlNFSe, null)</c>) — mesma
        /// chamada, mesmo tipo. Daí sai a entrada de
        /// <c>OpenDANFSeNacional.GerarPDF</c>.
        /// </summary>
        /// <returns>A nota desserializada, ou null se não houver XML.</returns>
        public static NotaFiscalServico? DesserializarNfse(string? xml)
            => string.IsNullOrWhiteSpace(xml)
                ? null
                : DFeDocument<NotaFiscalServico>.Load(xml);

        /// <summary>
        /// Esperas entre as tentativas do download do DANFSe (backoff fixo: 1s, 3s, 7s).
        /// </summary>
        private static readonly TimeSpan[] EsperasDanfse =
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(7)
        };

        /// <summary>
        /// Valida a chave contra <c>TSChaveNFSe</c> do XSD oficial:
        /// <c>[0-9]{6}([0-9A-Z]{14})[0-9]{30}</c> — 6 dígitos (IBGE), 14 alfanuméricos
        /// (inscrição federal) e 30 dígitos, totalizando 50.
        /// </summary>
        private static bool ChaveAcessoValida(string chave)
            => chave.Length == 50
               && chave.Take(6).All(char.IsAsciiDigit)
               && chave.Skip(6).Take(14).All(char.IsAsciiLetterOrDigit)
               && chave.Skip(20).All(char.IsAsciiDigit);

        /// <summary>
        /// Diz se a falha de HTTP vale uma nova tentativa. Cobre as respostas de
        /// indisponibilidade do ADN e a falha de rede que nem chegou a ter status.
        /// 404/403 ficam de fora de propósito: chave inexistente ou certificado
        /// recusado não melhoram repetindo.
        /// </summary>
        private static bool TentativaRecuperavel(HttpStatusCode? status)
            => status is null
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.BadGateway
                or HttpStatusCode.GatewayTimeout
                or HttpStatusCode.TooManyRequests
                or HttpStatusCode.RequestTimeout;

        /// <summary>
        /// Monta a configuração do OpenAC. Os valores NÃO são os defaults da biblioteca —
        /// cada linha abaixo existe por um motivo verificado no fonte da 1.4.7.
        /// </summary>
        private void AplicarConfiguracao(
            ConfiguracaoNFSe cfg, ServiceInvoice fatura, byte[] certificado, string? senha)
        {
            cfg.Geral.Versao = DpsBuilder.Versao;

            // Salvar = true (default) grava XML/JSON em disco a cada tentativa. Com
            // PathSalvar vazio isso estoura no File.WriteAllText DEPOIS da DPS já estar
            // assinada — ou seja, o erro aparece no lugar errado. Nada disso é necessário:
            // o XML que importa é persistido por nós, nas colunas da fatura.
            cfg.Geral.Salvar = false;
            cfg.Arquivos.Salvar = false;

            // Amarrar as duas versões é obrigatório: ValidarSchema faz
            // Arquivos.VersaoSchema = dps.Versao e GetSchema monta o caminho como
            // PathSchemas/DPS_v{versao}.xsd. Deixando o default divergir, um DPS 1.01
            // seria validado contra o XSD 1.00 — passando ou falhando por engano.
            cfg.Arquivos.VersaoSchema = DpsBuilder.Versao;
            cfg.Arquivos.PathSchemas = CaminhoDosSchemas();

            // Provedor NÃO é atribuível: tem setter privado e já nasce em
            // NFSeProvider.Nacional — que é o provider do padrão Nacional, o único que
            // queremos. Quem o troca é o setter de CodigoMunicipio/Municipio, que resolve
            // um provider MUNICIPAL pelo cadastro do OpenAC. Por isso esses dois campos
            // ficam intocados: preenchê-los desviaria a emissão do ambiente nacional.
            cfg.WebServices.ValidarSchemas = true;

            // O default do pacote é Ssl3+Tls+Tls11+Tls12 (4080). SSL3/TLS1.0/1.1 estão
            // desativados no OpenSSL 3 — no Linux o HttpClientHandler estoura
            // "The requested security protocol is not supported" ANTES de olhar o
            // certificado, ou seja, toda transmissão falharia por um motivo que não tem
            // nada a ver com a nota.
            //
            // NAO fixar apenas Tls12 e NAO deixar o default do pacote.
            //
            // Ate a 1.4.7 este valor era INERTE no .NET 9: o SendAsync criava o
            // HttpClientHandler sem tocar em SslProtocols, e o ServicePointManager.
            // SecurityProtocol que a lib setava e ignorado pelo SslStream/HttpClient
            // (SYSLIB0014). A partir da 1.5.0.3 a lib passou a aplicar de verdade
            // (DesktopNFSeHttpClientPool.CriarEntrada faz httpClientHandler.SslProtocols
            // = protocolos), entao o valor voltou a ter efeito — e o default do pacote
            // (Ssl3|Tls|Tls11|Tls12) inclui protocolos que o OpenSSL 3 recusa.
            //
            // Tls13 entra junto de Tls12 por ser o que os dois hosts preferem hoje
            // (negociacao medida: adn.nfse.gov.br e sefin.nfse.gov.br fecham em TLS 1.3
            // quando o cliente oferece). O ADN ACEITA TLS 1.2 tambem — a falha de
            // handshake que se ve ao forcar 1.2 com curl/openssl e o alert 40 emitido
            // DEPOIS do CertificateRequest, ou seja, exigencia de certificado de cliente
            // sem certificado apresentado, nao recusa de versao. Logo este pin e
            // defensivo, nao um contorno de incompatibilidade do ADN.
            cfg.WebServices.Protocolos = System.Net.SecurityProtocolType.Tls12
                                         | System.Net.SecurityProtocolType.Tls13;

            // O ambiente vem da FATURA, nunca da FiscalConfiguration: o usuário escolhe
            // por emissão no diálogo, e é o mesmo campo que o DpsBuilder usa no
            // <tpAmb> da DPS. Se as duas fontes divergissem, uma DPS de homologação
            // seria POSTada no ambiente de produção.
            cfg.WebServices.Ambiente = fatura.TipoAmbiente == AmbienteEnum.Producao
                ? DFeTipoAmbiente.Producao
                : DFeTipoAmbiente.Homologacao;

            cfg.Certificados.CertificadoBytes = certificado;
            cfg.Certificados.Senha = senha ?? string.Empty;
        }

        /// <summary>
        /// Pasta dos XSDs vendorizados. Copiada para o diretório de saída pelo
        /// WebApiCommercial.csproj — por isso BaseDirectory, e não WebRootPath (mesmo
        /// critério que o NFeService usa para NFSchemas).
        /// </summary>
        private static string CaminhoDosSchemas()
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DPSchemas");

        /// <summary>
        /// O certificado A1 pode estar em três formas, porque o campo
        /// <c>CertificadoDigital.Arquivo</c> é texto livre e o cadastro nunca foi
        /// validado: URL, base64 ou caminho de arquivo.
        /// </summary>
        private async Task<byte[]> ResolverCertificadoAsync(string arquivo)
        {
            if (arquivo.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                arquivo.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return await _downloadHttp.GetByteArrayAsync(arquivo);

            // Um .pfx em base64 começa com "MII" (SEQUENCE DER do PKCS#12) e tem milhares
            // de caracteres. O teste de tamanho evita confundir com um nome de arquivo.
            if (arquivo.Length > 100 && arquivo.StartsWith("MII", StringComparison.Ordinal))
                return Convert.FromBase64String(arquivo);

            foreach (var caminho in CaminhosPossiveis(arquivo))
            {
                // System.IO qualificado: `using Model.Registrations` traz um
                // Model.Registrations.File, e `File` fica ambíguo.
                if (System.IO.File.Exists(caminho))
                    return await System.IO.File.ReadAllBytesAsync(caminho);
            }

            throw new FileNotFoundException(
                $"Arquivo do certificado digital não encontrado: \"{arquivo}\". "
                + "Esperado um caminho absoluto, um caminho relativo à pasta certs do site, "
                + "uma URL ou o .pfx em base64.");
        }

        private IEnumerable<string> CaminhosPossiveis(string arquivo)
        {
            yield return arquivo;

            var nome = Path.GetFileName(arquivo);
            if (string.IsNullOrEmpty(nome)) yield break;

            // `?.` de propósito: sem ambiente web o fallback de wwwroot simplesmente não
            // existe, e é melhor cair no "certificado não encontrado" do que vazar um
            // NullReferenceException para dentro da mensagem de erro da fatura.
            if (!string.IsNullOrEmpty(_environment?.WebRootPath))
                yield return Path.Combine(_environment.WebRootPath, "certs", nome);

            // Mesmo fallback do NFeService para o deploy em container, onde o conteúdo
            // do site é montado em /app/wwwroot.
            yield return Path.Combine("/app/wwwroot/certs", nome);
        }

        private static string DescreverErros(RespostaEnvioDps? dados, string? jsonRetorno)
        {
            if (dados?.Erros is { Count: > 0 })
                return Truncar(string.Join(" | ", dados.Erros.Select(DescreverMensagem)));

            // Sem erro estruturado, o corpo cru é a ÚNICA evidência do que voltou
            // (proxy com HTML, 502, timeout de gateway). É justamente por isso que a
            // coluna ResponseJson existe.
            if (!string.IsNullOrWhiteSpace(jsonRetorno))
                return Truncar("SEFIN não retornou erros estruturados. Retorno bruto: " + jsonRetorno);

            return "SEFIN recusou a DPS sem detalhar o motivo.";
        }

        private static string DescreverMensagem(MensagemProcessamento mensagem)
        {
            var partes = new[] { mensagem.Codigo, mensagem.Descricao, mensagem.Mensagem, mensagem.Complemento }
                .Where(p => !string.IsNullOrWhiteSpace(p));

            var texto = string.Join(" - ", partes);
            return string.IsNullOrWhiteSpace(texto) ? "Rejeição sem descrição." : texto;
        }

        private static string DescreverExcecao(Exception ex)
        {
            var raiz = ex;
            while (raiz.InnerException != null) raiz = raiz.InnerException;

            // A causa mais provável em produção é o certificado: senha errada, arquivo
            // trocado ou .pfx corrompido. O erro cru do .NET ("ASN1 corrupted data",
            // "tagged with 'Application' class value '9'") não diz nada a quem opera, então
            // o texto em português vem primeiro e o original fica entre parênteses.
            // AsnContentException fica de fora de CryptographicException e é justamente o
            // que o .NET lança quando o arquivo não é um PKCS#12 — por isso as duas.
            if (raiz is System.Security.Cryptography.CryptographicException
                or System.Formats.Asn1.AsnContentException)
                return Truncar("Certificado digital inválido ou senha incorreta. "
                               + "Confira o arquivo .pfx e a senha na configuração fiscal. Detalhe: " + raiz.Message);

            var prefixo = raiz is HttpRequestException or TaskCanceledException
                ? "Falha de comunicação com o SEFIN: "
                : "Falha ao transmitir a DPS: ";

            return Truncar(prefixo + raiz.Message);
        }

        private static string Truncar(string texto)
        {
            if (string.IsNullOrEmpty(texto) || texto.Length <= MaxMotivoLength) return texto;
            return texto.Substring(0, MaxMotivoLength) + "...(truncado)";
        }
    }
}