using OldMusicBox.EIH.Client.Logging;
using OldMusicBox.EIH.Client.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Factory used by a server
    /// </summary>
    public class ArtifactResolveFactory : BaseFactory
    {
        /// <summary>
        /// Create LogoutRequest when sent by the IdP
        /// </summary>
        public ArtifactResolve From(HttpRequestBase request)
        {
            if (request == null) throw new ArgumentNullException();

            // read full soap envelope as XML
            var soapEnvelope                = new XmlDocument();
            soapEnvelope.XmlResolver        = null;
            soapEnvelope.PreserveWhitespace = true;
            soapEnvelope.Load(request.InputStream);

            // retrieve the actual payload
            var rawMessage = soapEnvelope.FromEnveloped();

            // log
            new LoggerFactory().For(this).Debug(Event.ArtifactResolve, rawMessage);

            var artifactResolve        = this.MessageSerializer.Deserialize<ArtifactResolve>(rawMessage, new MessageDeserializationParameters());
            artifactResolve.RawMessage = new RawMessage() { Payload = rawMessage };
            return artifactResolve;
        }
    }
}
