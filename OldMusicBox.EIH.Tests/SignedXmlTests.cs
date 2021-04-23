using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldMusicBox.EIH.Client.Model;
using OldMusicBox.EIH.Client.Serialization;
using OldMusicBox.EIH.Client.Signature;
using Org.BouncyCastle.Crypto.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Tests
{
    [TestClass]
    public class SignedXmlTests
    {
        [XmlRoot("ExampleModel")]
        public class ExampleModel : ISignableMessage, IVerifiableMessage
        {
            [XmlAttribute("ID")]
            public string ID { get; set; }
            public string ExampleData { get; set; }
            public RawMessage RawMessage { get; set; }
        }

        [TestMethod]
        public void ECDASignaturesAreSupported()
        {
            // arrange
            string clientCertName = "ExampleEncryptionCertificate";
            string clientCertPwd  = "123456";

            var certificate       = ClientCertificateProvider.GetCertificate(clientCertName, clientCertPwd);
            var privateKey        = ClientCertificateProvider.GetPrivateKey(clientCertName, clientCertPwd);

            var message = new ExampleModel() { ID = "_1", ExampleData = "2" };
            var signer  = new DefaultMessageSigner(new DefaultMessageSerializer() );

            // act
            var signedMessage = signer.Sign(
                message,
                new Client.Model.X509Configuration()
                {
                    SignatureAlgorithm   = SignatureAlgorithm.ECDSA256,
                    SignatureCertificate = certificate,
                    SignaturePrivateKey  = privateKey,
                    IncludeKeyInfo       = true

                });
            var signedMessageString = Encoding.UTF8.GetString(signedMessage);

            // assert
            Assert.IsNotNull(signedMessage);
            Assert.IsTrue(signedMessageString.IndexOf("Signature") > 0);

            // act
            message.RawMessage = new RawMessage() { Payload = signedMessageString };

            DateTime startDate = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                Org.BouncyCastle.X509.X509Certificate cert;
                var result = signer.Validate(message, out cert);
                // assert
                Assert.IsTrue(result);
            }
            DateTime endDate = DateTime.Now;

            Console.WriteLine(endDate - startDate);
        }

        [TestMethod]
        public void RSACSignaturesAreSupported()
        {
            // arrange
            string clientCertName = "ExampleRSACertificate";
            string clientCertPwd  = "123456";

            var certificate = ClientCertificateProvider.GetCertificate(clientCertName, clientCertPwd);
            var privateKey  = ClientCertificateProvider.GetPrivateKey(clientCertName, clientCertPwd);

            var message = new ExampleModel() { ID = "_1", ExampleData = "2" };
            var signer = new DefaultMessageSigner(new DefaultMessageSerializer());

            // act
            var signedMessage = signer.Sign(
                message,
                new Client.Model.X509Configuration()
                {
                    SignatureAlgorithm = SignatureAlgorithm.RSA256,
                    SignatureCertificate = certificate,
                    SignaturePrivateKey = privateKey,
                    IncludeKeyInfo = true
                });
            var signedMessageString = Encoding.UTF8.GetString(signedMessage);

            // assert
            Assert.IsNotNull(signedMessage);
            Assert.IsTrue(signedMessageString.IndexOf("Signature") > 0);

            // act
            message.RawMessage = new RawMessage() { Payload = signedMessageString };


            DateTime startDate = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                Org.BouncyCastle.X509.X509Certificate cert;
                var result = signer.Validate(message, out cert);
                // assert
                Assert.IsTrue(result);
            }
            DateTime endDate = DateTime.Now;

            Console.WriteLine(endDate - startDate);
        }

        [TestMethod]
        public void DSACSignaturesAreSupported()
        {
            // arrange
            string clientCertName = "ExampleDSACertificate";
            string clientCertPwd  = "123456";

            var certificate = ClientCertificateProvider.GetCertificate(clientCertName, clientCertPwd);
            var privateKey = ClientCertificateProvider.GetPrivateKey(clientCertName, clientCertPwd);

            var message = new ExampleModel() { ID = "_1", ExampleData = "2" };
            var signer = new DefaultMessageSigner(new DefaultMessageSerializer());

            // act
            var signedMessage = signer.Sign(
                message,
                new Client.Model.X509Configuration()
                {
                    SignatureAlgorithm   = SignatureAlgorithm.DSA256,
                    SignatureCertificate = certificate,
                    SignaturePrivateKey  = privateKey,
                    IncludeKeyInfo       = true
                });
            var signedMessageString = Encoding.UTF8.GetString(signedMessage);

            // assert
            Assert.IsNotNull(signedMessage);
            Assert.IsTrue(signedMessageString.IndexOf("Signature") > 0);

            // act
            message.RawMessage = new RawMessage() { Payload = signedMessageString };


            DateTime startDate = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                Org.BouncyCastle.X509.X509Certificate cert;
                var result = signer.Validate(message, out cert);
                // assert
                Assert.IsTrue(result);
            }
            DateTime endDate = DateTime.Now;

            Console.WriteLine(endDate - startDate);
        }

        [TestMethod]
        public void HMACSignaturesAreSupported()
        {
            // arrange
            byte[] key = new byte[] { 48 };

            var privateKey  = new HMacKeyParameter( key, true );

            var message = new ExampleModel() { ID = "_1", ExampleData = "2" };
            var signer  = new DefaultMessageSigner(new DefaultMessageSerializer());

            // act
            var signedMessage = signer.Sign(
                message,
                new Client.Model.X509Configuration()
                {
                    SignatureAlgorithm   = SignatureAlgorithm.HMAC256,
                    SignaturePrivateKey  = privateKey
                });
            var signedMessageString = Encoding.UTF8.GetString(signedMessage);

            // assert
            Assert.IsNotNull(signedMessage);
            Assert.IsTrue(signedMessageString.IndexOf("Signature") > 0);

            // act
            message.RawMessage = new RawMessage() { Payload = signedMessageString };


            DateTime startDate = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                var result = signer.Validate(message, privateKey);
                // assert
                Assert.IsTrue(result);
            }
            DateTime endDate = DateTime.Now;

            Console.WriteLine(endDate - startDate);
        }
    }
}
