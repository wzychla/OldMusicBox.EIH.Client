using OldMusicBox.EIH.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Signature
{
    /// <summary>
    /// Interface that marks verifiable messages
    /// </summary>
    public interface IVerifiableMessage
    {
        RawMessage RawMessage { get; set; }
    }
}
