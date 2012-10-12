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
    internal partial class Asset : Protocol
    {
        /// <summary>
        /// The amount of miliseconds it takes for a DataStore request to time out
        /// </summary>
        private const Int32 AssetRequestTimeout = 1000 * 15;
        private const Int32 AssetDataRequestTimeout = 1000 * 60;

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
            get { return (Byte)ClientProtocols.Asset; }
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
        /// Current map Id
        /// </summary>
        public static MongoObjectId Id { get; set; }

        private static LinkedHashMap<MongoObjectId, AssetRequest> _outstandingAssetRequests;
        private static LinkedHashMap<MongoObjectId, AssetDataRequest> _outstandingAssetDataRequests;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">Connection to bind to</param>
        /// <param name="networkManager"></param>
        public Asset(Connection connection, NetworkManager networkManager)
            : base(connection, networkManager)
        {
            Interlocked.CompareExchange(ref _outstandingAssetRequests, new LinkedHashMap<MongoObjectId, AssetRequest>(), null);
            Interlocked.CompareExchange(ref _outstandingAssetDataRequests, new LinkedHashMap<MongoObjectId, AssetDataRequest>(), null);

            GeneralCache<MongoObjectId, AssetRequest>.InitializeCache();
            GeneralCache<MongoObjectId, AssetDataRequest>.InitializeCache();
        }

        /// <summary>
        /// Processes incoming messages for this protocol
        /// </summary>
        /// <param name="msg"></param>
        internal override void IncomingMessage(Lidgren.Network.NetIncomingMessage msg)
        {
            // Read the action bits
            AssetAction action = (AssetAction)msg.ReadRangedInteger(0, (Int32)AssetAction.Max);
            msg.SkipPadBits();

            switch (action)
            {
                // Get asset action. This is received when an asset info message was requested.
                case AssetAction.Get:

                    // The one time key is generated foreach asset info request. Asset might update
                    // between two requests, so we don't want to cache the requests. 
                    MongoObjectId getOneTimeKey = new MongoObjectId(msg.ReadBytes(12));

                    // Queues the action, since we don't want multiple requests being processed 
                    // simultainously. Maybe something will happen with the request while it is
                    // processed, so this way it is no longer a problem.
                    this.QueueAction(() =>
                        {
                            // The request is not active (anymore)
                            if (!_outstandingAssetRequests.ContainsKey(getOneTimeKey))
                            {
                                // Mark it as faulty and cancel the operation.
                                Interlocked.Increment(ref Stats.Asset.NotRequested);
                                return;
                            }

                            // Gets the operation result enumaration bits, which hold the serverside
                            // state of the request. It has to be AssetOperationResult.Ok in order to
                            // be processable.
                            ERAUtils.Enum.AssetOperationResult getOperationResult = (ERAUtils.Enum.AssetOperationResult)msg.ReadByte();
                            ProjectERA.Data.Asset resultAsset = null;

                            Logger.Debug("Get asset from server w/key " + getOneTimeKey.ToString());

                            // The asset info is unpacked if the server side retrieval passed or it is
                            // marked as faulty when it failed (server side).
                            if (getOperationResult == ERAUtils.Enum.AssetOperationResult.Ok)
                                resultAsset = ProjectERA.Data.Asset.Unpack(msg);
                            else
                                Interlocked.Increment(ref Stats.Asset.Failed);

                            // Gets the actual request from the cache, sets the result and stops the 
                            // request from being processed, while updating the cache as well.
                            AssetRequest getRequest = _outstandingAssetRequests[getOneTimeKey];
                            getRequest.Data = resultAsset;
                            getRequest.Result = getOperationResult;
                            GeneralCache<MongoObjectId, AssetRequest>.UpdateCache(getRequest);
                            _outstandingAssetRequests.Remove(getOneTimeKey);
                        }
                    );
                    
                    break;

                // Get asset chunk action. When an asset is download, the file is retrieved in parts
                // of chunks roughly equal to the MTU of the network connection. 
                case AssetAction.GetChunk:
                    MongoObjectId getChunkOneTimeKey = new MongoObjectId(msg.ReadBytes(12));

                    this.QueueAction(() =>
                       {
                           // When the chunk was not requested, we mark it as faulty.
                           if (!_outstandingAssetDataRequests.ContainsKey(getChunkOneTimeKey))
                           {
                               Interlocked.Increment(ref Stats.Asset.ChunkNotRequested);
                               return;
                           }

                           // Gets the chunk id and operation (server side) result
                           MongoObjectId getChunkId = new MongoObjectId(msg.ReadBytes(12));
                           ERAUtils.Enum.AssetOperationResult getChunkOperationResult = (ERAUtils.Enum.AssetOperationResult)msg.ReadByte();

                           Byte[] chunk = new Byte[0];
                           Int32 part = 0;

                           // When the chunk result succeeded server side, we can obtain the data
                           if (getChunkOperationResult == ERAUtils.Enum.AssetOperationResult.Ok)
                           {
                               part = msg.ReadInt32();
                               chunk = msg.ReadBytes(msg.ReadInt32());

                               Logger.Verbose("Get chunk " + getChunkId.ToString() + " from server w/key " + getChunkOneTimeKey.ToString() + "/part: " + part);
                           }

                           AssetDataRequest getChunkRequest = _outstandingAssetDataRequests[getChunkOneTimeKey];

                           // When no data was retrieved, we mark that and update the cache
                           if (chunk.Length == 0)
                           {
                               Interlocked.Increment(ref Stats.Asset.ChunkFailed);
                               getChunkRequest.Result = getChunkOperationResult;
                               GeneralCache<MongoObjectId, AssetDataRequest>.UpdateCache(getChunkRequest);
                               _outstandingAssetDataRequests.Remove(getChunkOneTimeKey);
                           }
                           else
                           {
                               // Retrieve process the partial and when the chunk is complete...
                               if (getChunkRequest.ReceivePart(getChunkId, part, chunk))
                               {
                                   // ... mark it as completed
                                   getChunkRequest.Result = ERAUtils.Enum.AssetOperationResult.Ok;
                                   GeneralCache<MongoObjectId, AssetDataRequest>.UpdateCache(getChunkRequest);
                                   _outstandingAssetDataRequests.Remove(getChunkOneTimeKey);

                                   Logger.Verbose("Chunk was the final chunk");
                               }
                           }
                        }
                    );
                    
                    break;

                // Asset Download received. This is received when an asset download request was sent. It contains
                // structural data for the download request, such as chunksize and chunkid's.
                case AssetAction.Download:
                    MongoObjectId downloadOneTimeKey = new MongoObjectId(msg.ReadBytes(12));

                    this.QueueAction(() =>
                        {
                            // When the download was not requested, we mark it as faulty.
                            if (!_outstandingAssetDataRequests.ContainsKey(downloadOneTimeKey))
                            {
                                Interlocked.Increment(ref Stats.Asset.DownloadNotRequested);
                                return;
                            }

                            ERAUtils.Enum.AssetOperationResult downloadOperationResult = (ERAUtils.Enum.AssetOperationResult)msg.ReadByte();
                            MongoObjectId[] chunkIds = null;
                            Int32 chunkSize = 0, length = 0, partialSize = 0;

                            Logger.Debug("Download from server w/key " + downloadOneTimeKey.ToString());

                            AssetDataRequest getDownloadRequest = _outstandingAssetDataRequests[downloadOneTimeKey];

                            // When the operation succeeded server side, the data is read and saved
                            if (downloadOperationResult == ERAUtils.Enum.AssetOperationResult.Ok)
                            {
                                chunkSize = msg.ReadInt32();
                                partialSize = msg.ReadInt32();
                                length = msg.ReadInt32();
                                chunkIds = new MongoObjectId[msg.ReadInt32()];
                                for (int i = 0; i < chunkIds.Length; i++)
                                    chunkIds[i] = new MongoObjectId(msg.ReadBytes(12));
                            }
                            else
                            {
                                Interlocked.Increment(ref Stats.Asset.DownloadFailed);
                                getDownloadRequest.Result = downloadOperationResult;
                            }

                            // Create the meta data and update the cache
                            getDownloadRequest.MetaData = new ProjectERA.Services.Network.Protocols.Asset.AssetDataRequest.Meta(chunkIds, chunkSize, partialSize, length);
                            GeneralCache<MongoObjectId, AssetDataRequest>.UpdateCache(getDownloadRequest);

                            // Start downloading all chunks
                            foreach (var chunkId in getDownloadRequest.MetaData.Chunks)
                                DownloadAssetChunk(getDownloadRequest, getDownloadRequest.Info.Type, chunkId);
                        }
                    );

                    break;
            }
        }

        /// <summary>
        /// Creates an Outgoing Message with the specified action integer written to it.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static NetOutgoingMessage OutgoingMessage(AssetAction action)
        {
            NetOutgoingMessage msg = _connection.MakeMessage(_protocolIdentifier);
            msg.WriteRangedInteger(0, (Int32)AssetAction.Max, (Int32)action);
            msg.WritePadBits();
            return msg;
        }

        /// <summary>
        /// Creates an Outgoing Message with the specified action integer written to it.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static NetOutgoingMessage OutgoingMessage(AssetAction action, Int32 initialCapacity)
        {
            NetOutgoingMessage msg = _connection.MakeMessage(_protocolIdentifier, initialCapacity + ERAUtils.BitManipulation.BytesToHold((Int32)AssetAction.Max));
            msg.WriteRangedInteger(0, (Int32)AssetAction.Max, (Int32)action);
            msg.WritePadBits();
            return msg;
        }

    }
}
