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
using ERAUtils.Logger;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Asset : Protocol
    {
        /// <summary>
        /// Request an asset (information) from the server
        /// </summary>
        /// <param name="type">Type of the asset (namespace/collection)</param>
        /// <param name="name">Name of the asset</param>
        /// <param name="resultAction">Action to invoke on retrieval</param>
        internal Task<AssetOperationResult> RequestAsset(AssetType type, String name, Action<Boolean, ProjectERA.Data.Asset> resultAction = null)
        {
            AssetRequest req = new AssetRequest(type, name, resultAction);

            // Queue the request
            QueueAction(() =>
            {
                // If in storage
                AssetRequest assetRequest = GeneralCache<MongoObjectId, AssetRequest>.QueryCache(req.Key);
                if (assetRequest != null && assetRequest.Task != null && assetRequest.Task.Task != null && assetRequest.Task.Task.IsCompleted)
                {
                    // Result directly
                    req.Result = assetRequest.Result;
                }
                else
                {
                    // Create the outgoing request Message
                    NetOutgoingMessage msg = OutgoingMessage(AssetAction.Get, 12 + 2 + name.Length * 2);
                    msg.Write(req.Key.Id);
                    msg.Write((UInt16)req.Type);
                    msg.Write(req.Name);
                    
                    _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                    Logger.Debug("Request asset for " + req.Name + " w/key " + req.Key.ToString());

                    // Add it to outstanding Requests
                    _outstandingAssetRequests.Enqueue(req.Key, req);

                    // Start the retrieval timer
                    req.TimeOut = new Timer((object state) =>
                    {
                        // On timeout, set the result to none
                        req.Result = AssetOperationResult.TimedOut;

                        // Queue removal of request
                        QueueAction(() =>
                        {
                            _outstandingAssetRequests.Remove(req.Key);
                            Interlocked.Increment(ref Stats.Asset.RequestTimeout);
                        });

                    }, null, Asset.AssetRequestTimeout, System.Threading.Timeout.Infinite);
                    
                }
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// Downloads an asset from the server
        /// </summary>
        /// <param name="asset">Asset info</param>
        /// <param name="resultAction">Action to invoke upon retrieval</param>
        /// <param name="onChunkAction">Action on receiving a chunk</param>
        /// <param name="onPartialAction">Action on receiving a partial</param>
        internal Task<AssetOperationResult> DownloadAsset(ProjectERA.Data.Asset asset, Action<Boolean, Byte[]> resultAction = null, 
            Action<Int32, Int32, Int32, Int32> onPartialAction = null, Action<Int32, Int32> onChunkAction = null)
        {
            AssetDataRequest req = new AssetDataRequest(asset, onPartialAction, onChunkAction, resultAction);

            // Queue the request
            this.QueueAction(() =>
            {
                // If in storage
                AssetDataRequest assetRequest = GeneralCache<MongoObjectId, AssetDataRequest>.QueryCache(req.Key);
                if (assetRequest != null && assetRequest.Task != null && assetRequest.Task.Task != null && assetRequest.Task.Task.IsCompleted)
                {
                    // Result directly
                    req.Result = assetRequest.Result;
                }
                else
                {
                    // Create the outgoing request Message
                    NetOutgoingMessage msg = OutgoingMessage(AssetAction.Download, 12 + 1 + 12);
                    msg.Write(req.Key.Id);
                    msg.Write((UInt16)req.Info.Type);
                    msg.Write(req.Info.Id.Id);

                    Logger.Debug("Request asset download for " + req.Info.RemoteFileName + " w/key " + req.Key.ToString());

                    _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
                    
                    // Add it to outstanding Requests
                    _outstandingAssetDataRequests.Enqueue(req.Key, req);

                    // Start the retrieval timer
                    req.TimeOut = new Timer((object state) =>
                    {
                        // On timeout, set the result to none
                        req.Result = AssetOperationResult.TimedOut;

                        // Queue removal of request
                        QueueAction(() =>
                        {
                            _outstandingAssetDataRequests.Remove(req.Key);
                            Interlocked.Increment(ref Stats.Asset.DataRequestTimeout);
                        });

                    }, null, Asset.AssetDataRequestTimeout, System.Threading.Timeout.Infinite);
                }
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }

        /// <summary>
        /// Downloads an asset chunk
        /// </summary>
        /// <param name="req">Request to bind to</param>
        /// <param name="type">Asset type (chunk collection/namespace)</param>
        /// <param name="chunkId">Chunk id</param>
        internal void DownloadAssetChunk(AssetDataRequest req, AssetType type, MongoObjectId chunkId)
        {
            MongoObjectId oneTimeKey = req.Key;
            AssetType assetType = type;
            MongoObjectId assetChunkId = chunkId;

            this.QueueAction(() =>
                {
                    // If in storage
                    AssetDataRequest assetRequest = GeneralCache<MongoObjectId, AssetDataRequest>.QueryCache(oneTimeKey);
                    if (assetRequest != null && assetRequest.Task != null && assetRequest.Task.Task != null && assetRequest.Task.Task.IsCompleted)
                    {
                        // Result directly
                        req.Result = assetRequest.Task.Task.Result;
                    }
                    else
                    {
                        // Create the outgoing request Message
                        NetOutgoingMessage msg = OutgoingMessage(AssetAction.GetChunk, 12 + 2 + 12);
                        msg.Write(req.Key.Id);
                        msg.Write((UInt16)type);
                        msg.Write(chunkId.Id);

                        _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                        Logger.Debug(new String[] { "Request chunk for: ", req.Info.RemoteFileName, " w/id: ", chunkId.ToString(), " w/key ", req.Key.ToString()});
                    }
                });
        }

        /// <summary>
        /// This is the asynchronous request object used to store the request of assets.
        /// Upon creation a timeout is set which will invalidate the request is due time.
        /// Also, an action can be specified to be fired when the request is filled.
        /// </summary>
        private class AssetRequest : ICacheable<MongoObjectId>
        {
            /// <summary>
            /// Asset Id
            /// </summary>
            public MongoObjectId Key { get; set; }

            /// <summary>
            /// Asset Type
            /// </summary>
            public AssetType Type { get; private set; }

            /// <summary>
            /// Asset Name
            /// </summary>
            public String Name { get; private set; }

            /// <summary>
            /// Task that will yield Asset object
            /// </summary>
            internal TaskCompletionSource<AssetOperationResult> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<Boolean, ProjectERA.Data.Asset> Action { get; private set; }

            /// <summary>
            /// Retrieval Timer
            /// </summary>
            internal Timer TimeOut { get; set; }

            /// <summary>
            /// Creation of Request
            /// </summary>
            internal DateTime Creation { get; private set; }

            /// <summary>
            /// Creates a new request
            /// </summary>
            /// <param name="name">Asset type</param>
            /// <param name="type">Asset name</param>
            public AssetRequest(AssetType type, String name)
            {
                this.Key = MongoObjectId.GenerateRandom();
                this.Name = name;
                this.Type = type;
                this.Task = new TaskCompletionSource<AssetOperationResult>();
                this.Creation = DateTime.Now;
            }

            /// <summary>
            /// Creates a new request with callback action
            /// </summary>
            /// <param name="action">Action callback</param>
            /// <param name="name">Asset name</param>
            /// <param name="type">Asset type</param>
            public AssetRequest(AssetType type, String name, Action<Boolean, ProjectERA.Data.Asset> action)
                : this(type, name)
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            internal ProjectERA.Data.Asset Data
            {
                get;
                set;
            }

            /// <summary>
            /// Result
            /// </summary>
            /// <param name="player"></param>
            internal AssetOperationResult Result
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
                        this.Action.Invoke(value == AssetOperationResult.Ok, Data);

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
        /// 
        /// </summary>
        internal class AssetDataRequest : ICacheable<MongoObjectId>
        {
            private Meta _metaData;

            /// <summary>
            /// Request Id
            /// </summary>
            public MongoObjectId Key { get; set; }

            /// <summary>
            /// Requst meta information
            /// </summary>
            public ProjectERA.Data.Asset Info { get; set; }

            /// <summary>
            /// Bytes retrieved
            /// </summary>
            public Byte[] Bytes { get; private set; }

            /// <summary>
            /// Partials retrieved 
            /// </summary>
            public Boolean[][] Parts {get; private set;}

            /// <summary>
            /// Task that will yield Player object
            /// </summary>
            internal TaskCompletionSource<AssetOperationResult> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<Boolean, Byte[]> Action { get; private set; }

            /// <summary>
            /// Action on partial received
            /// </summary>
            internal Action<Int32, Int32, Int32, Int32> OnPartReceived { get; private set; }

            /// <summary>
            /// Action on complete chunk received
            /// </summary>
            internal Action<Int32, Int32> OnChunkReceived { get; private set; }

            /// <summary>
            /// Retrieval Timer
            /// </summary>
            internal Timer TimeOut { get; set; }

            /// <summary>
            /// Creation of Request
            /// </summary>
            internal DateTime Creation { get; private set; }

            /// <summary>
            /// Creates a new request
            /// </summary>
            /// <param name="asset">Asset info</param>
            public AssetDataRequest(ProjectERA.Data.Asset asset)
            {
                this.Key = MongoObjectId.GenerateRandom();
                this.Info = asset;
                this.Task = new TaskCompletionSource<AssetOperationResult>();
                this.Creation = DateTime.Now;
            }

            /// <summary>
            /// Creates a new request
            /// </summary>
            /// <param name="action">Asset info</param>
            /// <param name="asset">Action callback</param>
            public AssetDataRequest(ProjectERA.Data.Asset asset, Action<Boolean, Byte[]> action)
                : this(asset)
            {
                this.Action = action;
            }

            /// <summary>
            /// Creates a new request
            /// </summary>
            /// <param name="asset">Asset name</param>
            /// <param name="onChunkAction">Chunk received callback</param>
            /// <param name="onPartialAction">Partial received callback</param>
            public AssetDataRequest(ProjectERA.Data.Asset asset, Action<Int32, Int32, Int32, Int32> onPartialAction, Action<Int32, Int32> onChunkAction)
                : this(asset)
            {
                this.OnChunkReceived = onChunkAction;
                this.OnPartReceived = onPartialAction;
            }

            /// <summary>
            /// Creates a new request
            /// </summary>
            /// <param name="asset">Asset name</param>
            /// <param name="onChunkAction">Chunk received callback</param>
            /// <param name="onPartialAction">Partial received callback</param>
            /// <param name="onResult">Result callback</param>
            public AssetDataRequest(ProjectERA.Data.Asset asset, Action<Int32, Int32, Int32, Int32> onPartialAction, Action<Int32, Int32> onChunkAction, Action<Boolean, Byte[]> onResult)
                : this(asset, onPartialAction, onChunkAction)
            {
                this.Action = onResult;
            }

            /// <summary>
            /// Asset Request Intermediate Meta Data
            /// </summary>
            public Meta MetaData
            {
                get
                {
                    return _metaData;
                }

                set
                {
                    _metaData = value;

                    this.Bytes = new Byte[_metaData.Length];
                    this.Parts = new Boolean[_metaData.Chunks.Length][];
                    this.TimeOut.Change(Asset.AssetDataRequestTimeout, System.Threading.Timeout.Infinite);
                }
            }

            /// <summary>
            /// Receive partial
            /// </summary>
            /// <param name="chunkId">Partial is element of what chunk</param>
            /// <param name="partialIndex">Partial index in chunk</param>
            /// <param name="chunk">Data</param>
            /// <returns></returns>
            public Boolean ReceivePart(MongoObjectId chunkId, Int32 partialIndex, Byte[] chunk)
            {
                for (Int32 i = 0; i < _metaData.Chunks.Length; i++)
                {
                    if (chunkId == _metaData.Chunks[i])
                    {
                        Array.Copy(chunk, 0, this.Bytes, _metaData.ChunkSize * i + partialIndex * _metaData.ChunkPartialSize, chunk.Length);
                        if (this.Parts[i] == null)
                        {
                            if (i < _metaData.Chunks.Length - 1)
                                this.Parts[i] = new Boolean[(_metaData.ChunkSize - 1) / _metaData.ChunkPartialSize + 1];
                            else
                                this.Parts[i] = new Boolean[(_metaData.Length - (_metaData.Chunks.Length - 1) * _metaData.ChunkSize - 1) / _metaData.ChunkPartialSize + 1];
                        }

                        this.Parts[i][partialIndex] = true;

                        this.TimeOut.Change(Asset.AssetDataRequestTimeout, System.Threading.Timeout.Infinite);

                        System.Diagnostics.Debug.Write("Received " + i + " - " + partialIndex + "/" + this.Parts[i].Length);

                        // Invoke action [chunk index, chunk partials, partial count]
                        if (this.OnPartReceived != null)
                            this.OnPartReceived.Invoke(i, _metaData.Chunks.Length, this.Parts[i].Length, (_metaData.Length - 1) / _metaData.ChunkPartialSize + 1);

                        if (this.OnChunkReceived != null)
                            if (this.Parts[i].All(part => part))
                                this.OnChunkReceived.Invoke(i, _metaData.Chunks.Length);
                        break;
                    }
                }

                if (this.Parts.All(chunks => (chunks ?? new Boolean[] { false }).All(part => part))) 
                {
                    this.Result = AssetOperationResult.Ok;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Result
            /// </summary>
            internal AssetOperationResult Result
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
                        this.Action.Invoke(value == AssetOperationResult.Ok, this.Bytes);

                    // Kill actions
                    this.Action = null;
                    this.OnChunkReceived = null;
                    this.OnPartReceived = null;
                }
                get
                {
                    return this.Task.Task.Result;
                }
            }

            /// <summary>
            /// Inner Meta class
            /// </summary>
            internal struct Meta
            {
                private MongoObjectId[] _chunks;
                private Int32 _chunkSize, _length, _partialSize;

                /// <summary>
                /// Chunk id's
                /// </summary>
                public MongoObjectId[] Chunks { get { return _chunks; } private set { _chunks = value; } }

                /// <summary>
                /// Chunk size (bytes)
                /// </summary>
                public Int32 ChunkSize { get { return _chunkSize; } private set { _chunkSize = value; } }

                /// <summary>
                /// Partial size (bytes)
                /// </summary>
                public Int32 ChunkPartialSize { get { return _partialSize; } private set { _partialSize = value; } }

                /// <summary>
                /// Total Asset Data Length (bytes)
                /// </summary>
                public Int32 Length { get { return _length; } private set { _length = value; } }

                /// <summary>
                /// Creates new meta data container
                /// </summary>
                /// <param name="chunks">Chunk id's</param>
                /// <param name="chunkSize">Chunk size in bytes</param>
                /// <param name="partialSize">Partial size in bytes</param>
                /// <param name="length">Asset length in bytes</param>
                public Meta(MongoObjectId[] chunks, Int32 chunkSize, Int32 partialSize, Int32 length)
                {
                    _chunks = chunks;
                    _chunkSize = chunkSize;
                    _length = length;
                    _partialSize = partialSize;
                }
            }
        }
    }
}