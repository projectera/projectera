using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Lidgren.Network;
using ERAUtils.Logger;

namespace ERAAuthentication.SRP6
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
        public static void InitiateHandShake(NetPeer source, ref NetOutgoingMessage hail, String destination, Int32 port, String identifier, String secret, Int32 connectionGroup, Boolean connectionGroupIsStatic)
        {
            HandShake.InitiateHandShake(source, ref hail, new IPEndPoint(NetUtility.Resolve(destination), port), destination, secret, connectionGroup, connectionGroupIsStatic);
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
        public static void InitiateHandShake(NetPeer source, ref NetOutgoingMessage hail, IPEndPoint destination, String identifier, String secret, Int32 connectionGroup, Boolean connectionGroupIsStatic)
        {
            SRP6.HandShake handShake = new SRP6.HandShake(connectionGroup, connectionGroupIsStatic);
            SRP6.SRPRequest handShakeRequest = handShake.GetSRPRequest(
               identifier,
               secret);

            // Hail with name and SRPRequest
            SRP6.SRPRequest.GenerateMessage(hail, handShakeRequest);

            Logger.Verbose("Claire is sending SRPRequest");

            // Send over network
            NetConnection connection = source.Connect(destination, hail);

            if (connection != null)
                // Save handshake
                connection.Tag = handShake;
        }

        /// <summary>
        /// Processes a handshake that was not initated locally
        /// </summary>
        /// <param name="msg">Incomming msg with handshake</param>
        /// <param name="approvalHail">Outgoing hail message</param>
        public static void HandShakeFromActive(NetIncomingMessage msg, ref NetOutgoingMessage approvalHail, Int32 connectionGroup, Boolean connectionGroupIsStatic)
        {
            Logger.Verbose("Bob received SRPRequest");

            // Read request
            SRP6.SRPRequest request = new SRP6.SRPRequest();
            request.ExtractPacketData(msg);

            // Create response
            msg.SenderConnection.Tag = new SRP6.HandShake(connectionGroup, connectionGroupIsStatic, false);
            SRP6.SRPResponse response = ((SRP6.HandShake)msg.SenderConnection.Tag).ResponseFromRequest(request);

            // Respond SRPP counter part SRPP(s, B)
            SRP6.SRPResponse.GenerateMessage(approvalHail, response);

            Logger.Verbose("Bob sends SRPResponse (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");
        }

        /// <summary>
        /// Processes a handshake response (initiated locally)
        /// </summary>
        /// <param name="msg">Incoming message with resonse data</param>
        /// <param name="verificationMessage">VerificationMessage to write to</param>
        public static void HandShakeFromPassive(NetIncomingMessage msg, ref NetOutgoingMessage verificationMessage)
        {
            Logger.Verbose("Claire receives SRPResponse (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");

            // Get response
            SRP6.SRPResponse response = new SRP6.SRPResponse();
            response.ExtractPacketData(msg.SenderConnection.RemoteHailMessage);

            // Create Verification data
            SRP6.SRPVerification verification = ((SRP6.HandShake)msg.SenderConnection.Tag).KeyFromResponse(response);
            SRP6.SRPVerification.GenerateMessage(verificationMessage, verification);

            Logger.Verbose("Claire sends SRPVerification (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");
        }

        /// <summary>
        /// Processes the verification
        /// </summary>
        /// <param name="source">Local NetPeer</param>
        /// <param name="msg">Incoming message with verification data</param>
        /// <param name="key">out: session key</param>
        public static Boolean HandShakeVerification(NetIncomingMessage msg, ref NetOutgoingMessage verificationMessage, out Byte[] key)
        {
            Logger.Verbose("??? receives SRPVerification (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");

            // Get verification data
            SRP6.SRPVerification verification = new SRP6.SRPVerification();
            verification.ExtractPacketData(msg);

            // Get encryption key
            key = ((SRP6.HandShake)msg.SenderConnection.Tag).VerifyData(ref verification);

            // Respond with your verification if needed
            if (!verification.IsMessageGenerated)
            {
                SRP6.SRPVerification.GenerateMessage(verificationMessage, verification);

                Logger.Verbose("Bob sends SRPVerification (" + ((SRP6.HandShake)msg.SenderConnection.Tag).State + ")");

                return true;
            }

            return false;
        }
    }
}
