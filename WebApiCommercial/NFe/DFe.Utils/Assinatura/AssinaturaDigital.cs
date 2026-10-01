using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Shared.DFe.Utils;
using SignatureZeus = DFe.Classes.Assinatura.Signature;

namespace DFe.Utils.Assinatura
{
    public class AssinaturaDigital
    {
        public static SignatureZeus Assina<T>(T objeto, string id, X509Certificate2 certificado,
            string signatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
            string digestMethod = "http://www.w3.org/2000/09/xmldsig#sha1",
            bool cfgServicoRemoverAcentos = false) where T : class
        {
            var objetoLocal = objeto;
            if (id == null)
                throw new Exception("Não é possível assinar um objeto evento sem sua respectiva Id!");

            var documento = new XmlDocument { PreserveWhitespace = true };

            documento.LoadXml(cfgServicoRemoverAcentos
                ? FuncoesXml.ClasseParaXmlString(objetoLocal).RemoverAcentos()
                : FuncoesXml.ClasseParaXmlString(objetoLocal));

            var docXml = new SignedXml(documento) { SigningKey = certificado.PrivateKey };

            docXml.SignedInfo.SignatureMethod = signatureMethod;
            var reference = new Reference { Uri = "#" + id, DigestMethod = digestMethod };

            // adicionando EnvelopedSignatureTransform a referencia
            var envelopedSigntature = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(envelopedSigntature);

            var c14Transform = new XmlDsigC14NTransform();
            reference.AddTransform(c14Transform);

            docXml.AddReference(reference);

            // carrega o certificado em KeyInfoX509Data para adicionar a KeyInfo
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));

            docXml.KeyInfo = keyInfo;
            docXml.ComputeSignature();

            //// recuperando a representacao do XML assinado
            var xmlDigitalSignature = docXml.GetXml();
            var assinatura = FuncoesXml.XmlStringParaClasse<Classes.Assinatura.Signature>(xmlDigitalSignature.OuterXml);
            return assinatura;
        }

        /// <summary>
        /// Assinatura PKCS#1 (RSA + SHA-1) sobre bytes crus, devolvida como bytes.
        ///
        /// Existe aqui porque o MDF-e precisa assinar a CHAVE do manifesto — que é
        /// uma string, não um objeto serializável — para montar o parâmetro
        /// <c>&amp;sign=</c> do QR Code em contingência (<c>tpEmis=6</c>). O
        /// <see cref="Assina{T}"/> não serve para isso: ele serializa o objeto e
        /// assina o XML.
        ///
        /// Este método e <see cref="ObterHashSha1Bytes"/> foram portados do DFe.NET
        /// no tag 2026.9.24.1416 — a cópia vendorizada aqui é de uma revisão
        /// anterior e não os tinha, e por isso <c>ExtMDFe.cs</c> não compilava.
        /// Manter o corpo idêntico ao upstream: numa próxima re-vendorização isso
        /// deve virar um no-op em vez de um conflito.
        /// </summary>
        public static byte[] CriarAssinaturaPkcs1(X509Certificate2 certificado, byte[] valor)
        {
            var rsa = certificado.GetRSAPrivateKey();

            var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
            rsaFormatter.SetHashAlgorithm("SHA1");

            var hashSha1Bytes = ObterHashSha1Bytes(valor);

            var assinatura = rsaFormatter.CreateSignature(hashSha1Bytes);

            return assinatura;
        }

        /// <summary>Hash SHA-1 em bytes. Par de <see cref="CriarAssinaturaPkcs1"/>.</summary>
        public static byte[] ObterHashSha1Bytes(byte[] dados)
        {
            using (var sha1 = SHA1.Create())
            {
                var sha1HashBytes = sha1.ComputeHash(dados);

                return sha1HashBytes;
            }
        }
    }
}