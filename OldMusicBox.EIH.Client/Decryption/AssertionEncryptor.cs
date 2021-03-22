using OldMusicBox.EIH.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Encryption
{
    /// <summary>
    /// Electronic Identification Hub (Węzeł Krajowy) assertion encryptor
    /// </summary>
    public class AssertionEncryptor : AssertionCrypto
    {
        const string KdfDigestAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
        const string KwAesAlgorithm     = "http://www.w3.org/2001/04/xmlenc#kw-aes256";

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
                                            AlgorithmID = "0000002A687474703A2F2F7777772E77332E6F72672F323030312F30342F786D6C656E63236B772D616573323536",
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
                                                    URI = "urn:oid:1.2.840.10045.3.1.7",
                                                },
                                                PublicKey = new PublicKey()
                                            }
                                        }
                                    }
                                }
                            },
                            CipherData = new CipherData()
                        },
                    },
                    CipherData = new CipherData()
                }
            };
        }

        private void CreateDerivedKeySection(
            EncryptedAssertion                    assertion,
            RequiredParameters                    requiredParameters,
            Org.BouncyCastle.X509.X509Certificate encryptionKey
            )
        {
            // key
            var encryptedKey = assertion.EncryptedData.KeyInfo.EncryptedKey;

            // fill the information in the XML
            var concatKDFParams = encryptedKey.KeyInfo.AgreementMethod.KeyDerivationMethod.ConcatKDFParams;
            concatKDFParams.PartyUInfo  = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(requiredParameters.PartyUInfo));
            concatKDFParams.PartyVInfo  = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(requiredParameters.PartyVInfo));
            concatKDFParams.AlgorithmID = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(requiredParameters.AlgorithmID));
        }

        /// <summary>
        /// Public encrypt method
        /// </summary>
        public EncryptedAssertion Encrypt(
            ClaimsPrincipal principal,
            string IssuerDomain,
            string ConsumerDomain,
            Org.BouncyCastle.X509.X509Certificate encryptionKey
            )
        {
            var assertion          = this.CreateEmptyAssertion();

            var requiredParameters         = new RequiredParameters();
            requiredParameters.Principal   = principal;
            requiredParameters.PartyUInfo  = IssuerDomain;
            requiredParameters.PartyVInfo  = ConsumerDomain;
            requiredParameters.AlgorithmID = KwAesAlgorithm;

            this.CreateDerivedKeySection(assertion, requiredParameters, encryptionKey);

            return assertion;
        }
    }
}
