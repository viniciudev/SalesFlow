using CTe.CTeOSDocumento.Common;
using CTe.CTeOSDocumento.Soap;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
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
			// instancia do objeto responsável pela serialização
			XmlSerializer soapserializer = new XmlSerializer(typeof(T));

			// Armazena os dados em memória para manipulação
			MemoryStream memoryStream = new MemoryStream();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

			//Serializa o objeto de acordo com o formato
			soapserializer.Serialize(xmlTextWriter, soapEnvelope);
			xmlTextWriter.Formatting = Formatting.None;

			XmlDocument xmlDocument = new XmlDocument();

			//Remove o caractere especial BOM (byte order mark)
			string output = Encoding.UTF8.GetString(memoryStream.ToArray());
			string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
			if (output.StartsWith(_byteOrderMarkUtf8))
			{
				output = output.Remove(0, _byteOrderMarkUtf8.Length);
			}

			//Carrega os dados na instancia do XmlDocument
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
			if (!tipoEvento.HasValue && string.IsNullOrWhiteSpace(actionUrn))
				throw new ArgumentNullException("Informe tipoEvento ou actionUrn");

			if (tipoEvento.HasValue)
				actionUrn = new SoapUrls().GetSoapUrl(tipoEvento.Value);

			string xmlSoap = xmlEnvelop.InnerXml;

			Console.WriteLine("=== DEBUG CERT ===SendRequestAsync");
			Console.WriteLine($"Subject: {certificadoDigital.Subject}");
			Console.WriteLine($"Issuer: {certificadoDigital.Issuer}");
			Console.WriteLine($"HasPrivateKey: {certificadoDigital.HasPrivateKey}");
			Console.WriteLine($"Thumbprint: {certificadoDigital.Thumbprint}");

			var cert = new X509Certificate2(
					certificadoDigital.Export(X509ContentType.Pkcs12),
					(string)null,
					X509KeyStorageFlags.MachineKeySet |
					X509KeyStorageFlags.PersistKeySet |
					X509KeyStorageFlags.Exportable
			);

			using (HttpClientHandler handler = new HttpClientHandler())
			{
				handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
				handler.ClientCertificateOptions = ClientCertificateOption.Manual;
				handler.CheckCertificateRevocationList = false;

				handler.ClientCertificates.Clear();
				handler.ClientCertificates.Add(cert);

				using (HttpClient client = new HttpClient(handler))
				{
					client.Timeout = TimeSpan.FromSeconds(30);

					HttpRequestMessage request =
							new HttpRequestMessage(HttpMethod.Post, url);

					request.Content =
							new StringContent(xmlSoap, Encoding.UTF8, "application/soap+xml");

					request.Headers.Add("SOAPAction", actionUrn);

					HttpResponseMessage response =
							await client.SendAsync(request);

					response.EnsureSuccessStatusCode();

					return await response.Content.ReadAsStringAsync();
				}
			}
		}

		public string SendRequest(XmlDocument xmlEnvelop, X509Certificate2 certificadoDigital, string url, int timeOut,
				TipoEvento? tipoEvento = null, string actionUrn = "")
		{
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
			Console.WriteLine("=== DEBUG CERT ===SendRequest");
			Console.WriteLine($"Subject: {certificadoDigital.Subject}");
			Console.WriteLine($"Issuer: {certificadoDigital.Issuer}");
			Console.WriteLine($"HasPrivateKey: {certificadoDigital.HasPrivateKey}");
			Console.WriteLine($"Thumbprint: {certificadoDigital.Thumbprint}");
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", false);

			using (HttpClientHandler handler = new HttpClientHandler())
			{
				handler.SslProtocols = SslProtocols.Tls12;
				handler.ClientCertificateOptions = ClientCertificateOption.Manual;
				handler.CheckCertificateRevocationList = false;

				handler.ClientCertificates.Add(certificadoDigital);

				using (HttpClient client = new HttpClient(handler))
				{
					client.Timeout = TimeSpan.FromSeconds(30);

					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
					request.Version = HttpVersion.Version11;

					request.Content = new StringContent(xmlSoap, Encoding.UTF8, "application/soap+xml");
					request.Headers.Add("SOAPAction", actionUrn);

					HttpResponseMessage response;

					try
					{
						response = client.SendAsync(request).Result;
					}
					catch (Exception ex)
					{
						Console.WriteLine("=== ERRO AO ENVIAR REQUEST ===");
						Console.WriteLine($"Message: {ex.Message}");
						Console.WriteLine($"Type: {ex.GetType().FullName}");

						if (ex.InnerException != null)
						{
							Console.WriteLine("=== INNER EXCEPTION ===");
							Console.WriteLine($"Message: {ex.InnerException.Message}");
							Console.WriteLine($"Type: {ex.InnerException.GetType().FullName}");
						}

						Console.WriteLine("=== STACKTRACE ===");
						Console.WriteLine(ex.StackTrace);

						throw;
					}

					try
					{
						response.EnsureSuccessStatusCode();
					}
					catch (HttpRequestException ex)
					{
						Console.WriteLine("=== ERRO AO ENVIAR REQUEST ===");
						PrintException(ex);
						throw;
					}

					return response.Content.ReadAsStringAsync().Result;
				}
			}

		}
		private void PrintException(Exception ex, int level = 0)
		{
			if (ex == null) return;

			Console.WriteLine($"--- LEVEL {level} ---");
			Console.WriteLine($"Type: {ex.GetType().FullName}");
			Console.WriteLine($"Message: {ex.Message}");
			Console.WriteLine($"StackTrace: {ex.StackTrace}");

			if (ex.InnerException != null)
			{
				PrintException(ex.InnerException, level + 1);
			}
		}
	}
}