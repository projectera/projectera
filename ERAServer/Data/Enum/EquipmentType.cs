using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    [Flags]
    public enum EquipmentType : byte
    {
        /// <summary>
        /// (0000) No type
        /// </summary>
        None = 0,
        
        /// <summary>
        /// (0001) Weapon
        /// </summary>
        Weapon = (1 << 0),
        
        /// <summary>
        /// (0010) Shield
        /// </summary>
        Shield = (1 << 1),

        /// <summary>
        /// (0100) Double
        /// </summary>
        Double = (1 << 2),

        /// <summary>
        /// (0101) Double Weapon
        /// </summary>
        DoubleWeapon = Weapon | Double,

        /// <summary>
        /// (0110) Double Shield
        /// </summary>
        DoubleShield = Shield | Double,

        /// <summary>
        /// (1000) Armor
        /// </summary>
        Armor =  (1 << 3)
    }
}
