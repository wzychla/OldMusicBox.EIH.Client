using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Validation
{
    /// <summary>
    /// Validate token dates
    /// </summary>
    public class TimeWindowValidator : ISaml2TokenValidator
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
                        ValidateNotBefore(assertion.Conditions.NotBefore, configuration.MaxClockSkew);
                        ValidateNotOnOrAfter(assertion.Conditions.NotOnOrAfter, configuration.MaxClockSkew);
                    }
                    if (
                         assertion.Subject != null && 
                         assertion.Subject.SubjectConfirmation != null &&
                         assertion.Subject.SubjectConfirmation.SubjectConfirmationData != null
                        )
                    {
                        ValidateNotBefore(assertion.Subject.SubjectConfirmation.SubjectConfirmationData.NotBefore, configuration.MaxClockSkew);
                        ValidateNotOnOrAfter(assertion.Subject.SubjectConfirmation.SubjectConfirmationData.NotOnOrAfter, configuration.MaxClockSkew);
                    }
                }
            }
        }

        private void ValidateNotBefore(DateTime notBefore, TimeSpan maxClockSkew)
        {
            var now = DateTime.UtcNow;

            if ( notBefore != DateTime.MinValue &&
                 notBefore != DateTime.MaxValue &&
                 now + maxClockSkew < notBefore
                )
            {
                throw new ValidationException(string.Format("Token condition not yet valid. Now: {0}, not before: {1}", now, notBefore ));
            }

        }

        private void ValidateNotOnOrAfter(DateTime notOnOrAfter, TimeSpan maxClockSkew)
        {
            var now = DateTime.UtcNow;

            if ( notOnOrAfter != DateTime.MinValue &&
                 notOnOrAfter != DateTime.MaxValue &&
                 notOnOrAfter + maxClockSkew <= now
                )
            {
                throw new ValidationException(string.Format("Token condition already not valid. Now: {0}, not on or after: {1}", now, notOnOrAfter));
            }
        }
    }
}
