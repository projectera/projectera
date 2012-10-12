using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using ERAUtils.Enum;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Threading.Tasks;
using ERAUtils;
using ERAServer.Services;

namespace ERAServer.Data
{
    public class Map
    {
        private List<Interactable> _interactables;

        /// <summary>
        /// 
        /// </summary>
        internal static readonly Byte Layers = 3;

        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public ObjectId TilesetId
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public ObjectId RegionId
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public UInt16 Width
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public UInt16 Height
        {
            get; 
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public MapType Type
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public MapSettings Settings
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public UInt16[][][] Data
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public UInt32 Version
        {
            get; 
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Map()
        {
            _interactables = new List<Interactable>();
        }

        /// <summary>
        /// 
        /// </summary>
        public static Map Generate(ObjectId id, ObjectId tilesetId, ObjectId regionId, String name, MapType type, MapSettings settings, 
            UInt16 width, UInt16 height, UInt16[][][] data, UInt32 version)
        {
            Map result = new Map();
            result.Id = id;
            result.TilesetId = tilesetId;
            result.RegionId = regionId;
            result.Name = name;
            result.Type = type;
            result.Settings = settings;
            result.Width = width;
            result.Height = height;
            result.Data = data;
            result.Version = version;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interactable"></param>
        internal void AddInteractable(Interactable interactable)
        {
            _interactables.Add(interactable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interactable"></param>
        internal void RemoveInteractable(Interactable interactable)
        {
            _interactables.Remove(interactable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal List<ObjectId> GetInteractables()
        {
            List<ObjectId> ids = new List<ObjectId>();
            lock (_interactables)
                _interactables.ForEach(i => { ids.Add(i.Id); });
            return ids;
        }

        /// <summary>
        /// Gets a map from the db
        /// </summary>
        /// <param name="id">id of map to get</param>
        /// <returns></returns>
        internal static Task<Map> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets a map from the db
        /// </summary>
        /// <param name="name">name of map to get</param>
        /// <returns></returns>
        internal static Task<Map> Get(String name)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Gets a map from the db, blocks while retrieving
        /// </summary>
        /// <param name="id">id of map to get</param>
        /// <returns></returns>
        internal static Map GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as Map;
        }

        /// <summary>
        /// Gets a map from the db, blocks while retrieving
        /// </summary>
        /// <param name="name">name of map to get</param>
        /// <returns></returns>
        internal static Map GetBlocking(String name)
        {
            return GetCollection().FindOneAs<Map>(Query.Matches("Name", new BsonRegularExpression("^(?i)" + name + "$"))) as Map;
        }

        /// <summary>
        /// Gets the maps collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<Map> GetCollection()
        {
            return DataManager.Database.GetCollection<Map>("Maps");
        }

        /// <summary>
        /// Puts a map to the db
        /// </summary>
        public void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a map to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Map>(this, safemode);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.Id = ObjectId.Empty;
            this.Name = String.Empty;
            this.TilesetId = ObjectId.Empty;
            this.RegionId = ObjectId.Empty;
            this.Type = MapType.NotSpecified;
            this.Width = 0;
            this.Height = 0;
            this.Data = new UInt16[0][][];
            this.Version = 0;
        }

        /// <summary>
        /// Get HashCode
        /// </summary>
        /// <returns>HashCode for this object</returns>
        public override Int32 GetHashCode()
        {
            // Hash the TileData
            Int32 code = 0;
            foreach (UInt16[][] ystack in this.Data)
                foreach (UInt16[] stack in ystack)
                    foreach (UInt16 item in stack)
                        code = (code * 31) ^ item;

            // Return the Hashcode
            return (Int32)((MongoObjectId.GetHashCode(this.TilesetId.ToByteArray()) * 127) ^
                ((this.Name == null ? (String.Empty.GetHashCode()) : this.Name.GetHashCode()) * 63) ^
                (this.Width * 7) ^
                (this.Height * 15) ^
                ((Int32)this.Type * 3) ^
                (this.Version * 255) ^
                code);
        }

        /// <summary>
        /// 
        /// </summary>
        public class MapSettings
        {
            /// <summary>
            /// 
            /// </summary>
            public String FogAssetName
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            public Byte FogOpacity
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            public String PanormaAssetName
            {
                get;
                set;
            }
        }
    }
}
