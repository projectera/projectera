using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading;
using ERAAuthentication.SRP6;
using ERAUtils.Logger;
using System.Net;

namespace ProjectERA.Services.Network
{
    internal partial class NetworkManager
    {
        private const String HandShakeFailedMessage = "Handshake Failed";
        private const String HandShakeExpiredMessage = "Handshake Expired";
        private const String HandShakeCorruptedMessage = "Handshake Corrupted during state: ";
        private const String NoResponseMessage = "Failed to establish connection - no response from remote host";
        private const String TimedOutMessage = "Connection timed out";

        private TimeSpan ReleasePrematureMessageAfter = TimeSpan.FromSeconds(1);
        private TimeSpan RetryHandShakeAfter = TimeSpan.FromSeconds(2.5);

        /// <summary>
        /// 
        /// </summary>
        private void Loop()
        {
            try
            {
                while (this.IsRunning)
                {
                    NetIncomingMessage msg = _client.ReadMessage();

                    if (msg == null)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    try
                    {
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.Data:
                                // Still in handshake
                                if (msg.SenderConnection.Tag is HandShake)
                                {
                                    // Read the no protocol integer from the message.
                                    // Note: There might be a small change that a message passes even though it was early and did have
                                    // a protocol byte. However, the fallback corruption exception will then be thrown.
                                    if (msg.ReadByte() != 0 || msg.ReadByte() != 1)//(Byte)ServerProtocols.Extension || msg.ReadByte() != (Byte)ExtendedProtocols.Handshake)
                                    {
                                        Logger.Warning("Data received while not ready. Message on-hold.");

                                        // Copy message
                                        NetIncomingMessage releaseThis = msg;

                                        // Release later
                                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                                        {
                                            Thread.Sleep(ReleasePrematureMessageAfter);
                                            Logger.Debug("Prematurely received data will now be released again.");
                                            releaseThis.Position = 0;
                                            _client.ReleaseMessage(releaseThis);
                                        });
                                        continue;
                                    }

                                    msg.SkipPadBits();
                                    FinalizeVerification(msg);
                                }
                                else
                                {
                                    //((Connection)msg.SenderConnection.Tag).IncomingMessage(msg);
                                }
                                break;

                            // MESSAGETYPE: STATUS CHANGED
                            // Internal type that is triggered when a connection is initiated, responded too, connecting,
                            // disconnecting, connected or disconnected. Upon a connection, we might have received some
                            // RemoteHailData. This is part of the SRPP protocol and is proccesed accordingly. When
                            // disconnecting, the Connection is disposed, internal connection is disconnected and all is
                            // logged. 
                            case NetIncomingMessageType.StatusChanged:
                                NetConnectionStatus statusByte = (NetConnectionStatus)msg.ReadByte();
                                switch (statusByte)
                                {

                                    // When connection was approved at the other end of the pipe
                                    case NetConnectionStatus.Connected:

                                        // If approved on other side, we got a response, however, this can happen 
                                        // more than once. So, are we even doing a handshake?
                                        if (msg.SenderConnection.RemoteHailMessage != null &&
                                            msg.SenderConnection.Tag is HandShake &&
                                            ((HandShake)msg.SenderConnection.Tag).State == HandShakeState.Requested)
                                        {
                                            InitiateVerification(msg);
                                        }
                                        break;

                                    // When disconnect is called and processed
                                    case NetConnectionStatus.Disconnected:
                                        // If already connection established, destroy resources
                                        /*if (msg.SenderConnection.Tag is Connection &&
                                            !((Connection)msg.SenderConnection.Tag).IsDisposed)
                                            ((Connection)msg.SenderConnection.Tag).Dispose();*/

                                        // Received a reason for disconnecting? (e.a. Handshake Fail)
                                        String finalReason = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));

                                        // Expired Handshake or Corrupted or NoResponse (only when response was found) and I was connecting?
                                        // If Tag is null but we are verificating, must be the connecting party trying to update the tag to connection;
                                        if ((msg.SenderConnection.Tag == null && finalReason.EndsWith(HandShakeState.Verificating.ToString())) ||
                                            ((msg.SenderConnection.Tag is HandShake && ((HandShake)msg.SenderConnection.Tag).IsActiveParty) &&
                                            ((finalReason.StartsWith(HandShakeCorruptedMessage) || finalReason == HandShakeExpiredMessage) ||
                                            ((finalReason == NoResponseMessage || finalReason == TimedOutMessage) &&
                                            ((HandShake)msg.SenderConnection.Tag).State > HandShakeState.Requested))))
                                        {
                                            // Handshake passed, but expired/corrupted/timedout. Let's try again
                                            Logger.Debug("Handshake unexpectantly failed. Retrying in a while.");
                                            System.Threading.Tasks.Task.Factory.StartNew(() =>
                                            {
                                                Thread.Sleep(RetryHandShakeAfter);
                                                InitiateHandShake();

                                                Logger.Debug("Resending Handshake because ");
                                                Logger.Debug(finalReason);
                                            });
                                            break;
                                        }
                                        Logger.Debug("Disconnected from " + msg.SenderConnection.RemoteEndpoint.Address.ToString());
                                        Logger.Debug("-- reason " + finalReason);
                                        break;
                                }
                                break;
                        }
                    }
                    catch (HandShakeException)
                    {
                        // Get current state
                        HandShakeState handShakeState = HandShakeState.Failed;
                        if (msg.SenderConnection.Tag is HandShake)
                            handShakeState = ((HandShake)msg.SenderConnection.Tag).State;

                        // Send reason
                        if (handShakeState == HandShakeState.Failed)
                            // Failed (username, password, key, n, g invalid)
                            msg.SenderConnection.Disconnect(HandShakeFailedMessage);
                        else if (handShakeState == HandShakeState.Expired)
                            // Expired (too to long)
                            msg.SenderConnection.Disconnect(HandShakeExpiredMessage);
                        else
                        {
                            // Corrupt Data
                            msg.SenderConnection.Disconnect(HandShakeCorruptedMessage + handShakeState.ToString());
                            Logger.Error("Corrupt Package? Handshake violately terminated!");
                            msg.SenderConnection.Tag = null;
                        }
                    }
                }
            }
            finally
            {
                if (_client != null)
                    _client.Disconnect("Goodbye!");
            }
        }
    }
}
