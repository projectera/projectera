using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Lidgren.Network;
using ERAAuthentication.SRP6;
using ERAUtils.Logger;
using System.Threading;

namespace ProjectERA.Services.Network
{
    internal partial class NetworkManager
    {
        private const String ServerRetrieveAddress = "http://projectera.org/nearbyserver.php";
        private ConnectingStatus _connectingStatus;
        private WebClient _webClient;
        private IPEndPoint _nearbyServer;
        private String _username;
        private String _password;

        public event ConnectingStatusChangedEventHandler ConnectingStatusChanged;
        public ConnectingStatus GetConnectingStatus { get { return _connectingStatus; } }

        /// <summary>
        /// Starts connecting with username and password
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        public void AsyncConnect(String username, String password)
        {
            if (_connectingStatus == ConnectingStatus.None || _connectingStatus == ConnectingStatus.Cancelled || _connectingStatus != ConnectingStatus.Failed)
            {
                _username = username;
                _password = password;
                AsyncFindNearbyServer();
            }
        }

        /// <summary>
        /// Starts finding a nearby server asynchronously
        /// </summary>
        public void AsyncFindNearbyServer()
        {
            if (_webClient != null && _webClient.IsBusy)
                return;

            // Update the status
            _connectingStatus = ConnectingStatus.RequestedNearbyServer;

            if (ConnectingStatusChanged != null)
                ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.RequestedNearbyServer)); 

            // Start the download
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(_webClient_DownloadStringCompleted);
            _webClient.DownloadStringAsync(new Uri(ServerRetrieveAddress));
        }

        /// <summary>
        /// Event handler for completion of server retrieval
        /// </summary>
        /// <param name="sender">Source</param>
        /// <param name="e">Event arguments</param>
        private void _webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // If cancelled by the user
            if (e.Cancelled)
            {
                _connectingStatus = ConnectingStatus.Cancelled;
                if (ConnectingStatusChanged != null)
                    ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.Cancelled)); 
            }
            // If error by the webclient
            else if (e.Error is Exception)
            {
                _connectingStatus = ConnectingStatus.CouldNotConnectToServer;
                if (ConnectingStatusChanged != null)
                    ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.CouldNotConnectToServer)); 
            }
            // If succeeded
            else
            {
                _connectingStatus = ConnectingStatus.ReceivedNearbyServer;
                if (ConnectingStatusChanged != null)
                    ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.ReceivedNearbyServer)); 

                // Save the server and start the hand shake
                _nearbyServer = new IPEndPoint(NetUtility.Resolve(e.Result), ServerPort);
                InitiateHandShake();
            }

            // Remove this from events
            _webClient.DownloadStringCompleted -= _webClient_DownloadStringCompleted;
        }

        /// <summary>
        /// Initiates handshaking with nearbyServer
        /// </summary>
        private void InitiateHandShake()
        {
            // Creates a hail message and hails handshake
            NetOutgoingMessage hail = _client.CreateMessage();
            HandShake.InitiateHandShake(_client, ref hail, _nearbyServer, 
                _username, _password, (Int32)DefaultConnectionGroups.Clients,
                DefaultConnectionGroups.Static.HasFlag(DefaultConnectionGroups.Clients));

            // Update status
            _connectingStatus = ConnectingStatus.RequestedConnection;
            if (ConnectingStatusChanged != null)
                ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.RequestedConnection)); 
        }

        /// <summary>
        /// Initiates verification
        /// </summary>
        /// <param name="msg">Message with SRPResponse</param>
        private void InitiateVerification(NetIncomingMessage msg)
        {
            // Write Protocol and Verification data
            NetOutgoingMessage verificationMessage = _client.CreateMessage();
            verificationMessage.Write((Byte)0);//(Byte)ServerProtocols.Extension);
            verificationMessage.Write((Byte)1);//(Byte)ExtendedProtocols.Handshake);
            verificationMessage.WritePadBits();
            HandShake.HandShakeFromPassive(msg, ref verificationMessage);

            // Send verification data
            msg.SenderConnection.SendMessage(verificationMessage, NetDeliveryMethod.ReliableUnordered, 0);

            // Update status
            _connectingStatus = ConnectingStatus.ReceivedConnection;
            if (ConnectingStatusChanged != null)
                ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.ReceivedConnection));
        }

        /// <summary>
        /// Finializes verification (and connecting)
        /// </summary>
        /// <param name="msg">Message with SRPVerification</param>
        private void FinalizeVerification(NetIncomingMessage msg)
        {
            Byte[] key;
            NetOutgoingMessage altVerificationMessage = _client.CreateMessage();

            // Check if verification response is needed
            if (HandShake.HandShakeVerification(msg, ref altVerificationMessage, out key))
            {
                // Update status
                _connectingStatus = ConnectingStatus.Failed;
                if (ConnectingStatusChanged != null)
                    ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.Failed));

                throw new HandShakeException("You are the active party. You shouldn't need to verify twice");
            }

            Logger.Info("Connection established with " + msg.SenderEndpoint.Address.ToString());

            // Update status
            _connectingStatus = ConnectingStatus.Connected;
            if (ConnectingStatusChanged != null)
                ConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(ConnectingStatus.Connected));
            
            // Set key
            _encryption = new NetXtea(key);
        }

        /// <summary>
        /// ConnectingStatus enumeration
        /// </summary>
        internal enum ConnectingStatus
        {
            None = 0,
            RequestedNearbyServer = 1,
            ReceivedNearbyServer = 2,
            RequestedConnection = 3,
            ReceivedConnection = 4,
            Connected = 5,
            CouldNotFindNearbyServer = 10,
            CouldNotConnectToServer = 11,
            Cancelled = 12,
            Failed = 13,
        }

        internal delegate void ConnectingStatusChangedEventHandler(object sender, ConnectingStatusChangedEventArgs e);

        /// <summary>
        /// 
        /// </summary>
        internal class ConnectingStatusChangedEventArgs : EventArgs
        {
            /// <summary>
            /// New status
            /// </summary>
            public readonly ConnectingStatus Status;

            public ConnectingStatusChangedEventArgs(ConnectingStatus newStatus)
                : base()
            {
                this.Status = newStatus;
            }

        }
    }
}
