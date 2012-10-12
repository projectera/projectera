using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using ERAServer.Protocols.Server.Constants;
using ERAUtils.Logger;

namespace ERAServer.SRP6
{
    internal abstract class SRPPacketData
    {
        private Boolean _marked;
        private Boolean _readOnly;

        /// <summary>
        /// When a message is generated, it is marked in 
        /// the request object, so that you know request is going on
        /// </summary>
        public Boolean IsMessageGenerated
        {
            get { return _marked; }
        }

        /// <summary>
        /// When a message is generated from extracted data
        /// no message may be generated from this data
        /// </summary>
        public Boolean IsReadOnly
        {
            get { return _readOnly; }
        }

        /// <summary>
        /// Mark that a message was generated
        /// </summary>
        private void Mark()
        {
            _marked = true;
        }

        

        /// <summary>
        /// Generates a message from a request
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Message containing request</returns>
        public static NetOutgoingMessage GenerateMessage(NetOutgoingMessage result, SRPPacketData data)
        {
            if (data.IsReadOnly)
                throw new SRP6.SRPException("Can not generate a message from a readonly packet");

            Logger.Verbose(data.GetType().Name);
            Logger.Verbose("length before writing: " + result.LengthBits);
            // check for expiration time
            data.Puts(result);
            Logger.Verbose("length after writing: " + result.LengthBits);

            data.Mark();

            return result;
        }

        /// <summary>
        /// Extracts the data from a message
        /// </summary>
        /// <param name="message">message packed with data</param>
        public void ExtractPacketData(NetIncomingMessage message)
        {
            try
            {
                Logger.Verbose(this.GetType().Name);
                Logger.Verbose("position before reading: " + message.Position);
                Gets(message);
                Logger.Verbose("position after reading: " + message.Position);
            }
            catch (NetException)
            {
                CorruptPackage();
            }
            catch (ArgumentOutOfRangeException)
            {
                CorruptPackage();
            }
            catch (IndexOutOfRangeException)
            {
                CorruptPackage();
            }

            _readOnly = true;
        }

        private void CorruptPackage()
        {
            throw new HandShakeException("Received data was corrupt", new SRPException(this.GetType().Name));
        }

        protected abstract void Puts(NetOutgoingMessage message);
        protected abstract void Gets(NetIncomingMessage message);
    }
}
