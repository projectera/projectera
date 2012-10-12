using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline;
using ERAUtils.Logger;
using ERAUtils.Enum;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Data.Storage;
using ERAUtils;
using ProjectERA.Services.Network.Protocols;
using ProjectERA.Data.Enum;

namespace ProjectERA.Services.Data
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public partial class TilesetData : ICacheable<MongoObjectId>
    {

        #region Private fields
        private DataLoadResult _loadResult;
        #endregion

        #region Serializable fields
        /// <summary>
        /// Tileset ID. 
        /// Marked as id in XML.
        /// </summary>
        [ContentSerializer(ElementName = "id")]
        private Byte[] _tilesetId;

        /// <summary>
        /// Tileset Name. 
        /// Marked as name in XML.
        /// </summary>
        [ContentSerializer(ElementName = "name")]
        private String _name;

        /// <summary>
        /// Asset name. 
        /// Marked as filename in XML.
        /// </summary>
        [ContentSerializer(ElementName = "filename")]
        private String _assetName;

        /// <summary>
        /// Autotile Assetnames. 
        /// Marked as autotileFilenames [collection] and 
        /// autotileFilename [item] in XML.
        /// </summary>
        [ContentSerializer(ElementName = "autotileFilenames", CollectionItemName = "autotileFilename", AllowNull = false)]
        private List<String> _autotileAssetNames = new List<String>();

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer(ElementName = "autotileAnimationFlags", CollectionItemName="flag", Optional=true)]
        private List<Boolean> _autotileAnimationFlags = new List<Boolean>();

        /// <summary>
        /// Matrix for Tile Passages
        /// 0x00 Free to move
        /// 0x01 Cannot move down
        /// 0x02 Cannot move left
        /// 0x04 Cannot move right
        /// 0x08 Cannot move up
        /// </summary>
        [ContentSerializer(ElementName = "passages")]
        private Byte[] _passages;

        /// <summary>
        /// Matrix for Tile Priorities
        /// 0x00 no priority
        /// 0x01 y-coord priority
        /// 0x02 priority
        /// ...
        /// 0x05 priority
        /// </summary>
        [ContentSerializer(ElementName = "priorities")]
        private Byte[] _priorities;

        /// <summary>
        /// Matrix for Tile Flags
        /// 0x00 none
        /// 0x01 bush
        /// 0x02 counter
        /// 0x04 transparant passability
        /// </summary>
        [ContentSerializer(ElementName = "flags", Optional = true)]
        private Byte[] _flags;

        /// <summary>
        /// Matrix for Tile Tags
        /// 0x00 none
        /// 0x01 terrain tag 0
        /// ...
        /// 0x09 terrain tag 9
        /// </summary>
        [ContentSerializer(ElementName = "tags", Optional = true)]
        private Byte[] _tags;

        /// <summary>
        /// List of tiles that are opaque
        /// </summary>
        [ContentSerializer(ElementName = "opaqueTiles", Optional = true)]
        private Boolean[] _opaqueTiles;

        /// <summary>
        /// List of tiles that have transparant pixels
        /// </summary>
        [ContentSerializer(ElementName = "someSemiTransparantTiles", Optional = true)]
        private Boolean[] _someSemiTransparantTiles;

        /// <summary>
        /// Hash Code
        /// </summary>
        [ContentSerializer(ElementName = "hashcode", Optional = true)]
        private Int32 _hash;

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer(ElementName="tileCount", Optional = true )]
        private Int32 _tiles;
        /// <summary>
        /// Version
        /// </summary>
        [ContentSerializer(ElementName = "version")]
        private UInt32 _version;
        #endregion

        #region Properties
        /// <summary>
        /// Get/Set tileset ID
        /// </summary>
        [ContentSerializerIgnore]
        public MongoObjectId TilesetId
        {
            get { return (MongoObjectId)_tilesetId; }
            internal set { _tilesetId = value.Id; } //private
        }

        /// <summary>
        /// Tileset Name
        /// </summary>
        [ContentSerializerIgnore]
        public String Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        /// <summary>
        /// Tileset Assetname
        /// </summary>
        internal String AssetName
        {
            get { return _assetName; }
            set { _assetName = value; }
        }

        /// <summary>
        /// Autotile Assetnames
        /// </summary>
        internal List<String> AutotileAssetNames
        {
            get { return _autotileAssetNames; }
            set { _autotileAssetNames = value; }
        }

        /// <summary>
        /// IsAnimated flags list
        /// </summary>
        [ContentSerializerIgnore]
        internal List<Boolean> AutotileAnimationFlags
        {
            get { return _autotileAnimationFlags; }
            set { _autotileAnimationFlags = value; }
        }

        /// <summary>
        /// Matrix for Tile Passages
        /// 0x00 Free to move
        /// 0x01 Cannot move down
        /// 0x02 Cannot move left
        /// 0x04 Cannot move right
        /// 0x08 Cannot move up
        /// </summary>
        [ContentSerializerIgnore]
        public Byte[] Passages
        {
            get { return _passages; }
            internal set { _passages = value; }
        }

        /// <summary>
        /// Matrix for Tile Priorities
        /// 0x00 no priority
        /// 0x01 y-coord priority
        /// 0x02 priority
        /// ...
        /// 0x05 priority
        /// </summary>
        [ContentSerializerIgnore]
        public Byte[] Priorities
        {
            get { return _priorities; }
            internal set { _priorities = value; }
        }

        /// <summary>
        /// Matrix for Tile Flags
        /// 0x00 none
        /// 0x01 bush
        /// 0x02 counter
        /// </summary>
        [ContentSerializerIgnore]
        public Byte[] Flags
        {
            get { return _flags; }
            internal set { _flags = value; }
        }

        /// <summary>
        /// Matrix for Tile Tags
        /// 0x00 none
        /// 0x01 terrain tag 0
        /// ...
        /// 0x09 terrain tag 9
        /// </summary>
        [ContentSerializerIgnore]
        public Byte[] Tags
        {
            get { return _tags; }
            internal set { _tags = value; }
        }

        /// <summary>
        /// List of flags for complete opaque tiles
        /// </summary>
        [ContentSerializerIgnore]
        internal Boolean[] OpaqueTiles
        {
            get { return _opaqueTiles; }
            set { _opaqueTiles = value; }
        }

        /// <summary>
        /// List of flags for tiles with transparant pixels
        /// </summary>
        [ContentSerializerIgnore]
        internal Boolean[] SomeSemiTransparantTiles
        {
            get { return _someSemiTransparantTiles; }
            set { _someSemiTransparantTiles = value; }
        }

        /// <summary>
        /// Flag for uncomputed data
        /// </summary>
        [ContentSerializerIgnore]
        internal Boolean NeedComputation
        {
            get;
            private set;
        }

        /// <summary>
        /// Hashcode
        /// </summary>
        [ContentSerializerIgnore]
        internal Int32 HashCode
        {
            get { return _hash = GetHashCode(); }
        
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializerIgnore]
        internal Int32 Tiles
        {
            get { return _tiles; }
            set { _tiles = value; }
        }

        /// <summary>
        /// Version
        /// </summary>
        [ContentSerializerIgnore]
        internal UInt32 Version
        {
            get { return _version; }
            set { _version = value; }
        }

        [ContentSerializerIgnore]
        public DataLoadResult LoadResult
        {
            get { return _loadResult; }
            internal set { _loadResult = value; }
        }
#endregion

        public event EventHandler Loaded = delegate { };

        /// <summary>
        /// Constructor
        /// </summary>
        internal TilesetData()
        {
            // Serializing Constuctor
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tilesetId">tileset id</param>
        /// <param name="path">where to load the tileset</param>
        internal TilesetData(MongoObjectId tilesetId, String path, FileManager fileManager)
        {
            _path = path;
            this.TilesetId = tilesetId;

            Initialize(fileManager);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tilesetId">tileset id</param>
        internal TilesetData(MongoObjectId tilesetId, FileManager fileManager)
        {
            // Set ts ID
            this.TilesetId = tilesetId;

            // Init
            Initialize(fileManager);
        }

        /// <summary>
        /// Initialize Data
        /// </summary>
        internal void Initialize(FileManager fileManager)
        {
            _loadResult = DataLoadResult.None;
            IStorageDevice storage = fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine);

            if (storage.FileExists(GetContainerName(), GetFileName(this.TilesetId)))
            {
                storage.Load(GetContainerName(), GetFileName(this.TilesetId), (stream) =>
                    {
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            FetchData(IntermediateSerializer.Deserialize<TilesetData>(reader, TilesetData.GetFilePath(this.TilesetId)));
                        }

                        // Log read
                        Logger.Info(new StringBuilder("Tileset with id=").Append(this.TilesetId).Append(" was loaded from [::MACHINE::]").ToString());
                
                    });
            }
            else
            {
                if (fileManager.GetStorageDevice(FileLocationContainer.Title).FileExists(GetContainerName("Content"), GetFileName(this.TilesetId)))
                {
                    fileManager.GetStorageDevice(FileLocationContainer.Title).Load(GetContainerName("Content"), GetFileName(this.TilesetId), (stream) => 
                        {
                            using (XmlReader reader = XmlReader.Create(stream))
                                {
                                    FetchData(IntermediateSerializer.Deserialize<TilesetData>(reader, TilesetData.GetFilePath(this.TilesetId)));
                                    _loadResult |= DataLoadResult.FromTitle;
                                }
                        });
                } else {
                    // Log Error
                    Logger.Error(new StringBuilder("In ").Append(this.GetType().ToString()).Append(" the file for loading tileset with id=").Append(this.TilesetId).Append(" was not found.").ToString());
                    _loadResult |= DataLoadResult.NotFound;
                }
                
                // Save empty file or loaded file
                Save(fileManager);
            }

            // if Hash doesn't match
            if (_hash != this.HashCode)
            {
                // Log save
                Logger.Warning(new StringBuilder("Tileset with id=").Append(this.TilesetId).Append(" hashcode (f:").Append(_hash).Append("/g:").Append(this.HashCode).Append(") was invalid.").ToString());

                // Set hashcode
                _hash = this.HashCode;

                // Save new values
                Save(fileManager);

                // Set loadstatus
                _loadResult |= DataLoadResult.NotValidated;
            }
            else
            {
                Logger.Info(new StringBuilder("Tileset with id=").Append(this.TilesetId).Append(" passed validation (v:").Append(_hash).Append(")").ToString());
            }

            // Mark as loaded
            Loaded.Invoke(this, null);
        }

        /// <summary>
        /// Save this tileset to file
        /// </summary>
        internal void Save(FileManager fileManager)
        {
            fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Save(GetContainerName(), GetFileName(this.TilesetId), (stream) =>
                {
                    // Settings for the writer
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
                        IntermediateSerializer.Serialize(writer, this, TilesetData.GetFilePath(this.TilesetId));
                    }
                });
            
            // Log save
            Logger.Info(new StringBuilder("Tileset with id=").Append(this.TilesetId).Append(" was saved to file in [::MACHINE::].").ToString());           
        }

        /// <summary>
        /// Fetch all data
        /// <param name="source">Source to load from</param>
        /// </summary>
        private void FetchData(TilesetData source)
        {
            if (this.TilesetId != source.TilesetId)
            {
#if DEBUG && NOFAILSAFE
                throw (new InvalidContentException("Tilesetdata contents do not match expected contents"));
#endif
                _loadResult |= DataLoadResult.Invalid;
            }

            this.Name = source.Name;
            this.AssetName = source.AssetName;
            this.AutotileAssetNames = source.AutotileAssetNames;
            this.Passages = source.Passages;
            this.Priorities = source.Priorities;
            this.Flags = source.Flags;
            this.Tags = source.Tags;
            this.Tiles = source.Tiles;
            this.Version = source.Version;

            // Create flag arrays -- filled in when loading graphics
            if (source.SomeSemiTransparantTiles == null || source.OpaqueTiles == null)
                this.NeedComputation = true;

            if (this.Passages != null)
            {
                this.SomeSemiTransparantTiles = this.NeedComputation ? new Boolean[this.Passages.Length] : source.SomeSemiTransparantTiles;
                this.OpaqueTiles = this.NeedComputation ? new Boolean[this.Passages.Length] : source.OpaqueTiles;
            }
            else
            {
                _loadResult |= DataLoadResult.Invalid;
            }

            _hash = source.HashCode;
        }

        /// <summary>
        /// Compute Hashcode
        /// </summary>
        /// <returns>Hashcode</returns>
        public override Int32 GetHashCode()
        {
            Int32 code = 0;

            if (this.AutotileAssetNames != null)
                foreach (String item in this.AutotileAssetNames)
                    code = (code * 3 + item.Length) ^ item.GetHashCode();

            if (this.Passages != null)
                foreach (Byte item in this.Passages)
                    code = (code * 7 + item) ^ item.GetHashCode();

            if (this.Priorities != null)
                foreach (Byte item in this.Priorities)
                    code = (code * 19 + item) ^ item.GetHashCode();

            if (this.Flags != null)
                foreach (Byte item in this.Flags)
                    code = (code * 31 + item) ^ item.GetHashCode();

            if (this.Tags != null)
                foreach (Byte item in this.Tags)
                    code = (code * 61 + item) ^ item.GetHashCode();

            if (this.AssetName != null)
                code = this.AssetName.GetHashCode() ^ (code * 127);

            code = (Int32)(this.Version * 255) ^ code;

            return code;
        }

        /// <summary>
        /// Equality Check
        /// </summary>
        /// <param name="other">To check against</param>
        /// <returns>Equality Flag</returns>
        public override bool Equals(object obj)
        {
            if (typeof(TilesetData) == obj.GetType())
            {
                TilesetData other = (TilesetData)obj;

                return other.HashCode == this.HashCode &&
                    other.Version == this.Version &&
                    other.Passages == this.Passages &&
                    other.Priorities == this.Priorities &&
                    other.AutotileAssetNames == this.AutotileAssetNames &&
                    other.AssetName == this.AssetName;
            }

            return false;
        }

        private String _path = ".";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tilesetId"></param>
        /// <returns></returns>
        internal static String GetFilePath(MongoObjectId tilesetId)
        {
            return GetFilePath(tilesetId.Id);
        }

        /// <summary>
        /// Get file path
        /// </summary>
        /// <param name="tilesetId">tileset id</param>
        /// <returns>File path</returns>
        internal static String GetFilePath(Byte[] tilesetId)
        {
            return String.Join(@"\", GetContainerName(), GetFileName(tilesetId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static String GetContainerName()
        {
            return GetContainerName(".");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        internal static String GetContainerName(String prefix)
        {
            return String.Join(@"\", prefix, "Data", "Tilesets");
        }

        /// <summary>
        /// Get file name
        /// </summary>
        /// <param name="tilesetId">tileset id</param>
        /// <returns>File name</returns>
        internal static String GetFileName(MongoObjectId tilesetId)
        {
            return GetFileName(tilesetId.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tilesetId"></param>
        /// <returns></returns>
        internal static String GetFileName(Byte[] tilesetId)
        {
        
            StringBuilder pathBuilder = new StringBuilder("Tileset-");
            Int32 counter = 0;

            if (tilesetId == null)
            {
                pathBuilder.Append("null");
            }
            else
            {

                foreach (Byte b in tilesetId)
                {
                    pathBuilder.Append(b);
                    counter++;
                    if (counter == 4 || counter == 8)
                    {
                        pathBuilder.Append("-");
                    }
                }
                pathBuilder.Append(".xml");
            }

            return pathBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal Boolean Exists(FileManager fileManager)
        {
            if (fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).FileExists(GetContainerName(), GetFileName(this.TilesetId)))
                return true;

            if (fileManager.GetStorageDevice(FileLocationContainer.Title).FileExists(GetContainerName("Content"), GetFileName(this.TilesetId)))
                return true;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Delete(FileManager fileManager)
        {
            fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Delete(GetContainerName(), GetFileName(this.TilesetId));
            Logger.Info(new StringBuilder("Tileset with id=").Append(this.TilesetId).Append(" was removed from [::MACHINE::].").ToString());           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static TilesetData CreateFromGraphicFile(MongoObjectId id, String name, String fileName, Int32 width, Int32 height, List<String> autotileAssets)
        {
            TilesetData result = new TilesetData();
            result.TilesetId = id;
            result.Name = name;
            result.AssetName = fileName;
            result.AutotileAssetNames = autotileAssets;

            Logger.Info(new String[] { id.ToString(), name, fileName });

            Int32 numids = Math.Max(385, 384 + ((width == (8 * 32)) ? (height / 32) * 8 : (width / (8 * 32) * (2048 / 32 * 8))));
            result.Passages = new Byte[numids];
            result.Priorities = new Byte[numids];
            result.Tags = new Byte[numids];
            result.Flags = new Byte[numids];
            result.Tiles = numids - 384;
            result.Version = 1;

            return result;
        }

        /// <summary>
        /// Export to path
        /// </summary>
        /// <param name="path">Path to store</param>
        public void Export(String path)
        {
            String filename = GetFilePath(this.TilesetId);
            filename = filename.Substring(filename.LastIndexOf(@"\"));

            using(FileStream fs = new FileStream(path + filename, FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true; 
                
                using (XmlWriter writer = XmlWriter.Create(fs, settings))
                {
                    IntermediateSerializer.Serialize(writer, this, null);
                }
            }
        }

        #region ICacheable<MongoObjectId> Members

        [ContentSerializerIgnore]
        public MongoObjectId Key
        {
            get { return (MongoObjectId)_tilesetId; }
        }

        #endregion
    }
}
