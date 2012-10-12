using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data
{
    internal partial class AvatarStats
    {
        /// <summary>
        /// Level is derived from experience points. 
        /// </summary>
        public UInt16 Level
        {
            get 
            {
                return (UInt16)Math.Floor(Math.Sqrt(2* this.ExperiencePoints + .25d) + .5);
            }
        }

        /// <summary>
        /// This is the attacking power for this Avatar.
        /// </summary>
        /// <remarks>Physical Body Strength is derived from level and MagicNumber. </remarks>
        public UInt16 Strength
        {
            get 
            {
                return 0;
            }
        }

        /// <summary>
        /// This is the defensive force for this Avatar.
        /// </summary>
        /// <remarks>Physical Body Defense is derived from level and MagicNumber.</remarks>
        public UInt16 Defense
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// This is the attacking power (magic) for this Avatar.
        /// </summary>
        /// <remarks>Magical Body Strength (magic) is derived from level and MagicNumber.</remarks>
        public UInt16 MagicalStrength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// This is the defensive force (magic) for this Avatar.
        /// </summary>
        /// <remarks>Magical Body Defense (magic) is derived from level and MagicNumber.</remarks>
        public UInt16 MagicalDefense
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// This is the haste for this avatar. Higher haste values means more speed,
        /// but also means less carefull. (lower def?)
        /// </summary>
        public UInt16 Haste
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// This is the endurance for this avatar. Higher Endurance means (higher hp?)
        /// </summary>
        public UInt16 Endurance
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// This is the spiritual essence  for this avatar. Higher Essence means better magic.
        /// </summary>
        public UInt16 Essence
        {
            get
            {
                return 0;
            }
        }
    }
}
