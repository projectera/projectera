using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    internal class BattlerClass
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
        /// Class Name
        /// </summary>
        [BsonRequired]
        public Int32 ParentId
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
        /// Class Name
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
        public List<TalentTree> TalentTrees
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static BattlerClass Generate(String name, Int32 parentId, List<TalentTree> talentTree = null)
        {
            return Generate(name, Description.Empty, parentId, talentTree);
        }

        /// <summary>
        /// Generates a new class
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static BattlerClass Generate(String name, Description description, Int32 parentId, List<TalentTree> talentTree = null)
        {
            BattlerClass result = new BattlerClass();
            result.Id = DataManager.IncrementalId("BattlerClasses");
            result.Name = name;
            result.Description = description;
            result.ParentId = parentId;
            result.TalentTrees = talentTree ?? new List<TalentTree>();

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
        /// Gets an BattlerClass from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static Task<BattlerClass> Get(Int32 id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static BattlerClass GetBlocking(Int32 id)
        {
            BattlerClass result;
            if (!DataManager.Cache.BattlerClasses.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as BattlerClass;

                if (result != null && result.Id != 0)
                    DataManager.Cache.BattlerClasses.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<BattlerClass> GetCollection()
        {
            return DataManager.Database.GetCollection<BattlerClass>("Blueprint.Classes");
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
            // Classes
            BattlerClass confusedTraveller = BattlerClass.Generate("Confused Traveller", 0);
            BattlerClass bard = BattlerClass.Generate("Bard", confusedTraveller.Id);
            BattlerClass mercenary = BattlerClass.Generate("Mercenary", confusedTraveller.Id);
            BattlerClass stalker = BattlerClass.Generate("Stalker", confusedTraveller.Id);

            // Talent trees
            TalentTree confusedBardTree = TalentTree.Generate();
            confusedBardTree.PointId = bard.Id;
            TalentTree confusedMercenaryTree = TalentTree.Generate();
            confusedMercenaryTree.PointId = mercenary.Id;
            TalentTree confusedStalkerTree = TalentTree.Generate();
            confusedStalkerTree.PointId = stalker.Id;

            // End nodes
            TalentTree.Node toBard = TalentTree.ClassNode.Generate(0, 15, bard.Id);
            TalentTree.Node toMercenary = TalentTree.ClassNode.Generate(0, 15, mercenary.Id);
            TalentTree.Node toStalker = TalentTree.ClassNode.Generate(0, 15, stalker.Id);
            confusedBardTree.AddRoot(toBard);
            confusedMercenaryTree.AddRoot(toMercenary);
            confusedStalkerTree.AddRoot(toStalker);

            // Starting nodes
            TalentTree.SkillNode slash = TalentTree.SkillNode.Generate(0, 0, Skill.GetBlocking("Slash").Id) as TalentTree.SkillNode;
            confusedBardTree.AddRoot(slash.Empty());
            confusedMercenaryTree.AddRoot(slash.Empty());
            confusedStalkerTree.AddRoot(slash.Empty());

            // Skill nodes
            TalentTree.Node bard1 = TalentTree.SkillNode.Generate(0, 0, Skill.GetBlocking("Swift Reprisal").Id);
            TalentTree.Node bard2 = TalentTree.SkillNode.Generate(0, 0, Skill.GetBlocking("Anthem of Mending").Id);
            TalentTree.Node mercenary1 = TalentTree.SkillNode.Generate(0, 0, Skill.GetBlocking("Bash").Id);
            TalentTree.Node mercenary2 = TalentTree.SkillNode.Generate(0, 0, Skill.GetBlocking("Hack").Id);
            TalentTree.Node stalker1 = TalentTree.SkillNode.Generate(0, 0, Skill.GetBlocking("Preparation").Id);
            TalentTree.Node stalker2 = TalentTree.SkillNode.Generate(0, 0, Skill.GetBlocking("Quick Strike").Id);

            // Add skills
            confusedBardTree.AddRoot(bard1);
            confusedBardTree.AddRoot(bard2);
            confusedMercenaryTree.AddRoot(mercenary1);
            confusedMercenaryTree.AddRoot(mercenary2);
            confusedStalkerTree.AddRoot(stalker1);
            confusedStalkerTree.AddRoot(stalker2);

            confusedTraveller.TalentTrees.Add(confusedBardTree);
            confusedTraveller.TalentTrees.Add(confusedMercenaryTree);
            confusedTraveller.TalentTrees.Add(confusedStalkerTree);

            // Save
            confusedTraveller.Put();
            bard.Put();
            mercenary.Put();
            stalker.Put();
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
            return GetCollection().Save<BattlerClass>(this, safemode);
        }
    }
}
