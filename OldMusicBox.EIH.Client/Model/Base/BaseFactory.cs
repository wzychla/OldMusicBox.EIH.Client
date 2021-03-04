using OldMusicBox.EIH.Client.Serialization;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Base class for factories
    /// </summary>
    public abstract class BaseFactory
    {
        public BaseFactory()
        {
            this.MessageSerializer = new DefaultMessageSerializer();
            this.MessageSigner     = new DefaultMessageSigner(this.MessageSerializer);
            this.Encoding          = Encoding.UTF8;
        }

        /// <summary>
        /// Message serializer
        /// </summary>
        public IMessageSerializer MessageSerializer { get; set; }

        /// <summary>
        /// Message signer
        /// </summary>
        public IMessageSigner MessageSigner { get; set; }

        public Encoding Encoding { get; set; }
    }
}
