using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ERAUtils.Enum;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    internal class BattlerModifier
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
        /// Icon asset name
        /// </summary>
        [BsonRequired]
        public String IconAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 AnimationId
        {
            get;
            private set;
        }

        /// <summary>
        /// Restriction
        /// </summary>
        [BsonRequired]
        public ActionRestriction Restriction
        {
            get;
            private set;
        }

        /// <summary>
        /// Flags
        /// </summary>
        [BsonRequired]
        public BattlerModifierFlags Flags
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ReleaseValues Release
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public DamageValues Damage
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public TimeValues Time
        {
            get;
            private set;
        }

        /// <summary>
        /// Effects
        /// </summary>
        [BsonRequired]
        public BattlerValues Battler
        {
            get;
            private set;
        }

        /// <summary>
        /// 0: remove 1: inflict states and buffs
        /// </summary>
        [BsonRequired]
        public Dictionary<ObjectId, Boolean> StatesModified
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="iconAssetName"></param>
        /// <param name="actionRestriction"></param>
        /// <param name="battlerModifierFlags"></param>
        /// <param name="autoReleaseProbability"></param>
        /// <param name="shockReleaseProbability"></param>
        /// <param name="holdTime"></param>
        /// <param name="effects"></param>
        /// <param name="statesModifier"></param>
        /// <returns></returns>
        internal static BattlerModifier Generate(String name, String iconAssetName, ActionRestriction actionRestriction,
            BattlerModifierFlags battlerModifierFlags,  DamageValues damage, ReleaseValues release, TimeValues time
            , BattlerValues effects, Dictionary<ObjectId, Boolean> statesModifier)
        {
            return Generate(name, Description.Empty, iconAssetName, actionRestriction, battlerModifierFlags, 
                damage, release, time, effects, statesModifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="iconAssetName"></param>
        /// <param name="actionRestriction"></param>
        /// <param name="battlerModifierFlags"></param>
        /// <param name="autoReleaseProbability"></param>
        /// <param name="shockReleaseProbability"></param>
        /// <param name="holdTime"></param>
        /// <param name="effects"></param>
        /// <param name="statesModifier"></param>
        /// <returns></returns>
        internal static BattlerModifier Generate(String name, Description description, String iconAssetName, ActionRestriction actionRestriction,
            BattlerModifierFlags battlerModifierFlags, DamageValues damage, ReleaseValues release, TimeValues time, 
            BattlerValues effects,  Dictionary<ObjectId, Boolean> statesModifier)
        {
            BattlerModifier result = new BattlerModifier();
            result.Id = DataManager.IncrementalId("BattlerModifiers");
            result.Name = name;
            result.Description = description;
            result.IconAssetName = iconAssetName;
            result.Restriction = actionRestriction;
            result.Flags = battlerModifierFlags;
            result.Damage = damage;
            result.Release = release;
            result.Time = time;
            result.Battler = effects;
            result.StatesModified = statesModifier;

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
        /// Gets an BattlerModifier from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static Task<BattlerModifier> Get(Int32 id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static BattlerModifier GetBlocking(Int32 id)
        {
            BattlerModifier result;
            if (!DataManager.Cache.BattlerModifiers.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as BattlerModifier;

                if (result != null && result.Id != 0)
                    DataManager.Cache.BattlerModifiers.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<BattlerModifier> GetCollection()
        {
            return DataManager.Database.GetCollection<BattlerModifier>("Blueprint.BattlerModifiers");
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

            BattlerModifier.Generate("Knocked out", 
                Description.Generate("When knocked out, no actions are allowed and no experience is gained. You will be respawned at your last respawn location."), 
                String.Empty,
                ActionRestriction.NoAction, 
                BattlerModifierFlags.State | BattlerModifierFlags.ZeroHealth | BattlerModifierFlags.NoExperience, 
                DamageValues.None, 
                ReleaseValues.None,
                TimeValues.None,
                BattlerValues.None, 
                null).Put();

            BattlerModifier.Generate("Exhausted",
               Description.Generate(String.Empty),
               String.Empty,
               ActionRestriction.None,
               BattlerModifierFlags.State | BattlerModifierFlags.ZeroConcentration | BattlerModifierFlags.StackOnCondition,
               DamageValues.None,
               ReleaseValues.Generate(100, 0, 0),
               TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
               BattlerValues.Generate(-50, -50, -50, -50, -100, -50, -50, -50),
               null).Put();

            // No Action
            BattlerModifier.Generate("Stunned", 
                Description.Generate(String.Empty), 
                String.Empty,
                ActionRestriction.NoAction, 
                BattlerModifierFlags.State, 
                DamageValues.None,
                ReleaseValues.Generate(100, 0, 0),
                TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero), 
                BattlerValues.None,
                null).Put();

            // Slip Damage
            BattlerModifier.Generate("Poisoned",
                Description.Generate(String.Empty),
                String.Empty,
                ActionRestriction.None,
                BattlerModifierFlags.State,
                DamageValues.GenerateSlip(0, 1),
                ReleaseValues.None,
                TimeValues.Generate(TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                BattlerValues.None,
                null).Put();

            // CON -
            BattlerModifier.Generate("Dazzled",
                Description.Generate(String.Empty),
                String.Empty,
                ActionRestriction.None,
                BattlerModifierFlags.State,
                DamageValues.None,
                ReleaseValues.Generate(50, 0, 0),
                TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero), 
                BattlerValues.GenerateCon(-80),
                null).Put();

            // No Skill
            BattlerModifier.Generate("Muted",
                Description.Generate(String.Empty),
                String.Empty,
                ActionRestriction.NoSkill,
                BattlerModifierFlags.State,
                DamageValues.None,
                ReleaseValues.Generate(50, 0, 0),
                TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero), 
                BattlerValues.None,
                null).Put();

            // Attack allies
            BattlerModifier.Generate("Confused",
                Description.Generate(String.Empty),
                String.Empty,
                ActionRestriction.AttackAllies,
                BattlerModifierFlags.State,
                DamageValues.None,
                ReleaseValues.Generate(50, 25, 0),
                TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                BattlerValues.None,
                null).Put();

            // No Action
            BattlerModifier.Generate("Asleep",
                Description.Generate(String.Empty),
                String.Empty,
                ActionRestriction.NoAction,
                BattlerModifierFlags.State | BattlerModifierFlags.NoEvading,
                DamageValues.None,
                ReleaseValues.Generate(50, 50, 0),
                TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                BattlerValues.None,
                null).Put();

            // No Action
            BattlerModifier.Generate("Paralyzed",
               Description.Generate(String.Empty),
               String.Empty,
               ActionRestriction.NoAction,
               BattlerModifierFlags.State,
               DamageValues.None,
               ReleaseValues.Generate(25, 0, 0),
               TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
               BattlerValues.None,
               null).Put();

            // STR -
            BattlerModifier.Generate("Weakened",
               Description.Generate(String.Empty),
               String.Empty,
               ActionRestriction.None,
               BattlerModifierFlags.State,
               DamageValues.None,
               ReleaseValues.Generate(100, 0, 0),
               TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
               BattlerValues.GenerateStr(-50),
               null).Put();
            
            // CON -
            BattlerModifier.Generate("Clumsy",
               Description.Generate(String.Empty),
               String.Empty,
               ActionRestriction.None,
               BattlerModifierFlags.State,
               DamageValues.None,
               ReleaseValues.Generate(100, 0, 0),
               TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
               BattlerValues.GenerateCon(-50),
               null).Put();

            // HAS -
            BattlerModifier.Generate("Delayed",
               Description.Generate(String.Empty),
               String.Empty,
               ActionRestriction.None,
               BattlerModifierFlags.State,
               DamageValues.None,
               ReleaseValues.Generate(100, 0, 0),
               TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
               BattlerValues.GenerateHas(-50),
               null).Put();

            // ESS -
            BattlerModifier.Generate("Feeble",
               Description.Generate(String.Empty),
               String.Empty,
               ActionRestriction.None,
               BattlerModifierFlags.State,
               DamageValues.None,
               ReleaseValues.Generate(100, 0, 0),
               TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
               BattlerValues.GenerateEss(-50),
               null).Put();

            // MDEF -
            // DEF -
            // MSTR -
            // END -

            // CON +
            BattlerModifier.Generate("Sharpened",
              Description.Generate(String.Empty),
              String.Empty,
              ActionRestriction.None,
              BattlerModifierFlags.State,
              DamageValues.None,
              ReleaseValues.Generate(100, 0, 0),
              TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
              BattlerValues.GenerateCon(50),
              null).Put();

            // DEF +
            BattlerModifier.Generate("Barriered",
              Description.Generate(String.Empty),
              String.Empty,
              ActionRestriction.None,
              BattlerModifierFlags.Buff,
              DamageValues.None,
              ReleaseValues.Generate(100, 0, 0),
              TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
              BattlerValues.GenerateDef(50),
              null).Put();

            // MDEF + 
            BattlerModifier.Generate("Resisted",
              Description.Generate(String.Empty),
              String.Empty,
              ActionRestriction.None,
              BattlerModifierFlags.Buff,
              DamageValues.None,
              ReleaseValues.Generate(100, 0, 0),
              TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
              BattlerValues.GenerateMDef(50),
              null).Put();

            // HAS +
            BattlerModifier.Generate("Blinked",
              Description.Generate(String.Empty),
              String.Empty,
              ActionRestriction.None,
              BattlerModifierFlags.Buff,
              DamageValues.None,
              ReleaseValues.Generate(100, 0, 0),
              TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
              BattlerValues.GenerateHas(50),
              null).Put();

            // STR + 
            BattlerModifier.Generate("Pumped",
             Description.Generate(String.Empty),
             String.Empty,
             ActionRestriction.None,
             BattlerModifierFlags.State,
             DamageValues.None,
             ReleaseValues.Generate(100, 0, 0),
             TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
             BattlerValues.GenerateStr(50),
             null).Put();

            // MSTR + 
            BattlerModifier.Generate("Meditated",
             Description.Generate(String.Empty),
             String.Empty,
             ActionRestriction.None,
             BattlerModifierFlags.State,
             DamageValues.None,
             ReleaseValues.Generate(100, 0, 0),
             TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
             BattlerValues.GenerateMStr(50),
             null).Put();

            // ESS +
            // END +

            // HP-
            BattlerModifier.Generate("Drained",
             Description.Generate(String.Empty),
             String.Empty,
             ActionRestriction.None,
             BattlerModifierFlags.State,
             DamageValues.GenerateSlip(0, 1),
             ReleaseValues.Generate(100, 0, 0),
             TimeValues.Generate(TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(1)),
             BattlerValues.None,
             null).Put();

            // HP+
            BattlerModifier.Generate("Draining",
             Description.Generate(String.Empty),
             String.Empty,
             ActionRestriction.None,
             BattlerModifierFlags.Buff,
             DamageValues.GenerateSlip(0, -1),
             ReleaseValues.Generate(100, 0, 0),
             TimeValues.Generate(TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(1)),
             BattlerValues.None,
             null).Put();
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
            return GetCollection().Save<BattlerModifier>(this, safemode);
        }

        /// <summary>
        /// 
        /// </summary>
        internal class DamageValues
        {
            /// <summary>
            /// 
            /// </summary>
            public static readonly DamageValues None = DamageValues.Generate(0, 0, 0, 0, 0, 0);

            /// <summary>
            /// 
            /// </summary>
            public Double SlipInitial
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public Double SlipHp
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public Double ReleaseInitial
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public Double ReleaseHp
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public Double ReleaseTakenDamage
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public Double ReleaseDealtDamage
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slipInit"></param>
            /// <param name="slipHp"></param>
            /// <param name="releaseInit"></param>
            /// <param name="releaseHp"></param>
            /// <param name="releaseTakenDamage"></param>
            /// <param name="releaseDealtDamage"></param>
            /// <returns></returns>
            internal static DamageValues Generate(Double slipInit, Double slipHp, 
                Double releaseInit, Double releaseHp, Double releaseTakenDamage, Double releaseDealtDamage)
            {
                DamageValues result = new DamageValues();
                result.SlipInitial = slipInit;
                result.SlipHp = slipHp;
                result.ReleaseInitial = releaseInit;
                result.ReleaseHp = releaseHp;
                result.ReleaseTakenDamage = releaseTakenDamage;
                result.ReleaseDealtDamage = releaseDealtDamage;
                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slipInit"></param>
            /// <param name="slipHp"></param>
            /// <returns></returns>
            internal static DamageValues GenerateSlip(Double slipInit, Double slipHp)
            {
                return Generate(slipInit, slipHp, 0, 0, 0, 0);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="releaseInit"></param>
            /// <param name="releaseHp"></param>
            /// <param name="releaseTakenDamage"></param>
            /// <param name="releaseDealtDamage"></param>
            /// <returns></returns>
            internal static DamageValues GenerateRelease(Double releaseInit, Double releaseHp, Double releaseTakenDamage, Double releaseDealtDamage)
            {
                return Generate(0, 0, releaseInit, releaseHp, releaseTakenDamage, releaseDealtDamage);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        internal class TimeValues
        {
            /// <summary>
            /// 
            /// </summary>
            public static readonly TimeValues None = TimeValues.Generate(TimeSpan.Zero, TimeSpan.Zero);

            /// <summary>
            /// 
            /// </summary>
            public TimeSpan Duration
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public TimeSpan Interval
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="holdTime"></param>
            /// <returns></returns>
            internal static TimeValues Generate(TimeSpan holdTime, TimeSpan interval)
            {
                TimeValues result = new TimeValues();
                result.Duration = holdTime;
                result.Interval = interval;

                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal class ReleaseValues
        {
            /// <summary>
            /// 
            /// </summary>
            public static readonly ReleaseValues None = ReleaseValues.Generate(0, 0, 0);

            /// <summary>
            /// Release after time
            /// </summary>
            [BsonRequired]
            public Single AutoReleaseProbability
            {
                get;
                private set;
            }

            /// <summary>
            /// Release after shock
            /// </summary>
            [BsonRequired]
            public Single ShockReleaseProbability
            {
                get;
                private set;
            }

            /// <summary>
            /// Release after attack
            /// </summary>
            [BsonRequired]
            public Single AttackReleaseProbability
            {
                get;
                private set;
            }

            internal static ReleaseValues Generate(Single time, Single shock, Single attack)
            {
                ReleaseValues result = new ReleaseValues();
                result.AutoReleaseProbability = time;
                result.ShockReleaseProbability = shock;
                result.AttackReleaseProbability = attack;

                return result;
            }
        }
    }
}
