using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Validation
{
    /// <summary>
    /// Check if a valid cert was used to sign the token.
    /// 
    /// This consists of 
    /// * checking the token's certificate against the issuer name registry
    /// * checking the certificate's validity/chain/etc (if necessary)
    /// </summary>
    public class CertifcateValidator : ISaml2TokenValidator
    {
        public void Validate(Saml2SecurityToken token, SecurityTokenHandlerConfiguration configuration)
        {
            if (token == null || token.Response == null || 
                configuration == null) throw new ArgumentNullException();
            if (configuration.IssuerNameRegistry == null) throw new ArgumentNullException("configuration", "Issuer name registry cannot empty in the configuration");

            if ( token.Response.Assertions != null )
            {
                foreach ( var assertion in token.Response.Assertions )
                {
                    var securityToken = assertion.GetX509SecurityToken();
                    if ( securityToken != null )
                    {
                        var issuer = configuration.IssuerNameRegistry.GetIssuerName(securityToken);
                        if ( string.IsNullOrEmpty( issuer ) )
                        {
                            throw new ValidationException("Issuer name registry doesn't recognize token's certificate");
                        }
                    }
                }
            }
        }
    }
}
