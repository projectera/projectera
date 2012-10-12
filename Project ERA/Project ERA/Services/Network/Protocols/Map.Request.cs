using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Lidgren.Network;
using ProjectERA.Protocols;
using ERAUtils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ERAUtils.Enum;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Map : Protocol
    {
        /// <summary>
        /// Validate data request
        /// </summary>
        /// <param name="id">key</param>
        /// <param name="resultAction"></param>
        /// <returns></returns>
        internal Task<Int32> ValidateData(MongoObjectId id, Action<Int32> resultAction, out MongoObjectId tilesetId)
        {
            ValidationRequest req = new ValidationRequest(resultAction);

            MongoObjectId mapId = id;
            UInt32 versionMap, versionTileset;
            Int32 hashcodeMap, hashcodeTileset;

            using (Services.Data.MapManager mapManager = (Services.Data.MapManager)this.NetworkManager.Game.Services.GetService(typeof(Services.Data.MapManager)))
            {
                Services.Data.MapData mapData;
                Graphics.Sprite.TileMap mapGraphics;
                mapManager.FillMapObjects(id, out mapData, out mapGraphics);

                if (mapData == null)
                {
                    tilesetId = MongoObjectId.Empty;
                    versionTileset = 0;
                    versionMap = 0;
                    hashcodeMap = 0;
                    hashcodeTileset = 0;
                }
                else
                {
                    tilesetId = mapData.TilesetId;
                    versionMap = mapData.Version;
                    versionTileset = mapData.Version;
                    hashcodeMap = mapData.HashCode;
                    hashcodeTileset = mapData.TilesetDataHashCode;
                }
            }

            MongoObjectId deOutTilesetId = tilesetId;

            // Queue the request
            QueueAction(() =>
            {
                // Create the outgoing request Message
                NetOutgoingMessage msg = OutgoingMessage(MapAction.ValidateData, 52);
                msg.Write(req.Key.Id);
                
                // write map
                msg.Write(id.Id);
                msg.Write(versionMap);
                msg.Write(hashcodeMap);

                // write tileset
                msg.Write(deOutTilesetId.Id);
                msg.Write(versionTileset);
                msg.Write(hashcodeTileset);

                _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                _outstandingValidationRequest = req;

                // Start the retrieval timer
                req.TimeOut = new Timer((object state) =>
                {
                    // On timeout, set the result to none
                    req.Result = 0;

                    // Queue removal of request
                    QueueAction(() =>
                    {
                        _outstandingValidationRequest = null;
                    });

                }, null, Map.ValidationRequestTimeout, System.Threading.Timeout.Infinite);
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// Validate graphics request
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resultAction"></param>
        /// <returns></returns>
        internal Task<Int32> ValidateGraphics(MongoObjectId id, Action<Int32> resultAction)
        {
            ValidationRequest req = new ValidationRequest(resultAction);

            MongoObjectId mapId = id;
            MongoObjectId tilesetId = MongoObjectId.Empty;
            Data.TilesetData tilesetData;

            using (Services.Data.MapManager mapManager = (Services.Data.MapManager)this.NetworkManager.Game.Services.GetService(typeof(Services.Data.MapManager)))
            {
                Services.Data.MapData mapData;
                Graphics.Sprite.TileMap mapGraphics;
                mapManager.FillMapObjects(id, out mapData, out mapGraphics);

                if (mapData == null)
                {
                    tilesetId = MongoObjectId.Empty;
                    tilesetData = null;
                }
                else
                {
                    tilesetId = mapData.TilesetId;
                    tilesetData = mapData.TilesetData;
                }
            }

            // Queue the request
            QueueAction(() =>
            {
                // Create the outgoing request Message
                NetOutgoingMessage msg = OutgoingMessage(MapAction.ValidateGraphics);
                msg.Write(req.Key.Id);
                msg.Write(tilesetId.Id);
                                        
                using (Services.Display.TextureManager tm = (Services.Display.TextureManager)NetworkManager.Game.Services.GetService(typeof(Services.Display.TextureManager)))
                {
                    ContentManager cm = ((ProjectERA.Services.Display.ScreenManager)NetworkManager.Game.Services.GetService(typeof(ProjectERA.Services.Display.ScreenManager))).ContentManager;

                    String md5 = String.Empty;
                    Texture2D texture = tm.LoadStaticTexture(AssetPath.Get(AssetType.Tileset).Replace('.','/') + "/" + tilesetData.AssetName, cm, out md5);
                    msg.Write(texture != null ? md5 : String.Empty); // md5

                    if (texture != null)
                    {
                        tm.ReleaseStaticTexture(AssetPath.Get(AssetType.Tileset).Replace('.', '/') + "/"  +tilesetData.AssetName);
                    }

                    for (Int32 i = 0; i < tilesetData.AutotileAssetNames.Count; i++)
                    {
                        texture = tm.LoadStaticTexture(AssetPath.Get(AssetType.Autotile).Replace('.', '/') + "/" + tilesetData.AutotileAssetNames[i], cm, out md5);
                        msg.Write(texture != null ? md5 : String.Empty); // md5

                        if (texture != null)
                        {
                            tm.ReleaseStaticTexture(AssetPath.Get(AssetType.Autotile).Replace('.', '/') + "/" + tilesetData.AutotileAssetNames[i]);
                        }
                    }
                }

                _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                _outstandingValidationRequest = req;

                // Start the retrieval timer
                req.TimeOut = new Timer((object state) =>
                {
                    // On timeout, set the result to none
                    req.Result = 0;

                    // Queue removal of request
                    QueueAction(() =>
                    {
                        _outstandingValidationRequest = null;
                    });

                }, null, Map.ValidationRequestTimeout, System.Threading.Timeout.Infinite);
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// Request a map
        /// </summary>
        /// <param name="id">map id<param>
        /// <param name="ResultAction">callback</param>
        internal Task<ProjectERA.Services.Data.MapData> RequestMap(MongoObjectId id, Action<ProjectERA.Services.Data.MapData> resultAction)
        {
            MapDataRequest req = new MapDataRequest(id, resultAction);

            // Queue the request
            this.QueueAction(() =>
            {
                // If in storage
                MapDataRequest mapRequest = GeneralCache<MongoObjectId, MapDataRequest>.QueryCache(id);
                if (mapRequest != null && mapRequest.Task != null && mapRequest.Task.Task != null && mapRequest.Task.Task.Result != null)
                {
                    // Result directly
                    req.Result = mapRequest.Result;
                }
                else
                {
                    // Create the outgoing request Message
                    NetOutgoingMessage msg = OutgoingMessage(MapAction.Get, 12);
                    msg.Write(id.Id);
                    _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                    // Add it to outstanding Requests
                    if (_outstandingMapRequest == null || _outstandingMapRequest.Key != id)
                    {
                        _outstandingMapRequest = req;

                        // Start the retrieval timer
                        req.TimeOut = new Timer((object state) =>
                        {
                            // On timeout, set the result to none
                            req.Result = null;

                            // Queue removal of request
                            QueueAction(() =>
                            {
                                _outstandingMapRequest = null;
                            });

                        }, null, Map.MapDataRequestTimeout, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        req = _outstandingMapRequest;
                    }
                }
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// Requests tileset
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resultAction"></param>
        internal Task<ProjectERA.Services.Data.TilesetData> RequestTileset(MongoObjectId id, Action<ProjectERA.Services.Data.TilesetData> resultAction)
        {
            TilesetDataRequest req = new TilesetDataRequest(id, resultAction);

            // Queue the request
            this.QueueAction(() =>
            {
                // If in storage
                TilesetDataRequest tilesetRequest = GeneralCache<MongoObjectId, TilesetDataRequest>.QueryCache(id);
                if (tilesetRequest != null && tilesetRequest.Task != null && tilesetRequest.Task.Task != null && tilesetRequest.Task.Task.Result != null)
                {
                    // Result directly
                    req.Result = tilesetRequest.Result;
                }
                else
                {
                    // Create the outgoing request Message
                    NetOutgoingMessage msg = OutgoingMessage(MapAction.GetTilesetData, 12);
                    msg.Write(id.Id);
                    _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                    // Add it to outstanding Requests
                    if (_outstandingTilesetRequest == null || _outstandingTilesetRequest.Key != id)
                    {
                        _outstandingTilesetRequest = req;

                        // Start the retrieval timer
                        req.TimeOut = new Timer((object state) =>
                        {
                            // On timeout, set the result to none
                            req.Result = null;

                            // Queue removal of request
                            QueueAction(() =>
                            {
                                _outstandingTilesetRequest = null;
                            });

                        }, null, Map.TilesetDataRequestTimeout, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        req = _outstandingTilesetRequest;
                    }
                }
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// Request interactables on map
        /// </summary>
        /// <param name="id">map id</param>
        /// <param name="resultAction">result</param>
        internal Task<Boolean> RequestInteractables(MongoObjectId id, Action<Boolean> resultAction)
        {
            DataRequest req = new DataRequest(resultAction);

            // Queue the request
            this.QueueAction(() =>
            {
                // If in storage
                DataRequest dataRequest = GeneralCache<MongoObjectId, DataRequest>.QueryCache(_outstandingInteractableRequestKey);
                if (dataRequest != null && dataRequest.Task != null && dataRequest.Task.Task != null)
                {
                    // Result directly
                    req.Result = dataRequest.Result;
                }
                else
                {
                    // Create the outgoing request Message
                    NetOutgoingMessage msg = OutgoingMessage(MapAction.GetInteractables, 12);
                    msg.Write(req.Key.Id);
                    this.Connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                    // Add it to outstanding Requests
                    _outstandingDataRequests.Enqueue(req.Key, req);
                    _outstandingInteractableRequestKey = req.Key;

                    // Start the retrieval timer
                    req.TimeOut = new Timer((object state) =>
                    {
                        // On timeout, set the result to none
                        req.Result = false;

                        // Queue removal of request
                        QueueAction(() =>
                        {
                            _outstandingDataRequests.Remove(id);
                        });

                    }, null, Protocol.DataRequestTimeout, System.Threading.Timeout.Infinite);
                }
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// This is the asynchronous request object used to store the request of validation.
        /// Upon creation a timeout is set which will invalidate the request is due time.
        /// Also, an action can be specified to be fired when the request is filled.
        /// </summary>
        private class ValidationRequest
        {
            internal MongoObjectId Key { get; private set; }
            // internal Type

            /// <summary>
            /// Task that will yield Player object
            /// </summary>
            internal TaskCompletionSource<Int32> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<Int32> Action { get; private set; }

            /// <summary>
            /// Retrieval Timer
            /// </summary>
            internal Timer TimeOut { get; set; }

            /// <summary>
            /// Creation of Request
            /// </summary>
            internal DateTime Creation { get; private set; }

            /// <summary>
            /// Creates a new DataStoreRequest
            /// </summary>
            /// <param name="key">The key to request</param>
            public ValidationRequest()
            {
                this.Key = MongoObjectId.GenerateRandom();
                this.Task = new TaskCompletionSource<Int32>();
                this.Creation = DateTime.Now;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public ValidationRequest(Action<Int32> action)
                : this()
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            internal Int32 Result
            {
                set
                {
                    if (!this.Task.TrySetResult(value))
                    {

                    }

                    // Kill timout
                    if (this.TimeOut != null)
                        this.TimeOut.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                    // Run action
                    if (this.Action != null)
                        this.Action.Invoke(value);

                    // Kill action
                    this.Action = null;
                }
                get
                {
                    return this.Task.Task.Result;
                }
            }
        }

        /// <summary>
        /// This is the asynchronous request object used to store the request of mapdata.
        /// Upon creation a timeout is set which will invalidate the request is due time.
        /// Also, an action can be specified to be fired when the request is filled.
        /// </summary>
        private class MapDataRequest : ICacheable<MongoObjectId>
        {
            /// <summary>
            /// Interactable Id
            /// </summary>
            public MongoObjectId Key { get; set; }

            // internal Type

            /// <summary>
            /// Task that will yield Player object
            /// </summary>
            internal TaskCompletionSource<ProjectERA.Services.Data.MapData> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<ProjectERA.Services.Data.MapData> Action { get; private set; }

            /// <summary>
            /// Retrieval Timer
            /// </summary>
            internal Timer TimeOut { get; set; }

            /// <summary>
            /// Creation of Request
            /// </summary>
            internal DateTime Creation { get; private set; }

            /// <summary>
            /// Creates a new DataStoreRequest
            /// </summary>
            /// <param name="key">The key to request</param>
            public MapDataRequest(MongoObjectId key)
            {
                this.Key = key;
                this.Task = new TaskCompletionSource<ProjectERA.Services.Data.MapData>();
                this.Creation = DateTime.Now;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public MapDataRequest(MongoObjectId key, Action<ProjectERA.Services.Data.MapData> action)
                : this(key)
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            internal ProjectERA.Services.Data.MapData Result
            {
                set
                {
                    if (!this.Task.TrySetResult(value))
                    {

                    }

                    // Kill timout
                    if (this.TimeOut != null)
                        this.TimeOut.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                    // Run action
                    if (this.Action != null)
                        this.Action.Invoke(value);

                    // Kill action
                    this.Action = null;
                }
                get
                {
                    return this.Task.Task.Result;
                }
            }
        }

        /// <summary>
        /// This is the asynchronous request object used to store the request of tilesetdata.
        /// Upon creation a timeout is set which will invalidate the request is due time.
        /// Also, an action can be specified to be fired when the request is filled.
        /// </summary>
        private class TilesetDataRequest : ICacheable<MongoObjectId>
        {
            /// <summary>
            /// Interactable Id
            /// </summary>
            public MongoObjectId Key { get; set; }

            // internal Type

            /// <summary>
            /// Task that will yield Player object
            /// </summary>
            internal TaskCompletionSource<ProjectERA.Services.Data.TilesetData> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<ProjectERA.Services.Data.TilesetData> Action { get; private set; }

            /// <summary>
            /// Retrieval Timer
            /// </summary>
            internal Timer TimeOut { get; set; }

            /// <summary>
            /// Creation of Request
            /// </summary>
            internal DateTime Creation { get; private set; }

            /// <summary>
            /// Creates a new DataStoreRequest
            /// </summary>
            /// <param name="key">The key to request</param>
            public TilesetDataRequest(MongoObjectId key)
            {
                this.Key = key;
                this.Task = new TaskCompletionSource<ProjectERA.Services.Data.TilesetData>();
                this.Creation = DateTime.Now;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public TilesetDataRequest(MongoObjectId key, Action<ProjectERA.Services.Data.TilesetData> action)
                : this(key)
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            internal ProjectERA.Services.Data.TilesetData Result
            {
                set
                {
                    if (!this.Task.TrySetResult(value))
                    {

                    }

                    // Kill timout
                    if (this.TimeOut != null)
                        this.TimeOut.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                    // Run action
                    if (this.Action != null)
                        this.Action.Invoke(value);

                    // Kill action
                    this.Action = null;
                }
                get
                {
                    return this.Task.Task.Result;
                }
            }
        }
    }
}