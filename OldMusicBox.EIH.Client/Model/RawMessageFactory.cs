using OldMusicBox.EIH.Client.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Factory
    /// </summary>
    public class RawMessageFactory
    {
        public RawMessageFactory()
        {
            this.encoding = Encoding.UTF8;
        }

        private Encoding encoding;

        /// <summary>
        /// Used when the IdP sends the SAMLResponse
        /// </summary>
        public virtual RawMessage FromIdpResponse(HttpRequestBase request)
        {
            if (request == null) throw new ArgumentNullException("request");

            // response is always base64 encoded
            var response = request.Form[Elements.SAMLRESPONSE];

            if (!string.IsNullOrEmpty(response))
            {
                var data = Convert.FromBase64String(response);
                var token = this.encoding.GetString(data);

                return new RawMessage()
                {
                    Payload = token
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Used when the IdP sends the SAMLRequest
        /// </summary>
        public virtual RawMessage FromIdpRequest(HttpRequestBase request)
        {
            if (request == null) throw new ArgumentNullException("request");

            // response is always base64 encoded
            var response = request.Form[Elements.SAMLREQUEST];

            if (!string.IsNullOrEmpty(response))
            {
                var data = Convert.FromBase64String(response);
                var token = this.encoding.GetString(data);

                return new RawMessage()
                {
                    Payload = token
                };
            }
            else
            {
                return null;
            }
        }
    }
}
