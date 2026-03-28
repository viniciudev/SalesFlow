using CTe.CTeOSDocumento.Common;
using CTe.CTeOSDocumento.Soap;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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

		public async Task<string> SendRequestAsync(
				XmlDocument xmlEnvelop,
				X509Certificate2 certificadoDigital,
				string url,
				int timeOut,
				TipoEvento? tipoEvento = null,
				string actionUrn = "")
		{
			return await Task.Run(() => SendRequest(xmlEnvelop, certificadoDigital, url, timeOut, tipoEvento, actionUrn));
		}

		public string SendRequest(
				XmlDocument xmlEnvelop,
				X509Certificate2 certificadoDigital,
				string url,
				int timeOut,
				TipoEvento? tipoEvento = null,
				string actionUrn = "")
		{
			if (!tipoEvento.HasValue && string.IsNullOrWhiteSpace(actionUrn))
			{
				throw new ArgumentNullException("Informe tipoEvento ou actionUrn");
			}

			if (tipoEvento.HasValue)
			{
				actionUrn = new SoapUrls().GetSoapUrl(tipoEvento.Value);
			}

			// Remove ?wsdl da URL se existir
			string cleanUrl = url;
			if (cleanUrl.Contains("?wsdl"))
			{
				cleanUrl = cleanUrl.Replace("?wsdl", "");
			}

			string xmlSoap = xmlEnvelop.InnerXml;

			Console.WriteLine("=== ENVIANDO PARA SEFAZ ===");
			Console.WriteLine($"URL: {cleanUrl}");
			Console.WriteLine($"SOAPAction: {actionUrn}");
			Console.WriteLine($"Certificado Subject: {certificadoDigital.Subject}");
			Console.WriteLine($"Certificado Issuer: {certificadoDigital.Issuer}");
			Console.WriteLine($"HasPrivateKey: {certificadoDigital.HasPrivateKey}");

			// 🔧 IMPORTANTE: Recria o certificado com as flags corretas para Linux
			var cert = new X509Certificate2(
					certificadoDigital.Export(X509ContentType.Pkcs12),
					(string)null,
					X509KeyStorageFlags.MachineKeySet |
					X509KeyStorageFlags.PersistKeySet |
					X509KeyStorageFlags.Exportable
			);

			// 🔧 Configuração do HttpClientHandler
			var handler = new HttpClientHandler();
			handler.SslProtocols = SslProtocols.Tls12;
			handler.ClientCertificateOptions = ClientCertificateOption.Manual;
			handler.CheckCertificateRevocationList = false;

			// Limpa e adiciona o certificado
			handler.ClientCertificates.Clear();
			handler.ClientCertificates.Add(cert);

			// 🔧 Callback de validação SSL (aceita tudo em homologação)
			handler.ServerCertificateCustomValidationCallback = (sender, serverCert, chain, sslPolicyErrors) =>
			{
				Console.WriteLine($"Server Certificate: {serverCert?.Subject}");
				Console.WriteLine($"SSL Policy Errors: {sslPolicyErrors}");
				// Aceita todos os erros em homologação
				return true;
			};

			using (var client = new HttpClient(handler))
			{
				client.Timeout = TimeSpan.FromSeconds(timeOut == 0 ? 60 : timeOut);

				var request = new HttpRequestMessage(HttpMethod.Post, cleanUrl);
				request.Content = new StringContent(xmlSoap, Encoding.UTF8, "application/soap+xml");
				request.Headers.Add("SOAPAction", actionUrn);

				// 🔧 Headers adicionais que podem ajudar
				request.Headers.Add("Accept", "*/*");
				request.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)");

				try
				{
					Console.WriteLine("Enviando requisição...");
					var response = client.SendAsync(request).Result;

					Console.WriteLine($"Status Code: {response.StatusCode}");

					string result = response.Content.ReadAsStringAsync().Result;

					if (!response.IsSuccessStatusCode)
					{
						throw new Exception($"HTTP {response.StatusCode}: {result}");
					}

					Console.WriteLine($"✅ Sucesso! Resposta: {result.Length} bytes");
					return result;
				}
				catch (Exception ex)
				{
					// Captura detalhes completos do erro SSL
					Console.WriteLine("=== ERRO SSL DETALHADO ===");
					Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
					Console.WriteLine($"Message: {ex.Message}");

					if (ex.InnerException != null)
					{
						Console.WriteLine($"Inner Type: {ex.InnerException.GetType().FullName}");
						Console.WriteLine($"Inner Message: {ex.InnerException.Message}");

						// Tenta obter a mensagem de erro do OpenSSL
						if (ex.InnerException.Message.Contains("SSL") || ex.InnerException.Message.Contains("CERT"))
						{
							Console.WriteLine($"OpenSSL Error: {ex.InnerException.Message}");
						}
					}

					// Log da pilha completa
					Console.WriteLine($"StackTrace: {ex.StackTrace}");
					throw;
				}
			}
		}
	}
}