using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data.Enum
{
    internal enum DamageScope
    {
        None = 0,
        Self = 1,
        Other = 2,

        Area = 4,
        AreaSelf = Area | Self,      // 5
        AreaOther = Area | Other,    // 6

        Team = 7,
        Faction = 8,
        Class = 9,
        Race = 10,
    }
}
