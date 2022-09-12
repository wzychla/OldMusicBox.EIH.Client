using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Logging
{
    /// <summary>
    /// Low lever events/objects that should be trackable
    /// </summary>
    public enum Event
    {
        // raw authnrequest token
        RawAuthnRequest,
        // complete post binding page
        PostBindingPage,
        // the artifact response
        ArtifactPostResponse,
        // whatever comes as SAMLResponse
        RawResponse,
        // a signed message
        SignedMessage,
        // artifact resolve
        ArtifactResolve,
        // artifact response
        ArtifactResponse,
        // logout request
        LogoutRequest,
        // logout response
        LogoutResponse
    }
}
