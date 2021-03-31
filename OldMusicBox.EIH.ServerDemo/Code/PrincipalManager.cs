using OldMusicBox.EIH.Client.Constants;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace OldMusicBox.EIH.ServerDemo
{
    public class PrincipalManager
    {
        const string SERVICE_PROVIDER = "http://oldmusicbox/2021/02/serviceprovider";
        const string LOGOUT_INVOKER   = "http://oldmusicbox/2021/02/logoutinvoker";
        const string PESEL_CLAIM      = "http://oldmusicbox/2021/02/pesel";
        const string SESSION_INDEX    = "http://oldmusicbox/2021/02/sessionindex";

        private SessionAuthenticationModule SAM
        {
            get
            {
                return FederatedAuthentication.SessionAuthenticationModule;
            }
        }

        /// <summary>
        /// Creates SAM cookie
        /// </summary>
        /// <param name="name"></param>
        public ClaimsPrincipal CreateSessionPrincipal(
            string name,
            string pesel,
            string givenName,
            string surname,
            DateTime dateOfBirth,
            string sessionIndex)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();

            var identity = new ClaimsIdentity("federated");

            identity.AddClaim(new Claim(ClaimTypes.Name, name));
            identity.AddClaim(new Claim(Eidas.FamilyNameClaim, surname));
            identity.AddClaim(new Claim(Eidas.FirstNameClaim, givenName));
            identity.AddClaim(new Claim(Eidas.PersonIdentifierClaim, pesel));
            identity.AddClaim(new Claim(Eidas.DateOfBirthClaim, dateOfBirth.ToString("d")));
            identity.AddClaim(new Claim(SESSION_INDEX, sessionIndex));

            var principal = new ClaimsPrincipal(identity);

            var token = this.SAM.CreateSessionSecurityToken(principal, string.Empty,
                                DateTime.UtcNow, DateTime.UtcNow.AddMinutes(45), false);

            this.SAM.WriteSessionTokenToCookie(token);

            return principal;
        }

        public string GetSessionIndexFromPrincipal(IPrincipal principal)
        {
            if (principal is ClaimsPrincipal)
            {
                var identity = principal.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    var claim = identity.FindFirst(c => c.Type == SESSION_INDEX);
                    if (claim != null)
                        return claim.Value;
                }
            }

            return null;
        }
    }
}