using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace ERAAuthentication.SRP6
{
    internal class SRPVerification : SRPPacketData
    {
        public Byte[] M;
        public Byte[] M2
        {
            get { return M; }
            set { M = value; }
        }

        public SRPVerification()
        {

        }

        /// <summary>
        /// Creates a new SRPVerification
        /// </summary>
        /// <param name="M">Verification Value (M or M2)</param>
        public SRPVerification(Byte[] M)
        {
            this.M = M;
        }

        protected override void Puts(NetOutgoingMessage message)
        {
            message.Write(new NetBigInteger(M).ToString());
        }

        protected override void Gets(NetIncomingMessage message)
        {
            M = new NetBigInteger(message.ReadString()).ToByteArray();
        }
    }
}
