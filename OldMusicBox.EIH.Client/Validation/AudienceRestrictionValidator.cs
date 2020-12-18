using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Validation
{
    /// <summary>
    /// Audience restriction validator
    /// </summary>
    public class AudienceRestrictionValidator : ISaml2TokenValidator
    {
        public void Validate(Saml2SecurityToken token, SecurityTokenHandlerConfiguration configuration)
        {
            if (token == null || token.Response == null || configuration == null) throw new ArgumentNullException();

            if (token.Response.Assertions != null)
            {
                foreach (var assertion in token.Response.Assertions)
                {
                    if (assertion.Conditions != null)
                    {
                        if (configuration.AudienceRestriction.AudienceMode == AudienceUriMode.Always)
                        {
                            if (configuration.AudienceRestriction.AllowedAudienceUris.Count() == 0)
                                throw new ValidationException("AudienceRestriction's allowed uris must be provided if AudienceMode is set to Always. Set to Never to suppress this.");

                            if (assertion.Conditions.AudienceRestriction.Count() == 0)
                                throw new ValidationException("Token doesn't contain any audience to validate the configuration against");

                            var security = new SamlSecurityTokenRequirement();
                            foreach (var restriction in assertion.Conditions.AudienceRestriction)
                                security.ValidateAudienceRestriction(configuration.AudienceRestriction.AllowedAudienceUris, restriction.Audience.Select( a => new Uri(a)).ToList());
                        }
                        if (configuration.AudienceRestriction.AudienceMode == AudienceUriMode.BearerKeyOnly)
                        {
#warning TODO!
                        }

                    }
                }
            }
        }
    }
}
