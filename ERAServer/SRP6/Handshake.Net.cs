using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Lidgren.Network;
using ERAUtils.Logger;

namespace ERAServer.SRP6
{
    internal partial class HandShake
    {
        /// <summary>
        /// Iniates a handshake (also connects)
        /// </summary>
        /// <param name="source">Local Netpeer</param>
        /// <param name="hail">Hail message</param>
        /// <param name="destination">Remote adress</param>
        /// <param name="port">Remote port</param>
        /// <param name="identifier">Identifier</param>
        /// <param name="secret">Secret</param>
        /// <param name="server">IsServerconnection Flag</param>
        public static void InitiateHandShake(NetPeer source, ref NetOutgoingMessage hail, String destination, Int32 port, String identifier, String secret, Boolean server)
        {
            HandShake.InitiateHandShake(source, ref hail, new IPEndPoint(NetUtility.Resolve(destination), port), destination, secret, server);
        }

        /// <summary>
        /// Iniates a handshake (also connects)
        /// </summary>
        /// <param name="source">Local NetPeer</param>
        /// <param name="hail">Hail message</param>
        /// <param name="destination">Remote endpoint</param>
        /// <param name="identifier">Identifier</param>
        /// <param name="secret">Secret</param>
        /// <param name="server">IsServerconnection Flag</param>
        public static void InitiateHandShake(NetPeer source, ref NetOutgoingMessage hail, IPEndPoint destination, String identifier, String secret, Boolean server)
        {
            SRP6.HandShake handShake = new SRP6.HandShake(server, true);
            SRP6.SRPRequest handShakeRequest = handShake.GetSRPRequest(
               identifier,
               secret);

            // Hail with name and SRPRequest
            SRP6.SRPRequest.GenerateMessage(hail, handShakeRequest);

            Logger.Debug("Claire is sending SRPRequest");

            // Send over network
            NetConnection connection = source.Connect(destination, hail);

            // Save handshake
            connection.Tag = handShake;
        }

        /// <summary>
        /// Processes a handshake that was not initated locally
        /// </summary>
        /// <param name="msg">Incomming msg with handshake</param>
        /// <param name="approvalHail">Outgoing hail message</param>
        public static void HandShakeFromActive(NetIncomingMessage msg, ref NetOutgoingMessage approvalHail)
        {
            Logger.Debug("Bob received SRPRequest");

            // Read request
            SRP6.SRPRequest request = new SRP6.SRPRequest();
            request.ExtractPacketData(msg);

            // Create response
            msg.SenderConnection.Tag = new SRP6.HandShake(true, false);
            SRP6.SRPResponse response = ((SRP6.HandShake)msg.SenderConnection.Tag).ResponseFromRequest(request);

            // Respond SRPP counter part SRPP(s, B)
            SRP6.SRPResponse.GenerateMessage(approvalHail, response);

            Logger.Debug("Bob sends SRPResponse (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");
        }

        /// <summary>
        /// Processes a handshake response (initiated locally)
        /// </summary>
        /// <param name="msg">Incoming message with resonse data</param>
        /// <param name="verificationMessage">VerificationMessage to write to</param>
        public static void HandShakeFromPassive(NetIncomingMessage msg, ref NetOutgoingMessage verificationMessage)
        {
            Logger.Debug("Claire receives SRPResponse (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");

            // Get response
            SRP6.SRPResponse response = new SRP6.SRPResponse();
            response.ExtractPacketData(msg.SenderConnection.RemoteHailMessage);

            // Create Verification data
            SRP6.SRPVerification verification = ((SRP6.HandShake)msg.SenderConnection.Tag).KeyFromResponse(response);
            SRP6.SRPVerification.GenerateMessage(verificationMessage, verification);

            Logger.Debug("Claire sends SRPVerification (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");
        }

        /// <summary>
        /// Processes the verification
        /// </summary>
        /// <param name="source">Local NetPeer</param>
        /// <param name="msg">Incoming message with verification data</param>
        /// <param name="key">out: session key</param>
        public static Boolean HandShakeVerification(NetIncomingMessage msg, ref NetOutgoingMessage verificationMessage, out Byte[] key)
        {
            Logger.Debug("??? receives SRPVerification (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");

            // Get verification data
            SRP6.SRPVerification verification = new SRP6.SRPVerification();
            verification.ExtractPacketData(msg);

            // Get encryption key
            key = ((SRP6.HandShake)msg.SenderConnection.Tag).VerifyData(ref verification);

            // Respond with your verification if needed
            if (!verification.IsMessageGenerated)
            {
                SRP6.SRPVerification.GenerateMessage(verificationMessage, verification);

                Logger.Debug("Bob sends SRPVerification (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");

                return true;
            }

            return false;
        }
    }
}
