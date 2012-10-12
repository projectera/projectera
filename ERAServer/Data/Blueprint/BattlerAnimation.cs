using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Threading.Tasks;
using MongoDB.Driver;
using ERAUtils.Enum;
using MongoDB.Driver.Builders;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    internal class BattlerAnimation
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public Int32 Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String AssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// Frames
        /// </summary>
        [BsonRequired]
        public List<Frame> Frames
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public AnimationPosition Position
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static BattlerAnimation Generate(String name, String assetName, List<Frame> frames, AnimationPosition position)
        {
            BattlerAnimation result = new BattlerAnimation();
            result.Id = DataManager.IncrementalId("BattlerAnimations");
            result.Name = name;
            result.AssetName = assetName;
            result.Frames = frames;
            result.Position = position;
           
            return result;
        }

        /// <summary>
        /// Gets an BattlerAnimation from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static Task<BattlerAnimation> Get(Int32 id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static BattlerAnimation GetBlocking(Int32 id)
        {
            BattlerAnimation result;
            if (!DataManager.Cache.BattlerAnimations.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as BattlerAnimation;

                if (result != null && result.Id != 0)
                    DataManager.Cache.BattlerAnimations.Enqueue(id, result);
            }

            return result;
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
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<BattlerAnimation> GetCollection()
        {
            return DataManager.Database.GetCollection<BattlerAnimation>("Blueprint.BattlerAnimations");
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
            return GetCollection().Save<BattlerAnimation>(this, safemode);
        }

        /// <summary>
        /// 
        /// </summary>
        public class Frame
        {
            [BsonRequired]
            public TimeSpan Duration
            {
                get;
                private set;
            }
        }
    }
}
