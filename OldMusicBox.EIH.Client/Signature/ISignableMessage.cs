using OldMusicBox.EIH.Client.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Signature
{
    /// <summary>
    /// Interface that marks signable messages
    /// </summary>
    public interface ISignableMessage : ISerializableMessage
    {
        string ID { get; set; }
    }
}
