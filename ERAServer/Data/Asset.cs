using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Builders;
using System.IO;
using ERAUtils.Enum;
using System.Security.Cryptography;
using ERAServer.Services;

namespace ERAServer.Data
{
    public class Asset
    {
        /// <summary>
        /// 
        /// </summary>
        public ObjectId Id
        {
            get;
            protected set;
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
        public AssetType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Alternate filenames
        /// </summary>
        public String[] Aliases
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String ServerMD5
        {
            get;
            protected set;
        }

        /// <summary>
        /// All filenames this file will return on
        /// </summary>
        public String[] QueryableByArray
        {
            get
            {
                String name = RemoteFileName.EndsWith(".png") ? RemoteFileName.Remove(RemoteFileName.LastIndexOf('.')) : RemoteFileName;
                String alias = RemoteFileName.EndsWith(".png") ? RemoteFileName : RemoteFileName + ".png";

                return Aliases.Concat(new List<String> { name, alias }).Distinct().ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assetId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static AssetOperationResult GetFile(AssetType type, ObjectId assetId, out Asset result)
        {
            result = new Asset();

            MongoGridFS gridFs = new MongoGridFS(DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(type), SafeMode.True));
            MongoGridFSFileInfo file = gridFs.FindOneById(assetId);

            if (file == null || !file.Exists)
                return AssetOperationResult.NotFound;

            result.Id = file.Id.AsObjectId;
            result.RemoteFileName = file.Name;
            result.Type = type;
            result.Aliases = file.Aliases;
            result.ServerMD5 = file.MD5;

            return AssetOperationResult.Ok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assetId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static AssetOperationResult GetChunks(AssetType type, ObjectId assetId, out ObjectId[] result, out Int32 chunkSize, out Int32 length)
        {
            result = null;
            chunkSize = 0;
            length = 0;

            MongoGridFS gridFs = new MongoGridFS(DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(type), SafeMode.True));
            MongoGridFSFileInfo file = gridFs.FindOneById(assetId);

            if (file == null || !file.Exists)
                return AssetOperationResult.NotFound;

            chunkSize = file.ChunkSize;
            length = (int)file.Length;

            var numberOfChunks = (length + chunkSize - 1) / chunkSize;
            result = new ObjectId[numberOfChunks];
            for (int n = 0; n < numberOfChunks; n++)
            {
                var query = Query.And(Query.EQ("files_id", file.Id), Query.EQ("n", n));
                var chunk = gridFs.Chunks.FindOne(query);
                
                if (chunk == null)
                {
                    //String errorMessage = String.Format("Chunk {0} missing for GridFS file '{1}'.", n, file.Name);
                    //throw new MongoGridFSException(errorMessage);

                    return AssetOperationResult.InComplete;
                }

                result[n] = chunk["_id"].AsObjectId;
            }

            return AssetOperationResult.Ok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunkId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static AssetOperationResult GetChunk(AssetType type, ObjectId chunkId, out BsonBinaryData result)
        {
            result = null;

            MongoGridFS gridFs = new MongoGridFS(DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(type), SafeMode.True));
            var chunk = gridFs.Chunks.FindOne(Query.EQ("_id", chunkId));

            if (chunk == null)
            {
                return AssetOperationResult.NotFound;
            }

            result = chunk["data"].AsBsonBinaryData;

            return AssetOperationResult.Ok;
        }

        /// <summary>
        /// Get file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static AssetOperationResult GetFile(AssetType type, String fileName, out Asset result)
        {
            result = new Asset();

            MongoGridFS gridFs = new MongoGridFS(DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(type), SafeMode.True));
            MongoGridFSFileInfo file = gridFs.FindOne(fileName) ?? gridFs.FindOne(Query.EQ("aliases", fileName));

            if (file == null || !file.Exists)
                return AssetOperationResult.NotFound;

            result.Id = file.Id.AsObjectId;
            result.RemoteFileName = file.Name;
            result.Type = type;
            result.Aliases = file.Aliases;
            result.ServerMD5 = file.MD5;

            return AssetOperationResult.Ok;
        }

        /// <summary>
        /// Save file
        /// </summary>
        /// <param name="LocalImage">local file path</param>
        internal AssetOperationResult SaveFile(String fileName, String previousName)
        {
            MongoGridFS gridFs = new MongoGridFS(DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(this.Type), SafeMode.True));

            String[] queryable = QueryableByArray;
            String name = RemoteFileName.EndsWith(".png") ? RemoteFileName.Remove(RemoteFileName.LastIndexOf('.')) : RemoteFileName;

            // Get MD5
            String md5Local = String.Empty;

            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open))
                {
                    md5Local = FileMD5(gridFs.Settings, file);
                }
                this.ServerMD5 = md5Local;
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(500);
                return SaveFile(fileName, previousName); 
            }

            // Find old file
            MongoGridFSFileInfo updateFile = gridFs.FindOne(previousName);

            List<String> aliases = new List<String>();
            aliases = aliases.Union(this.QueryableByArray).ToList();

            if (updateFile == null) // Loaded file!
            {
                // Filename exists
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(Query.And(Query.EQ("filename", name), Query.NE("md5", md5Local)));
                if (matchedFile != null)
                {
                    //MessageBox.Show("There already exists a file with that name but different graphic. Please edit <" + matchedFile.Name + "> instead!", "Name already exists", MessageBoxButtons.OK);
                    return AssetOperationResult.AlreadyExists;
                }

                // Created aliases list
                aliases.Remove(name);
                this.Aliases = aliases.Distinct().ToArray();

                // Only update aliases
                matchedFile = gridFs.FindOne(Query.EQ("md5", md5Local));
                if (matchedFile != null)
                {
                    this.RemoteFileName = matchedFile.Name;

                    if (this.RemoteFileName != name)
                    {
                        gridFs.MoveTo(this.RemoteFileName, name);
                        aliases.Remove(name);
                        aliases.Add(this.RemoteFileName);
                        this.Aliases = aliases.Distinct().ToArray();
                    }

                    gridFs.SetAliases(matchedFile, this.Aliases);
                    return AssetOperationResult.Ok;
                }

                // Create new file
                ObjectId id = gridFs.Upload(fileName, name).Id.AsObjectId;
                MongoGridFSFileInfo fileInfo = gridFs.FindOneById(id);

                // Add the aliases
                gridFs.SetAliases(fileInfo, this.Aliases);
                return AssetOperationResult.Ok;
            }

            if (updateFile.Name == name && updateFile.MD5 == md5Local)
            {
                // Only update aliases 
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(name);
                aliases.Remove(matchedFile.Name);
                this.Aliases = aliases.Distinct().ToArray();

                gridFs.SetAliases(matchedFile, this.Aliases);
                return AssetOperationResult.Ok;
            }

            if (updateFile.MD5 == md5Local)
            {
                // only name changed
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(Query.And(Query.EQ("filename", name), Query.NE("md5", md5Local)));
                if (matchedFile != null)
                {
                    //MessageBox.Show("There already exists a file with that name but different graphic. Please edit <" + matchedFile.Name + "> instead!", "Name already exists", MessageBoxButtons.OK);
                    return AssetOperationResult.AlreadyExists;
                }

                gridFs.MoveTo(previousName, name);
                aliases.Remove(name);
                aliases.Add(previousName);
                this.Aliases = aliases.Distinct().ToArray();

                gridFs.SetAliases(gridFs.FindOne(name), this.Aliases);
                return AssetOperationResult.Ok;
            }

            if (updateFile.Name == name)
            {
                // Delete previous version
                gridFs.Delete(name);

                // only graphic changed
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(Query.EQ("md5", md5Local));
                if (matchedFile != null)
                {
                    aliases = aliases.Union(matchedFile.Aliases).ToList();
                    aliases.Add(matchedFile.Name);

                    // Delete all graphics that equal the new one
                    gridFs.Delete(matchedFile.Name);
                }

                aliases.Remove(name);
                this.Aliases = aliases.Distinct().ToArray();

                MongoGridFSFileInfo newFile = gridFs.Upload(fileName, name);
                gridFs.SetAliases(newFile, this.Aliases);
                return AssetOperationResult.Ok;
            }

            MongoGridFSFileInfo updatedPeekFile = gridFs.FindOne(Query.Or(Query.EQ("filename", name), Query.EQ("md5", md5Local)));
            if (updatedPeekFile != null)
            {
                //MessageBox.Show("There already exists a file with that name or that graphic. Please edit <" + updatedPeekFile.Name + "> instead!", "Graphic already exists", MessageBoxButtons.OK);
                return AssetOperationResult.AlreadyExists;
            }

            gridFs.Delete(updateFile.Name);

            aliases.Remove(name);
            aliases.Add(updateFile.Name);
            this.Aliases = aliases.Distinct().ToArray();

            MongoGridFSFileInfo newPeekFile = gridFs.Upload(fileName, name);
            gridFs.SetAliases(newPeekFile, this.Aliases);

            return AssetOperationResult.Ok;
        }

        /// <summary>
        /// Gets a file list
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        public static AssetOperationResult GetFileList(AssetType assetType, String searchQuery, out String[] result)
        {
            MongoGridFS gridFs = new MongoGridFS(DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(assetType), SafeMode.True));
            result = gridFs.Find(Query.Or(Query.Matches("filename", ".*" + searchQuery + ".*"), Query.Matches("aliases", ".*" + searchQuery + ".*"))).Select(a => a.Name).OrderBy(b => b).ToArray();

            return AssetOperationResult.Ok;
        }

        /// <summary>
        /// Downloads a file
        /// </summary>
        /// <param name="fileName">destination</param>
        public AssetOperationResult Download(String fileName)
        {
            try
            {
                String dir = Path.GetDirectoryName(fileName);

                if (String.IsNullOrWhiteSpace(dir) == false)
                    Directory.CreateDirectory(dir);

                if (File.Exists(fileName))
                {
                    // Get MD5
                    String md5Local;
                    using (FileStream file = new FileStream(fileName, FileMode.Open))
                    {
                        md5Local = FileMD5(MongoGridFSSettings.Defaults, file);
                    }

                    Asset remoteCopy; 
                    AssetOperationResult getResult = GetFile(this.Type, this.RemoteFileName, out remoteCopy);

                    if (getResult != AssetOperationResult.Ok)
                    {
                        return getResult; // inner error
                    }

                    if (md5Local == remoteCopy.ServerMD5)
                    {
                        return AssetOperationResult.Ok;
                    }
                }

                MongoGridFS gridFs = new MongoGridFS(DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(this.Type), SafeMode.True));
                gridFs.Download(fileName, Query.EQ("_id", this.Id));
            }
            catch (IOException)
            {
                // File in use, update later
                return AssetOperationResult.InUse;
            }

            return AssetOperationResult.Ok;
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

                if (otherAsset.Id != ObjectId.Empty && this.Id != ObjectId.Empty &&
                    otherAsset.ServerMD5 != String.Empty && this.ServerMD5 != String.Empty)
                    return otherAsset.Id.Equals(this.Id);

                if (otherAsset.Type == this.Type)
                {
                    Asset otherServer, thisServer;
                    
                    AssetOperationResult resultThere = Asset.GetFile(otherAsset.Type, otherAsset.RemoteFileName, out otherServer);
                    AssetOperationResult resultHere = Asset.GetFile(this.Type, this.RemoteFileName, out thisServer);

                    if (resultThere == AssetOperationResult.NotFound || resultHere == AssetOperationResult.NotFound )
                        return false;

                    if (otherServer.Id != ObjectId.Empty && thisServer.Id != ObjectId.Empty)
                        return otherServer.Id.Equals(thisServer.Id) && otherAsset.ServerMD5 == otherServer.ServerMD5;

                    return false;
                }
            }
            else if (other is String)
            {
                String otherFilename = (String)other;

                Asset otherServer, thisServer;

                AssetOperationResult resultThere = Asset.GetFile(this.Type, this.RemoteFileName, out otherServer);
                AssetOperationResult resultHere = Asset.GetFile(this.Type, otherFilename, out thisServer);

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
            return FileMD5(MongoGridFSSettings.Defaults, stream);
        }

        /// <summary>
        /// Calculates the md5 value
        /// </summary>
        /// <param name="gridFs">Settings</param>
        /// <param name="stream">File Stream</param>
        /// <returns></returns>
        public static String FileMD5(MongoGridFSSettings gridFSSettings, Stream stream)
        {
            var chunkSize = gridFSSettings.ChunkSize;
            var buffer = new Byte[chunkSize];
            var length = 0;

            using (var md5Algorithm = MD5.Create())
            {
                for (int n = 0; true; n++)
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

                    byte[] data = buffer;
                    if (bytesRead < chunkSize)
                    {
                        data = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, data, 0, bytesRead);
                    }

                    md5Algorithm.TransformBlock(data, 0, data.Length, null, 0);

                    if (bytesRead < chunkSize)
                    {
                        break; // EOF after partial chunk
                    }
                }

                md5Algorithm.TransformFinalBlock(new byte[0], 0, 0);
                return BsonUtils.ToHexString(md5Algorithm.Hash);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AssetOperationResult GetServerMD5(out String result)
        {
            Asset asset;
            AssetOperationResult operationResult = GetFile(this.Type, this.RemoteFileName, out asset);

            result = asset.ServerMD5;

            return operationResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getMessage"></param>
        internal void Pack(Lidgren.Network.NetOutgoingMessage msg)
        {
            msg.Write(this.Id.ToByteArray());
            msg.Write((UInt16)this.Type);
            msg.Write(this.RemoteFileName);
            msg.Write(this.ServerMD5);
        }
    }
}