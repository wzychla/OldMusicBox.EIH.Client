using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Logging;
using OldMusicBox.EIH.Client.Model;
using OldMusicBox.EIH.Client.Serialization;
using OldMusicBox.EIH.Client.Validation;
using Org.BouncyCastle.Crypto.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OldMusicBox.EIH.Client.Signature
{
    /// <summary>
    /// Default message signer
    /// </summary>
    public class DefaultMessageSigner : IMessageSigner
    {
        public DefaultMessageSigner(IMessageSerializer serializer)
        {
            if ( serializer == null )
            {
                throw new ArgumentNullException("serializer");
            }

            this.messageSerializer = serializer;
            this.encoding          = new UTF8Encoding(false);
        }

        private IMessageSerializer messageSerializer { get; set; }
        private Encoding encoding { get; set; }

        /// <summary>
        /// Message signing
        /// </summary>
        public virtual byte[] Sign(
            ISignableMessage message, 
            X509Configuration x509Configuration)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if ( x509Configuration == null ||
                 x509Configuration.SignatureCertificate == null 
                )
            {
                throw new ArgumentNullException("certificate");
            }

            // first, serialize to XML
            var messageBody = this.messageSerializer.Serialize(message, new MessageSerializationParameters()
            {
                ShouldBase64Encode = false,
                ShouldDeflate      = false
            });

            var xml = new XmlDocument();
            xml.LoadXml(messageBody);

            // sign the node with the id
            var reference = new Reference("#"+message.ID);
            var envelope  = new XmlDsigEnvelopedSignatureTransform(true);
            reference.AddTransform(envelope);

            // canonicalization
            var c14       = new XmlDsigExcC14NTransform();
            c14.Algorithm = SignedXml.XmlDsigExcC14NTransformUrl;
            reference.AddTransform(c14);

            // some more spells depending on SHA1 vs SHA256
            var signed                               = new SignedXml(xml);
            signed.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            switch ( x509Configuration.SignatureAlgorithm )
            {
                case SignatureAlgorithm.SHA1:
                    signed.SigningKey                 = x509Configuration.SignaturePrivateKey;
                    signed.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
                    reference.DigestMethod            = SignedXml.XmlDsigSHA1Url;
                    break;
                case SignatureAlgorithm.SHA256:
                    signed.SigningKey                 = x509Configuration.SignaturePrivateKey;
                    signed.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
                    reference.DigestMethod            = SignedXml.XmlDsigSHA256Url;
                    break;
                case SignatureAlgorithm.ECDSA256:
                    signed.SigningKey                 = x509Configuration.SignaturePrivateKey;
                    signed.SignedInfo.SignatureMethod = SignedXml.XmlDsigEcdsaSha256Url;
                    reference.DigestMethod            = SignedXml.XmlDsigSHA256Url;
                    break;
            }

            if ( x509Configuration.IncludeKeyInfo )
            {
                var key     = new Org.BouncyCastle.Crypto.Xml.KeyInfo();
                var keyData = new KeyInfoX509Data(x509Configuration.SignatureCertificate);
                key.AddClause(keyData);
                signed.KeyInfo = key;
            }

            // show the reference
            signed.AddReference(reference);
            // create the signature
            signed.ComputeSignature();
            var signature = signed.GetXml();

            // insert the signature into the document
            var element = xml.DocumentElement.ChildNodes[0];
            xml.DocumentElement.InsertAfter(xml.ImportNode(signature, true), element);

            // log
            new LoggerFactory().For(this).Debug(Event.SignedMessage, xml.OuterXml);

            // convert
            var result = this.encoding.GetBytes(xml.OuterXml);
            return result;
        }


        private bool VerifyXml(byte[] data)
        {
            XmlDocument xd = new XmlDocument();
            xd.PreserveWhitespace = true;

            using (var stream = new MemoryStream(data))
            {
                stream.Seek(0, SeekOrigin.Begin);
                xd.Load(stream);
            }

            SignedXml signedXml = new SignedXml(xd);

            // load the first <signature> node and load the signature  
            XmlNode MessageSignatureNode = xd.GetElementsByTagName("Signature")[0];
            if (MessageSignatureNode == null)
            {
                MessageSignatureNode = xd.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];
            }

            signedXml.LoadXml((XmlElement)MessageSignatureNode);
            signedXml.SafeCanonicalizationMethods.Add("http://www.w3.org/TR/1999/REC-xpath-19991116");

            // get the cert from the signature
            Org.BouncyCastle.X509.X509Certificate certificate = null;
            foreach (KeyInfoClause clause in signedXml.KeyInfo)
            {
                if (clause is KeyInfoX509Data)
                {
                    if (((KeyInfoX509Data)clause).Certificates.Count > 0)
                    {
                        certificate =
                        (Org.BouncyCastle.X509.X509Certificate)((KeyInfoX509Data)clause).Certificates[0];
                    }
                }
            }

            // check the signature and return the result.
            return signedXml.CheckSignature(certificate, true);
        }



        private void SetPrefix(string prefix, XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
                SetPrefix(prefix, n);
            node.Prefix = prefix;
        }

        public virtual bool Validate(
            IVerifiableMessage message,
            Org.BouncyCastle.X509.X509Certificate certificate
            )
        {
            if ( message == null || message.RawMessage == null ||
                 string.IsNullOrEmpty( message.RawMessage.Payload )
                )
            {
                throw new ArgumentNullException("message");
            }
            if ( certificate == null )
            {
                throw new ArgumentNullException("certificate");
            }

            // search for signatures (possibly multiple)
            var xml = new XmlDocument();
            xml.LoadXml(message.RawMessage.Payload);

            var signatureNodes = xml.GetElementsByTagName("Signature", Namespaces.XMLDSIG);
            foreach (XmlElement signatureNode in signatureNodes)
            {
                var signedXml = new SignedXml(signatureNode.ParentNode as XmlElement);
                signedXml.LoadXml(signatureNode);
                signedXml.SafeCanonicalizationMethods.Add("http://www.w3.org/TR/1999/REC-xpath-19991116");

                var result = signedXml.CheckSignature(certificate.GetPublicKey());

                if (!result)
                {
                    throw new ValidationException(string.Format("{0} signature validation failed", message.GetType().Name ));
                }

                return result;
            }
            return false;
        }
    }
}
