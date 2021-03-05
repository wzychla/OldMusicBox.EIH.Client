using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Serialization;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Model
{ 
    /// <summary>
    /// Artifact response
    /// </summary>
    [XmlRoot("ArtifactResponse", Namespace = Namespaces.PROTOCOL)]
    public class ArtifactResponse : 
        ISignableMessage
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
        public ArtifactResponseContent Response { get; set; }
    }

    public class ArtifactResponseContent : IXmlSerializable
    {
        public string Contents { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.Contents = reader.ReadOuterXml();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (this.Contents != null)
            {
                writer.WriteRaw(this.Contents);
            }
        }

        /// <summary>
        /// Conversion operator
        /// </summary>
        /// <param name="content"></param>
        public static implicit operator string(ArtifactResponseContent content)
        {
            if ( content != null )
            {
                return content.Contents;
            }
            else
            {
                return null;
            }
        }

        public static implicit operator ArtifactResponseContent( string content )
        {
            return new ArtifactResponseContent() { Contents = content };
        }
    }
}
