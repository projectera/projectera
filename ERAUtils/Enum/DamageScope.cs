using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    internal enum DamageScope
    {
        None        = 0,    // No damage
        Self        = 1,    // Damage on user
        Other       = 2,    // Damage on single

        Area        = 4,    // Damage on area (friendly + enemy)
        AreaSelf    = Area | Self,     // Damage on area (friendly)
        AreaOther   = Area | Other,    // Damage on area (enemy)

        Team        = 7,    // Damage on specific Team
        Faction     = 8,    // Damage on specific Faction
        Class       = 9,    // Damage on specific Class
        Race        = 10,   // Damage on specific Race
    }
}
