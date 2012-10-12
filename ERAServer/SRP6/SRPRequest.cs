using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using ERAServer.Protocols.Server.Constants;

namespace ERAServer.SRP6
{
    /// <summary>
    /// This is the Secure Remote Password Protocol Request object. If Claire is someone that 
    /// tries to connect, then Claire's username and a random runtime-generated public key
    /// is packed into this packet.
    /// </summary>
    internal class SRPRequest : SRPPacketData
    {
        /// <summary>
        /// Username
        /// </summary>
        public String Username;

        /// <summary>
        /// Public key A
        /// </summary>
        public NetBigInteger A;

        /// <summary>
        /// Creates a new SRPRequest
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="A">Public value</param>
        public SRPRequest(String username, NetBigInteger A)
        {
            this.Username = username;
            this.A = A;
        }

        /// <summary>
        /// Creates a new SRPRequest
        /// </summary>
        public SRPRequest()
        {   
        }

        /// <summary>
        /// Puts data into message
        /// </summary>
        /// <param name="message">desination</param>
        protected override void Puts(NetOutgoingMessage message)
        {
            message.Write(Username);
            message.Write(A.ToString());
        }

        /// <summary>
        /// Gets data from message
        /// </summary>
        /// <param name="message">source</param>
        protected override void Gets(NetIncomingMessage message)
        {
            Username = message.ReadString();
            A = new NetBigInteger(message.ReadString());
        }
    }
}
