using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OldMusicBox.EIH.Client
{
    /// <summary>
    /// An auxiliary class capable of adding/extracting soap envelopes
    /// </summary>
    public static class SoapEnvelopeProcessor
    {
        /// <summary>
        /// Take a body and wrap it to form a SOAP envelope
        /// </summary>
        public static string AsEnveloped(this byte[] soapBody, Encoding encoding)
        {
            if (soapBody == null)
            {
                throw new ArgumentNullException("soapBody");
            }

            return string.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap-env:Header xmlns:soap-env=\"http://schemas.xmlsoap.org/soap/envelope/\"/><s:Body>{0}</s:Body></s:Envelope>", encoding.GetString( soapBody ));
        }

        /// <summary>
        /// Take a body and wrap it to form a SOAP envelope
        /// </summary>
        public static string AsEnveloped( this string soapBody )
        {
            if (string.IsNullOrEmpty(soapBody))
            {
                throw new ArgumentNullException("soapBody");
            }

            return string.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap-env:Header xmlns:soap-env=\"http://schemas.xmlsoap.org/soap/envelope/\"/><s:Body>{0}</s:Body></s:Envelope>", soapBody);
        }

        /// <summary>
        /// Take a SOAP envelope and return its body
        /// </summary>
        public static string FromEnveloped(this XmlDocument soapEnvelope)
        {
            if (soapEnvelope == null)
            {
                throw new ArgumentNullException("soapEnvelope");
            }

            var manager = new XmlNamespaceManager(soapEnvelope.NameTable);
            manager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");

            var body = soapEnvelope.SelectSingleNode("//s:Envelope/s:Body", manager);
            if (body == null)
            {
                throw new ArgumentException("Supplied envelope seems to be invalid");
            }

            return body.InnerXml;
        }

        /// <summary>
        /// Take a SOAP envelope and return its body
        /// </summary>
        public static string FromEnveloped( this string soapEnvelope )
        {
            if ( string.IsNullOrEmpty( soapEnvelope ) )
            {
                throw new ArgumentNullException("soapEnvelope");
            }

            var xml                = new XmlDocument();
            xml.PreserveWhitespace = true;
            xml.LoadXml(soapEnvelope);

            return FromEnveloped(xml);
        }
    }
}
