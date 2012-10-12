using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Xml.Serialization;
using ERAUtils.Logger;
using ProjectERA.Services.Data.Storage;
using ERAUtils;
using ProjectERA.Services.Network.Protocols;
using ERAUtils.Enum;

namespace ProjectERA.Services.Data
{
    /// <summary>
    /// The MapData object reflects more or less Game.Map [and RPG::Map] from
    /// RMXP. It holds all data loaded from an XML file and holds all event and
    /// monster data collected sofar - usually from the networkmanager.
    /// </summary>
    [Serializable]
    internal partial class MapData : ICacheable<MongoObjectId>
    {

        internal static readonly Byte Layers = 3;

        #region Serializable fields
        [ContentSerializer(ElementName = "id")]
        private Byte[] _mapId;
        [ContentSerializer(ElementName = "innerId", Optional = true)]
        private Byte _innerId;
        [ContentSerializer(ElementName = "innerIds", Optional = true)]
        private Byte[] _innerIds;
        [ContentSerializer(ElementName = "tilesetId")]
        private Byte[] _tilesetId;
        [ContentSerializer(ElementName = "regionId")]
        private Byte[] _regionId;
        [ContentSerializer(ElementName = "mapName")]
        private String _name;
        [ContentSerializer(ElementName = "mapTilesWidth")]
        private UInt16 _width;
        [ContentSerializer(ElementName = "mapTilesHeight")]
        private UInt16 _height;
        [ContentSerializer(ElementName = "mapType")]
        private MapType _mapType;
        [ContentSerializer(ElementName = "mapSettings", Optional = true)]
        private MapSettings _mapSettings;
        [ContentSerializer(ElementName = "mapData", AllowNull = false)]
        private UInt16[][][] _mapData = new UInt16[0][][];
        [ContentSerializer(ElementName = "hashcode", Optional = true)]
        private Int32 _hash;
        [ContentSerializer(ElementName = "version")]
        private UInt32 _version;
        #endregion

        #region Ignored fields
        [ContentSerializerIgnore]
        private TilesetData _tilesetData;

        /// <summary>
        /// Gets/Sets Map ID
        /// </summary>
        [ContentSerializerIgnore]
        internal MongoObjectId MapId
        {
            get { return (MongoObjectId)_mapId; }
            set { _mapId = value.Id; }
        }

        /// <summary>
        /// Gets/Sets Inner Map ID
        /// </summary>
        [ContentSerializerIgnore]
        internal Byte InnerMapId
        {
            get { return _innerId; }
            set { _innerId = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializerIgnore]
        internal Byte[] InnerMapIs
        {
            get { return _innerIds; }
            set { _innerIds = value; }
        }

        /// <summary>
        /// Gets/Sets Tileset ID
        /// </summary>
        [ContentSerializerIgnore]
        internal MongoObjectId TilesetId
        {
            get { return (MongoObjectId)_tilesetId; }
            set { _tilesetId = value.Id; }
        }

        /// <summary>
        /// Gets/Sets Region ID
        /// </summary>
        [ContentSerializerIgnore]
        internal MongoObjectId RegionId
        {
            get { return (MongoObjectId)_regionId; }
            set { _regionId = value.Id; }
        }

        /// <summary>
        /// Gets/Sets Map Name
        /// </summary>
        [ContentSerializerIgnore]
        internal String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets/Sets Map Width in Tiles
        /// </summary>
        [ContentSerializerIgnore]
        internal UInt16 Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Gets/Sets Map Height in Tiles
        /// </summary>
        [ContentSerializerIgnore]
        internal UInt16 Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Gets/Sets MapType
        /// </summary>
        [ContentSerializerIgnore]
        internal MapType MapType
        {
            get { return _mapType; }
            set { _mapType = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializerIgnore]
        internal MapSettings MapSettings
        {
            get { return _mapSettings; }
            set { _mapSettings = value; }
        }

        /// <summary>
        /// Gets/Sets Tile Data
        /// </summary>
        [ContentSerializerIgnore]
        internal UInt16[][][] TileData
        {
            get { return _mapData; }
            set { _mapData = value; }
        }

        /// <summary>
        /// Gets HashCode
        /// </summary>
        [ContentSerializerIgnore]
        internal Int32 HashCode
        {
            get { return this.GetHashCode(); }
        }

        [ContentSerializerIgnore]
        internal UInt32 Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Gets Tileset HashCode
        /// </summary>
        [ContentSerializerIgnore]
        internal Int32 TilesetDataHashCode
        {
            get { return this.TilesetData.HashCode; }
        }

        /// <summary>
        /// Gets TilesetData
        /// </summary>
        [ContentSerializerIgnore]
        internal TilesetData TilesetData
        {
            get { return _tilesetData; }
            set { _tilesetData = value; }
        }

        [ContentSerializerIgnore]
        internal Dictionary<MongoObjectId, ProjectERA.Data.Interactable> Interactables
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        internal MapData()
        {
            // Constructor for Serializer
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="tsManager"></param>
        internal MapData(MongoObjectId mapId, Byte innerId, TilesetManager tsManager, FileManager fileManager)
        {
            // Set MapId
            this.MapId = mapId;
            this.InnerMapId = innerId;
            this.TilesetId = new MongoObjectId(1);

            Initialize(tsManager, fileManager);
        }

        /// <summary>
        /// Initialize mapdata
        /// </summary>
        /// <param name="tsManager"></param>
        private void Initialize(TilesetManager tsManager, FileManager fileManager)
        {
            // Get device
            IsolatedStorageStorageDevice device = (IsolatedStorageStorageDevice)fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine);
       
            // If file exists
            if (device.FileExists(GetContainerName(), GetFileName(this.MapId, this.InnerMapId), FileLocationContainer.IsolatedMachine))
            {
                // Load it
                device.Load(GetContainerName(), GetFileName(this.MapId, this.InnerMapId), stream =>
                    {
                        // On load file
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            FetchData(IntermediateSerializer.Deserialize<MapData>(reader, MapData.GetFilePath(this.MapId, this.InnerMapId)), tsManager, fileManager);
                        }

                        // Log read
                        Logger.Info(new StringBuilder("Map with id=").Append(this.MapId).Append(" was loaded from [::MACHINE::]").ToString());

                    }, FileLocationContainer.IsolatedMachine);
            }
            else
            {
                try
                {
                    using (Stream s = Microsoft.Xna.Framework.TitleContainer.OpenStream(@"Content\" + MapData.GetFilePath(this.MapId, this.InnerMapId)))
                    {
                        using (XmlReader reader = XmlReader.Create(s))
                        {
                            FetchData(IntermediateSerializer.Deserialize<MapData>(reader, @"Content\" + MapData.GetFilePath(this.MapId, this.InnerMapId)), tsManager, fileManager);
                        }
                    }

                    // Log read
                    Logger.Info(new StringBuilder("Map with id=").Append(this.MapId).Append(" was loaded from [::MACHINE::]").ToString());

                }
                catch (FileNotFoundException)
                {
                    // Log error
                    Logger.Error(new StringBuilder("In ").Append(this.GetType().ToString()).Append(" the file (n:").Append(MapData.GetFilePath(this.MapId, this.InnerMapId)).Append(") for loading map with id=").Append(this.MapId).Append(" was not found.").ToString());
                }

                // Save empty file
                Save(fileManager);
            }

            // Load TilesetData
            this.TilesetData = (tsManager == null ? new TilesetData(this.TilesetId, fileManager) : tsManager.FetchTilesetData(this.TilesetId));

            // if Hash doesn't match
            if (_hash != this.HashCode)
            {
                // Log save
                Logger.Warning(new StringBuilder("Map with id=").Append(this.MapId).Append(" hashcode (f:").Append(_hash).Append("/g:").Append(this.HashCode).Append(") was invalid.").ToString());

                // Set hashcode
                _hash = this.HashCode;

                // Save new values
                Save(fileManager);
            }
            else
            {
                Logger.Info(new StringBuilder("Map with id=").Append(this.MapId).Append(" passed validation (v:").Append(_hash).Append(")").ToString());
            }

            _passabilityGrid = new Boolean[_width][][];

            for (Int32 x = 0; x < _width; x++)
            {
                _passabilityGrid[x] = new Boolean[_height][];
                for (Int32 y = 0; y < _height; y++)
                {
                    CalculatePassable(x, y);
                }
            }

            return;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Save(FileManager fileManager)
        {
            // Save settings
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            IsolatedStorageStorageDevice device = (IsolatedStorageStorageDevice)fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine);
            device.Save(GetContainerName(), GetFileName(this.MapId, this.InnerMapId), isfs =>
                {
                    using (XmlWriter writer = XmlWriter.Create(isfs, settings))
                    {
                        IntermediateSerializer.Serialize(writer, this, null);
                    }
                }, FileLocationContainer.IsolatedMachine);
          
            // Log save
            Logger.Info(new StringBuilder("Map with id=").Append(this.MapId).Append(" was saved to file in [::MACHINE::].").ToString());           
        }

        /// <summary>
        /// Fetch Data from Source MapData
        /// </summary>
        /// <param name="source">Source to fetch from</param>
        /// <param name="tsManager">TilesetManager to fetch from</param>
        private void FetchData(MapData source, TilesetManager tsManager, FileManager fileManager)
        {
            if (this.MapId.Equals(source.MapId) == false || this.InnerMapId != source.InnerMapId)
                throw (new Exception("Mapdata contents do not match expected contents"));

            // Data load
            this.TilesetId = source.TilesetId;
            this.TilesetData = (tsManager == null ? new TilesetData(this.TilesetId, fileManager) : tsManager.FetchTilesetData(this.TilesetId));
            this.RegionId = source.RegionId;
            this.Name = source.Name;
            this.Width = source.Width;
            this.Height = source.Height;
            this.MapType = source.MapType;
            this.MapSettings = source.MapSettings;
            this.TileData = source.TileData;
            this.Version = source.Version;

            // Hashcode load
            _hash = source.HashCode;
        }

        /// <summary>
        /// Get HashCode
        /// </summary>
        /// <returns>HashCode for this object</returns>
        public override Int32 GetHashCode()
        {
            // Hash the TileData
            Int32 code = 0;
            foreach (UInt16[][] ystack in this.TileData)
                foreach (UInt16[] stack in ystack)
                    foreach (UInt16 item in stack)
                        code = (code * 31) ^ item;

            // Return the Hashcode
            return (Int32)((this.TilesetId == null ? MongoObjectId.Empty.GetHashCode() : this.TilesetId.GetHashCode() * 127) ^
                ((this.Name == null ? (String.Empty.GetHashCode()) : this.Name.GetHashCode()) * 63) ^
                (this.Width * 7) ^
                (this.Height * 15) ^
                ((Int32)this.MapType * 3) ^
                (this.Version* 255) ^
                code);
        }

        /// <summary>
        /// Equality Check
        /// </summary>
        /// <param name="obj">Obj to compare with</param>
        /// <returns>True if equal, false otherwise</returns>
        public override Boolean Equals(object obj)
        {
            if (typeof(MapData) == obj.GetType())
            {

                MapData other = ((MapData)obj);

                return other.HashCode == this.HashCode &&
                    other.Version == this.Version && 
                    other.TilesetId == this.TilesetId &&
                    other.Name == this.Name &&
                    other.Width == this.Width &&
                    other.Height == this.Height &&
                    other.MapType == this.MapType &&
                    other.TileData.Equals(this.TileData);
            }

            return false;
        }

        /// <summary>
        /// Determine Valid Coordinates
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns></returns>
        internal Boolean IsValid(Int32 x, Int32 y)
        {
            return (x >= 0 && x < this.Width && y >= 0 && y < this.Height);
        }

        /// <summary>
        /// Determine if Passable
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="d">direction (0,2,4,6,8,10)
        /// 0,10 = determine if all directions are impassable</param>
        /// <param name="self_event">Self (If event is determined passable)</param>
        /// <returns></returns>
        internal Boolean IsPassable(Int32 x, Int32 y, Byte d, ProjectERA.Data.Interactable self_event)
        {
            // If coordinates given are outside of the map
            if (IsValid(x, y) == false)
                // Impassable
                return false;

            // Change direction (0,2,4,6,8,10) to obstacle bit (0,1,2,4,8,0)
            Int32 bit = (1 << (d / 2 - 1)) & 0x0f;

            // Get Events to Consider
            List<ProjectERA.Data.Interactable> eventsOnPos = Interactables.Values.ToList().FindAll(ev => ev != self_event && ev.MapX == x && ev.MapY == y);
            
            // Find Events that Match passability bit or blocking
            if (eventsOnPos.Any(ev => ev.IsBlocking || (ev.HasAppearance && ev.Appearance.TileId > 0 && ((ev.PassagesBits & bit) != 0 || (ev.PassagesBits & 0x0f) == 0x0f))))
                // If obstacle bit is set in this or all directions
                // Impassable
                return false;

            // If event is on ground and not transparant passability
            if (eventsOnPos.Any(ev => (ev.HasAppearance && this.TilesetData.Priorities[ev.Appearance.TileId] == 0 && !ev.IsTransparantBlocking))) 
                // If priorities other than that are 0
                // Passable
                return true;
            
            // No Blockades Found
            return _passabilityGrid[x][y][bit];
        }

        Boolean[][][] _passabilityGrid;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal void CalculatePassable(Int32 x, Int32 y)
        {
            _passabilityGrid[x][y] = (_passabilityGrid[x][y] != null ? _passabilityGrid[x][y] : new Boolean[9]);
            if (this.TilesetData.LoadResult == ProjectERA.Data.Enum.DataLoadResult.NotFound || 
                this.TilesetData.LoadResult ==  ProjectERA.Data.Enum.DataLoadResult.Invalid)
                return;

            for (Int32 bit = 0; bit < 10; bit = bit == 0 ? 1 : bit*2)
            {
                // Loop searches in order from top of layer
                for (Int32 i = 2; i >= 0; i--)
                {
                    // Get Tile Id
                    UInt16 tile_id = this.TileData[x][y][i];
                    // Tile ID acquistion failure or transparant tile
                    if (tile_id == 0 || ((TilesetFlags)this.TilesetData.Flags[tile_id]).HasFlag(TilesetFlags.TransparantPassability))
                    {
                        // next layer
                        continue;
                    }
                    // If obstacle bit is set (this or in all directions)
                    else if ((this.TilesetData.Passages[tile_id] & bit) != 0 || (this.TilesetData.Passages[tile_id] & 0x0f) == 0x0f)
                    {
                        // Impassable
                        _passabilityGrid[x][y][bit] = false;
                        break;
                    }
                    // If this tile is blocking
                    else if (this.TilesetData.Flags != null && (((TilesetFlags)this.TilesetData.Flags[tile_id]).HasFlag(TilesetFlags.Blocking)))
                    {
                        _passabilityGrid[x][y][bit] = false;
                        break;
                    }
                    // If priorities other than that are 0 
                    else if (this.TilesetData.Priorities[tile_id] == 0)
                    {
                        // Passable
                        _passabilityGrid[x][y][bit] = true;
                        break;
                    }

                    if (i == 0)
                    {
                        _passabilityGrid[x][y][bit] = true;
                    }
                }
                
            }

            _passabilityGrid[x][y][3] = _passabilityGrid[x][y][1] & _passabilityGrid[x][y][2];
            _passabilityGrid[x][y][5] = _passabilityGrid[x][y][1] & _passabilityGrid[x][y][4];
            _passabilityGrid[x][y][6] = _passabilityGrid[x][y][2] & _passabilityGrid[x][y][4];
            _passabilityGrid[x][y][7] = _passabilityGrid[x][y][1] & _passabilityGrid[x][y][2] & _passabilityGrid[x][y][4];
        }

        /// <summary>
        /// Determine if Passable
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="d">direction (0,2,4,6,8,10)
        /// 0,10 = determine if all directions are impassable</param>
        /// <returns></returns>
        internal Boolean IsPassable(Int32 x, Int32 y, Byte d)
        {
            return IsPassable(x, y, d, null);
        }

        /// <summary>
        /// Determine Thicket
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns></returns>
        internal Boolean IsBush(Int32 x, Int32 y)
        {
            if (!this.MapId.Equals(MongoObjectId.Empty) && x > 0 && y > 0 && x < this.Width && y < this.Height)
            {
                // Loop searches in order from top of layer
                for (Int32 i = 2; i >= 0; i--)
                {
                    // Get Tile Id
                    UInt16 tile_id = this.TileData[x][y][i];
                    // If failed to find...
                    if (this.TilesetData.Flags != null && (((TilesetFlags)this.TilesetData.Flags[tile_id]).HasFlag(TilesetFlags.Bush)))
                        // This is A bush
                        return true;
                }
            }
            // No Bush Found
            return false;
        }
        /// <summary>
        /// Determine Counter
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns></returns>
        internal Boolean IsCounter(Int32 x, Int32 y)
        {
            if (!this.MapId.Equals(MongoObjectId.Empty) && x > 0 && y > 0 && x < this.Width && y < this.Height)
            {
                // Loop searches in order from top of layer
                for (Int32 i = 2; i >= 0; i--)
                {
                    // Get Tile Id
                    UInt16 tile_id = this.TileData[x][y][i];
                    // If failed to find...
                    if (tile_id == 0)
                        // Defaults to no Counter
                        return false;
                    // If Counter is Set
                    else if (this.TilesetData.Flags != null && (((TilesetFlags)this.TilesetData.Flags[tile_id]).HasFlag(TilesetFlags.Counter)))
                        // This is A Counter
                        return true;
                }
            }
            // No Counter Found
            return false;

        }
        /// <summary>
        /// Get Terrain Tag
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns></returns>
        internal Byte TerrainTag(UInt16 x, UInt16 y)
        {
            if (!this.MapId.Equals(MongoObjectId.Empty) && x > 0 && y > 0 && x < this.Width && y < this.Height)
            {
                // Loop searches in order from top of layer
                for (Int32 i = 2; i >= 0; i--)
                {
                    // Get Tile Id
                    UInt16 tile_id = this.TileData[x][y][i];
                    // If failed to find...
                    if (tile_id == 0)
                        // Defaults to no Tag
                        return 0;
                    // If Counter is Set
                    else if (this.TilesetData.Tags[tile_id] > 0)
                        // This is A Tag
                        return this.TilesetData.Tags[tile_id];
                }
            }
            // No Terrain Tag Found
            return 0;
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        internal void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            foreach (ProjectERA.Data.Interactable gc in this.Interactables.Values)
                gc.Update(gameTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static String GetContainerName()
        {
            return @"Data\Maps";
        }

        /// <summary>
        /// Get file Name
        /// </summary>
        /// <param name="mapId">Map Id</param>
        /// <param name="inner">Inner map Id</param>
        /// <returns>File name</returns>
        internal static String GetFileName(MongoObjectId mapId, Byte inner)
        {
            StringBuilder pathBuilder = new StringBuilder(@"Map-");
            Int32 counter = 0;
            foreach (Byte b in mapId.Id)
            {
                pathBuilder.Append(b);
                counter++;
                if (counter == 4 || counter == 8)
                {
                    pathBuilder.Append("-");
                }
            }
            pathBuilder.Append(".xml");

            return pathBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="inner"></param>
        /// <returns></returns>
        internal static String GetFilePath(MongoObjectId mapId, Byte inner)
        {
            return String.Join(@"\", GetContainerName(), GetFileName(mapId, inner));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="inner"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static MapData GenerateFrom(MongoObjectId mapId, Byte inner, UInt16[][][] data, MongoObjectId tileid)
        {
            MapData result = new MapData();
            result.TileData = data;
            result.MapId = mapId;
            result.InnerMapId = inner;
            result.Width = (UInt16)data.Length;
            result.Height = (UInt16)data[0].Length;
            result.TilesetId = tileid;

            return result;
        }

        #region ICacheable<MongoObjectId> Members

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializerIgnore]
        public MongoObjectId Key
        {
            get { return (MongoObjectId)_mapId; }
        }

        #endregion
    }

    [Serializable]
    internal class MapSettings
    {
        [ContentSerializer(Optional = true)]
        internal String FogAssetName
        {
            get;
            set;
        }

        [ContentSerializer(Optional = true)]
        internal Byte FogOpacity
        {
            get;
            set;
        }

        [ContentSerializer(Optional = true)]
        internal String PanormaAssetName
        {
            get;
            set;
        }
    }
}
