using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    /// <summary>
    /// 
    /// </summary>
    internal class Effort
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String Reward
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Condition> Conditions // TODO Tree ?
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static Effort Generate(String name, String reward)
        {
            Effort result = new Effort();
            result.Id = ObjectId.GenerateNewId();
            result.Name = name;
            result.Reward = reward;

            return result;
        }

        /// <summary>
        /// Gets an item from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        internal static Task<Effort> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        internal static Effort GetBlocking(ObjectId id)
        {
            Effort result;
            if (!DataManager.Cache.Efforts.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as Effort;
                DataManager.Cache.Efforts.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Effort> GetCollection()
        {
            return DataManager.Database.GetCollection<Effort>("Blueprint.Effort");
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        internal void Acquire(Interactable receiver)
        {
            // TODO JAVASCRIPT RUN ON REWARD ACTION
        }

        /// <summary>
        /// Returns true if InteractableItem was generated with this as blueprint
        /// </summary>
        /// <param name="obj">InteractableItem</param>
        /// <returns></returns>
        internal Boolean HasGenerated(object obj)
        {
            if (obj is Data.Effort)
            {
                Data.Effort item = (Data.Effort)obj;
                return item.BlueprintId == this.Id;
            }

            return false;
        }

        /// <summary>
        /// Puts a item to the db
        /// </summary>
        internal virtual void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts an item to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal virtual SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Effort>(this, safemode);
        }

        /// <summary>
        /// 
        /// </summary>
        public class Condition
        {

        }
    }
}
