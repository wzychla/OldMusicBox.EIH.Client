using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Serialization;
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
    /// Saml2 Response model
    /// </summary>
    [XmlRoot("Response", Namespace = Namespaces.PROTOCOL)]
    public class Response
        : ISignableMessage
    {
        [XmlIgnore]
        public Assertion[] Assertions
        {
            get
            {
                if ( this.UnencryptedAssertions != null )
                {
                    return this.UnencryptedAssertions;
                }
                else
                {
                    return new Assertion[0];
                }
            }
        }

        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("IssueInstant")]
        public DateTime IssueInstant { get; set; }

        [XmlAttribute("Destination")]
        public string Destination { get; set; }

        [XmlAttribute("Consent")]
        public string Consent { get; set; }

        [XmlAttribute("InResponseTo")]
        public string InResponseTo { get; set; }

        [XmlElement("Issuer", Namespace = Namespaces.ASSERTION)]
        public string Issuer { get; set; }

        [XmlElement("Status", Namespace = Namespaces.PROTOCOL)]
        public Status Status { get; set; }

        [XmlElement("Assertion", Namespace = Namespaces.ASSERTION)]
        public Assertion[] UnencryptedAssertions { get; set; }

        [XmlElement("EncryptedAssertion", Namespace = Namespaces.ASSERTION)]
        public EncryptedAssertion[] EncryptedAssertion { get; set; }
    }
}
