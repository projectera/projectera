using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Protocols;
using ERAUtils;
using System.Threading.Tasks;
using System.Threading;
using Lidgren.Network;
using ERAUtils.Logger;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Interactable : Protocol
    {
        // <summary>
        /// The amount of miliseconds it takes for a DataStore request to time out
        /// </summary>
        private const Int32 InteractableRequestTimeout = 1000 * 30;

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
            get { return (Byte)ClientProtocols.Interactable; }
        }

         /// <summary>
        /// The list of outstanding requests
        /// </summary>
        private LinkedHashMap<MongoObjectId, InteractableRequest> _outstandingInteractableRequests;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">Connection to bind to</param>
        /// <param name="networkManager"></param>
        public Interactable(Connection connection, NetworkManager networkManager)
            : base(connection, networkManager)
        {
            // DISCUSS: There is only one of this, so static? Then can use more threadsafe?
            GeneralCache<MongoObjectId, InteractableRequest>.InitializeCache();
            Interlocked.CompareExchange(ref _outstandingInteractableRequests, new LinkedHashMap<MongoObjectId, InteractableRequest>(), null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ProjectERA.Data.Interactable> Get(Action<ProjectERA.Data.Interactable> ResultAction)
        {
            return Get(MongoObjectId.Empty, ResultAction); // 0 indicates active
        }

        /// <summary>
        /// This function handles incoming messages for this Protocol
        /// </summary>
        /// <param name="msg"></param>
        internal override void IncomingMessage(NetIncomingMessage msg)
        {
            InteractableAction action = (InteractableAction)msg.ReadRangedInteger(0, (Int32)InteractableAction.Max);
            msg.SkipPadBits();
            switch (action)
            {
                // Gets the interactable data
                case InteractableAction.Get:
                    MongoObjectId gid = (MongoObjectId)msg.ReadBytes(12);
                    if (!_outstandingInteractableRequests.ContainsKey(gid))
                        break;
                    InteractableRequest gr = _outstandingInteractableRequests[gid];
                    gr.Result = Interactable.Unpack(msg);
                    GeneralCache<MongoObjectId, InteractableRequest>.UpdateCache(gr);
                    _outstandingInteractableRequests.Remove(gid);
                    break;

                // Broadcasts a message on the interactable
                case InteractableAction.Message:
                    MongoObjectId messageId = (MongoObjectId)msg.ReadBytes(12);
                    String message = msg.ReadString();
                    ProjectERA.Data.Interactable messageInteractable = Map.Interactables[messageId];

                    Logger.Debug(String.Format("Received message {0} at {1} from {2}", message, DateTime.Now.ToShortTimeString(), messageId));

                    if (messageInteractable != null)
                    {
                        //messageInteractable.AddMessage(message);
                        Map.Notificate(String.Format("Received message {0} at {1} from {2}",
                            message,
                            DateTime.Now.ToShortTimeString(),
                            messageInteractable.Name));
                    }
                    else
                    {
                        Interlocked.Increment(ref Stats.Interactable.MessageNull);
                    }
                    break;

                // Adds a body part to the appearance component
                case InteractableAction.AppearanceAddBodyPart:
                    MongoObjectId appearanceAddId = (MongoObjectId)msg.ReadBytes(12);
                    ProjectERA.Data.InteractableBodyPart partAdded = ProjectERA.Data.InteractableBodyPart.Unpack(msg);
                    if (Map.Interactables.ContainsKey(appearanceAddId))
                    {
                        ProjectERA.Data.Interactable appearanceAddInteractable = Map.Interactables[appearanceAddId];

                        if (appearanceAddInteractable != null)
                            appearanceAddInteractable.Appearance.AddPart(partAdded);
                        else
                            Interlocked.Increment(ref Stats.Interactable.AppearanceNull);
                    }
                    break;

                // Removes a body part from the appearance component
                case InteractableAction.AppearanceRemoveBodyPart:
                    MongoObjectId appearanceRemoveId = (MongoObjectId)msg.ReadBytes(12);
                    ERAUtils.Enum.BodyPart partRemoved = (ERAUtils.Enum.BodyPart)msg.ReadByte();
                    Int32 hashRemoved = msg.ReadInt32();

                    if (Map.Interactables.ContainsKey(appearanceRemoveId))
                    {
                        ProjectERA.Data.Interactable appearanceRemovedInteractable = Map.Interactables[appearanceRemoveId];

                        if (appearanceRemovedInteractable != null)
                            appearanceRemovedInteractable.Appearance.RemovePart(partRemoved, hashRemoved);
                        else
                            Interlocked.Increment(ref Stats.Interactable.AppearanceNull); 
                        
                    }
                    break;

                // Moves an interactable
                case InteractableAction.Movement:
                    MongoObjectId moveId = (MongoObjectId)msg.ReadBytes(12);
                    Int32 moveToX = msg.ReadInt32();
                    Int32 moveToY = msg.ReadInt32();
                    Byte moveToD = msg.ReadByte();

                    if (Map.Interactables.ContainsKey(moveId))
                    {
                        ProjectERA.Data.Interactable moveInteractable = Map.Interactables[moveId];

                        if (moveInteractable != null)
                        {
                            Logic.GameInteractable.MoveTo(moveInteractable, moveToX, moveToY, moveToD);
                        }
                        else
                        {
                            Interlocked.Increment(ref Stats.Interactable.MoveNull);
                        }
                    }
                    else
                    {
                        //Protocol protocol;
                        //if (this.Connection.TryGetProtocol(typeof(Interactable), out protocol))
                        //{
                            //((Interactable)protocol).Get(moveId, (a) => { Map.Interactables.Add(moveId, a); });
                        //}
                        break;
                    }

                    ERAUtils.Logger.Logger.Info(new String[] { "Received movement for ", moveId.ToString(), " coords ", moveToX.ToString(), ", ", moveToY.ToString() });
                    

                    break;

                //
                case InteractableAction.MovementTeleport:
                    MongoObjectId moveTeleportId = (MongoObjectId)msg.ReadBytes(12);
                    //MongoObjectId moveTeleportMapId = (MongoObjectId)msg.ReadBytes(12);
                    Int32 moveTeleportToX = msg.ReadInt32();
                    Int32 moveTeleportToY = msg.ReadInt32();
                    Byte moveTeleportToD = msg.ReadByte();

                    if (Map.Interactables.ContainsKey(moveTeleportId))
                    {
                        ProjectERA.Data.Interactable moveTeleportInteractable = Map.Interactables[moveTeleportId];

                        if (moveTeleportInteractable != null)
                        {
                            Logic.GameInteractable.TeleportTo(moveTeleportInteractable, moveTeleportToX, moveTeleportToY, moveTeleportToD);
                        }
                    }

                    break;
                default:
                    throw new NetException("No such action in this protocol " + this.GetType());
            }
        }

        /// <summary>
        /// Creates an Outgoing Message with the specified action integer written to it.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static NetOutgoingMessage OutgoingMessage(InteractableAction action, Int32 initialCapacity)
        {
            NetOutgoingMessage msg = _connection.MakeMessage(_protocolIdentifier, initialCapacity + ERAUtils.BitManipulation.BytesToHold((Int32)InteractableAction.Max));
            msg.WriteRangedInteger(0, (Int32)InteractableAction.Max, (Int32)action);
            msg.WritePadBits();
            return msg;
        }

        /// <summary>
        /// Disconnect protocol
        /// </summary>
        internal override void Disconnect()
        {
            // Clear cache and outstanding requests
            _outstandingInteractableRequests.Clear();
            GeneralCache<MongoObjectId, InteractableRequest>.ClearCache();

            // Disconnect base
            base.Disconnect();
        }
    }
}
