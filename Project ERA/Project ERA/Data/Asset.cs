using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ERAUtils.Enum;
using System.Security.Cryptography;
using ERAUtils;
using ProjectERA.Services.Network;
using System.Threading.Tasks;

namespace ProjectERA.Data
{
    public class Asset
    {
        private const int DefaultChunkSize = 256 * 1024; // 256 KiB MTU 1000 - 1500 -> so perhaps split chunks in 256 parts?
        private NetworkManager _networkManager;

        /// <summary>
        /// 
        /// </summary>
        public MongoObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Filename on server
        /// </summary>
        public String RemoteFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Asset Type (denotes namespace)
        /// </summary>
        internal AssetType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Alternate filenames
        /// </summary>
        internal String[] Aliases
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal String ServerMD5
        {
            get;
            private set;
        }

        /// <summary>
        /// All filenames this file will return on
        /// </summary>
        internal String[] QueryableByArray
        {
            get
            {
                String name = RemoteFileName.EndsWith(".png") ? RemoteFileName.Remove(RemoteFileName.LastIndexOf('.')) : RemoteFileName;
                String alias = RemoteFileName.EndsWith(".png") ? RemoteFileName : RemoteFileName + ".png";

                return Aliases.Concat(new List<String> { name, alias }).Distinct().ToArray();
            }
        }

        /// <summary>
        /// Get file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static AssetOperationResult GetFile(NetworkManager networkManager, AssetType type, String fileName, out Asset result)
        {
            Services.Network.Protocols.Protocol protocol = null;
            if (!networkManager.TryGetProtocol(typeof(Services.Network.Protocols.Asset), out protocol))
                throw new InvalidOperationException("No asset protocol found!");

            // Gets the file
            Asset internalResult = null;
            Task<AssetOperationResult> task = ((Services.Network.Protocols.Asset)protocol).RequestAsset(type, fileName, (succes, asset) => { if (succes) internalResult = asset; });
            Task.WaitAll(task);

            // Now process the result
            result = internalResult; 
            result._networkManager = networkManager;
            return task.Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkManager"></param>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static Task<Asset> GetFileAsync(NetworkManager networkManager, AssetType type, String fileName)
        {
            return Task<Asset>.Factory.StartNew(() => { Asset result = null; GetFile(networkManager, type, fileName, out result); return result; });
        }


        /// <summary>
        /// Downloads a file
        /// </summary>
        /// <param name="fileName">destination</param>
        internal AssetOperationResult Download(NetworkManager networkManager, String fileName)
        {
            try
            {
                // If file exists
                if (File.Exists(fileName))
                {
                    // Get MD5
                    String md5Local;
                    using (FileStream file = new FileStream(fileName, FileMode.Open))
                    {
                        md5Local = FileMD5(DefaultChunkSize, file); //MongoGridFSSettings.Defaults
                    }

                    Asset remoteCopy;
                    AssetOperationResult getResult = GetFile(networkManager, this.Type, this.RemoteFileName, out remoteCopy);

                    if (getResult != AssetOperationResult.Ok)
                        return getResult; // server error

                    if (md5Local == remoteCopy.ServerMD5)
                        return AssetOperationResult.Ok;
                }

                // File does not exists yet or is outdated
                Services.Network.Protocols.Protocol protocol = null;
                if (!networkManager.TryGetProtocol(typeof(Services.Network.Protocols.Asset), out protocol))
                    throw new InvalidOperationException("No asset protocol found!");

                // Gets the file
                TaskCompletionSource<Boolean> innerTask = new TaskCompletionSource<Boolean>();
                Task<AssetOperationResult> task = ((Services.Network.Protocols.Asset)protocol).DownloadAsset(this,
                    (succeeded, bytes) =>
                    {
                        // Save if retrieved
                        if (succeeded)
                        {
                            using (FileStream stream = File.Open(fileName, FileMode.Create))
                            {
                                stream.Write(bytes, 0, bytes.Length);
                            }
                        }

                        // Saving done
                        innerTask.SetResult(succeeded);
                    });

                Task.WaitAll(task, innerTask.Task);

                // Now process the result
                return task.Result;               
            }
            catch (IOException)
            {
                // File in use, update later
                return AssetOperationResult.InUse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override Boolean Equals(Object other)
        {
            if (other is Asset)
            {
                Asset otherAsset = (Asset)other;

                if (otherAsset.Id != MongoObjectId.Empty && this.Id != MongoObjectId.Empty &&
                    otherAsset.ServerMD5 != String.Empty && this.ServerMD5 != String.Empty)
                    return otherAsset.Id.Equals(this.Id) && this.ServerMD5 == otherAsset.ServerMD5;

                if (otherAsset.Type == this.Type)
                {
                    Asset otherServer, thisServer;

                    AssetOperationResult resultThere = Asset.GetFile(_networkManager, otherAsset.Type, otherAsset.RemoteFileName, out otherServer);
                    AssetOperationResult resultHere = Asset.GetFile(_networkManager, this.Type, this.RemoteFileName, out thisServer);

                    if (resultThere == AssetOperationResult.NotFound || resultHere == AssetOperationResult.NotFound)
                        return false;

                    if (otherServer.Id != MongoObjectId.Empty && thisServer.Id != MongoObjectId.Empty)
                        return otherServer.Id.Equals(thisServer.Id) && thisServer.ServerMD5 == thisServer.ServerMD5;

                    return false;
                }
            }
            else if (other is String)
            {
                String otherFilename = (String)other;

                Asset otherServer, thisServer;

                AssetOperationResult resultThere = Asset.GetFile(_networkManager, this.Type, this.RemoteFileName, out otherServer);
                AssetOperationResult resultHere = Asset.GetFile(_networkManager, this.Type, otherFilename, out thisServer);

                if (resultThere == AssetOperationResult.NotFound || resultHere == AssetOperationResult.NotFound)
                    return false;

                return otherServer.Equals(thisServer);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.Id.GetHashCode() * 63) ^ this.ServerMD5.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static String FileMD5(Stream stream)
        {
            return FileMD5(DefaultChunkSize, stream);
        }

        /// <summary>
        /// Calculates the md5 value
        /// </summary>
        /// <param name="gridFs">Settings</param>
        /// <param name="stream">File Stream</param>
        /// <returns></returns>
        public static String FileMD5(Int32 chunkSize, Stream stream)
        {
            var buffer = new Byte[chunkSize];
            var length = 0;

            using (var md5Algorithm = MD5.Create())
            {
                for (Int32 n = 0; true; n++)
                {
                    // might have to call Stream.Read several times to get a whole chunk
                    var bytesNeeded = chunkSize;
                    var bytesRead = 0;
                    while (bytesNeeded > 0)
                    {
                        var partialRead = stream.Read(buffer, bytesRead, bytesNeeded);
                        if (partialRead == 0)
                        {
                            break; // EOF may or may not have a partial chunk
                        }
                        bytesNeeded -= partialRead;
                        bytesRead += partialRead;
                    }
                    if (bytesRead == 0)
                    {
                        break; // EOF no partial chunk
                    }
                    length += bytesRead;

                    Byte[] data = buffer;
                    if (bytesRead < chunkSize)
                    {
                        data = new Byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, data, 0, bytesRead);
                    }

                    md5Algorithm.TransformBlock(data, 0, data.Length, null, 0);

                    if (bytesRead < chunkSize)
                    {
                        break; // EOF after partial chunk
                    }
                }

                md5Algorithm.TransformFinalBlock(new Byte[0], 0, 0);

                Byte[] hash = md5Algorithm.Hash;

                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AssetOperationResult GetServerMD5(out String result)
        {
            Asset asset;
            AssetOperationResult operationResult = GetFile(_networkManager, this.Type, this.RemoteFileName, out asset);

            result = asset.ServerMD5;

            return operationResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal static Asset Unpack(Lidgren.Network.NetIncomingMessage msg)
        {
            Asset result = new Asset();
            result.Id = new MongoObjectId(msg.ReadBytes(12));
            result.Type = (AssetType)msg.ReadUInt16();
            result.RemoteFileName = msg.ReadString();
            result.ServerMD5 = msg.ReadString();
            result.Aliases = new String[0];

            return result;
        }
    }
}