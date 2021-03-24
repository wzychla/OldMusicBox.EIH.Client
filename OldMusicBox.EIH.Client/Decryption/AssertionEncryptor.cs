using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Crypto;
using OldMusicBox.EIH.Client.Model;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Encryption
{
    /// <summary>
    /// Electronic Identification Hub (Węzeł Krajowy) assertion encryptor
    /// </summary>
    public class AssertionEncryptor : AssertionCrypto
    {
        const string KdfDigestAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
        const string KwAesAlgorithm     = "http://www.w3.org/2001/04/xmlenc#kw-aes256";
        const string ECNamedCurveURN    = "urn:oid:1.2.840.10045.3.1.7";

        public class RequiredParameters : AssertionCrypto.RequiredParametersBase
        {
            // principal
            public ClaimsPrincipal Principal;
        }

        public EncryptedAssertion CreateEmptyAssertion()
        {
            return new EncryptedAssertion()
            {
                EncryptedData = new EncryptedData()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "http://www.w3.org/2001/04/xmlenc#Element",
                    EncryptionMethod = new EncryptionMethod()
                    {
                        Algorithm = "http://www.w3.org/2009/xmlenc11#aes256-gcm",
                        DigestMethod = new DigestMethod()
                        {
                            Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256"
                        }
                    },
                    KeyInfo = new KeyInfo()
                    {
                        EncryptedKey = new EncryptedKey()
                        {
                            Id = Guid.NewGuid().ToString(),
                            EncryptionMethod = new EncryptionMethod()
                            {
                                Algorithm = KwAesAlgorithm,
                            },
                            KeyInfo = new KeyInfo()
                            {
                                AgreementMethod = new AgreementMethod()
                                {
                                    Algorithm = "http://www.w3.org/2009/xmlenc11#ECDH-ES",
                                    KeyDerivationMethod = new KeyDerivationMethod()
                                    {
                                        Algorithm = "http://www.w3.org/2009/xmlenc11#ConcatKDF",
                                        ConcatKDFParams = new ConcatKDFParams()
                                        {
                                            // AlgorithmID string is hardcoded 
                                            // AlgorithmID = "0000002A687474703A2F2F7777772E77332E6F72672F323030312F30342F786D6C656E63236B772D616573323536",
                                            DigestMethod = new DigestMethod()
                                            {
                                                Algorithm = KdfDigestAlgorithm 
                                            }

                                        }
                                    },
                                    OriginatorKeyInfo = new OriginatorKeyInfo()
                                    {
                                        KeyValue = new KeyValue()
                                        {
                                            ECKeyValue = new ECKeyValue()
                                            {
                                                NamedCurve = new NamedCurve()
                                                {
                                                    URI = ECNamedCurveURN,
                                                },
                                                PublicKey = new PublicKey()
                                            }
                                        }
                                    }
                                }
                            },
                            CipherData = new CipherData()
                            {
                                CipherValue = new CipherValue()
                            }
                        },
                    },
                    CipherData = new CipherData()
                    { 
                        CipherValue = new CipherValue()
                    }
                }
            };
        }

        /// <summary>
        /// Public encrypt method
        /// </summary>
        public EncryptedAssertion Encrypt(
            ClaimsPrincipal principal,
            string IssuerDomain,
            string ConsumerDomain,
            Org.BouncyCastle.X509.X509Certificate encryptionKey,
            AsymmetricKeyParameter privateKey
            )
        {
            var assertion          = this.CreateEmptyAssertion();

            var requiredParameters         = new RequiredParameters();

            requiredParameters.Principal   = principal;

            requiredParameters.AlgorithmID   = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(KwAesAlgorithm));
            requiredParameters.PartyUInfo    = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(IssuerDomain));
            requiredParameters.PartyVInfo    = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(ConsumerDomain));

            requiredParameters.DigestMethodString  = KdfDigestAlgorithm.Substring(KdfDigestAlgorithm.IndexOf("#") + 1);
            requiredParameters.NamedCurveOid       = ECNamedCurveURN.Replace("urn:oid:", "");
            requiredParameters.KeyEncryptionMethod = KwAesAlgorithm;

            // fill-in initial data
            this.CreateDerivedKeySection(assertion, requiredParameters);

            // compute kw-aes public key from the client's public certificate
            ECPoint publicSessionKey = this.CreatePublicKeyBytes(assertion, requiredParameters, encryptionKey);

            // encrypt session key
            byte[] sessionKey = this.EncryptSessionKey(publicSessionKey, assertion, requiredParameters, privateKey);

            this.EncryptPrincipal(sessionKey, assertion, principal);

            return assertion;
        }

        private void EncryptPrincipal(byte[] sessionKey, EncryptedAssertion assertion, ClaimsPrincipal principal)
        {
            // key
            var encryptedDataSection = assertion.EncryptedData;

            EncryptionService es = new EncryptionService(256, 128, 96);
            string assertionText = this.CreateAssertionText(principal);
            byte[] encryptedDoc  = es.EncryptWithKey(Encoding.UTF8.GetBytes(assertionText), sessionKey);

            encryptedDataSection.CipherData.CipherValue.Text = Convert.ToBase64String(encryptedDoc);
        }

        private string CreateAssertionText(ClaimsPrincipal principal)
        {
            Assertion assertion = new Assertion()
            {
                Subject = new Subject()
                { 
                    NameID = new Model.NameID()
                    {
                        Text = principal.Identity.Name
                    }
                },
                AttributeStatement = new AttributeStatement()
                {
                    /*
                    Attributes = new[]
                    {
                        new Model.Attribute()
                        {
                            Name           = ClaimTypes.Name,
                            AttributeValue = new [] { principal.FindFirst.. }
                        }
                    }
                    */
                }
            };

            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                var xmlWriterSettings                = new XmlWriterSettings();
                xmlWriterSettings.Encoding           = Encoding.UTF8;
                xmlWriterSettings.OmitXmlDeclaration = true;

                using (var xmlWriter = XmlWriter.Create(writer, xmlWriterSettings))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Assertion), Namespaces.ASSERTION);

                    xs.Serialize(xmlWriter, assertion);
                }
            }

            return sb.ToString();
        }

        private byte[] EncryptSessionKey(
            ECPoint publicKey,
            EncryptedAssertion assertion,
            RequiredParameters requiredParameters,
            AsymmetricKeyParameter privateKey
            )
        {
            // key
            var encryptedKeySection = assertion.EncryptedData.KeyInfo.EncryptedKey;

            // ECC parameters
            ECDomainParameters ecNamedCurveParameterSpec = GetCurveParameters(requiredParameters.NamedCurveOid);

            // klucz publiczny efemeryczny nadawcy
            AsymmetricKeyParameter ecPublicKey = new ECPublicKeyParameters(publicKey, ecNamedCurveParameterSpec);

            // Wykonanie operacji KeyAgreement
            IBasicAgreement keyAgreement = AgreementUtilities.GetBasicAgreement("ECDH");
            keyAgreement.Init(privateKey);

            // zrzucenie efektu key agreement do tablicy
            byte[] sharedSecret  = keyAgreement.CalculateAgreement(ecPublicKey).ToByteArrayUnsigned();
            IDigest digestMethod = DigestUtilities.GetDigest(requiredParameters.DigestMethodString);

            // wykonanie funkcji KDF
            byte[] wrappedKeyBytes = this.DeriveKey(requiredParameters, sharedSecret, digestMethod);

            // odszyfrowanie klucza ktorym nadawca zaszyfrowal dane
            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", wrappedKeyBytes);
            byte[] sessionKey         = Org.BouncyCastle.Crypto.Xml.EncryptedXml.EncryptKey(wrappedKeyBytes, keyParameter);

            encryptedKeySection.CipherData.CipherValue.Text = Convert.ToBase64String(sessionKey);

            return wrappedKeyBytes;
            //return sessionKey;
        }

        private void CreateDerivedKeySection(
            EncryptedAssertion assertion,
            RequiredParameters requiredParameters
            )
        {
            // key
            var encryptedKey = assertion.EncryptedData.KeyInfo.EncryptedKey;

            // fill the information in the XML
            var concatKDFParams         = encryptedKey.KeyInfo.AgreementMethod.KeyDerivationMethod.ConcatKDFParams;
            concatKDFParams.AlgorithmID = requiredParameters.AlgorithmID;
            concatKDFParams.PartyUInfo  = requiredParameters.PartyUInfo;
            concatKDFParams.PartyVInfo  = requiredParameters.PartyVInfo;
        }

        private ECPoint CreatePublicKeyBytes(
            EncryptedAssertion assertion,
            RequiredParameters requiredParameters,
            Org.BouncyCastle.X509.X509Certificate encryptionKey
            )
        {
            // key
            var encryptedKey = assertion.EncryptedData.KeyInfo.EncryptedKey;

            // compute public key
            //ECDomainParameters ecNamedCurveParameterSpec = GetCurveParameters(requiredParameters.NamedCurveOid);
            //ECCurve curve                                = ecNamedCurveParameterSpec.Curve;

            ECPoint ecPoint = (encryptionKey.GetPublicKey() as ECPublicKeyParameters).Q;
            //ECPoint q = (encryptionKey.GetPublicKey() as ECPublicKeyParameters).Q;
            //q.Normalize();

            //ECPoint ecPoint = curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger());
            //ecPoint.Normalize();

            // fill
            encryptedKey.KeyInfo.AgreementMethod.OriginatorKeyInfo.KeyValue.ECKeyValue.PublicKey.Text = Convert.ToBase64String(ecPoint.GetEncoded());

            return ecPoint;
        }
    }
}
