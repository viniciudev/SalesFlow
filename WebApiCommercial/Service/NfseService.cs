#nullable enable
using Microsoft.AspNetCore.Hosting;
using Model.DTO;
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.NFSe.Nacional;
using OpenAC.Net.NFSe.Nacional.Common;
using OpenAC.Net.NFSe.Nacional.Common.Model;
using OpenAC.Net.NFSe.Nacional.Webservice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        /// Cliente HTTP só para BAIXAR o certificado, quando ele é guardado como URL.
        /// A transmissão em si usa o cliente interno do OpenAC (que anexa o certificado
        /// no handler), então este não é reutilizado por ela.
        /// </summary>
        private static readonly HttpClient _downloadHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public NfseService(IWebHostEnvironment environment)
        {
            _environment = environment;
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
            if (string.IsNullOrWhiteSpace(fatura.ChaveAcesso))
                throw new InvalidOperationException(
                    "NFS-e sem chave de acesso: não há DANFSe a baixar.");

            if (!TemCertificadoConfigurado(config))
                throw new InvalidOperationException(
                    "Empresa sem certificado digital configurado: o download do DANFSe exige autenticação.");

            var certificado = await ResolverCertificadoAsync(config.CertificadoDigital!.Arquivo!);

            var open = new OpenNFSeNacional();
           
/////////jogar para build
            var evento = new PedidoRegistroEvento
            {
                
                Versao = VersaoNFSe.Ve100,
                Informacoes = new InfPedReg
                {
                    
                    Id = "PRE" + fatura.ChaveAcesso + TipoEventoCod.Cancelamento,
                    TipoAmbiente = DFeTipoAmbiente.Producao,
                    DhEvento = DateTime.Now,
                    ChNFSe = fatura.ChaveAcesso,
                    CNPJAutor =SomenteDigitos( config.Emitente.Cnpj) , 
                    Evento = new EventoCancelamento
                    {
                        
                        CodMotivo = MotivoCancelamento.ErroEmissao,
                        Descricao = cancelReason
                        
                    }
                }
            };
            try
            {
                // evento.Assinar(open.Configuracoes);
                AplicarConfiguracao(open.Configuracoes, fatura, certificado, config.CertificadoDigital.Senha);
                var retornoEvento= await open.EnviarEventoAsync(evento);
                return retornoEvento.Sucesso;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
 
        }
        
        public static string? SomenteDigitos(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            var digitos = new string(texto.Where(char.IsDigit).ToArray());
            if (digitos.Length == 0)
                return null;

            return digitos;
        }
        public async Task<byte[]> ObterDanfseAsync(ServiceInvoice fatura, FiscalConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(fatura.ChaveAcesso))
                throw new InvalidOperationException(
                    "NFS-e sem chave de acesso: não há DANFSe a baixar.");

            if (!TemCertificadoConfigurado(config))
                throw new InvalidOperationException(
                    "Empresa sem certificado digital configurado: o download do DANFSe exige autenticação.");

            var certificado = await ResolverCertificadoAsync(config.CertificadoDigital!.Arquivo!);

            var open = new OpenNFSeNacional();
            AplicarConfiguracao(open.Configuracoes, fatura, certificado, config.CertificadoDigital.Senha);

            return await open.DownloadDANFSeAsync(fatura.ChaveAcesso);
        }

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
            // nada a ver com a nota. Fixar TLS 1.2 (o mínimo que o SEFIN aceita) resolve.
            // TLS 1.3 fica de fora até a homologação provar que o endpoint negocia.
            cfg.WebServices.Protocolos = System.Net.SecurityProtocolType.Tls12;

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