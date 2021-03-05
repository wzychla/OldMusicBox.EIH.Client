using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldMusicBox.EIH.Client;
using OldMusicBox.EIH.Client.Decryption;
using OldMusicBox.EIH.Client.Model;

namespace OldMusicBox.EIH.Tests
{
    [TestClass]
    public class CoreTests
    {
        [TestMethod]
        public void BasicDecryptionTest()
        {
            // arrange
            var token            = GetSecurityTokenFromAssertionResponse();
            var privateKey       = ClientCertificateProvider.GetEncPrivateKey();

            // act
            var decrypted = new AssertionDecryptor().Decrypt(token, privateKey);

            // assert
            Assert.IsNotNull(decrypted);
            Assert.IsNotNull(decrypted.AttributeStatement);
            Assert.IsNotNull(decrypted.AttributeStatement.Attributes);

            // there are 4 assertions
            Assert.AreEqual(4, decrypted.AttributeStatement.Attributes.Count());
            Assert.AreEqual("Apitz", decrypted.AttributeStatement.Attributes[0].AttributeValue[0]);
        }

        #region Aux


        public static Saml2SecurityToken GetSecurityTokenFromAssertionResponse()
        {
            var fileContent = File.ReadAllText("./Resources/encryptedEC.xml", Encoding.UTF8);

            using (StringReader sr = new StringReader(fileContent))
            {
                var xs = new XmlSerializer(typeof(ArtifactResponse));
                var artifactResponse = (ArtifactResponse)xs.Deserialize(sr);

                return new Saml2SecurityToken(artifactResponse.Response);
            }
        }

        #endregion
    }
}
