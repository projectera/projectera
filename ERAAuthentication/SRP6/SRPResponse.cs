using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace ERAAuthentication.SRP6
{
    /// <summary>
    /// This is the Secure Remote Password Protocol Response object. Remember Claire?
    /// She has send a request to Bob, the one she want to connect to. When Bob receives
    /// Claire's username and public Key, he will send Claire her personal Salt and a 
    /// random runtime-generated public key.
    /// </summary>
    internal class SRPResponse : SRPPacketData
    {
        /// <summary>
        /// Salt
        /// </summary>
        public Byte[] Salt;

        /// <summary>
        /// Public key B
        /// </summary>
        public NetBigInteger B;

        /// <summary>
        /// Creates a new SRPResponse
        /// </summary>
        public SRPResponse()
        {
        }

        /// <summary>
        /// Creates a new SRPResponse
        /// </summary>
        /// <param name="salt">Salt</param>
        /// <param name="B">Public value</param>
        public SRPResponse(Byte[] salt, NetBigInteger B)
        {
            this.Salt = salt;
            this.B = B;
        }

        /// <summary>
        /// Puts data into message
        /// </summary>
        /// <param name="message">desination</param>
        protected override void Puts(NetOutgoingMessage message)
        {
            message.Write(B.ToString());
            message.Write(new NetBigInteger(Salt).ToString());
        }

        /// <summary>
        /// Gets data from message
        /// </summary>
        /// <param name="message">source</param>
        protected override void Gets(NetIncomingMessage message)
        {
            B = new NetBigInteger(message.ReadString());
            Salt = new NetBigInteger(message.ReadString()).ToByteArray();
        }
    }
}
