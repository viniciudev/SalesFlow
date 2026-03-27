using CTe.CTeOSDocumento.Common;
using CTe.CTeOSDocumento.Soap;
using System;
using System.IO;
using System.Net;
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

			XmlDocument xmlDocument = new XmlDocument();

			string output = Encoding.UTF8.GetString(memoryStream.ToArray());
			string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
			if (output.StartsWith(_byteOrderMarkUtf8))
			{
				output = output.Remove(0, _byteOrderMarkUtf8.Length);
			}

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
			// Validações
			if (certificadoDigital == null)
				throw new ArgumentNullException(nameof(certificadoDigital));

			if (!certificadoDigital.HasPrivateKey)
				throw new ArgumentException("Certificado digital não contém chave privada");

			if (certificadoDigital.NotAfter < DateTime.Now)
				throw new ArgumentException($"Certificado expirado em {certificadoDigital.NotAfter}");

			if (!tipoEvento.HasValue && string.IsNullOrWhiteSpace(actionUrn))
				throw new ArgumentNullException("Informe tipoEvento ou actionUrn");

			if (tipoEvento.HasValue)
				actionUrn = new SoapUrls().GetSoapUrl(tipoEvento.Value);

			string xmlSoap = xmlEnvelop.InnerXml;

			// Log para debug
			Console.WriteLine("=== DEBUG CERT ===");
			Console.WriteLine($"Subject: {certificadoDigital.Subject}");
			Console.WriteLine($"Issuer: {certificadoDigital.Issuer}");
			Console.WriteLine($"HasPrivateKey: {certificadoDigital.HasPrivateKey}");
			Console.WriteLine($"Thumbprint: {certificadoDigital.Thumbprint}");
			Console.WriteLine($"Url: {url}");
			Console.WriteLine($"SOAPAction: {actionUrn}");

			// Configurações globais
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.CheckCertificateRevocationList = false;
			ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

			// Cria a requisição com HttpWebRequest (funciona no Linux)
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ContentType = "application/soap+xml; charset=utf-8";
			request.Headers.Add("SOAPAction", actionUrn);
			request.Timeout = timeOut == 0 ? 60000 : timeOut;
			request.ReadWriteTimeout = timeOut == 0 ? 60000 : timeOut;
			request.KeepAlive = false;
			request.ProtocolVersion = HttpVersion.Version11;
			request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";

			// 🔑 ADICIONA O CERTIFICADO (aqui está a diferença)
			request.ClientCertificates.Add(certificadoDigital);

			try
			{
				// Envia o XML
				using (Stream stream = request.GetRequestStream())
				using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
				{
					writer.Write(xmlSoap);
				}

				// Recebe a resposta
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				using (Stream stream = response.GetResponseStream())
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
				{
					string result = reader.ReadToEnd();
					Console.WriteLine($"Resposta recebida com sucesso. Tamanho: {result.Length} bytes");
					return result;
				}
			}
			catch (WebException ex)
			{
				string responseBody = "";
				if (ex.Response != null)
				{
					using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
					{
						responseBody = reader.ReadToEnd();
					}
				}

				Console.WriteLine("=== ERRO NA REQUISIÇÃO ===");
				Console.WriteLine($"Status: {(ex.Response as HttpWebResponse)?.StatusCode}");
				Console.WriteLine($"Message: {ex.Message}");
				Console.WriteLine($"Response: {responseBody}");

				throw new Exception($"Erro na requisição: {ex.Message}. Resposta: {responseBody}", ex);
			}
			catch (Exception ex)
			{
				Console.WriteLine("=== ERRO INESPERADO ===");
				Console.WriteLine($"Type: {ex.GetType().FullName}");
				Console.WriteLine($"Message: {ex.Message}");
				Console.WriteLine($"StackTrace: {ex.StackTrace}");
				throw;
			}
		}
	}
}