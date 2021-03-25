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

        /// <summary>
        /// Returns a list of active SAML2 service provider sessions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetServicesFromSessionPrincipal()
        {
            SessionSecurityToken token = null;
            if (this.SAM.TryReadSessionTokenFromCookie(out token))
            {
                var principal = token.ClaimsPrincipal;
                var identity = (ClaimsIdentity)principal.Identity;

                return
                    identity
                        .FindAll(c => c.Type == SERVICE_PROVIDER)
                        .Select(c => c.Value)
                        .ToList();
            }

            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// Returns a list of active SAML2 service provider sessions
        /// </summary>
        /// <returns></returns>
        public string GetLogoutInvokerFromSessionPrincipal()
        {
            SessionSecurityToken token = null;

            if (this.SAM.TryReadSessionTokenFromCookie(out token))
            {
                var principal = token.ClaimsPrincipal;
                var identity = (ClaimsIdentity)principal.Identity;

                var claim = identity.FindFirst(c => c.Type == LOGOUT_INVOKER);
                if (claim != null)
                    return claim.Value;
                else
                    return null;
            }
            return null;
        }

        /// <summary>
        /// Returns a list of active SAML2 service provider sessions
        /// </summary>
        /// <returns></returns>
        public string GetSessionIndexFromSessionPrincipal()
        {
            SessionSecurityToken token = null;
            if (this.SAM.TryReadSessionTokenFromCookie(out token))
            {
                var principal = token.ClaimsPrincipal;

                return GetSessionIndexFromPrincipal(principal);
            }
            return null;
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

        public void StoreLogoutInvokerToSessionPrincipal(string invoker)
        {
            if (string.IsNullOrEmpty(invoker)) return;

            SessionSecurityToken token = null;
            if (this.SAM.TryReadSessionTokenFromCookie(out token))
            {
                var principal = token.ClaimsPrincipal;
                var identity = (ClaimsIdentity)principal.Identity;

                var issuerClaim = identity.Claims.FirstOrDefault(c => c.Type == LOGOUT_INVOKER);
                if (issuerClaim == null)
                {
                    // add new information to session token
                    identity.AddClaim(new Claim(LOGOUT_INVOKER, invoker));

                    var newToken = new SessionSecurityToken(
                        new ClaimsPrincipal(identity),
                        token.Context,
                        token.ValidFrom,
                        token.ValidTo);

                    this.SAM.WriteSessionTokenToCookie(newToken);
                }
            }
        }

        /// <summary>
        /// Rewrites the SAM cookie to include service provider
        /// </summary>
        public void AddServiceToSessionPrincipal(string service)
        {
            if (string.IsNullOrEmpty(service)) return;

            SessionSecurityToken token = null;
            if (this.SAM.TryReadSessionTokenFromCookie(out token))
            {
                var principal = token.ClaimsPrincipal;
                var identity = (ClaimsIdentity)principal.Identity;

                var issuerClaim = identity.Claims.FirstOrDefault(c => c.Type == SERVICE_PROVIDER && c.Value == service);
                if (issuerClaim == null)
                {
                    // add new information to session token
                    identity.AddClaim(new Claim(SERVICE_PROVIDER, service));

                    var newToken = new SessionSecurityToken(
                        new ClaimsPrincipal(identity),
                        token.Context,
                        token.ValidFrom,
                        token.ValidTo);

                    this.SAM.WriteSessionTokenToCookie(newToken);
                }
            }
        }

        /// <summary>
        /// Rewrites the SAM cookie to remove service provider
        /// </summary>
        public void RemoveServiceFromSessionPrincipal(string service)
        {
            if (string.IsNullOrEmpty(service)) return;

            SessionSecurityToken token = null;
            if (this.SAM.TryReadSessionTokenFromCookie(out token))
            {
                var principal = token.ClaimsPrincipal;
                var identity = (ClaimsIdentity)principal.Identity;

                var issuerClaim = identity.Claims.FirstOrDefault(c => c.Type == SERVICE_PROVIDER && c.Value == service);
                if (issuerClaim != null)
                {
                    // add new information to session token
                    identity.RemoveClaim(issuerClaim);

                    var newToken = new SessionSecurityToken(
                        new ClaimsPrincipal(identity),
                        token.Context,
                        token.ValidFrom,
                        token.ValidTo);

                    this.SAM.WriteSessionTokenToCookie(newToken);
                }
            }
        }

        public void DeleteSessionTokenCookie()
        {
            this.SAM.DeleteSessionTokenCookie();
        }
    }
}