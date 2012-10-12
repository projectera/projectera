using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using System.Threading.Tasks;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    internal class Skill
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
        [BsonRequired]
        public String Name
        {
            get;
            private set;
        }

        public Description Description
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String IconAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 AnimationOrigin
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 AnimationTarget
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Single Delay
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Single Duration
        {
            get;
            private set;
        }

        // TODO strength of skill

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="iconAssetName"></param>
        /// <param name="animationOrigin"></param>
        /// <param name="animationTarget"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static Skill Generate(String name, String iconAssetName, Int32 animationOrigin, Int32 animationTarget,
            Single delay, Single duration)
        {
            return Generate(name, Description.Empty, iconAssetName, animationOrigin, animationTarget, delay, duration);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="iconAssetName"></param>
        /// <param name="animationOrigin"></param>
        /// <param name="animationTarget"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static Skill Generate(String name, Description description, String iconAssetName, Int32 animationOrigin, Int32 animationTarget,
            Single delay, Single duration)
        {
            Skill result = new Skill();
            result.Id = DataManager.IncrementalId("Skills");
            result.Name = name;
            result.Description = description;
            result.IconAssetName = iconAssetName;
            result.AnimationOrigin = animationOrigin;
            result.AnimationTarget = animationTarget;

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
        /// Gets an item from the db
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static Task<Skill> Get(Int32 id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an Skill from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static Skill GetBlocking(Int32 id)
        {
            Skill result;
            if (!DataManager.Cache.Skills.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as Skill;

                if (result != null && result.Id != 0)
                    DataManager.Cache.Skills.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static Skill GetBlocking(String name)
        {
            return GetCollection().FindOne(Query.Matches("Name", name)) as Skill;
        }

        /// <summary>
        /// Gets the skills collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<Skill> GetCollection()
        {
            return DataManager.Database.GetCollection<Skill>("Blueprint.Skills");
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
            Skill.Generate("Slash", String.Empty, 0, 0, 0, 0).Put();

            Skill.Generate("Swift Reprisal", Description.Generate("Enter a state of Swift Reprisal. For 6 seconds, every succesful hit will cause your physical damage and attack speed to increase by 10%, until the end of the effect."), String.Empty, 0, 0, 0, 6).Put();
            Skill.Generate("Anthem of Mending", Description.Generate("You cast a healing chant. After 8 seconds, all party members affected by the chant are healed for 30% of the damage done during that time."), String.Empty, 0, 0, 4, 0).Put();

            Skill.Generate("Bash", Description.Generate("Bash the target with the hilt of your weapon, causing weapon damage, knocking the target back for 3 squares, and reducing their movement speed by 50% for 5 seconds."), String.Empty, 0, 0, 0, 0).Put();
            Skill.Generate("Hack", Description.Generate("Damage the target with a rending blow, dealing weapon damage plus 10% and deal 50% of the damage done over 8 seconds."), String.Empty, 0, 0, 0, 8).Put();

            Skill.Generate("Preparation", Description.Generate(String.Join(" ", "Focus your mind, causing certain", "Stalker", "abilities to have additional effects. Requires a full concentration bar.")), String.Empty, 0, 0, 10, 0).Put();
            Skill.Generate("Quick Strike", Description.Generate(String.Join(" ", "Instantly strike the target, causing weapon damage plus 10%. Damage is increased by 20% when", "Prepared.")), String.Empty, 0, 0, 0, 0).Put();
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
            return GetCollection().Save<Skill>(this, safemode);
        }
    }
}
