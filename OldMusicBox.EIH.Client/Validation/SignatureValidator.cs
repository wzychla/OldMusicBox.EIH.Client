using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Core;
using Org.BouncyCastle.Crypto.Xml;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OldMusicBox.EIH.Client.Validation
{
    /// <summary>
    /// Validate token's signature
    /// </summary>
    public class SignatureValidator : ISaml2TokenValidator
    {
        public void Validate(Saml2SecurityToken token, SecurityTokenHandlerConfiguration configuration)
        {
            if (token == null || token.RawResponse == null ||
                configuration == null) throw new ArgumentNullException();

            // search for signatures (possibly multiple)
            var signatureNodes = token.RawResponse.GetElementsByTagName("Signature", Namespaces.XMLDSIG);
            foreach ( XmlElement signatureNode in signatureNodes )
            {
                var signedXml = new SignedXml(signatureNode.ParentNode as XmlElement);
                signedXml.LoadXml(signatureNode);
                signedXml.SafeCanonicalizationMethods.Add("http://www.w3.org/TR/1999/REC-xpath-19991116");

                var result = signedXml.CheckSignature();

                if ( !result )
                {
                    throw new ValidationException("Token's signature validation failed");
                }
            }
        }
    }
}
