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
    internal class Achievement
    {
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        [BsonRequired]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static Achievement Generate(String name)
        {
            Achievement result = new Achievement();
            result.Id = ObjectId.GenerateNewId();
            result.Name = name;

            return result;
        }

        /// <summary>
        /// Gets an item from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static Task<Achievement> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static Achievement GetBlocking(ObjectId id)
        {
            Achievement result;
            if (!DataManager.Cache.Achievements.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as Achievement;

                if (result != null && result.Id != ObjectId.Empty)
                    DataManager.Cache.Achievements.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<Achievement> GetCollection()
        {
            return DataManager.Database.GetCollection<Achievement>("Blueprint.Achievements");
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
            return GetCollection().Save<Achievement>(this, safemode);
        }
    }
}
