using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Protocols;
using Lidgren.Network;
using System.Threading.Tasks;
using System.Threading;
using ERAUtils;
using System.Collections.Concurrent;
using System.Net;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Player : Protocol
    {
        /// <summary>
        /// The amount of miliseconds it takes for a DataStore request to time out
        /// </summary>
        private const Int32 PlayerRequestTimeout = 1000 * 15;

        /// <summary>
        /// Private static list of this protocols instances
        /// </summary>
        private static List<Protocol> _instances;

        /// <summary>
        /// Player Instances
        /// </summary>
        /// <remarks>Static</remarks>
        public override List<Protocol> Instances
        {
            get
            {
                return _instances;
            }
            set
            {
                _instances = value;
            }
        }
        
        /// <summary>
        /// Private connection
        /// </summary>
        private static Connection _connection;
        /// <summary>
        /// Global Player Connection
        /// </summary>
        public override Connection Connection
        {
            get
            {
                return _connection;
            }

            protected set
            {
                _connection = value;
            }
        }

        /// <summary>
        /// The id of this protocol
        /// </summary>
        public override Byte ProtocolIdentifier
        {
            get { return _protocolIdentifier; }
        }

        /// <summary>
        /// 
        /// </summary>
        private static Byte _protocolIdentifier
        {
            get { return (Byte)ClientProtocols.Player; }
        }

        /// <summary>
        /// The list of outstanding requests
        /// </summary>
        private static LinkedHashMap<MongoObjectId, PlayerRequest> _outstandingPlayerRequests;
        private static Action<MongoObjectId> _pickAvatarAction;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">Connection to bind to</param>
        /// <param name="networkManager"></param>
        public Player(Connection connection, NetworkManager networkManager)
            : base(connection, networkManager)
        {
            // DISCUSS: There is only one of this, so static? Then can use more threadsafe?
            GeneralCache<MongoObjectId, PlayerRequest>.InitializeCache();
            Interlocked.CompareExchange(ref _outstandingPlayerRequests, new LinkedHashMap<MongoObjectId, PlayerRequest>(), null);
        }

        /// <summary>
        /// Gets the player data
        /// </summary>
        /// <returns></returns>
        internal Task<ProjectERA.Data.Player> Get(Action<ProjectERA.Data.Player> ResultAction)
        {
            return Get(MongoObjectId.Empty, ResultAction); // 0 indicates me
        }

        /// <summary>
        /// Gets a player data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Task<ProjectERA.Data.Player> Get(MongoObjectId id, Action<ProjectERA.Data.Player> ResultAction)
        {
            PlayerRequest req = new PlayerRequest(id, ResultAction);
            
            // Queue the request
            QueueAction(() =>
            {
                // If in storage
                PlayerRequest playerRequest = GeneralCache<MongoObjectId, PlayerRequest>.QueryCache(id);
                if (playerRequest != null && playerRequest.Task != null && playerRequest.Task.Task != null && playerRequest.Task.Task.Result != null)
                {
                    // Result directly
                    req.Result = playerRequest.Result;
                }
                else
                {
                    // Create the outgoing request Message
                    NetOutgoingMessage msg = OutgoingMessage(PlayerAction.Get, 12);
                    msg.Write(id.Id);
                    _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                    // Add it to outstanding Requests
                    if (_outstandingPlayerRequests.Enqueue(id, req))
                    {
                        // Start the retrieval timer
                        req.TimeOut = new Timer((object state) =>
                        {
                            // On timeout, set the result to none
                            req.Result = null;

                            // Queue removal of request
                            QueueAction(() =>
                            {
                                _outstandingPlayerRequests.Remove(id);
                            });

                        }, null, Player.PlayerRequestTimeout, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        req = _outstandingPlayerRequests[id];
                    }
                }
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// Local copy of PlayerData
        /// </summary>
        internal static ProjectERA.Data.Player PlayerData
        {
            get { return Data.DataManager.LocalPlayer; }
            set { Data.DataManager.LocalPlayer = value; }
        }

        /// <summary>
        /// Local copy of InteractableData
        /// </summary>
        internal static ProjectERA.Data.Interactable InteractableData
        {
            get
            {
                if (Player.PlayerData == null)
                    return null;
                return Player.PlayerData.ActiveAvatar;
            }
            set
            {
                Player.PlayerData.ActiveAvatar = value;
            }
        }

        /// <summary>
        /// This function handles incoming messages for this Protocol
        /// </summary>
        /// <param name="msg"></param>
        internal override void IncomingMessage(NetIncomingMessage msg)
        {
            PlayerAction action = (PlayerAction)msg.ReadRangedInteger(0, (Int32)PlayerAction.Max);
            msg.SkipPadBits();
            switch (action)
            {
                // When there was a GET response
                case PlayerAction.Get:
                    MongoObjectId gid = (MongoObjectId)msg.ReadBytes(12);
                    if (!_outstandingPlayerRequests.ContainsKey(gid))
                        break;
                    PlayerRequest gr = _outstandingPlayerRequests[gid];
                    gr.Result = Player.Unpack(msg);
                    GeneralCache<MongoObjectId, PlayerRequest>.UpdateCache(gr); // NOTE: we do not need to queueaction this. UpdateCache is ThreadSafe
                    _outstandingPlayerRequests.Remove(gid);

                    // If got own player, set data
                    if (gid == MongoObjectId.Empty)
                        Player.PlayerData = gr.Result; // TODO: threadsafe
                    break;

                // When there was an INVALIDATE push-notification
                case PlayerAction.Update:
                    MongoObjectId uid = (MongoObjectId)msg.ReadBytes(12);
                    PlayerRequest ur = GeneralCache<MongoObjectId, PlayerRequest>.QueryCache(uid); // NOTE: we do not need to queueaction this. QueryCache is ThreadSafe
                    if (ur == null || ur.Task == null)
                        break;
                    ur.Result = Player.Unpack(msg);
                    GeneralCache<MongoObjectId, PlayerRequest>.UpdateCache(ur);

                    // If got own player, set data
                    if (uid == MongoObjectId.Empty)
                    {
                        List<ProjectERA.Data.Interactable> temp = Player.PlayerData.Avatars; // HACK: save avatars
                        Player.PlayerData = ur.Result; // TODO thread safe
                        Player.PlayerData.Avatars = temp; 
                    }
                    break;

                //
                case PlayerAction.Message:
                    MongoObjectId dialogueId = (MongoObjectId)msg.ReadBytes(12);
                    
                    break;

                //
                case PlayerAction.MessageAttachment:

                    break;

                // Pickavatar response
                case PlayerAction.PickAvatar:
                    Player.InteractableData = Interactable.Unpack(msg);
                    Interactable.CacheInteractable(Player.InteractableData); // TODO remove this part -- will do earlier in avatar selection?
                    Map.Id = Player.InteractableData.MapId;

                    if (_pickAvatarAction != null)
                    {
                        _pickAvatarAction.Invoke(Map.Id);
                        _pickAvatarAction = null;
                    }
                    break;

                // We will be transfering to another server
                case PlayerAction.Transfer:
                    IPEndPoint destination = msg.ReadIPEndpoint();
                    this.NetworkManager.Transfer(destination); 
                    break;

                // When action does not exist
                default:
                    throw new NetException("No such action in this protocol " + this.GetType());
            }
        }

        /// <summary>
        /// Creates an Outgoing Message with the specified action integer written to it.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static NetOutgoingMessage OutgoingMessage(PlayerAction action, Int32 initialCapacity)
        {
            NetOutgoingMessage msg = _connection.MakeMessage(_protocolIdentifier, initialCapacity + ERAUtils.BitManipulation.BytesToHold((Int32)PlayerAction.Max));
            msg.WriteRangedInteger(0, (Int32)PlayerAction.Max, (Int32)action);
            msg.WritePadBits();
            return msg;
        }
      
        /// <summary>
        /// Ondisconnect
        /// </summary>
        internal override void Disconnect()
        {
            _outstandingPlayerRequests.Clear();
            GeneralCache<MongoObjectId, PlayerRequest>.ClearCache();

            // TODO: recyle ALL players?
            Pool<ProjectERA.Data.Player>.Recycle(Player.PlayerData);
            base.Disconnect();
        }
    }
}
