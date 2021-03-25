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
        // default token lifespan
        TimeSpan DefaultTokenLifeSpan   = new TimeSpan(0, 60, 0);

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
        public virtual EncryptedAssertion Encrypt(
            ClaimsPrincipal principal,
            string   assertionIssuer,
            string   assertionConsumer,
            string   inResponseTo,
            string   sessionIndex,
            TimeSpan tokenLifeSpan,
            Org.BouncyCastle.X509.X509Certificate serverPublicKey,
            AsymmetricKeyParameter                serverPrivateKey,
            AsymmetricKeyParameter                clientPublicKey
            )
        {
            this.ValidatePrincipal(principal);

            var assertion                  = this.CreateEmptyAssertion();
            var requiredParameters         = new RequiredParameters();

            requiredParameters.Principal   = principal;

            requiredParameters.AlgorithmID   = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(KwAesAlgorithm));
            requiredParameters.PartyUInfo    = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(assertionIssuer));
            requiredParameters.PartyVInfo    = Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(Encoding.UTF8.GetBytes(assertionConsumer));

            requiredParameters.DigestMethodString  = KdfDigestAlgorithm.Substring(KdfDigestAlgorithm.IndexOf("#") + 1);
            requiredParameters.NamedCurveOid       = ECNamedCurveURN.Replace("urn:oid:", "");
            requiredParameters.KeyEncryptionMethod = KwAesAlgorithm;

            // fill-in initial data
            this.WriteDerivedKeySection(assertion, requiredParameters);

            // compute kw-aes public key from the client's public certificate
            ECPoint serverKeyPoint = this.ComputePublicKeyPoint(serverPublicKey);
            ECPoint clientKeyPoint = this.ComputePublicKeyPoint(clientPublicKey);

            this.WritePublicKeyPoint(assertion, serverKeyPoint);

            // encrypt and write session key
            byte[] rawSessionKey;
            byte[] sessionKey = this.EncryptSessionKey(requiredParameters, clientKeyPoint, serverPrivateKey, out rawSessionKey);
            this.WriteSessionKey(assertion, rawSessionKey);

            byte[] encryptedData = this.EncryptPrincipal(
                sessionKey, 
                principal, assertionIssuer, assertionConsumer,
                inResponseTo, sessionIndex,
                tokenLifeSpan );
            this.WriteEncryptedPrincipal(assertion, encryptedData);

            return assertion;
        }

        #region Validation

        /// <summary>
        /// Check if principal has all required claims
        /// </summary>
        /// <param name="principal"></param>
        protected virtual void ValidatePrincipal(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentException("Principal must not be empty");

            foreach ( var claimType in 
                new []
                {
                    ClaimTypes.Name,
                    Eidas.FirstNameClaim,
                    Eidas.FamilyNameClaim,
                    Eidas.DateOfBirthClaim,
                    Eidas.PersonIdentifierClaim,
                })
            {
                if ( !principal.HasClaim( p => p.Type == claimType ) )
                {
                    throw new ArgumentException($"Principal is missing the ${claimType} claim");
                }
            }
        }

        #endregion

        #region Computations

        protected virtual byte[] EncryptPrincipal(
            byte[]   sessionKey, 
            ClaimsPrincipal principal,
            string   assertionIssuer,
            string   assertionConsumer,
            string   inResponseTo,
            string   sessionIndex,
            TimeSpan tokenLifeSpan)
        {

            EncryptionService es = new EncryptionService(256, 128, 96);
            string assertionText = this.CreatePrincipalAssertionXml(
                principal, 
                assertionIssuer, assertionConsumer,
                inResponseTo, sessionIndex, tokenLifeSpan
                );
            byte[] encryptedDoc  = es.EncryptWithKey(Encoding.UTF8.GetBytes(assertionText), sessionKey);

            return encryptedDoc;
        }

        private ECPoint ComputePublicKeyPoint(
            Org.BouncyCastle.X509.X509Certificate encryptionKey
            )
        {
            // compute public key
            ECPoint ecPoint = (encryptionKey.GetPublicKey() as ECPublicKeyParameters).Q;
            ecPoint.Normalize();

            return ecPoint;
        }

        private ECPoint ComputePublicKeyPoint(
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter encryptionKey
        )
        {
            // compute public key
            ECPoint ecPoint = (encryptionKey as ECPublicKeyParameters).Q;
            ecPoint.Normalize();

            return ecPoint;
        }

        protected virtual string CreatePrincipalAssertionXml(
            ClaimsPrincipal principal,
            string   assertionIssuer,
            string   assertionConsumer,
            string   inResponseTo,
            string   sessionIndex,
            TimeSpan tokenLifeSpan)
        {
            DateTime _now           = DateTime.UtcNow;
            DateTime _tokenLifeSpan = _now.Add(tokenLifeSpan != null ? tokenLifeSpan : this.DefaultTokenLifeSpan);

            Assertion assertion = new Assertion()
            {
                ID           = $"_ID{Guid.NewGuid()}",
                Version      = "2.0",
                IssueInstant = _now,
                Issuer       = assertionIssuer,
                Subject      = new Subject()
                {
                    NameID = new Model.NameID()
                    {
                        Text   = principal.Identity.Name,
                        Format = Constants.NameID.UNSPECIFIED
                    },
                    SubjectConfirmation = new Model.SubjectConfirmation()
                    {
                        Method = Client.Constants.SubjectConfirmation.BEARER,
                        SubjectConfirmationData = new SubjectConfirmationData()
                        {
                            InResponseTo = inResponseTo,
                            NotOnOrAfter = _tokenLifeSpan,
                            Recipient    = assertionConsumer
                        }
                    },
                },
                Conditions = new Conditions()
                {
                    NotBefore           = _now,
                    NotOnOrAfter        = _tokenLifeSpan,
                    AudienceRestriction = new[]
                    {
                        new AudienceRestriction()
                        {
                            Audience = new []
                            {
                                assertionConsumer
                            }
                        }
                    }
                },
                AuthnStatement = new AuthnStatement()
                {
                    AuthnInstant = _now,
                    SessionIndex = sessionIndex,
                    AuthnContext = new AuthnContext()
                    {
                        AuthnConextClassRef     = Eidas.LOA_SUBSTANTIAL,
                        AuthenticatingAuthority = assertionIssuer
                    }
                },
                AttributeStatement = new AttributeStatement()
                {
                    Attributes = new[]
                    {
                        new Model.Attribute()
                        {
                            FriendlyName   = Eidas.FamilyName,
                            Name           = Eidas.FamilyNameClaim,
                            NameFormat     = Constants.NameID.URI,
                            AttributeValue = new []
                            {
                                principal.FindFirst( c => c.Type == Eidas.FamilyNameClaim ).Value
                            }
                        },
                        new Model.Attribute()
                        {
                            FriendlyName   = Eidas.DateOfBirth,
                            Name           = Eidas.DateOfBirthClaim,
                            NameFormat     = Constants.NameID.URI,
                            AttributeValue = new []
                            {
                                principal.FindFirst( c => c.Type == Eidas.DateOfBirthClaim ).Value
                            }
                        },
                        new Model.Attribute()
                        {
                            FriendlyName   = Eidas.FirstName,
                            Name           = Eidas.FirstNameClaim,
                            NameFormat     = Constants.NameID.URI,
                            AttributeValue = new []
                            {
                                principal.FindFirst( c => c.Type == Eidas.FirstNameClaim ).Value
                            }
                        },
                        new Model.Attribute()
                        {
                            FriendlyName   = Eidas.PersonIdentifier,
                            Name           = Eidas.PersonIdentifierClaim,
                            NameFormat     = Constants.NameID.URI,
                            AttributeValue = new []
                            {
                                principal.FindFirst( c => c.Type == Eidas.PersonIdentifierClaim ).Value
                            }
                        }
                    }
                }
            };

            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                var xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Encoding = Encoding.UTF8;
                xmlWriterSettings.OmitXmlDeclaration = true;

                using (var xmlWriter = XmlWriter.Create(writer, xmlWriterSettings))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Assertion), Namespaces.ASSERTION);

                    xs.Serialize(xmlWriter, assertion);
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Structure

        protected virtual void WriteEncryptedPrincipal(
            EncryptedAssertion assertion,
            byte[] encryptedData
            )
        {
            // key
            var encryptedDataSection = assertion.EncryptedData;

            encryptedDataSection.CipherData.CipherValue.Text = Convert.ToBase64String(encryptedData);
        }

        protected virtual void WriteDerivedKeySection(
            EncryptedAssertion assertion,
            RequiredParameters requiredParameters
        )
        {
            // key
            var encryptedKey = assertion.EncryptedData.KeyInfo.EncryptedKey;

            // fill the information in the XML
            var concatKDFParams = encryptedKey.KeyInfo.AgreementMethod.KeyDerivationMethod.ConcatKDFParams;
            concatKDFParams.AlgorithmID = requiredParameters.AlgorithmID;
            concatKDFParams.PartyUInfo = requiredParameters.PartyUInfo;
            concatKDFParams.PartyVInfo = requiredParameters.PartyVInfo;
        }

        protected virtual void WritePublicKeyPoint(
            EncryptedAssertion assertion,
            ECPoint ecPoint)
        {
            // key
            var encryptedKey = assertion.EncryptedData.KeyInfo.EncryptedKey;

            // fill
            encryptedKey.KeyInfo.AgreementMethod.OriginatorKeyInfo.KeyValue.ECKeyValue.PublicKey.Text = Convert.ToBase64String(ecPoint.GetEncoded());
        }

        protected virtual void WriteSessionKey(
            EncryptedAssertion assertion,
            byte[] sessionKey
            )
        {
            // key
            var encryptedKeySection                         = assertion.EncryptedData.KeyInfo.EncryptedKey;
            encryptedKeySection.CipherData.CipherValue.Text = Convert.ToBase64String(sessionKey);
        }

        #endregion

        protected virtual byte[] EncryptSessionKey(
            RequiredParameters     requiredParameters,
            ECPoint                clientPublicKey,
            AsymmetricKeyParameter serverPrivateKey,
            out byte[]             rawSessionKey
            )
        {
            // clients public key ECC parameters
            ECDomainParameters ecNamedCurveParameterSpec = GetCurveParameters(requiredParameters.NamedCurveOid);
            AsymmetricKeyParameter ecPublicKey = new ECPublicKeyParameters(clientPublicKey, ecNamedCurveParameterSpec);

            // KeyAgreement
            IBasicAgreement keyAgreement = AgreementUtilities.GetBasicAgreement("ECDH");
            keyAgreement.Init(serverPrivateKey);

            byte[] sharedSecret  = keyAgreement.CalculateAgreement(ecPublicKey).ToByteArrayUnsigned();
            IDigest digestMethod = DigestUtilities.GetDigest(requiredParameters.DigestMethodString);

            // executing the KDF
            byte[] wrappedKeyBytes = this.DeriveKey(requiredParameters, sharedSecret, digestMethod);

            // encrypting the key
            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", wrappedKeyBytes);
            rawSessionKey             = Org.BouncyCastle.Crypto.Xml.EncryptedXml.EncryptKey(wrappedKeyBytes, keyParameter);

            return wrappedKeyBytes;
        }
    }
}
