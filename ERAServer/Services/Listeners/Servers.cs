#define NOINTERNALMESSAGES

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ERAServer.Properties;
using ERAServer.Protocols.Server;
using ERAServer.Protocols.Server.Misc;
using ERAUtils.Logger;
using Lidgren.Network;
using MongoDB.Bson;
using Lidgren.Network.Authentication;

namespace ERAServer.Services.Listeners
{
    /// <summary>
    /// Interfaces gameserver <-> gameserver connections
    /// </summary>
    internal partial class Servers
    {
        private const Boolean __DISABLELOOP = true;
        private const Boolean __SIMULTATENETWORK = false;

        /// <summary>
        /// Gets the heartbeat base webadress
        /// </summary>
        private String HeartbeatWebAddress
        {
            get
            {
                return String.Join("/", ERAServer.Properties.Settings.Default.HearbeatHostname, "server", "heartbeat.php");
            }
        }

        /// <summary>
        /// Returns the time after which early messages are released
        /// </summary>
        private TimeSpan ReleasePrematureMessageAfter = TimeSpan.FromSeconds(1);
        
        /// <summary>
        /// Returns the time after which a handshake is retried
        /// </summary>
        private TimeSpan RetryHandShakeAfter = TimeSpan.FromSeconds(2.5);

        private NetPeer _network;
        private Thread _serverThread;
        private UInt64 _selfId;

        private Int64 _minutesOffline;
        private System.Timers.Timer _heartbeatTimer;
        private WebClient _heartbeatWebClient;

        /// <summary>
        /// Serverloop running flag
        /// </summary>
        public Boolean IsRunning { get; set; }

        /// <summary>
        /// Number of connections
        /// </summary>
        public Int32 ConnectionCount 
        { 
            get 
            {
                // No need to lock, this is a copy
                return _network.Connections.Count((c) => { return c.Tag is Connection; }); 
            } 
        }

        /// <summary>
        /// Holds the Connection mappings for MapId's
        /// </summary>
        internal static ConcurrentDictionary<ObjectId, Connection> MapConnectionMapping = new ConcurrentDictionary<ObjectId, Connection>();

        /// <summary>
        /// 
        /// </summary>
        internal Servers()
        {
            // CONFIGURATION
            // First the server is configured to accept discovery requests and responses. This wil be 
            // used to look up the other servers in the subdomain. CustomServers and known nodes will
            // be used do find DistantServers. ConnectionApproval must be enabled, to go be able to
            // accept and deny connections.
            NetPeerConfiguration config = new NetPeerConfiguration("ERA.Peer");
            config.AcceptIncomingConnections = true;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            //config.EnableMessageType(NetIncomingMessageType.HandshakeMessage);

            #if NOINTERNALMESSAGES
                config.DisableMessageType(NetIncomingMessageType.VerboseDebugMessage);
                config.DisableMessageType(NetIncomingMessageType.DebugMessage);
                config.DisableMessageType(NetIncomingMessageType.WarningMessage);
            #endif

            #if DEBUG
            // Simultation
            if (!__DISABLELOOP && __SIMULTATENETWORK)
            {
                #pragma warning disable 0162
                config.SimulatedMinimumLatency = 1.2f;      // 1200 ms latency
                config.SimulatedRandomLatency = 3f;         // + 0..3000 ms
                config.SimulatedDuplicatesChance = 0.1f;    // 10 % duplicates
                config.SimulatedLoss = 0.01f;               // 1 % lost
                #pragma warning restore 0162
            }
            #endif

            // Server connections should be persistant
            config.ConnectionTimeout = 25; //Single.MaxValue;
            // 2 times the handshakes in the same time + additional 5 afterwards
            config.MaximumHandshakeAttempts = 15;
            //config.ResendHandshakeInterval = 1.5f;
            //config.DifferentiateDenySRP = false;
            //config.SRPKeySize = ERAServer.Properties.Settings.Default.SRP6Keysize;
            //config.EnableSRP = true;
            //config.ForceSRP = true; // Servers should ALWAYS use SRP
            //config.LogonManager = new LogonManager();
            config.UseMessageRecycling = true;
            config.Port = 41363;
            
            // CREATION
            // The network is a node (netpeer). If the code fails at this point, multiple bindings
            // occurred at one port. This is not allowed, and so the server is terminated.
            _network = new NetPeer(config);
            Logger.Notice("Server [" + Settings.Default.ServerName + "]");
            Logger.Debug("-- NetPeer created at " + NetTime.ToReadable(NetTime.Now));
            try
            {
                _network.Start();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                // Notify the console
                Logger.Fatal(e.Message);

                // HACK: two server enabling
                config = config.Clone();
                config.Port++;
                _network = new NetPeer(config);
                ERAUtils.Environment.MachineName += "2";
                _network.Start();
                
                NetOutgoingMessage msg = _network.CreateMessage(8 + Settings.Default.ServerName.Length + 1);
                msg.Write(ERAUtils.Environment.LongMachineId);
                msg.Write(ERAServer.Properties.Settings.Default.ServerName);

                //_network.ConnectSRP("downshare.nl", config.Port - 1, msg, //"downshare.nl", config.Port - 1, msg,
                //    ERAServer.Properties.Settings.Default.ServerName,
                //    ERAServer.Properties.Settings.Default.Secret, new Byte[] { 31 });
                //return;
            }

            // CREATES MACHINE ID
            // Creates an unique machine id derived from all the networkinterfaces. These interfaces
            // contain mac adresses, unique to each adapter. This way unique machine identifiers can
            // be generated.
            _selfId = ERAUtils.Environment.LongMachineId;
            Logger.Notice("ServerId [" + _selfId.ToString() + " | " + ERAUtils.Environment.MachineId.ToString() + " | " + ERAUtils.Environment.ShortMachineId.ToString() + "]");


            // LOCAL BROADCAST
            // Broadcasts a DiscoveryRequest on the local subnet.
            Logger.Debug("Discovering Peers on " + config.Port + " at " + NetTime.ToReadable(NetTime.Now));
            _network.DiscoverLocalPeers(config.Port);
            

            // CUSTOM SERVERS
            // All custom servers (outside the local broadcast area) are
            // nodes in the network, so we should connect to each of those.
            String serverList = Properties.Settings.Default.CustomServers;
            if (serverList != null && serverList.Length != 0)
            {
                String[] servers = serverList.Split(new Char[] { '|' });
                foreach (String server in servers)
                {
                    Logger.Info("Trying to connect to " + server);

                    NetOutgoingMessage msg = _network.CreateMessage(8 + Settings.Default.ServerName.Length + 1);
                    msg.Write(ERAUtils.Environment.LongMachineId);
                    msg.Write(ERAServer.Properties.Settings.Default.ServerName);

                    #if DEBUG
                    try
                    {
                    #endif
                        _network.Connect(server, config.Port, msg);
                        // ERAServer.Properties.Settings.Default.ServerName,
                            //ERAServer.Properties.Settings.Default.Secret, new Byte[] { 31 }
                    #if DEBUG
                    }
                    catch (Exception)
                    {

                    }
                    #endif
                }
            }


            // RUN SERVER
            _serverThread = new Thread(new ThreadStart(ServerLoop));
            _serverThread.Name = "Server thread";
            this.IsRunning = true;
            _serverThread.Start();

            // Configuration of Heartbeats
            _heartbeatWebClient = new WebClient();
            _heartbeatWebClient.BaseAddress = HeartbeatWebAddress;
            _heartbeatTimer = new System.Timers.Timer(1000 * 60);
            _heartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(HeartBeat);
            _heartbeatTimer.Start();

            // First heartbeat
            if (!Defibrilate())
            {
                Interlocked.Exchange(ref _minutesOffline, 5); // Offline (simulate offline >= 5 minutes)
                Logger.Error("Defribilation is failing. Defribilation needed before clients will find this server.");
            }
        }

        /// <summary>
        /// Starts server heartbeat
        /// </summary>
        private Boolean Defibrilate()
        {
            try
            {
                String[] parameters = new String[] { "?create", String.Join("=", "host", ERAServer.Properties.Settings.Default.ServerHostname) };
                String defibrilated = _heartbeatWebClient.DownloadString(String.Join("&", parameters));
                if (defibrilated.StartsWith("<!DOCTYPE"))
                {
                    throw new WebException();
                }
                Logger.Verbose("Heart " + defibrilated + " at " + NetTime.ToReadable(NetTime.Now));

                Interlocked.Exchange(ref _minutesOffline, 0);

                return true;
            }
            catch (WebException) 
            {
                // Did not respond
                Logger.Warning("Heart did not respond to defibrilation.");

                if (Interlocked.Increment(ref _minutesOffline) >= 5)
                    Logger.Error("Defribilation is failing");

                return false;
            }
            catch (NotSupportedException) 
            {
                // Concurrent I/O
                Logger.Notice("Heart could not be defibrilated because was already being operated on.");

                return false;
            }
        }

        /// <summary>
        /// Sends heartbeat to webserver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeartBeat(object sender, EventArgs e)
        {
            try
            {
                String beat = _heartbeatWebClient.DownloadString("?update");

                if (beat.StartsWith("<!DOCTYPE"))
                {
                    throw new WebException();
                }

                Logger.Verbose("Heart" + beat + " at " + NetTime.ToReadable(NetTime.Now));

                // Server unknown
                switch (beat)
                {
                    case "coding":
                        Interlocked.Exchange(ref _minutesOffline, 5); // Only coding if offline >= 5
                        Defibrilate();
                        break;
                    case "beat":
                        Interlocked.Exchange(ref _minutesOffline, 0); // Online
                        break;
                }
                
            }
            catch (WebException)
            {
                // Did not respond
                Logger.Notice("Heart did not beat.");

                if (Interlocked.Increment(ref _minutesOffline) >= 5)
                    Logger.Error("Heart is flatlinening: Defribilation needed before clients will find this server.");
            }
            catch (NotSupportedException)
            {
                // Concurrent I/O
                Logger.Notice("Heart could not beat because was already being operated on.");
            }
        }

        /// <summary>
        /// Runs while the server is running
        /// </summary>
        public void ServerLoop()
        {
            Logger.Debug("Server Service is Running at " + NetTime.ToReadable(NetTime.Now));

            NetIncomingMessage msg;
            while (this.IsRunning)
            {
                msg = _network.ReadMessage();

                // Releave CPU for some milliseconds
                if (msg == null)
                {
                    System.Threading.Thread.Sleep(10);
                    continue;
                }

                switch (msg.MessageType)
                {
                    // MESSAGETYPE: DATA
                    // The main message type in networking is the data type. When the connection is not linked, the
                    // data is the verification data of the handshake and will be processed accordingly. If not, 
                    // the message is passed onto the Connection and processed by their respective protocol.
                    case NetIncomingMessageType.Data:

                        /*
                        // Error if the handshake was not initiated yet
                        if (!msg.SenderConnection.IsUsingSRP)
                        {

                            return;
                        }*/

                        // It got JUST a little bit too early
                        /*
                        if (msg.SenderConnection.Tag == null)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                Thread.Sleep(ReleasePrematureMessageAfter);

                                msg.Position = 0;
                                _network.ReleaseMessage(msg);
                            });
                            return;
                        }*/

                        if (msg.SenderConnection.Tag is Connection)
                            ((Connection)msg.SenderConnection.Tag).IncomingMessage(msg);
                        else
                        {
                            var handshake = Lidgren.Network.Lobby.NetLobby.IncomingMessage(msg);

                            switch(handshake) {
                                case Handshake.Contents.Succes:
                            
                                    Connection connection = new Connection(_network, msg.SenderConnection, (msg.SenderConnection.Tag as Handshake).CreateEncryption());
                                    RegisterProtocols(connection);

                                    // HACK to simulate transfering
                                    MapConnectionMapping.AddOrUpdate(ObjectId.Empty, (Connection)msg.SenderConnection.Tag,
                                        (ObjectId id, Connection value) => (Connection)msg.SenderConnection.Tag);
                                break;

                                case Handshake.Contents.Error:
                                case Handshake.Contents.Denied:
                                    msg.SenderConnection.Disconnect("Error occured during handshake.");
                                    Logger.Error("Error occured during handshake.");
                                    break;
                                case Handshake.Contents.Expired:
                                    var username = ERAServer.Properties.Settings.Default.ServerName;
                                    var password = ERAServer.Properties.Settings.Default.Secret;
                                    var data = new Byte[] { 31 };
                                    Lidgren.Network.Lobby.NetLobby.Authenticate(msg.SenderConnection, username, password, data);
                                    Logger.Info("Handshake expired");
                                    break;
                            }
                        }
                            
                        break;	

                    // MESSAGETYPE: CONNECTION APPROVAL
                    // The ConnectionApproval message type is seen when a node yields the peer#connect function. When
                    // the RemoteEndpoint specified is reached, a loose connection is made. It's up to the other end,
                    // the one that is connected too, to deny or approve the connection. 
                    case NetIncomingMessageType.ConnectionApproval:

                        // Hailname from data
                        UInt64 otherid = msg.ReadUInt64();
                        String hailname = msg.ReadString();
                        Logger.Info("Connection request (" + hailname + " | " + otherid + ") from " + msg.SenderEndpoint);

                        if (__DISABLELOOP && otherid == _selfId && Settings.Default.ServerName == hailname)
                        {
                            Logger.Info("Self-Connect means not connecting");
                            msg.SenderConnection.Deny("Loop connections are not allowed");
                        }
                        else
                        {
                            NetOutgoingMessage approvalHail = _network.CreateMessage();
                            msg.SenderConnection.Approve(approvalHail);
                        }
                        break;

                    // MESSAGETYPE: HANDSHAKEMESSAGE
                    // Contains the Reason byte and information about that reason~message. Occurs when Handshake fails,
                    // is denied, succeeds or expires.
                    // TODO: remove the following code:
                    /*
                    case NetIncomingMessageType.HandshakeMessage:
                        HandshakeMessageReason reason = (HandshakeMessageReason)msg.ReadByte();
                        switch (reason)
                        {
                            case HandshakeMessageReason.Error:
                            case HandshakeMessageReason.Expired:
                                // Reconnect?
                                break;

                            case HandshakeMessageReason.Denied:
                            case HandshakeMessageReason.Denied | HandshakeMessageReason.Username:
                            case HandshakeMessageReason.Denied | HandshakeMessageReason.Password:
                                // This shouldn't happen
                                break;

                            case HandshakeMessageReason.Succes:
                                // awesome
                                Connection connection = new Connection(_network, msg.SenderConnection, msg.SenderConnection.CreateEncryption());
                                RegisterProtocols(connection);

                                // HACK to simulate transfering
                                MapConnectionMapping.AddOrUpdate(ObjectId.Empty, (Connection)msg.SenderConnection.Tag, 
                                    (ObjectId id, Connection value) => (Connection)msg.SenderConnection.Tag);
                                break;
                        }
                        break;
                        */

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
                            case NetConnectionStatus.Disconnecting:
                                Logger.Debug("Disconnecting from " + msg.SenderEndpoint.Address.ToString());
                                break;

                            // When disconnect is called and processed
                            case NetConnectionStatus.Disconnected:
                                // If already connection established, destroy resources
                                if (msg.SenderConnection.Tag is Connection &&
                                    !((Connection)msg.SenderConnection.Tag).IsDisposed)
                                {
                                    ((Connection)msg.SenderConnection.Tag).Dispose();
                                }

                                // Received a reason for disconnecting? (e.a. Handshake Fail)
                                String finalReason = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));
                                
                                // Remove any tag
                                msg.SenderConnection.Tag = null;

                                Logger.Verbose("Disconnected from " + msg.SenderEndpoint.Address.ToString());
                                Logger.Verbose("-- reason " + finalReason);
                                break;

                            // When connection was approved at the other end of the pipe
                            case NetConnectionStatus.Connected:

                                // On Connect: Should send a list of connected peers. Some peers may only be locally
                                // found, and thus only added on the local broadcast. However, as they are not in the
                                // CustomServers connection list, they will not be found outside the local network
                                // by other servers.

                                Logger.Debug("Connection Established with " + msg.SenderEndpoint.ToString());
                                UpgradeToSrp(msg.SenderConnection);
                                break;

                            default:
                                String statusChange = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));
                                Logger.Verbose("ConnectionStatus: " + statusByte + " for " + msg.SenderEndpoint.Address.ToString());
                                Logger.Verbose("-- reason " + statusChange);
                                break;
                        }

                        break;

                    // MESSAGETYPE: DISCOVERY RESPONSE
                    // Only yielded when this peer calls peer#DiscoverLocalPeers. A DiscoveryResponse is sent when 
                    // another peer chooses to respond when it receives the DiscoveryRequest message type. In
                    // general, this means a local server tries to connect.
                    case NetIncomingMessageType.DiscoveryResponse:
                        UInt64 otherId = msg.ReadUInt64();
                        String name = msg.ReadString();
                        Logger.Info("Discovery response (" + name + ") from " + msg.SenderEndpoint.Address);

                        if ((msg.SenderEndpoint.Address.Equals(IPAddress.Loopback) || __DISABLELOOP) && _selfId == otherId && ERAServer.Properties.Settings.Default.ServerName == name)
                        {
                            Logger.Info("Self-Broadcast means not connecting");
                        }
                        else
                        {
                            Int32 drsBytes = Encoding.UTF8.GetByteCount(ERAServer.Properties.Settings.Default.ServerName);
                            NetOutgoingMessage msgR = _network.CreateMessage(8 + (drsBytes > 127 ? 2 : 1) + drsBytes);
                            msgR.Write(ERAUtils.Environment.LongMachineId);
                            msgR.Write(ERAServer.Properties.Settings.Default.ServerName);

                            _network.Connect(msg.SenderEndpoint, msgR);
                            //_network.ConnectSRP(msg.SenderEndpoint, msgR,
                            //    ERAServer.Properties.Settings.Default.ServerName,
                            //    ERAServer.Properties.Settings.Default.Secret, new Byte[] { 31 });
                        }
                        break;

                    // MESSAGETYPE: DISCOVERY REQUEST
                    // This is the actual broadcast message send from the peer who is trying to discover
                    // local peers. The receiving peer may choose not to respond, but generally such
                    // message is only yielded when a server tries to connect.
                    case NetIncomingMessageType.DiscoveryRequest:

                        Int32 drqBytes = Encoding.UTF8.GetByteCount(ERAServer.Properties.Settings.Default.ServerName);
                        NetOutgoingMessage discoveryResponseHail = _network.CreateMessage(8 + (drqBytes > 127 ? 2 : 1) + drqBytes);
                        discoveryResponseHail.Write(_selfId);
                        discoveryResponseHail.Write(ERAServer.Properties.Settings.Default.ServerName);
                        _network.SendDiscoveryResponse(discoveryResponseHail, msg.SenderEndpoint);
                        Logger.Debug("Discovery Request from " + msg.SenderEndpoint.ToString());
                        break;

#if DEBUG
                    case NetIncomingMessageType.DebugMessage:
                        String debugMessage = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));
                        Logger.Verbose("INT " + debugMessage);
                        break;

                    case NetIncomingMessageType.VerboseDebugMessage:
                        String vbdebugMessage = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));
                        Logger.Debug("INTVB " + vbdebugMessage);
                        break;

#endif

                    case NetIncomingMessageType.WarningMessage:
                        String warningMessage = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));
                        Logger.Verbose("INT " + warningMessage);
                        break;

                    case NetIncomingMessageType.ErrorMessage:
                        String errorMessage = Encoding.UTF8.GetString(msg.ReadBytes((Int32)msg.ReadVariableUInt32()));
                        Logger.Verbose("INT " + errorMessage);
                        break;

                    default:
                        throw new NetException("MessageType: " + msg.MessageType + " is not supported.");
                }

                // Recycle please
                _network.Recycle(msg);
            }

            _heartbeatTimer.Stop();
            _network.Shutdown("Server shutting down");

            try
            {
                while (_heartbeatWebClient.IsBusy)
                    Thread.Sleep(10);

                String flatline = _heartbeatWebClient.DownloadString("?delete");

                if (!String.IsNullOrWhiteSpace(flatline))
                    Logger.Verbose("Heart " + flatline);
            }
            catch (WebException)
            {
                Logger.Warning("Could not kill heartbeat.");
            }
            catch (NotSupportedException)
            {
                Logger.Warning("Could not kill heartbeat: concurrent update."); 
            }

            Logger.Debug("Server Service is Running at " + NetTime.ToReadable(NetTime.Now));

            _network.Shutdown("Final shutdown");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="netConnection"></param>
        private void UpgradeToSrp(NetConnection netConnection)
        {
            var username = ERAServer.Properties.Settings.Default.ServerName;
            var password = ERAServer.Properties.Settings.Default.Secret;
            var data = new Byte[] { 31 };

            Lidgren.Network.Lobby.NetLobby.Authenticate(netConnection, username, password, data);
        }

        /// <summary>
        /// Adds the protocols for this type of server to a new connection
        /// </summary>
        /// <param name="connection">The connection to add the protocols to</param>
        internal void RegisterProtocols(Connection connection)
        {
            //connection.RegisterProtocol(new DataStore(connection));
            connection.RegisterProtocol(new PeerExchange(connection));
            connection.RegisterProtocol(new Misc(connection));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        internal static void BroadcastRunningMap(ObjectId map)
        {
            // SEND TO ALL
            
        }
    }
}
