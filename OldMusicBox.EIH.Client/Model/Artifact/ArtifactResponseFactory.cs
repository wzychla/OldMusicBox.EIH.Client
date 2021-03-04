using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// ArtifactResponse factory
    /// </summary>
    public class ArtifactResponseFactory : BaseFactory
    {
        public ArtifactResponse From( ArtifactResolve res, ClaimsPrincipal principal, string issuer )
        {
            if ( res == null || principal == null )
            {
                throw new ArgumentNullException();
            }

            var response = new ArtifactResponse();

            response.ID           = "_" + Guid.NewGuid();
            response.Version      = "2.0";
            response.IssueInstant = DateTime.UtcNow;
            response.Consent      = Constants.Consent.UNSPECIFIED;

            response.InResponseTo = res.ID;
            response.Issuer       = issuer;

            response.Status       = Status.Success;

            return response;
        }
    }
}
