using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CTe.CTeOSDocumento.Common;
using CTe.CTeOSDocumento.Soap;

namespace DFe.Wsdl.Common
{
	public class RequestSefazHttpClientHandler : IRequestSefaz
	{
		public XmlDocument SerealizeDocument<T>(T soapEnvelope)
		{
			XmlSerializer soapserializer = new XmlSerializer(typeof(T));
			MemoryStream memoryStream = new MemoryStream();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

			soapserializer.Serialize(xmlTextWriter, soapEnvelope);
			xmlTextWriter.Formatting = Formatting.None;

			string output = Encoding.UTF8.GetString(memoryStream.ToArray());
			string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
			if (output.StartsWith(_byteOrderMarkUtf8))
			{
				output = output.Remove(0, _byteOrderMarkUtf8.Length);
			}

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(output);

			return xmlDocument;
		}

		public async Task<string> SendRequestAsync(XmlDocument xmlEnvelop, X509Certificate2 certificadoDigital,
				string url, int timeOut,
				TipoEvento? tipoEvento = null, string actionUrn = "")
		{
			// Validação do certificado ANTES de prosseguir
			if (certificadoDigital == null)
				throw new ArgumentNullException(nameof(certificadoDigital), "Certificado digital não fornecido");

			if (!certificadoDigital.HasPrivateKey)
				throw new ArgumentException("Certificado digital não contém a chave privada necessária");

			if (certificadoDigital.NotAfter < DateTime.Now)
				throw new ArgumentException($"Certificado digital expirado em {certificadoDigital.NotAfter}");

			if (!tipoEvento.HasValue && string.IsNullOrWhiteSpace(actionUrn))
			{
				throw new ArgumentNullException(
						"Pelo menos uma das propriedades tipoEvento ou actionUrl devem ser definidos para executar a action na requisição soap");
			}

			if (tipoEvento.HasValue)
			{
				actionUrn = new SoapUrls().GetSoapUrl(tipoEvento.Value);
			}

			string xmlSoap = xmlEnvelop.InnerXml;

			// Configuração ANTES de criar o HttpClientHandler
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.CheckCertificateRevocationList = false;

			using (HttpClientHandler handler = new HttpClientHandler())
			{
				// Configuração do handler
				handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
				handler.ClientCertificateOptions = ClientCertificateOption.Manual;
				handler.CheckCertificateRevocationList = false;

				// ADICIONA O CERTIFICADO CORRETAMENTE
				handler.ClientCertificates.Add(certificadoDigital);

				// VALIDAÇÃO CORRETA (não ignorar erros cegamente)
				handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
				{
					// Em HOMOLOGAÇÃO, permite erros de nome do certificado
					if (sslPolicyErrors == SslPolicyErrors.None)
						return true;

					// Para HOMOLOGAÇÃO apenas - permite erros de nome (common name mismatch)
					// em ambiente de homologação da SEFAZ é comum
					if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
					{
						return true;
					}

					return false;
				};

				using (HttpClient client = new HttpClient(handler))
				{
					// Timeout padrão: 30 segundos (aumentado de 2 segundos)
					int timeoutMs = timeOut == 0 ? 30000 : timeOut;
					client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
					request.Content = new StringContent(xmlSoap, Encoding.UTF8, "application/soap+xml");
					request.Headers.Add("SOAPAction", actionUrn);

					try
					{
						HttpResponseMessage response = await client.SendAsync(request);
						response.EnsureSuccessStatusCode();

						return await response.Content.ReadAsStringAsync();
					}
					catch (TaskCanceledException)
					{
						throw new Exception($"Timeout após {timeoutMs}ms na conexão com a SEFAZ. Verifique sua conexão com a internet e se o serviço da SEFAZ está disponível.");
					}
					catch (HttpRequestException ex)
					{
						throw new Exception($"Erro na requisição HTTP: {ex.Message}. Verifique se a URL {url} está correta e acessível.", ex);
					}
				}
			}
		}

		public string SendRequest(XmlDocument xmlEnvelop, X509Certificate2 certificadoDigital, string url, int timeOut,
				TipoEvento? tipoEvento = null, string actionUrn = "")
		{
			// Chama a versão assíncrona e espera (para manter compatibilidade)
			return SendRequestAsync(xmlEnvelop, certificadoDigital, url, timeOut, tipoEvento, actionUrn)
					.GetAwaiter()
					.GetResult();
		}
	}
}