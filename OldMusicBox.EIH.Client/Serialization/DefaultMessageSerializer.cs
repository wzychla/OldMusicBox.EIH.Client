using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Logging;
using OldMusicBox.EIH.Client.Signature;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Serialization
{
    /// <summary>
    /// Default message serializer
    /// </summary>
    public class DefaultMessageSerializer
        : IMessageSerializer
    {
        public DefaultMessageSerializer() : this( Encoding.UTF8 )
        {

        }

        public DefaultMessageSerializer( Encoding encoding )
        {
            if ( encoding == null )
            {
                throw new ArgumentNullException("encoding", "Encoding cannot be null");
            }

            this.Encoding = encoding;
        }

        public Encoding Encoding { get; set; }

        public virtual T Deserialize<T>(
            string input,
            MessageDeserializationParameters parameters
            )
            where T : class, ISerializableMessage
        {
            // base64?
            var data =
                parameters.ShouldDebase64Encode
                ? Convert.FromBase64String(input)
                : Encoding.GetBytes(input);

            // inflate?
            if (parameters.ShouldInflate)
            {
                data = this.DeflateDecompress(data);
            }

            var rawString = Encoding.GetString(data);

            using (var reader = new StringReader(rawString))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var rawObject = xmlSerializer.Deserialize(reader);

                var result = rawObject as T;

                if (result is IVerifiableMessage)
                {
                    ((IVerifiableMessage)result).RawMessage = new Model.RawMessage() { Payload = rawString };
                }

                return result;
            }
        }

        /// <summary>
        /// Saml2 serialization
        /// </summary>
        public virtual string Serialize(
            ISerializableMessage entity,
            MessageSerializationParameters parameters )
        {
            byte[] serializedBytes;

            // serialize to byte[]
            using (var writer = new StringWriter())
            {
                var xmlWriterSettings                = new XmlWriterSettings();
                xmlWriterSettings.Encoding           = this.Encoding;
                xmlWriterSettings.OmitXmlDeclaration = true;

                using (var xmlWriter = XmlWriter.Create(writer, xmlWriterSettings))
                {
                    var xmlSerializer = new XmlSerializer(entity.GetType());
                    xmlSerializer.Serialize(xmlWriter, entity, Namespaces.SerializerNamespaces);
                }

                var rawEntity = writer.ToString();

                // log
                new LoggerFactory().For(this).Debug(Event.RawAuthnRequest, rawEntity);

                serializedBytes = this.Encoding.GetBytes(rawEntity);
            }

            // convert the byte[] according to parameters
            if ( parameters.ShouldDeflate )
            {
                serializedBytes = this.DeflateCompress(serializedBytes);
            }
            if ( parameters.ShouldBase64Encode )
            {
                serializedBytes = this.Encoding.GetBytes(Convert.ToBase64String(serializedBytes));
            }

            return this.Encoding.GetString(serializedBytes);
        }

        #region Implementation

        /// <summary>
        /// Deflate/inflate
        /// </summary>
        public virtual byte[] DeflateCompress(byte[] bytes)
        {
            using (var ms = new MemoryStream())
            {
                using (var deflate = new DeflateStream(ms, CompressionMode.Compress))
                {
                    deflate.Write(bytes, 0, bytes.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deflate/inflate
        /// </summary>
        public virtual byte[] DeflateDecompress(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                using (var inflate = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    using (var reader = new BinaryReader(inflate, this.Encoding))
                    {
                        return ReadAllBytes( reader );
                    }
                }
            }
        }

        private byte[] ReadAllBytes(BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }

        #endregion
    }
}
