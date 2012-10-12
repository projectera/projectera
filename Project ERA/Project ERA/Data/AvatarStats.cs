using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;

namespace ProjectERA.Data
{
    internal partial class AvatarStats : IResetable
    {
        #region Private fields

        private Int32 _magicNumber, 
            _experiencePoints, _additionalPoints;

        private Double _healthPoints,
            _concentrationPoints;

        // [...]

        private List<BattlerModifier> _states, _buffs;

        #endregion

        #region Properties

        /// <summary>
        /// Magic avatar number for random number generation
        /// </summary>
        internal Int32 MagicNumber
        {
            get { return _magicNumber; }
            set { _magicNumber = value; }
        }

        /// <summary>
        /// Health points (Percentage!)
        /// </summary>
        internal Double Health
        {
            get { return _healthPoints; }
            set { _healthPoints = value; }
        }

        /// <summary>
        /// Concentration points (Percentage!)
        /// </summary>
        internal Double Concentration
        {
            get { return _concentrationPoints; }
            set { _concentrationPoints = value; }
        }

        /// <summary>
        /// Experience points
        /// </summary>
        internal Int32 ExperiencePoints
        {
            get { return _experiencePoints; }
            set { _experiencePoints = value; }
        }

        /// <summary>
        /// Additional points
        /// </summary>
        internal Int32 AdditionalPoints
        {
            get { return _additionalPoints; }
            set { _additionalPoints = value; }
        }

        /// <summary>
        /// States
        /// </summary>
        internal List<BattlerModifier> States
        {
            get { return _states; }
            set { _states = value; }
        }

        /// <summary>
        /// Buffs
        /// </summary>
        internal List<BattlerModifier> Buffs
        {
            get { return _buffs; }
            set { _buffs = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public AvatarStats()
        {
            this.States = new List<BattlerModifier>();
            this.Buffs = new List<BattlerModifier>();
        }

        /// <summary>
        /// Reset Avatar
        /// </summary>
        public void Clear()
        {
            this.MagicNumber = 0;
            this.Health = 0;
            this.Concentration = 0;
            this.ExperiencePoints = 0;
            this.AdditionalPoints = 0;

            for (Int32 i = 0; i < this.States.Count; i++)
            {
                this.States[i].Clear();
            }

            for (Int32 i = 0; i < this.Buffs.Count; i++)
            {
                this.Buffs[i].Clear();
            }

            this.States.Clear();
            this.Buffs.Clear();
        }
    }
}
