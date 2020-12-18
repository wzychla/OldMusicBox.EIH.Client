using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Validation
{
    /// <summary>
    /// A class that validates some aspect of the token against given configuration
    /// </summary>
    public interface ISaml2TokenValidator
    {
        void Validate(Saml2SecurityToken token, SecurityTokenHandlerConfiguration configuration);
    }
}
