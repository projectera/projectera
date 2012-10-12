using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils.Enum;
using System.Threading.Tasks;
using ERAUtils;
using Microsoft.Xna.Framework.Content;
using ProjectERA.Graphics.Sprite;

namespace ProjectERA.Data
{
    [Serializable]
    internal class BattlerModifier
    {
        /// <summary>
        /// Content Id
        /// </summary>
        [ContentSerializer(ElementName = "Id")]
        public Int32 ModifierId
        {
            get;
            private set;
        }

        /// <summary>
        /// Description
        /// </summary>
        [ContentSerializer()]
        public String Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Name
        /// </summary>
        [ContentSerializer()]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Icon asset name
        /// </summary>
        [ContentSerializer()]
        public Icon IconAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public Int32 AnimationId
        {
            get;
            private set;
        }

        /// <summary>
        /// Restriction
        /// </summary>
        [ContentSerializer()]
        public ActionRestriction Restriction
        {
            get;
            private set;
        }

        /// <summary>
        /// Flags
        /// </summary>
        [ContentSerializer()]
        public BattlerModifierFlags Flags
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public ReleaseValues Release
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public DamageValues Damage
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public TimeValues Time
        {
            get;
            private set;
        }

        /// <summary>
        /// Effects
        /// </summary>
        [ContentSerializer()]
        public BattlerValues Battler
        {
            get;
            private set;
        }

        /// <summary>
        /// 0: remove 1: inflict states and buffs
        /// </summary>
        [ContentSerializer()]
        public Dictionary<MongoObjectId, Boolean> StatesModified
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsState
        {
            get { return this.Flags.HasFlag(BattlerModifierFlags.State);  }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsBuff
        {
            get { return this.Flags.HasFlag(BattlerModifierFlags.Buff); }
        }

        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public BattlerModifier()
        {

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
        internal BattlerModifier(Int32 id, String name, String iconAssetName, ActionRestriction actionRestriction,
            BattlerModifierFlags battlerModifierFlags, DamageValues damage, ReleaseValues release, TimeValues time
            , BattlerValues effects, Dictionary<MongoObjectId, Boolean> statesModifier)
            : this(id, name, String.Empty, iconAssetName, actionRestriction, battlerModifierFlags,
                damage, release, time, effects, statesModifier)
        {

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
        internal BattlerModifier(Int32 id, String name, String description, String iconAssetName, ActionRestriction actionRestriction,
            BattlerModifierFlags battlerModifierFlags, DamageValues damage, ReleaseValues release, TimeValues time,
            BattlerValues effects, Dictionary<MongoObjectId, Boolean> statesModifier)
        {
            this.ModifierId = id;
            this.Name = name;
            this.Description = description;
            this.IconAssetName = Icon.Generate(iconAssetName);
            this.Restriction = actionRestriction;
            this.Flags = battlerModifierFlags;
            this.Damage = damage;
            this.Release = release;
            this.Time = time;
            this.Battler = effects;
            this.StatesModified = statesModifier;
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(String description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(params String[] description)
        {
            SetDescription(String.Join(" ", description));
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        internal class DamageValues
        {
            /// <summary>
            /// 
            /// </summary>
            public static readonly DamageValues None = DamageValues.Generate(0, 0, 0, 0, 0, 0);

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
            public Double SlipInitial
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
            public Double SlipHp
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
            public Double ReleaseInitial
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
            public Double ReleaseHp
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
            public Double ReleaseTakenDamage
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
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
        [Serializable]
        internal class TimeValues
        {
            /// <summary>
            /// 
            /// </summary>
            public static readonly TimeValues None = TimeValues.Generate(TimeSpan.Zero, TimeSpan.Zero);

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
            public TimeSpan Duration
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            [ContentSerializer()]
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
        [Serializable]
        internal class ReleaseValues
        {
            /// <summary>
            /// 
            /// </summary>
            public static readonly ReleaseValues None = ReleaseValues.Generate(0, 0, 0);

            /// <summary>
            /// Release after time
            /// </summary>
            [ContentSerializer()]
            public Single AutoReleaseProbability
            {
                get;
                private set;
            }

            /// <summary>
            /// Release after shock
            /// </summary>
            [ContentSerializer()]
            public Single ShockReleaseProbability
            {
                get;
                private set;
            }

            /// <summary>
            /// Release after attack
            /// </summary>
            [ContentSerializer()]
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
