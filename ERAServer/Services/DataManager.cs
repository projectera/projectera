using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using ERAServer.Protocols.Server;
using ERAServer.Properties;
using ERAUtils.Logger;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ERAUtils;
using MongoDB.Bson.Serialization.Attributes;

namespace ERAServer.Services
{
    /// <summary>
    /// This class will handle the retrieval and storage of data.
    /// </summary>
    public static class DataManager
    {
        public static MongoServer Server { get; set; }
        public static MongoDatabase Database { get; set; }
        public static GameConstants Constants { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static void Initialize()
        {
            try
            {
                Server = MongoServer.Create(Settings.Default.MongoConnectionString);
                Database = Server.GetDatabase(Settings.Default.MongoDatabaseName);
                Constants = GameConstants.GetBlocking() ?? new GameConstants();
                Cache.Initialize();
            }
            catch (Exception e)
            {
                Logger.Fatal("MongoDB error: " + e.Message);

#if DEBUG
                Logger.Warning("Attempting to connect to the fallback Mongo Database");
                try
                {
                    Server = MongoServer.Create("mongodb://localhost");
                    Database = Server.GetDatabase(Settings.Default.MongoDatabaseName);
                    Constants = GameConstants.GetBlocking() ?? new GameConstants();
                    Cache.Initialize();
                }
                catch (Exception f)
                {
                    Logger.Fatal("MongoDB error: " + f.Message);
                }

#else
                throw;
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Int32 IncrementalId(String type)
        {
            FindAndModifyResult result = Database.GetCollection("Counters").FindAndModify(
                Query.EQ("Type", type),
                SortBy.Ascending("Type"),
                Update.Inc("Count", 1), true, true);

            return result.ModifiedDocument["Count"].AsInt32;
        }

        /// <summary>
        /// Holds small game constant data
        /// </summary>
        public class GameConstants
        {
            /// <summary>
            /// 
            /// </summary>
            [BsonId]
            public ObjectId Version
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public HashSet<GameConstant> Colors;

            /// <summary>
            /// 
            /// </summary>
            public HashSet<GameConstant> Skins;

            /// <summary>
            /// 
            /// </summary>
            public GameConstants()
            {
                this.Colors = new HashSet<GameConstant>();
                this.Skins = new HashSet<GameConstant>();
                
                AddSkin("body-male");
                
                AddColor("Blonde");
                AddColor("Brown");
                AddColor("Black");
                AddColor("Grey");
                AddColor("Red");
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public Int32 AddColor(String name)
            {
                // Already exists
                GameConstant existant = this.Colors.FirstOrDefault((a) => (String)a.Value == name);
                if (existant != null)
                    return existant.Id;

                // Generate and add
                GameConstant result = GameConstant.Generate(name, "Colors");
                this.Colors.Add(result);

                // Return identifier
                return result.Id;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public Int32 AddSkin(String name)
            {
                // Already exists
                GameConstant existant = this.Skins.FirstOrDefault((a) => (String)a.Value == name);
                if (existant != null)
                    return existant.Id;

                // Generate and add
                GameConstant result = GameConstant.Generate(name, "Skins");
                this.Skins.Add(result);

                // Return identifier
                return result.Id;
            }

            /// <summary>
            /// 
            /// </summary>
            public class GameConstant
            {
                [BsonRequired]
                internal Object Value
                {
                    get;
                    private set;
                }

                [BsonId]
                internal Int32 Id
                {
                    get;
                    set;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                /// <param name="type"></param>
                /// <returns></returns>
                internal static GameConstant Generate(Object value, String type)
                {
                    GameConstant result = new GameConstant();
                    result.Value = value;
                    result.Id = DataManager.IncrementalId("GameConstant." + type);
                    return result;
                }
            }

            /// <summary>
            /// Gets an item from the db, blocks while retrieving
            /// </summary>
            /// <returns></returns>
            public static GameConstants GetBlocking()
            {
                return GetCollection().FindAll().SetSortOrder(SortBy.Descending("Version")).FirstOrDefault();
            }

            /// <summary>
            /// Gets an item from the db, blocks while retrieving
            /// </summary>
            /// <param name="username">id of item to get</param>
            /// <returns></returns>
            public static GameConstants GetBlocking(ObjectId id)
            {
                return GetCollection().FindOneById(id) as GameConstants;
            }

            /// <summary>
            /// Gets the items collection
            /// </summary>
            /// <returns></returns>
            public static MongoCollection<GameConstants> GetCollection()
            {
                return DataManager.Database.GetCollection<GameConstants>("GameConstants");
            }

            /// <summary>
            /// Puts a item to the db
            /// </summary>
            public virtual void Put()
            {
                Put(SafeMode.False);
            }

            /// <summary>
            /// Puts an item to the db
            /// <param name="safemode">Sets the safemode on this query</param>
            /// </summary>
            public virtual SafeModeResult Put(SafeMode safemode)
            {
                Version = ObjectId.GenerateNewId();
                return GetCollection().Save<GameConstants>(this, safemode);
            }
        }

        /// <summary>
        /// Caches blueprint data from the database
        /// </summary>
        internal static class Cache
        {
            public static LinkedHashMap<ObjectId, Data.Blueprint.Achievement> Achievements;
            public static LinkedHashMap<ObjectId, Data.Blueprint.Effort> Efforts;

            public static LinkedHashMap<Int32, Data.Blueprint.BattlerAnimation> BattlerAnimations;
            public static LinkedHashMap<Int32, Data.Blueprint.BattlerClass> BattlerClasses;
            public static LinkedHashMap<Int32, Data.Blueprint.BattlerModifier> BattlerModifiers;
            public static LinkedHashMap<Int32, Data.Blueprint.BattlerRace> BattlerRaces;
            public static LinkedHashMap<Int32, Data.Blueprint.Equipment> Equipments;
            public static LinkedHashMap<Int32, Data.Blueprint.Item> Items;
            public static LinkedHashMap<Int32, Data.Blueprint.Skill> Skills;

            public static LinkedHashMap<String, Data.Blueprint.MongoScript> Scripts;

            /// <summary>
            /// 
            /// </summary>
            public static void Initialize()
            {
                Cache.Achievements = new LinkedHashMap<ObjectId, Data.Blueprint.Achievement>();
                Cache.Efforts = new LinkedHashMap<ObjectId, Data.Blueprint.Effort>();

                Cache.BattlerAnimations = new LinkedHashMap<Int32, Data.Blueprint.BattlerAnimation>();
                Cache.BattlerClasses = new LinkedHashMap<Int32, Data.Blueprint.BattlerClass>();
                Cache.BattlerModifiers = new LinkedHashMap<Int32, Data.Blueprint.BattlerModifier>();
                Cache.BattlerRaces = new LinkedHashMap<Int32, Data.Blueprint.BattlerRace>();
                Cache.Equipments = new LinkedHashMap<Int32, Data.Blueprint.Equipment>();
                Cache.Items = new LinkedHashMap<Int32, Data.Blueprint.Item>();
                Cache.Skills = new LinkedHashMap<Int32, Data.Blueprint.Skill>();

                Cache.Scripts = new LinkedHashMap<String, Data.Blueprint.MongoScript>();
            }
        }

        /// <summary>
        /// Returns random gameconstants upon request
        /// </summary>
        internal static class Random
        {
            /// <summary>
            /// 
            /// </summary>
            public static Byte SkinId
            {
                get
                {
                    Int32 count = DataManager.Constants.Skins.Count;
                    Int32 index = Lidgren.Network.NetRandom.Instance.Next(count);
                    return (Byte)DataManager.Constants.Skins.ElementAt(index).Id;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public static Byte ColorId
            {
                get
                {
                    Int32 count = DataManager.Constants.Colors.Count;
                    Int32 index = Lidgren.Network.NetRandom.Instance.Next(count);
                    return (Byte)DataManager.Constants.Colors.ElementAt(index).Id;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public static Byte EyesId
            {
                get { return 2; }
            }

            /// <summary>
            /// 
            /// </summary>
            public static Byte HairId
            {
                get { return 4; }
            }
        }
    }
}
