using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    internal class BattlerRace
    {
        /// <summary>
        /// Content Id
        /// </summary>
        [BsonId]
        public Int32 Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Description
        /// </summary>
        public Description Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Name
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Effects
        /// </summary>
        [BsonRequired]
        public BattlerValues EquipmentBase
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="equipmentBase"></param>
        /// <returns></returns>
        internal static BattlerRace Generate(String name, BattlerValues equipmentBase)
        {
            return Generate(name, Description.Empty, equipmentBase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="equipmentBase"></param>
        /// <returns></returns>
        internal static BattlerRace Generate(String name, Description description, BattlerValues equipmentBase)
        {
            BattlerRace result = new BattlerRace();
            result.Id = DataManager.IncrementalId("BattlerRaces");
            result.Name = name;
            result.Description = description;
            result.EquipmentBase = equipmentBase;

            return result;
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(String description)
        {
            SetDescription(Description.Generate(description));
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(params String[] description)
        {
            SetDescription(Description.Generate(String.Join(" ", description)));
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(Description description, Boolean autoUpdate = true)
        {
            this.Description = description;

            if (autoUpdate)
                GetCollection().Update(Query.EQ("_id", this.Id), Update.Set("Description", this.Description.ToBsonDocument()));
        }

        /// <summary>
        /// Gets an BattlerRace from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static Task<BattlerRace> Get(Int32 id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

 
        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static BattlerRace GetBlocking(Int32 id)
        {
            BattlerRace result;
            if (!DataManager.Cache.BattlerRaces.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as BattlerRace;

                if (result != null && result.Id != 0)
                    DataManager.Cache.BattlerRaces.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<BattlerRace> GetCollection()
        {
            return DataManager.Database.GetCollection<BattlerRace>("Blueprint.Races");
        }

        /// <summary>
        /// Clears collection
        /// </summary>
        public static void ClearCollection()
        {
            ClearCollection(true);
        }

        /// <summary>
        /// Drops collection and recreates content
        /// </summary>
        /// <param name="autoPopulate"></param>
        public static void ClearCollection(Boolean autoPopulate)
        {
            if (GetCollection().Exists())
                GetCollection().Drop();
            

            if (autoPopulate)
                PopulateCollection();
        }

        /// <summary>
        /// Populates collection with default data 
        /// TODO: load from file?
        /// </summary>
        public static void PopulateCollection()
        {
            BattlerRace.Generate("Lewan", BattlerValues.Generate(5, 5, 5, 5, 5, 5, 5, 5)).Put();
            BattlerRace.Generate("Sumnian", BattlerValues.Generate(5, 5, 5, 5, 5, 5, 5, 5)).Put();
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
            return GetCollection().Save<BattlerRace>(this, safemode);
        }
    }
}
