using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace OldMusicBox.EIH.ServerDemo
{
    /// <summary>
    /// In the Artifact flow of SAML2, a session index must be tied with a principal
    /// in a persistent storage so that when the client browser comes to an SSO endpoint
    /// the pair (session, principal) is stored in the storage
    /// and then when the server comes to exchange an artifact for a token, the storage can be queried.
    /// 
    /// This class serves as an example, in-memory storage
    /// 
    /// Do not use in production code as the implementation just wastes memory by keeping track of all possible sessions
    /// </summary>
    public class SessionArtifactRepository
    {
        static Dictionary<string, ClaimsPrincipal> _sessions = new Dictionary<string, ClaimsPrincipal>();

        public void StoreSessionPrincipal( string sessionIndex, ClaimsPrincipal principal )
        {
            if ( _sessions.ContainsKey( sessionIndex ) )
            {
                _sessions.Remove(sessionIndex);
            }
            _sessions.Add(sessionIndex, principal);
        }

        public ClaimsPrincipal QuerySessionPrincipal(string sessionIndex)
        {
            if ( _sessions.ContainsKey( sessionIndex ) )
            {
                return _sessions[sessionIndex];
            }
            else
            {
                return null;
            }
        }

        public bool RemoveSession( string sessionIndex )
        {
            if (_sessions.ContainsKey(sessionIndex))
            {
                return _sessions.Remove(sessionIndex);
            }
            else
            {
                return false;
            }
        }
    }
}