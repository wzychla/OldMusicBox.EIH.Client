using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Model
{ 
    /// <summary>
    /// Artifact response
    /// </summary>
    [XmlRoot("ArtifactResponse", Namespace = Namespaces.PROTOCOL)]
    public class ArtifactResponse : ISerializableMessage
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("IssueInstant")]
        public DateTime IssueInstant { get; set; }

        [XmlAttribute("Consent")]
        public string Consent { get; set; }

        [XmlAttribute("InResponseTo")]
        public string InResponseTo { get; set; }

        [XmlElement("Issuer", Namespace = Namespaces.ASSERTION)]
        public string Issuer { get; set; }

        [XmlElement("Status", Namespace = Namespaces.PROTOCOL)]
        public Status Status { get; set; }

        [XmlAnyElement("Response", Namespace = Namespaces.PROTOCOL)]
        public XmlElement Response { get; set; }
    }
}
