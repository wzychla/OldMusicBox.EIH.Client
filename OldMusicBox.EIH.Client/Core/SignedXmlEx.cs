using Org.BouncyCastle.Crypto.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OldMusicBox.EIH.Client.Core
{
    public class SignedXmlEx : SignedXml
    {
        public SignedXmlEx() : base() { }

        public SignedXmlEx(XmlDocument doc) : base(doc) { }

        public SignedXmlEx(XmlElement el) : base(el) { }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if (string.IsNullOrEmpty(idValue))
                return null;

            var element = base.GetIdElement(document, idValue);
            if (element != null)
                return element;

            var node = FindNodeRecursive(new[] { document.DocumentElement }, "Id", idValue);
            if (node != null)
                return node;

            return null;
        }

        private XmlElement FindNodeRecursive(IEnumerable<XmlNode> nodes, string AttributeName, string AttributeValue)
        {
            if (nodes == null) return null;

            foreach (XmlNode node in nodes)
            {
                // attributes
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (string.Equals(attribute.Name, AttributeName, StringComparison.InvariantCultureIgnoreCase) &&
                         string.Equals(attribute.Value, AttributeValue, StringComparison.InvariantCultureIgnoreCase)
                        )
                    {
                        return node as XmlElement;
                    }
                }
                // recursion
                var result = FindNodeRecursive(node.ChildNodes.OfType<XmlNode>(), AttributeName, AttributeValue);
                if (result != null)
                {
                    return result;
                }

            }

            return null;
        }
    }
}

