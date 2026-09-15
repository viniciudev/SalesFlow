using System;

namespace Model.DTO
{
    /// <summary>
    /// Resultado de uma operação de NFS-e junto ao SEFIN.
    ///
    /// É um POCO nosso DE PROPÓSITO: nenhum tipo do OpenAC atravessa esta fronteira.
    /// Assim a biblioteca de emissão fica trocável e o resto do sistema (services,
    /// controllers, DTOs de resposta) não passa a depender dela.
    /// </summary>
    public class NfseEmissaoResult
    {
        /// <summary>true somente quando o SEFIN AUTORIZOU a NFS-e.</summary>
        public bool Sucesso { get; set; }

        /// <summary>
        /// Chave de acesso da NFS-e (50 dígitos). Preenchida na autorização.
        /// </summary>
        public string? ChaveAcesso { get; set; }

        /// <summary>Id da DPS efetivamente transmitida.</summary>
        public string? IdDps { get; set; }

        /// <summary>XML da NFS-e autorizada.</summary>
        public string? XmlNfse { get; set; }

        /// <summary>XML assinado que foi enviado (para diagnóstico e reprocessamento).</summary>
        public string? XmlEnvio { get; set; }

        /// <summary>
        /// Corpo JSON efetivamente enviado — o envelope <c>{ "xmlDps": "&lt;DPS.../&gt;" }</c>.
        /// É o que vai para <c>ServiceInvoice.RequestPayloadJson</c>, espelhando o padrão já
        /// usado em <c>NFeEmission</c>.
        /// </summary>
        public string? RequestPayloadJson { get; set; }

        /// <summary>Mensagens de rejeição do SEFIN. Vazio quando autorizada.</summary>
        public string? MensagemErro { get; set; }

        /// <summary>
        /// Retorno cru do SEFIN. Guardado porque quando a transmissão falha por infraestrutura
        /// (proxy devolveu HTML, 502, timeout) os campos estruturados vêm vazios e este é o
        /// único registro do que realmente voltou.
        /// </summary>
        public string? ResponseJson { get; set; }
    }
}
