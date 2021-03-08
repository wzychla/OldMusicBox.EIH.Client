using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Encrypted Assertion Factory
    /// </summary>
    public class EncryptedAssertionFactory
    {
        public EncryptedAssertionFactory()
        {
            this.EncryptedAssertion = new EncryptedAssertion()
            {
                EncryptedData = new EncryptedData()
                {
                    Id               = Guid.NewGuid().ToString(),
                    Type             = "http://www.w3.org/2001/04/xmlenc#Element",
                    EncryptionMethod = new EncryptionMethod()
                    {
                        Algorithm    = "http://www.w3.org/2009/xmlenc11#aes256-gcm",
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
                                Algorithm = "http://www.w3.org/2001/04/xmlenc#kw-aes256",
                            },
                            KeyInfo = new KeyInfo()
                            { 
                                AgreementMethod = new AgreementMethod()
                                {
                                    Algorithm           = "http://www.w3.org/2009/xmlenc11#ECDH-ES",
                                    KeyDerivationMethod = new KeyDerivationMethod()
                                    {
                                        Algorithm       = "http://www.w3.org/2009/xmlenc11#ConcatKDF",
                                        ConcatKDFParams = new ConcatKDFParams()
                                        {
                                            AlgorithmID  = "0000002A687474703A2F2F7777772E77332E6F72672F323030312F30342F786D6C656E63236B772D616573323536",
                                            DigestMethod = new DigestMethod()
                                            { 
                                                Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256"
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

        public EncryptedAssertion EncryptedAssertion { get; set; }

        public EncryptedAssertion[] Build()
        {
            return new[] 
            { 
                this.EncryptedAssertion 
            };
        }
    }
}
