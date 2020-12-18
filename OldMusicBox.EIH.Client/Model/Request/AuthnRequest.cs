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
    /// SAML2 AuthnRequest model
    /// </summary>
    [XmlRoot("AuthnRequest", Namespace=Namespaces.PROTOCOL)]
    public class AuthnRequest : 
        ISignableMessage
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("IssueInstant")]
        public DateTime IssueInstant { get; set; }
        [XmlAttribute("ForceAuthn")]
        public bool ForceAuthn { get; set; }

        public bool ShouldSerializeForceAuthn()
        {
            return ForceAuthn == true;
        }

        [XmlAttribute("AssertionConsumerServiceURL")]
        public string AssertionConsumerServiceURL { get; set; }

        [XmlAttribute("Destination")]
        public string Destination { get; set; }

        [XmlAttribute("ProtocolBinding")]
        public string ProtocolBinding { get; set; }

        [XmlElement("Issuer", Namespace = Namespaces.ASSERTION )]
        public string Issuer { get; set; }

        [XmlElement("Extensions", Namespace = Namespaces.PROTOCOL)]
        public Extensions Extensions { get; set; }

        [XmlElement("NameIDPolicy", Namespace = Namespaces.PROTOCOL)]
        public NameIDPolicy NameIDPolicy { get; set; }

        [XmlElement("RequestedAuthnContext", Namespace = Namespaces.PROTOCOL )]
        public RequestAuthContext RequestedAuthnContext { get; set; }
    }

    public class Extensions
    {
        [XmlElement("SPType", Namespace = Namespaces.EIDAS )]
        public string SPType { get; set; }

        [XmlArray("RequestedAttributes", Namespace = Namespaces.EIDAS)]
        public RequestedAttribute[] RequestedAttributes { get; set; }
    }

    [XmlRoot( "RequestedAttribute", Namespace = Namespaces.EIDAS )]
    public class RequestedAttribute
    {
        [XmlAttribute("FriendlyName", Namespace = "")]
        public string FriendlyName { get; set; }
        [XmlAttribute("Name", Namespace = "")]
        public string Name { get; set; }
        [XmlAttribute("NameFormat", Namespace = "")]
        public string NameFormat { get; set; }
        [XmlAttribute("isRequired", Namespace = "")]
        public bool IsRequired { get; set; }
    }

    public class NameIDPolicy
    {
        public NameIDPolicy()
        {
            this.Format = Constants.NameID.UNSPECIFIED;
        }

        [XmlAttribute("AllowCreate")]
        public bool AllowCreate { get; set; }

        [XmlAttribute("Format")]
        public string Format { get; set; }
    }

    public class RequestAuthContext
    {
        [XmlAttribute("Comparison")]
        public AuthnContextComparisonType Comparison { get; set; }

        [XmlElement("AuthnContextClassRef", Namespace = Namespaces.ASSERTION)]
        public virtual string AuthnContextClassRef { get; set; }
    }

    public enum AuthnContextComparisonType
    {
        [XmlEnum("exact")]
        Exact,
        [XmlEnum("minimum")]
        Minimum,
        [XmlEnum("maximum")]
        Maximum,
        [XmlEnum("better")]
        Better,
    }
}
