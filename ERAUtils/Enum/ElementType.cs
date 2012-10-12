using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    [Flags]
    public enum ElementType : short
    {
        None = 0,
        
        Fire        = (1 << 0),  // Pyro
        Water       = (1 << 1),  // Hydro
        Air         = (1 << 2),  // Aero
        Earth       = (1 << 3),  // Terra

        Endurance   = (1 << 4),  // Stamina
        Mind        = (1 << 5),  // Mental
        Strength    = (1 << 6),  // Physical
        Agility     = (1 << 7),  // Athletic

        Light       = (1 << 8),  // Sol
        Dark        = (1 << 9),  // Luna
        Spirit      = (1 << 10), // Twilight
        Reserved    = (1 << 11),

        Heat = Fire | Water,     // Steam
        Nature = Water | Earth,  // Gaya
        Searing = Air | Fire,    // Blaze

        Destruction = Light | Dark,  // Cataclysmic
        Domination = Light | Spirit, // Eclipse
        Reduction = Dark | Spirit    // Anti
    }
}
