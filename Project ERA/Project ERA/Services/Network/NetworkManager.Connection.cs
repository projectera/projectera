using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Lidgren.Network;
using ERAUtils.Logger;
using System.Threading;
using ProjectERA.Protocols;
using ProjectERA.Services.Data;
using ProjectERA.Services.Network.Protocols;
using System.Threading.Tasks;

namespace ProjectERA.Services.Network
{
    internal partial class NetworkManager
    {
        #region Options
        internal const String ServerRetrieveAddress = "http://projectera.org/server/get.php";
        private const Int32 MaxReconnectRounds = 0;
        #endregion

        #region Private Fields
        private ConnectingStatus _connectingStatus;
        private WebClient _webClient;
        private IPEndPoint _nearbyServer;
        private String _username;
        private String _password;
        private Int32 _reconnectCounter;
        private NetClient _transferClient;
        #endregion

        #region Connecting
        /// <summary>
        /// Starts connecting with username and password
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        public void AsyncConnect(String username, String password)
        {
            if (_connectingStatus == ConnectingStatus.None || _connectingStatus == ConnectingStatus.Cancelled || _connectingStatus == ConnectingStatus.Failed)
            {
                // Reset reconnection counter
                _reconnectCounter = 0;

                // Save credentials
                _username = username.ToLower().Trim();
                _password = password;

                // Reset nearby server
                _nearbyServer = null;

                // Find server to connect to
                AsyncFindNearbyServer();
            }
        }

        /// <summary>
        /// Cancells any running connections
        /// </summary>
        internal void CancelConnect()
        {
            UnregisterHandShakeEvents();

            if (_webClient != null && _webClient.IsBusy)
                _webClient.CancelAsync();

            if (_client != null)
                _client.Disconnect("Cancelled connecting");

            ConnectingStatusChange(ConnectingStatus.Cancelled);
        }

        /// <summary>
        /// Starts finding a nearby server asynchronously
        /// </summary>
        public void AsyncFindNearbyServer(Boolean newServer)
        {
            if (_webClient != null && _webClient.IsBusy)
                return;

            ConnectingStatusChange(ConnectingStatus.RequestedNearbyServer);

            // Start the download
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(_webClient_DownloadStringCompleted);
            _webClient.DownloadStringAsync(new Uri(ServerRetrieveAddress), newServer);
        }

        /// <summary>
        /// Async finds new server
        /// </summary>
        public void AsyncFindNearbyServer()
        {
            AsyncFindNearbyServer(false);
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
                ConnectingStatusChange(ConnectingStatus.Cancelled);
            }
            // If error by the webclient
            else if (e.Error is Exception)
            {
#if DEBUG
                IPAddress server = NetUtility.Resolve("localhost");
                // Save the server and start the hand shake
                _nearbyServer = new IPEndPoint(server, ServerPort);
                InitiateHandShake();
#else

               ConnectingStatusChange(ConnectingStatus.CouldNotConnectToServer);
#endif
            }
            // If succeeded
            else
            {
                ConnectingStatusChange(ConnectingStatus.ReceivedNearbyServer);

                if (!String.IsNullOrWhiteSpace(e.Result))
                {
                    IPAddress server = NetUtility.Resolve(e.Result);

#if DEBUG
                    if (server == null)
                        server = NetUtility.Resolve("localhost");
#endif

                    // NOTE doesn't work if there are 3 severs, with 2 unreachable. Fix it!
                    if (_nearbyServer != null && server.Equals(_nearbyServer.Address))
                    {
                        ConnectingStatusChange(ConnectingStatus.CouldNotFindNearbyServer);
                            
                        // Remove this from events
                        _webClient.DownloadStringCompleted -= _webClient_DownloadStringCompleted;

                        // Return early
                        return;
                    }

                    // Save the server and start the hand shake
                    _nearbyServer = new IPEndPoint(server , ServerPort);
                    InitiateHandShake();
                }
                else
                {
                    ConnectingStatusChange(ConnectingStatus.CouldNotFindNearbyServer);
                }
            }

            // Remove this from events
            _webClient.DownloadStringCompleted -= _webClient_DownloadStringCompleted;
        }
        #endregion

        #region Secure Remote Password Protocol
        /// <summary>
        /// Initiates handshaking with nearbyServer
        /// </summary>
        private void InitiateHandShake()
        {
            this.OnHandShakeCompleted += new EventHandler(NetworkManager_OnHandShakeCompleted);
            this.OnHandShakeFailed += new EventHandler(NetworkManager_OnHandShakeFailed);
            this.OnHandShakeNoResponse += new BooleanEventHandler(NetworkManager_OnHandShakeNoResponse);

            // Creates a hail message and hails handshake
            NetOutgoingMessage hail = _client.CreateMessage();
            //_client.ConnectSRP(_nearbyServer, _username, _password);
            NetConnection connection = _client.Connect(_nearbyServer, hail);
           // NetLobby.NetLobby.Authenticate(connection, _username, _password);
           
            // Update status
            ConnectingStatusChange(ConnectingStatus.RequestedConnection);
        }

        #region HandShakeEvents
        /// <summary>
        /// Event: Handshake no response (timeout)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">when true: reconnect, when false: find next server</param>
        private void NetworkManager_OnHandShakeNoResponse(object sender, BooleanEventArgs e)
        {
            UnregisterHandShakeEvents();

            // If cancelled, do not wait for connection
            if (_connectingStatus != ConnectingStatus.Cancelled)
            {
                ConnectingStatusChange(ConnectingStatus.NoResponse);

                if (e.Value)
                    InitiateHandShake();
                else
                    AsyncFindNearbyServer(true);
            }
        }

        /// <summary>
        /// Event: Handshake failed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkManager_OnHandShakeFailed(object sender, EventArgs e)
        {
            UnregisterHandShakeEvents();

            ConnectingStatusChange(ConnectingStatus.Failed);
        }

        /// <summary>
        /// Event: Handshake completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkManager_OnHandShakeCompleted(object sender, EventArgs e)
        {
            //UnregisterHandShakeEvents();

            // Reset reconnectionCounter
            _reconnectCounter = 0;
        }

        /// <summary>
        /// Unregisters all handshake events
        /// </summary>
        private void UnregisterHandShakeEvents()
        {
            this.OnHandShakeCompleted -= NetworkManager_OnHandShakeCompleted;
            this.OnHandShakeFailed -= NetworkManager_OnHandShakeFailed;
            this.OnHandShakeNoResponse -= NetworkManager_OnHandShakeNoResponse;
        }
        #endregion
        #endregion

        #region Transfer Protocol
        public void StartTransfer(IPEndPoint newDestination)
        {
            // TODO: lot of checks

            Logger.Info("Transfer requested to :" + newDestination);

            // Create configuration for new transfer
            NetPeerConfiguration config = _client.Configuration.Clone();
            config.Port = TransferPort;

            // Create TransferClient
            _transferClient = new NetClient(config);
            _transferClient.Start();

            // Start connecting
            NetOutgoingMessage hailMessage = _transferClient.CreateMessage();
            hailMessage.Write(_connection.NodeId);
            _transferClient.Connect(newDestination, hailMessage);

            // TODO: connection completion, transfer loop!
            Task.Factory.StartNew(() => 
            {

                NetIncomingMessage message;
                NetConnectionStatus status;

                // Wait for transfer client to connect
                do
                {
                    do
                    {
                        _transferClient.MessageReceivedEvent.Set();
                        while ((message = _transferClient.ReadMessage()) == null) ;
                    } while (message.MessageType != NetIncomingMessageType.StatusChanged);
                    status = (NetConnectionStatus)message.ReadByte();

                    Logger.Debug("Transfer status: " + status);

                } while (status != NetConnectionStatus.Connected && status != NetConnectionStatus.Disconnected);

                // If disconnected, error must have occurred!
                if (status == NetConnectionStatus.Disconnected)
                {
                    Logger.Error(message.ReadString());
                    return;
                }

                Logger.Info("Transfer destination connected.");

                _transferClient.Tag = _client.Tag;
                _client.Disconnect("Transfer issued.");
            });
            
        }
        #endregion

        /// <summary>
        /// Adds the protocols for this type of server to a new connection
        /// </summary>
        /// <param name="connection">The connection to add the protocols to</param>
        public void RegisterProtocols(Connection connection)
        {
            Protocol pp = new Protocols.Player(connection, this);
            connection.RegisterProtocol(pp);

            Protocol pi = new Protocols.Interactable(connection, this);
            connection.RegisterProtocol(pi);

            Protocol pm = new Protocols.Map(connection, this);
            connection.RegisterProtocol(pm);

            Protocol pa = new Protocols.Asset(connection, this);
            connection.RegisterProtocol(pa);
        }

        /// <summary>
        /// When player protocol has got user info, set the id
        /// </summary>
        /// <param name="p"></param>
        internal void SetPlayerId(Byte[] id)
        {
            _connection.NodeId = id;
        }

        /// <summary>
        /// Disconnects client and resets connecting status
        /// </summary>
        /// <param name="message">message to disconnect with</param>
        public void Disconnect(String message)
        {
            ConnectingStatusChange(ConnectingStatus.None);
            if (_client != null)
                _client.Disconnect(message);
            if (_transferClient != null)
                _transferClient.Disconnect(message);
        }

        #region Connecting Status
        /// <summary>
        /// Is yielded when connection status changes
        /// </summary>
        public event ConnectingStatusChangedEventHandler OnConnectingStatusChanged = delegate { };
        internal delegate void ConnectingStatusChangedEventHandler(object sender, ConnectingStatusChangedEventArgs e);

        /// <summary>
        /// Gets the current connectingstatus
        /// </summary>
        public ConnectingStatus ConnectingStatusNow 
        { 
            get 
            { 
                return _connectingStatus; 
            } 
        }

        /// <summary>
        /// Gets the current connectionstatus
        /// </summary>
        public NetConnectionStatus ConnectionStatus 
        { 
            get 
            {
                if (_client == null)
                    return NetConnectionStatus.None;

                return _client.ConnectionStatus; 
            } 
        }

        /// <summary>
        /// Currently connecting
        /// </summary>
        public Boolean IsConnecting
        {
            get
            { 
                return !(ConnectingStatusNow == ConnectingStatus.None || ConnectingStatusNow == ConnectingStatus.Cancelled || ConnectingStatusNow == ConnectingStatus.Connected ||
                    ConnectingStatusNow == ConnectingStatus.Failed || ConnectingStatusNow == ConnectingStatus.CouldNotConnectToServer ||
                    ConnectingStatusNow == ConnectingStatus.CouldNotFindNearbyServer);
            }
        }

        /// <summary>
        /// Currently connected
        /// </summary>
        public Boolean IsConnected 
        { 
            get
            {
                return ConnectingStatusNow == ConnectingStatus.Connected;
            }
        }

        /// <summary>
        /// Updates status 
        /// </summary>
        /// <param name="newStatus">New status</param>
        private void ConnectingStatusChange(ConnectingStatus newStatus)
        {
            _connectingStatus = newStatus;
            if (OnConnectingStatusChanged != null)
                OnConnectingStatusChanged.Invoke(this, new ConnectingStatusChangedEventArgs(newStatus));
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
            NoResponse = 14,
        }

        /// <summary>
        /// Connecting Status Changed Event Arguments hold the new status
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
        #endregion
    }
}
