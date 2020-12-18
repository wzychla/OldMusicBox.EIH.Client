using OldMusicBox.EIH.Client.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// XmlDsig basic model
    /// </summary>
    public class Signature
    {
        [XmlElement("KeyInfo", Namespace = Namespaces.XMLDSIG)]
        public KeyInfo KeyInfo { get; set; }
    }

    public class KeyInfo
    {
        [XmlElement("X509Data", Namespace = Namespaces.XMLDSIG)]
        public X509Data X509Data { get; set; }

        [XmlElement("EncryptedKey", Namespace = Namespaces.XMLENC)]
        public EncryptedKey EncryptedKey { get; set; }

        [XmlElement("AgreementMethod", Namespace = Namespaces.XMLENC)]
        public AgreementMethod AgreementMethod { get; set; }
    }

    public class X509Certificate
    {
        [XmlText]
        public virtual string Text { get; set; }
    }

    public class X509Data
    {
        [XmlElement("X509Certificate", Namespace = Namespaces.XMLDSIG)]
        public X509Certificate Certificate { get; set; }
    }

    public class EncryptedKey
    { 
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlElement("EncryptionMethod", Namespace = Namespaces.XMLENC)]
        public EncryptionMethod EncryptionMethod { get; set; }

        [XmlElement("KeyInfo", Namespace = Namespaces.XMLDSIG)]
        public KeyInfo KeyInfo { get; set; }

        [XmlElement("CipherData", Namespace = Namespaces.XMLENC)]
        public CipherData CipherData { get; set; }
    }

    public class AgreementMethod
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }

        [XmlElement("KeyDerivationMethod", Namespace = Namespaces.XMLENC11)]
        public KeyDerivationMethod KeyDerivationMethod { get; set; }

        [XmlElement("OriginatorKeyInfo", Namespace = Namespaces.XMLENC)]
        public OriginatorKeyInfo OriginatorKeyInfo { get; set; }
    }

    public class KeyDerivationMethod
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }

        [XmlElement("ConcatKDFParams", Namespace = Namespaces.XMLENC11)]
        public ConcatKDFParams ConcatKDFParams { get; set; }
    }

    public class ConcatKDFParams
    {
        [XmlAttribute("AlgorithmID")]
        public string AlgorithmID { get; set; }

        [XmlAttribute("PartyUInfo")]
        public string PartyUInfo { get; set; }

        [XmlAttribute("PartyVInfo")]
        public string PartyVInfo { get; set; }

        [XmlElement("DigestMethod", Namespace = Namespaces.XMLDSIG)]
        public DigestMethod DigestMethod { get; set; }
    }

    public class OriginatorKeyInfo
    {
        [XmlElement("KeyValue", Namespace = Namespaces.XMLDSIG)]
        public KeyValue KeyValue { get; set; }
    }

    public class KeyValue
    {
        [XmlElement("ECKeyValue", Namespace = Namespaces.XMLDSIG11)]
        public ECKeyValue ECKeyValue { get; set; }
    }

    public class ECKeyValue
    {
        [XmlElement("NamedCurve", Namespace = Namespaces.XMLDSIG11)]
        public NamedCurve NamedCurve { get; set; }

        [XmlElement("PublicKey", Namespace = Namespaces.XMLDSIG11)]
        public PublicKey PublicKey { get; set; }
    }

    public class NamedCurve
    {
        [XmlAttribute("URI")]
        public string URI { get; set; }
    }

    public class PublicKey
    {
        [XmlText]
        public string Text { get; set; }
    }

}

                                
