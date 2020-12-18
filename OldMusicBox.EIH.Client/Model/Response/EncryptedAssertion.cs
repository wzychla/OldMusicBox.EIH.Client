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
    /// Encrypted assertion
    /// </summary>
    public class EncryptedAssertion
    {
        [XmlElement("EncryptedData", Namespace = Namespaces.XMLENC)]
        public EncryptedData EncryptedData { get; set; }
    }

    public class EncryptedData
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlElement("EncryptionMethod", Namespace = Namespaces.XMLENC)]
        public EncryptionMethod EncryptionMethod { get; set; }

        [XmlElement("KeyInfo", Namespace = Namespaces.XMLDSIG)]
        public KeyInfo KeyInfo { get; set; }

        [XmlElement("CipherData", Namespace = Namespaces.XMLENC)]
        public CipherData CipherData { get; set; }
    }

    #region Encryption method

    public class EncryptionMethod
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }

        [XmlElement("DigestMethod", Namespace = Namespaces.XMLDSIG)]
        public DigestMethod DigestMethod { get; set; }
    }

    public class DigestMethod
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }
    }

    #endregion

    #region Cipher data

    public class CipherData
    {
        [XmlElement("CipherValue", Namespace = Namespaces.XMLENC)]
        public CipherValue CipherValue { get; set; }
    }

    public class CipherValue
    {
        [XmlText]
        public string Text { get; set; }
    }

    #endregion
}

