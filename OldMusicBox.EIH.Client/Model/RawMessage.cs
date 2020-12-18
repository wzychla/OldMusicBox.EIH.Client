using System;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Raw message 
    /// </summary>
    public class RawMessage
    {
        /// <summary>
        /// Payload
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// Signature (if detached)
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// RelayState
        /// </summary>
        public string RelayState { get; set; }
    }
}
