using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Constants
{
    /// <summary>
    /// Saml2 namespaces
    /// </summary>
    public class Namespaces
    {
        /// <summary>
        /// Assertion namespace
        /// </summary>
        public const string ASSERTION = "urn:oasis:names:tc:SAML:2.0:assertion";

        /// <summary>
        /// Protocol namespace
        /// </summary>
        public const string PROTOCOL = "urn:oasis:names:tc:SAML:2.0:protocol";

        /// <summary>
        /// Schema instance
        /// </summary>
        public const string XMLSCHEMAINSTANCE = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// EIDAS
        /// </summary>
        public const string EIDAS = "http://eidas.europa.eu/saml-extensions";

        /// <summary>
        /// XMLENC
        /// </summary>
        public const string XMLENC   = "http://www.w3.org/2001/04/xmlenc#";
        public const string XMLENC11 = "http://www.w3.org/2009/xmlenc11#";

        /// <summary>
        /// XmlDsig namespace
        /// </summary>
        public const string XMLDSIG   = "http://www.w3.org/2000/09/xmldsig#";
        public const string XMLDSIG11 = "http://www.w3.org/2009/xmldsig11#";

        /// <summary>
        /// Natural person
        /// </summary>
        public const string NATURALPERSON = "http://eidas.europa.eu/attributes/naturalperson";

        /// <summary>
        /// In order to conform to common SAML namespacing, the serializer
        /// needs to be told how do to it
        /// </summary>
        public static XmlSerializerNamespaces SerializerNamespaces
        {
            get
            {
                var serializerNamespaces = new XmlSerializerNamespaces();

                serializerNamespaces.Add("ds", Namespaces.XMLDSIG);
                serializerNamespaces.Add("saml", Namespaces.ASSERTION);
                serializerNamespaces.Add("saml2p", Namespaces.PROTOCOL);
                serializerNamespaces.Add("eidas", Namespaces.EIDAS);
                serializerNamespaces.Add("ds", Namespaces.XMLDSIG);

                return serializerNamespaces;
            }
        }

        /// <summary>
        /// Same as above but for deserialization
        /// </summary>
        public static XmlNamespaceManager DeserializerNamespaces
        {
            get
            {
                var deserializerNamespaces = new XmlNamespaceManager(new NameTable());

                deserializerNamespaces.AddNamespace("ds", Namespaces.XMLDSIG);
                deserializerNamespaces.AddNamespace("saml", Namespaces.ASSERTION);
                deserializerNamespaces.AddNamespace("samlp", Namespaces.PROTOCOL);

                return deserializerNamespaces;
            }
        }
    }
}
