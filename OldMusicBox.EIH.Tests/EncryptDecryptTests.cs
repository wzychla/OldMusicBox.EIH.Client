using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldMusicBox.EIH.Client.Model;
using System.Security.Claims;
using OldMusicBox.EIH.Client.Signature;
using OldMusicBox.EIH.Client;
using System.IdentityModel.Tokens;
using System.IdentityModel.Selectors;

namespace OldMusicBox.EIH.Tests
{
    [TestClass]
    public class EncryptDecryptTests
    {
        /// <summary>
        /// Encryption and decryption
        /// </summary>
        /// <remarks>
        /// This test uses the same configuration of certificates for both the server and the client
        /// This is unusual, in normal circumstances client/server don't have access 
        /// to private keys when not necessary:
        /// * only server has private keys to sign SAML 
        /// * only client has private keys to decrypt assertions
        /// </remarks>
        [TestMethod]
        public void EncryptDecryptTest()
        {
            string serverCertName = "ExampleSigningCertificate";
            string serverCertPwd  = "123456";
            string clientCertName = "ExampleEncryptionCertificate";
            string clientCertPwd  = "123456";

            string name             = "foo";
            string firstName        = "joe";
            string familyName       = "doe";
            string dateOfBirth      = "2000-01-01T00:00:00";
            string personIdentifier = "12345678901";
            string issuer           = "foo.bar.qux";

            Org.BouncyCastle.X509.X509Certificate _serverCert                = ClientCertificateProvider.GetCertificate(serverCertName, serverCertPwd);
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter _serverPrivateKey = ClientCertificateProvider.GetPrivateKey(serverCertName, serverCertPwd);

            Org.BouncyCastle.X509.X509Certificate _clientCert = ClientCertificateProvider.GetCertificate(clientCertName, clientCertPwd);
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter _clientPrivateKey = ClientCertificateProvider.GetPrivateKey(clientCertName, clientCertPwd);

            // arrange
            var principal =
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.Name, name),
                            new Claim(Client.Constants.Eidas.FirstNameClaim,        firstName),
                            new Claim(Client.Constants.Eidas.FamilyNameClaim,       familyName),
                            new Claim(Client.Constants.Eidas.DateOfBirthClaim,      dateOfBirth),
                            new Claim(Client.Constants.Eidas.PersonIdentifierClaim, personIdentifier),
                        }
                    ));

            var artifactResponseFactory   = new ArtifactResponseFactory();
            var responseFactory           = new ResponseFactory();
            var encryptedAssertionFactory = new EncryptedAssertionFactory();
            var saml2module               = new Saml2AuthenticationModule();

            var server509Configuration = new X509Configuration()
            {
                SignatureCertificate    = _serverCert,
                SignaturePrivateKey     = _serverPrivateKey,
                IncludeKeyInfo          = true,
                SignatureAlgorithm      = SignatureAlgorithm.ECDSA256,
                EncryptionCertificate   = _serverCert,
                EncryptionPrivateKey    = _serverPrivateKey,
                EncryptionCoCertificate = _clientCert
            };
            var client509Configuration = new X509Configuration()
            {
                SignatureCertificate  = null, 
                SignaturePrivateKey   = null,
                IncludeKeyInfo        = true,
                SignatureAlgorithm    = SignatureAlgorithm.ECDSA256,
                EncryptionCertificate = null,
                EncryptionPrivateKey  = _clientPrivateKey
            };

            var configuration = new SecurityTokenHandlerConfiguration
            {
                CertificateValidator = X509CertificateValidator.None,
                IssuerNameRegistry   = new DemoClientIssuerNameRegistry(),
                DetectReplayedTokens = false
            };
            configuration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;
            var tokenHandler = new Client.Saml2SecurityTokenHandler()
            {
                Configuration = configuration
            };

            responseFactory.X509Configuration = server509Configuration;
            responseFactory.InResponseTo      = Guid.NewGuid().ToString();
            responseFactory.Issuer            = issuer;

            artifactResponseFactory.X509Configuration = server509Configuration;
            artifactResponseFactory.InResponseTo      = Guid.NewGuid().ToString();
            artifactResponseFactory.Issuer            = issuer;

            // data to be encrypted
            encryptedAssertionFactory.Principal          = principal;
            encryptedAssertionFactory.AssertionIssuer    = "issuer.domain.com";
            encryptedAssertionFactory.AssertionConsumer  = "consumer.domain.com";
            encryptedAssertionFactory.X509Configuration  = server509Configuration;

            // build the crypted assertion
            responseFactory.EncryptedAssertions               = encryptedAssertionFactory.Build();
            artifactResponseFactory.ArtifactResponse.Response = responseFactory.Build();

            // act
            var artifactResponse = artifactResponseFactory.Create();
            var saml2Token       = saml2module.ParseArtifactResponse(artifactResponse);

            // assert
            Assert.IsNotNull(saml2Token);

            saml2module.TryDecryptingEncryptedAssertions(saml2Token, client509Configuration);
            Assert.IsNotNull(saml2Token.Assertion);

            var identities = tokenHandler.ValidateToken(saml2Token);
            Assert.IsNotNull(identities);
            Assert.IsTrue(identities.Count > 0);
            var identity = identities[0];
            Assert.IsNotNull(identity);

            // claims
            Assert.AreEqual(name, identity.FindFirst(ClaimTypes.NameIdentifier).Value);
            Assert.AreEqual(familyName, identity.FindFirst(Client.Constants.Eidas.FamilyNameClaim).Value);
            Assert.AreEqual(firstName, identity.FindFirst(Client.Constants.Eidas.FirstNameClaim).Value);
            Assert.AreEqual(dateOfBirth, identity.FindFirst(Client.Constants.Eidas.DateOfBirthClaim).Value);
            Assert.AreEqual(personIdentifier, identity.FindFirst(Client.Constants.Eidas.PersonIdentifierClaim).Value);
        }

        /// <summary>
        /// This test validates that computing the shared secret is symmetric:
        /// * server public key/client private key
        /// * client public key/server private key
        /// give same shared secret
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/52648023/what-makes-ecdh-rely-on-two-public-keys-alone
        /// </remarks>
        [TestMethod]
        public void ECDHSymmetricSharedSecret()
        {
            // arrange
            string serverCertName = "ExampleSigningCertificate";
            string serverCertPwd = "123456";
            string clientCertName = "ExampleEncryptionCertificate";
            string clientCertPwd = "123456";

            var _serverCert       = ClientCertificateProvider.GetCertificate(serverCertName, serverCertPwd);
            var _serverPrivateKey = ClientCertificateProvider.GetPrivateKey(serverCertName, serverCertPwd);

            var _clientCert       = ClientCertificateProvider.GetCertificate(clientCertName, clientCertPwd);
            var _clientPrivateKey = ClientCertificateProvider.GetPrivateKey(clientCertName, clientCertPwd);

            // act
            var sharedSecret1 = this.ComputeSharedSecret(_serverCert, _clientPrivateKey);
            var sharedSecret2 = this.ComputeSharedSecret(_clientCert, _serverPrivateKey);

            // assert
            CollectionAssert.AreEqual(sharedSecret1, sharedSecret2);
        }

        /// <summary>
        /// Auxiliary function to compute the shared secret
        /// </summary>
        private byte[] ComputeSharedSecret(Org.BouncyCastle.X509.X509Certificate publicKey, Org.BouncyCastle.Crypto.AsymmetricKeyParameter privateKey)
        {
            var keyAgreement = Org.BouncyCastle.Security.AgreementUtilities.GetBasicAgreement("ECDH");
            keyAgreement.Init(privateKey);

            var parameters   = publicKey.GetPublicKey() as Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters;
            var ecPoint      = parameters.Q;
            var ecPublicKey  = new Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(ecPoint, parameters.Parameters);

            var sharedSecret = keyAgreement.CalculateAgreement(ecPublicKey).ToByteArrayUnsigned();

            return sharedSecret;
        }
    }

    public class DemoClientIssuerNameRegistry : IssuerNameRegistry
    {
        public override string GetIssuerName(SecurityToken securityToken)
        {
            X509SecurityToken x509Token = securityToken as X509SecurityToken;
            if (x509Token != null)
                return x509Token.Certificate.Subject;
            else
                return null;
        }
    }
}
