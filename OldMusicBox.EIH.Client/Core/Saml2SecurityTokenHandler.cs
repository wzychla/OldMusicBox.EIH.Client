using OldMusicBox.EIH.Client.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client
{
    /// <summary>
    /// The SAML2 security token handler
    /// </summary>
    public class Saml2SecurityTokenHandler : SecurityTokenHandler
    {
        public Saml2SecurityTokenHandler()
        {
            this.SecurityTokenRequirement = new SamlSecurityTokenRequirement();
        }

        #region Properties

        /// <summary>
        /// Can be used to configure audience restrictions
        /// </summary>
        public virtual SamlSecurityTokenRequirement SecurityTokenRequirement { get; set; }

        public override string[] GetTokenTypeIdentifiers()
        {
            // #TODO return actual token types
            return new string[0];
        }

        public override Type TokenType
        {
            get
            {
                return typeof(Saml2SecurityToken);
            }
        }

        #endregion

        #region Validation steps

        /// <summary>
        /// This will be implemented soon
        /// </summary>
        protected override void DetectReplayedToken(SecurityToken token)
        {
            if (this.Configuration.DetectReplayedTokens)
            {
                base.DetectReplayedToken(token);
            }
        }

        #endregion

        #region Claims identity creation

        protected virtual ClaimsIdentity CreateIdentity( Saml2SecurityToken token )
        {
            if ( token.Response == null || token.Response.Assertions == null )
            {
                throw new ArgumentException("SAML2 token response is empty or doesn't contain any assertions");
            }

            var identity = new ClaimsIdentity("SAML2", this.SecurityTokenRequirement.NameClaimType, this.SecurityTokenRequirement.RoleClaimType);

            // process first assertion
            var assertion = token.Response.Assertions.FirstOrDefault();
            if (assertion != null)
            {
                // name identifier (if exists)
                if ( assertion.Subject != null && assertion.Subject.NameID != null )
                {
                    var nameIdClaim = new Claim(ClaimTypes.NameIdentifier, assertion.Subject.NameID.Text);
                    identity.AddClaim(nameIdClaim);
                }
                // other claims (name, role, etc)
                if ( assertion.AttributeStatement != null && assertion.AttributeStatement.Attributes != null )
                {
                    foreach ( var attribute in assertion.AttributeStatement.Attributes )
                    {
                        if (attribute.AttributeValue != null)
                        {
                            foreach (var attributeValue in attribute.AttributeValue)
                            {
                                var attributeClaim = new Claim(attribute.Name, attributeValue);
                                identity.AddClaim(attributeClaim);
                            }
                        }
                    }
                }
            }

            return identity;
        }

        #endregion

        /// <summary>
        /// Creates identities out of validated claim
        /// </summary>
        /// <remarks>
        /// Claim validation consists in several steps:
        /// * status code validation
        /// * replayed token detection
        /// * audience restriction checking
        /// * expiration checking
        /// * signature checking
        /// * accepting/rejecting of the certificate 
        /// 
        /// Each steps throws if something is wrong
        /// </remarks>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            var saml2Token = token as Saml2SecurityToken;

            if ( saml2Token == null )
            {
                throw new ArgumentException("The Saml2SecurityTokenHandler can only be used to validate SAML2 tokens");
            }
            if ( this.Configuration == null )
            {
                throw new ArgumentNullException("Configuration");
            }

            // validation
            this.DetectReplayedToken(token);

            var validators = new ISaml2TokenValidator[]
            {
                new StatusCodeValidator(),
                new AudienceRestrictionValidator(),
                new TimeWindowValidator(),
                // new SignatureValidator(),
                new CertifcateValidator(),
            };

            foreach ( var validator in validators )
            {
                validator.Validate(saml2Token, this.Configuration);
            }

            // if this is reached, token is validated correctly

            var identity = this.CreateIdentity(saml2Token);

            return new ReadOnlyCollection<ClaimsIdentity>(new[] { identity });
        }
    }
}
