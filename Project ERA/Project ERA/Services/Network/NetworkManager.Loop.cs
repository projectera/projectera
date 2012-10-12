using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading;
using ERAUtils.Logger;
using System.Net;
using ProjectERA.Services.Data;
using System.Threading.Tasks;
using Lidgren.Network.Authentication;

namespace ProjectERA.Services.Network
{
    internal partial class NetworkManager : Microsoft.Xna.Framework.GameComponent
    {
        #region Options
        private TimeSpan ReleasePrematureMessageAfter = TimeSpan.FromSeconds(1);
        private TimeSpan RetryHandShakeAfter = TimeSpan.FromSeconds(2.5);
        #endregion

        #region Events
        public event EventHandler OnHandShakeCompleted = delegate { };
        public event EventHandler OnHandShakeFailed = delegate { };
        public event BooleanEventHandler OnHandShakeNoResponse = delegate { };
        #endregion

        /// <summary>
        /// Loops until the network thread may not run anymore
        /// </summary>
        private void Loop()
        {
            try
            {
                // This is checked each cycle
                while (this.IsRunning)
                {
                    NetIncomingMessage msg = _client.ReadMessage();

                    // No message received, please poll
                    if (msg == null)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    switch (msg.MessageType)
                    {
                        // MESSAGETYPE: DATA
                        // The main message type in networking is the data type. When the connection is not linked, the
                        // data is the verification data of the handshake and will be processed accordingly. If not, 
                        // the message is passed onto the Connection and processed by their respective protocol.
                        case NetIncomingMessageType.Data:
                            // Still in handshake
                            // It got JUST a little bit too early
                            /*if (msg.SenderConnection.Tag == null)
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    Thread.Sleep(ReleasePrematureMessageAfter);

                                    msg.Position = 0;
                                    _client.ReleaseMessage(msg);
                                });
                                break;
                            }*/

                            if (msg.SenderConnection.Tag is Connection)
                                ((Connection)msg.SenderConnection.Tag).IncomingMessage(msg);
                            else
                            {
                                var handshake = NetLobby.NetLobby.IncommingMessage(msg);

                                switch(handshake) {
                                    case Handshake.Contents.Succes:

                                        try
                                        {
                                            _connection = new Connection(_client, msg.SenderConnection, (msg.SenderConnection.Tag as Handshake).CreateEncryption());
                                            RegisterProtocols(_connection);
                                            ConnectingStatusChange(ConnectingStatus.Connected);
                                            OnHandShakeCompleted.Invoke(this, EventArgs.Empty);
                                        }
                                        catch (InvalidOperationException)
                                        {
                                            OnHandShakeFailed.Invoke(this, EventArgs.Empty);
                                        }
                                    break;

                                    case Handshake.Contents.Password:
                                        ConnectingStatusChange(ConnectingStatus.ReceivedConnection);
                                        break;

                                    case Handshake.Contents.Error:
                                    case Handshake.Contents.Denied:
                                        msg.SenderConnection.Disconnect("Error occured during handshake.");
                                        OnHandShakeFailed(this, EventArgs.Empty);
                                        Logger.Error("Error occured during handshake.");
                                        break;
                                    case Handshake.Contents.Expired:
                                        var username = _username;
                                        var password = _password;

                                        //OnHandShakeFailed)
                                        NetLobby.NetLobby.Authenticate(msg.SenderConnection, username, password);
                                        Logger.Info("Handshake expired");
                                        break;
                                }
                            }
                            

                            break;

                        // MESSAGETYPE: HANDSHAKEMESSAGE
                        // Contains the Reason byte and information about that reason~message. Occurs when Handshake fails,
                        // is denied, succeeds or expires.
                         
                        // TODO: remove the following:
                        /*
                        case NetIncomingMessageType.HandshakeMessage:
                            HandshakeMessageReason reason = (HandshakeMessageReason)msg.ReadByte();
                            switch (reason)
                            {
                                case HandshakeMessageReason.Succes:
                                    try
                                    {
                                        _connection = new Connection(_client, msg.SenderConnection, msg.SenderConnection.CreateEncryption());
                                        RegisterProtocols(_connection);
                                        ConnectingStatusChange(ConnectingStatus.Connected);
                                        OnHandShakeCompleted.Invoke(this, EventArgs.Empty);
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        OnHandShakeFailed.Invoke(this, EventArgs.Empty);
                                    }
                                    break;
                                case HandshakeMessageReason.Expired:
                                case HandshakeMessageReason.Error:
                                case HandshakeMessageReason.Denied:
                                case HandshakeMessageReason.Denied | HandshakeMessageReason.Username:
                                case HandshakeMessageReason.Denied | HandshakeMessageReason.Password:
                                    OnHandShakeFailed.Invoke(this, EventArgs.Empty);
                                    break;;
                            }
                            break; */

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
                                    NetLobby.NetLobby.Authenticate(msg.SenderConnection, _username, _password);
                                    break;

                                // When disconnect is called and processed
                                case NetConnectionStatus.Disconnected:

                                    // TODO: rewrite the following:
                                    /*
                                    if (_transferClient != null && _transferClient.ConnectionStatus == NetConnectionStatus.Connected)
                                    {
                                        if (_client.ConnectionStatus == NetConnectionStatus.Connected)
                                            Logger.Error("Client still connected, but transfer is taking place now.");

                                        Logger.Info("Transfer from " + msg.SenderConnection.RemoteEndpoint + " to " + _transferClient.ServerConnection.RemoteEndpoint + " completed.");
                                        _transferClient.SaveTransferConnection(ref _client); // merge pools and statistics
                                        _connection.NetConnection = _transferClient.ServerConnection; // move active NetConnection
                                        _connection.NetConnection.Tag = _connection;  // move active Connection
                                        _client = _transferClient; // move active NetPeer
                                        _transferClient = null; 
                                        break;
                                    } */


                                    // If already connection established, destroy resources
                                    if (msg.SenderConnection.Tag is Connection &&
                                        !((Connection)msg.SenderConnection.Tag).IsDisposed)
                                        ((Connection)msg.SenderConnection.Tag).Dispose();

                                    // Received a reason for disconnecting? (e.a. Handshake Fail)
                                    String finalReason = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));

                                    if (finalReason.StartsWith("Failed to establish connection") || finalReason.StartsWith("Connection timed out"))
                                    {
                                        Logger.Info("No response from host. Reconnecting in a bit.");

                                        Task.Factory.StartNew(() =>
                                        {
                                            Thread.Sleep(RetryHandShakeAfter);
                                            OnHandShakeNoResponse.Invoke(this, new BooleanEventArgs(_reconnectCounter++ < MaxReconnectRounds));
                                        });
                                        
                                        return;
                                    }

                                    OnHandShakeFailed.Invoke(this, EventArgs.Empty);

                                    // Log other disconnects
                                    Logger.Info("Disconnected from " + msg.SenderConnection.RemoteEndpoint.Address.ToString());
                                    Logger.Info("-- reason " + finalReason);
                                    break;
                            }
                            break;
                    }
                }   
            }
            finally
            {
                // Disconnect from client if needed
                Disconnect("Client teminated. Goodbye!");
            }
            
            // Restart loop if still running
            if (this.IsRunning)
                Loop();
        }
    }
}
