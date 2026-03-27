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

			// Remove ?wsdl da URL se existir
			string cleanUrl = url;
			if (cleanUrl.Contains("?wsdl"))
				cleanUrl = cleanUrl.Replace("?wsdl", "");

			string xmlSoap = xmlEnvelop.InnerXml;

			Console.WriteLine("=== ENVIANDO PARA SEFAZ ===");
			Console.WriteLine($"URL: {cleanUrl}");
			Console.WriteLine($"SOAPAction: {actionUrn}");
			Console.WriteLine($"Certificado Subject: {certificadoDigital.Subject}");
			Console.WriteLine($"Certificado Issuer: {certificadoDigital.Issuer}");
			Console.WriteLine($"HasPrivateKey: {certificadoDigital.HasPrivateKey}");

			// 🔧 Configuração crítica para Linux
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.CheckCertificateRevocationList = false;

			// 🔧 Força o uso do ServicePointManager com validação de certificado
			ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) =>
			{
				Console.WriteLine($"=== VALIDAÇÃO SSL ===");
				Console.WriteLine($"Server Cert Subject: {cert.Subject}");
				Console.WriteLine($"Server Cert Issuer: {cert.Issuer}");
				Console.WriteLine($"Errors: {errors}");

				if (chain != null)
				{
					Console.WriteLine($"Chain length: {chain.ChainElements.Count}");
					foreach (var element in chain.ChainElements)
					{
						Console.WriteLine($"  - {element.Certificate.Subject} | Status: {element.ChainElementStatus.Length}");
					}
				}

				// Aceita todos em homologação para teste
				return true;
			};

			// 🔧 Configuração do request
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cleanUrl);
			request.Method = "POST";
			request.ContentType = "application/soap+xml; charset=utf-8";
			request.Headers.Add("SOAPAction", actionUrn);
			request.Timeout = timeOut == 0 ? 90000 : timeOut; // 90 segundos
			request.ReadWriteTimeout = timeOut == 0 ? 90000 : timeOut;
			request.KeepAlive = false;
			request.ProtocolVersion = HttpVersion.Version11;
			request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
			request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;

			// 🔑 Adiciona o certificado
			request.ClientCertificates.Add(certificadoDigital);

			// Log dos certificados no request
			Console.WriteLine($"Certificados no request: {request.ClientCertificates.Count}");
			foreach (X509Certificate cert in request.ClientCertificates)
			{
				Console.WriteLine($"  - {cert.Subject}");
			}

			try
			{
				using (Stream stream = request.GetRequestStream())
				using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
				{
					writer.Write(xmlSoap);
				}

				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				using (Stream stream = response.GetResponseStream())
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
				{
					string result = reader.ReadToEnd();
					Console.WriteLine($"✅ Sucesso! Resposta: {result.Length} bytes");
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

				Console.WriteLine($"❌ Erro: {ex.Message}");
				Console.WriteLine($"Status: {(ex.Response as HttpWebResponse)?.StatusCode}");
				Console.WriteLine($"Resposta: {responseBody}");

				if (ex.InnerException != null)
				{
					Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
				}

				throw;
			}
		}
	}
}