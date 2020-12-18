using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Logout request
    /// </summary>
    [XmlRoot("LogoutRequest", Namespace = Namespaces.PROTOCOL)]
    public class LogoutRequest : 
        ISignableMessage,
        IVerifiableMessage
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("IssueInstant")]
        public DateTime IssueInstant { get; set; }

        [XmlAttribute("Destination")]
        public string Destination { get; set; }

        [XmlElement("Issuer", Namespace = Namespaces.ASSERTION)]
        public string Issuer { get; set; }

        [XmlElement("NameID", Namespace = Namespaces.ASSERTION)]
        public NameID NameID { get; set; }

        [XmlElement("SessionIndex", Namespace = Namespaces.PROTOCOL)]
        public string SessionIndex { get; set; }

        /// <summary>
        /// Message source
        /// </summary>
        [XmlIgnore]
        public RawMessage RawMessage { get; set; }
    }
}
