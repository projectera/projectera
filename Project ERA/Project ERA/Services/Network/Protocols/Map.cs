using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Protocols;
using System.Threading.Tasks;
using ERAUtils.Logger;
using Lidgren.Network;
using ERAUtils;
using ProjectERA.Services.Data.Storage;
using System.Threading;
using ProjectERA.Data.Enum;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Map : Protocol
    {
        /// <summary>
        /// The amount of miliseconds it takes for a DataStore request to time out
        /// </summary>
        private const Int32 ValidationRequestTimeout = 1000 * 3; 
        private const Int32 MapDataRequestTimeout = 1000 * 15;
        private const Int32 TilesetDataRequestTimeout = 1000 * 15;

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
            get { return (Byte)ClientProtocols.Map; }
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
        /// Event that fires when an interactable joins
        /// </summary>
        public static event EventHandler OnInteractableJoined;
        /// <summary>
        /// Event that fires when an interactable leaves
        /// </summary>
        public static event EventHandler OnInteractableLeft;

        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler OnNotification;

        /// <summary>
        /// Dictionairy of interactables
        /// </summary>
        internal static Dictionary<MongoObjectId, ProjectERA.Data.Interactable> Interactables
        {
            get;
            set;
        }

        /// <summary>
        /// Current map Id
        /// </summary>
        public static MongoObjectId Id { get; set; }

        private static ValidationRequest _outstandingValidationRequest;
        private static MapDataRequest _outstandingMapRequest;
        private static TilesetDataRequest _outstandingTilesetRequest;
        private static MongoObjectId _outstandingInteractableRequestKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">Connection to bind to</param>
        /// <param name="networkManager"></param>
        public Map(Connection connection, NetworkManager networkManager)
            : base(connection, networkManager)
        {
            OnInteractableJoined = delegate { };
            OnInteractableLeft = delegate { };
            OnNotification = delegate { };

            Map.Interactables = new Dictionary<MongoObjectId, ProjectERA.Data.Interactable>();
            Map.Id = MongoObjectId.Empty;

            Interlocked.CompareExchange(ref _outstandingMapRequest, null, null);
            Interlocked.CompareExchange(ref _outstandingTilesetRequest, null, null);
            Interlocked.CompareExchange(ref _outstandingValidationRequest, null, null);
            Interlocked.CompareExchange(ref _outstandingDataRequests, new LinkedHashMap<MongoObjectId, DataRequest>(), null);

            GeneralCache<MongoObjectId, MapDataRequest>.InitializeCache();
            GeneralCache<MongoObjectId, TilesetDataRequest>.InitializeCache();
            GeneralCache<MongoObjectId, DataRequest>.InitializeCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        internal override void IncomingMessage(Lidgren.Network.NetIncomingMessage msg)
        {
            MapAction action = (MapAction)msg.ReadRangedInteger(0, (Int32)MapAction.Max);
            msg.SkipPadBits();
            switch (action)
            {
                // Gets mapData
                case MapAction.Get:
                    Data.MapData resultMap = Map.UnpackMapData(msg);

                    // Save it
                    resultMap.Save(((FileManager)NetworkManager.Game.Services.GetService(typeof(FileManager))));
                    ((Services.Data.MapManager)NetworkManager.Game.Services.GetService(typeof(Services.Data.MapManager))).ReloadMapData(resultMap.MapId);

                    if (_outstandingMapRequest != null && _outstandingMapRequest.Key == resultMap.MapId)
                    {
                        _outstandingMapRequest.Result = resultMap;
                        GeneralCache<MongoObjectId, MapDataRequest>.UpdateCache(_outstandingMapRequest);
                    }
                    break;

                // Gets tileset data
                case MapAction.GetTilesetData:
                    Data.TilesetData resultTileset = Map.UnpackTilesetData(msg);

                    // Save it
                    resultTileset.LoadResult = DataLoadResult.Downloaded;
                    resultTileset.Save(((FileManager)NetworkManager.Game.Services.GetService(typeof(FileManager))));
                    ((Services.Data.MapManager)NetworkManager.Game.Services.GetService(typeof(Services.Data.MapManager))).ReloadTilesetData(resultTileset.TilesetId);

                    if (_outstandingTilesetRequest != null && _outstandingTilesetRequest.Key == resultTileset.TilesetId)
                    {
                        _outstandingTilesetRequest.Result = resultTileset;
                        GeneralCache<MongoObjectId, TilesetDataRequest>.UpdateCache(_outstandingTilesetRequest);
                    }
                    break;

                // Gets array of interactable id's on this map
                case MapAction.GetInteractables:
                    // Get protocol
                    Protocol iProtocol;
                    this.NetworkManager.TryGetProtocol((Byte)ClientProtocols.Interactable, out iProtocol);
                    Interactable interactableProtocol = iProtocol as Interactable;

                    // Get player id's
                    Int32 iPart = msg.ReadInt32();
                    Int32 iParts = msg.ReadInt32();
                    Int32 iCount = msg.ReadInt32();
                    Task<ProjectERA.Data.Interactable>[] getInteractableTasks = new Task<ProjectERA.Data.Interactable>[iCount];

                    MongoObjectId currentMapIdgi = Map.Id;

                    Logger.Info(String.Format("Received {0} interactables [{1}/{2}]", iCount.ToString(), iPart + 1, iParts));

                    while (iCount-- > 0)
                    {
                        MongoObjectId currentId = (MongoObjectId)msg.ReadBytes(12);
                        getInteractableTasks[iCount] = interactableProtocol.Get(currentId,
                            (interactable) => QueueAction(() => {
                                if (Map.Id.Equals(currentMapIdgi))
                                {
                                    if (Map.Interactables.ContainsKey(currentId))
                                        Map.Interactables.Remove(currentId);

                                    Map.Interactables.Add(currentId, interactable);
                                }
                            }));
                    }

                    DataRequest req;
                    if (_outstandingDataRequests != null && _outstandingDataRequests.TryGetValue(_outstandingInteractableRequestKey, out req))
                    {
                        if (req.ReceivePartial(iPart, iParts))
                        {
                            _outstandingDataRequests.Remove(_outstandingInteractableRequestKey);
                            _outstandingInteractableRequestKey = MongoObjectId.Empty;

                            Logger.Info("Received all interactables");
                        }
                    }
                    break;

                // Validates Data
                case MapAction.ValidateData:
                    Int32 validationField;
                    MongoObjectId validationKey = Map.UnpackValidation(msg, out validationField);

                    Logger.Info("Validation [" + validationKey.ToString() + "] resulted in  [ m: " + 
                        ((validationField & (1 << 0)) == (1 << 0) ? "valid" : "needs update") + "/t: " + 
                        ((validationField & (1 << 1)) == (1 << 1) ? "valid" : "needs update") + "]");

                    if (_outstandingValidationRequest != null && _outstandingValidationRequest.Key == validationKey)
                    {
                        _outstandingValidationRequest.Result = validationField;
                    }
                    break;

                // Validates Graphics
                case MapAction.ValidateGraphics:
                    Int32 validationField2;
                    MongoObjectId validationKey2 = Map.UnpackValidation(msg, out validationField2);

                    Logger.Info("Validation [" + validationField2.ToString() + "] resulted in  [ t: " +
                        ((validationField2 & (1 << 0)) == (1 << 0) ? "valid" : "needs update") + "/a: " +
                        ((validationField2 & (255)) == (255) ? "valid" : "needs update") + "]");

                    if (_outstandingValidationRequest != null && _outstandingValidationRequest.Key == validationKey2)
                    {
                        _outstandingValidationRequest.Result = validationField2;
                    }
                    break;

                // Event: when an interactable joins
                case MapAction.InteractableJoined:
                    MongoObjectId joinedInteractableId = (MongoObjectId)msg.ReadBytes(12); // TODO: UPDATE POSITION AND APPEARANCE STUFF
                    MongoObjectId joinedInteractableMapId = (MongoObjectId)msg.ReadBytes(12);

                    Logger.Info("Interactable [" + joinedInteractableId.ToString() + "] joined Map [" + joinedInteractableMapId.ToString() + "]");

                    if (joinedInteractableMapId.Equals(Map.Id) == false)
                        break;

                    // Get protocol
                    Protocol iJoinedProtocol;
                    this.NetworkManager.TryGetProtocol((Byte)ClientProtocols.Interactable, out iJoinedProtocol);
                    Interactable interactableJProtocol = iJoinedProtocol as Interactable;

                    MongoObjectId currentMapIdij = Map.Id;

                    // Get Interactable
                    Task<ProjectERA.Data.Interactable> interactableJoinedTask = interactableJProtocol.Get(joinedInteractableId, 
                        (interactable) => QueueAction(() => {
                            if (interactable == null)
                            {
                                Interlocked.Increment(ref Stats.Interactable.JoinNull);
                                return;
                            }

                            if (Map.Id.Equals(currentMapIdij))
                            {
                                // update position
                                interactable.MapX = msg.ReadInt32();
                                interactable.MapY = msg.ReadInt32();
                                interactable.Appearance.MapDir = msg.ReadByte();

                                if (Map.Interactables.ContainsKey(joinedInteractableId) == false)
                                {
                                    Map.Interactables.Add(joinedInteractableId, interactable);
                                }
                                OnInteractableJoined.Invoke(interactable, EventArgs.Empty);
                            }
                        }));
                    break;

                // Event: when an interactable leaves
                case MapAction.InteractableLeft:
                    MongoObjectId leftInteractableId = (MongoObjectId)msg.ReadBytes(12);
                    MongoObjectId leftInteractableMapId = (MongoObjectId)msg.ReadBytes(12);

                    Logger.Info("Interactable [" + leftInteractableId.ToString() + "] left Map [" + leftInteractableMapId.ToString() + "]");

                    if (leftInteractableMapId.Equals(Map.Id))
                    {
                        if (Map.Interactables.ContainsKey(leftInteractableId))
                        {
                            ProjectERA.Data.Interactable leftInteractable = Map.Interactables[leftInteractableId];
                            OnInteractableLeft.Invoke(leftInteractable, EventArgs.Empty);
                            Map.Interactables.Remove(leftInteractableId);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates an Outgoing Message with the specified action integer written to it.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static NetOutgoingMessage OutgoingMessage(MapAction action)
        {
            NetOutgoingMessage msg = _connection.MakeMessage(_protocolIdentifier);
            msg.WriteRangedInteger(0, (Int32)MapAction.Max, (Int32)action);
            msg.WritePadBits();
            return msg;
        }

        /// <summary>
        /// Creates an Outgoing Message with the specified action integer written to it.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static NetOutgoingMessage OutgoingMessage(MapAction action, Int32 initialCapacity)
        {
            NetOutgoingMessage msg = _connection.MakeMessage(_protocolIdentifier, initialCapacity + ERAUtils.BitManipulation.BytesToHold((Int32)MapAction.Max));
            msg.WriteRangedInteger(0, (Int32)MapAction.Max, (Int32)action);
            msg.WritePadBits();
            return msg;
        }

    }
}
