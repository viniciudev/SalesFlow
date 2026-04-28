//using CTe.CTeOSDocumento.Common;
//using CTe.CTeOSDocumento.Soap;
//using System;
//using System.IO;
//using System.Net;
//using System.Net.Http;
//using System.Net.Security;
//using System.Security.Authentication;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Serialization;

//namespace DFe.Wsdl.Common
//{
//	public class RequestSefazHttpClientHandler : IRequestSefaz
//	{
//		public XmlDocument SerealizeDocument<T>(T soapEnvelope)
//		{
//			XmlSerializer soapserializer = new XmlSerializer(typeof(T));
//			MemoryStream memoryStream = new MemoryStream();
//			XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

//			soapserializer.Serialize(xmlTextWriter, soapEnvelope);
//			xmlTextWriter.Formatting = Formatting.None;

//			string output = Encoding.UTF8.GetString(memoryStream.ToArray());
//			string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
//			if (output.StartsWith(_byteOrderMarkUtf8))
//			{
//				output = output.Remove(0, _byteOrderMarkUtf8.Length);
//			}

//			XmlDocument xmlDocument = new XmlDocument();
//			xmlDocument.LoadXml(output);
//			return xmlDocument;
//		}

//		public async Task<string> SendRequestAsync(
//				XmlDocument xmlEnvelop,
//				X509Certificate2 certificadoDigital,
//				string url,
//				int timeOut,
//				TipoEvento? tipoEvento = null,
//				string actionUrn = "")
//		{
//			return await Task.Run(() => SendRequest(xmlEnvelop, certificadoDigital, url, timeOut, tipoEvento, actionUrn));
//		}

//		public string SendRequest(
//				XmlDocument xmlEnvelop,
//				X509Certificate2 certificadoDigital,
//				string url,
//				int timeOut,
//				TipoEvento? tipoEvento = null,
//				string actionUrn = "")
//		{
//			if (!tipoEvento.HasValue && string.IsNullOrWhiteSpace(actionUrn))
//				throw new ArgumentNullException("Informe tipoEvento ou actionUrn");

//			if (tipoEvento.HasValue)
//				actionUrn = new SoapUrls().GetSoapUrl(tipoEvento.Value);

//			string cleanUrl = url.Replace("?wsdl", "");
//			string xmlSoap = xmlEnvelop.InnerXml;

//			Console.WriteLine("=== ENVIANDO NFCe ===");
//			Console.WriteLine($"URL: {cleanUrl}");
//			Console.WriteLine($"SOAPAction: {actionUrn}");
//			Console.WriteLine($"Certificado: {certificadoDigital.Subject}");
//			Console.WriteLine($"Emissor: {certificadoDigital.Issuer}");
//			Console.WriteLine($"HasPrivateKey: {certificadoDigital.HasPrivateKey}");

//			//Recria o certificado com flags compatíveis com Linux
//			//var rawCert = certificadoDigital.Export(X509ContentType.Pkcs12);
//			//var cert = new X509Certificate2(rawCert, (string)null,
//			//		X509KeyStorageFlags.MachineKeySet |
//			//		X509KeyStorageFlags.PersistKeySet |
//			//		X509KeyStorageFlags.Exportable);

//			// Usa HttpWebRequest (mais compatível com certificados legados)
//			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
//			ServicePointManager.ServerCertificateValidationCallback = (sender, serverCert, chain, errors) =>
//			{
//				Console.WriteLine($"Server Cert: {serverCert?.Subject}");
//				Console.WriteLine($"SSL Errors: {errors}");
//				return true; // Aceita todos para homologação
//			};

//			var request = (HttpWebRequest)WebRequest.Create(cleanUrl);
//			request.Method = "POST";
//			request.ContentType = "application/soap+xml; charset=utf-8";
//			request.Headers.Add("SOAPAction", actionUrn);
//			request.Timeout = timeOut == 0 ? 90000 : timeOut;
//			request.KeepAlive = false;
//			request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
//			request.ClientCertificates.Clear();
//			request.ClientCertificates.Add(certificadoDigital);

//			try
//			{
//				using (var stream = request.GetRequestStream())
//				using (var writer = new StreamWriter(stream, Encoding.UTF8))
//				{
//					writer.Write(xmlSoap);
//				}

//				using (var response = (HttpWebResponse)request.GetResponse())
//				using (var stream = response.GetResponseStream())
//				using (var reader = new StreamReader(stream, Encoding.UTF8))
//				{
//					string result = reader.ReadToEnd();
//					Console.WriteLine($"✅ Sucesso! Resposta: {result.Length} bytes");
//					return result;
//				}
//			}
//			catch (WebException ex)
//			{
//				string responseBody = "";
//				if (ex.Response != null)
//				{
//					using (var reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
//					{
//						responseBody = reader.ReadToEnd();
//					}
//				}
//				Console.WriteLine($"❌ Erro: {ex.Message}");
//				Console.WriteLine($"Response: {responseBody}");
//				throw new Exception($"Erro na transmissão: {ex.Message}. Resposta: {responseBody}", ex);
//			}
//		}
//	}
//}
using CTe.CTeOSDocumento.Common;
using CTe.CTeOSDocumento.Soap;
using System;
using System.IO;
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
			XmlSerializer serializer = new XmlSerializer(typeof(T));

			using (MemoryStream ms = new MemoryStream())
			{
				XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
				serializer.Serialize(writer, soapEnvelope);

				string xml = Encoding.UTF8.GetString(ms.ToArray());
				string bom = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

				if (xml.StartsWith(bom))
					xml = xml.Remove(0, bom.Length);

				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);

				return doc;
			}
		}
		public string SendRequest(
		XmlDocument xmlEnvelop,
		X509Certificate2 certificadoDigital,
		string url,
		int timeOut,
		TipoEvento? tipoEvento = null,
		string actionUrn = "")
		{
			try
			{
				return SendRequestAsync(
						xmlEnvelop,
						certificadoDigital,
						url,
						timeOut,
						tipoEvento,
						actionUrn
				).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Console.WriteLine("❌ Erro síncrono: " + ex.Message);
				throw;
			}
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

			if (certificadoDigital == null)
				throw new ArgumentNullException("certificadoDigital");

			if (!certificadoDigital.HasPrivateKey)
				throw new Exception("Certificado sem chave privada");

			Console.WriteLine(certificadoDigital.PrivateKey.GetType().FullName);
			Console.WriteLine("=== ENVIO SEFAZ ===");
			Console.WriteLine(certificadoDigital.Subject);

			HttpClientHandler handler = new HttpClientHandler();

			handler.ClientCertificates.Add(certificadoDigital);
			handler.ClientCertificateOptions = ClientCertificateOption.Manual;
			handler.SslProtocols = SslProtocols.Tls12;

			handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) =>
			{
				return errors == System.Net.Security.SslPolicyErrors.None;
			};

			HttpClient client = new HttpClient(handler);
			client.Timeout = TimeSpan.FromMilliseconds(timeOut == 0 ? 90000 : timeOut);

			StringContent content = new StringContent(
					xmlEnvelop.OuterXml,
					new UTF8Encoding(false),
					"application/soap+xml"
			);

			HttpRequestMessage request = new HttpRequestMessage(
					HttpMethod.Post,
					url.Replace("?wsdl", "")
			);

			request.Content = content;

			if (!string.IsNullOrWhiteSpace(actionUrn))
				request.Headers.Add("SOAPAction", actionUrn);

			try
			{
				HttpResponseMessage response = await client.SendAsync(request);

				string result = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception(
							"Erro HTTP " + (int)response.StatusCode + ": " + result
					);
				}

				Console.WriteLine("✅ OK: " + result.Length + " bytes");

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine("❌ Erro: " + ex.Message);
				throw;
			}
			finally
			{
				client.Dispose();
				handler.Dispose();
			}
		}
	}
}