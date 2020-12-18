using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Serialization
{
    /// <summary>
    /// Generic entity serializer/deserializer that follows Saml2 specs
    /// </summary>
    public interface IMessageSerializer
    {
        string Serialize(ISerializableMessage entity, MessageSerializationParameters parameters );
        T Deserialize<T>(string input, MessageDeserializationParameters parameters) where T : class, ISerializableMessage;
    }

    public class MessageSerializationParameters
    {
        public MessageSerializationParameters() { }

        public MessageSerializationParameters(
            bool shouldBase64Encode,
            bool shouldDeflate
            )
        {
            this.ShouldBase64Encode = shouldBase64Encode;
            this.ShouldDeflate      = shouldDeflate;
        }

        public bool ShouldBase64Encode { get; set; }
        public bool ShouldDeflate { get; set; }
    }

    public class MessageDeserializationParameters
    {
        public MessageDeserializationParameters() { }

        public MessageDeserializationParameters(
            bool shouldDebase64Encode,
            bool shouldInflate
            )
        {
            this.ShouldDebase64Encode = shouldDebase64Encode;
            this.ShouldInflate        = shouldInflate;
        }

        public bool ShouldDebase64Encode { get; set; }
        public bool ShouldInflate { get; set; }
    }
}
