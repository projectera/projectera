using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    [Flags]
    public enum BattlerModifierFlags : byte
    {
        None = 0,
        State = (1 << 0),
        Buff = (1 << 1),

        NoResistance = (1 << 2),
        NoEvading = (1 << 3),
        NoExperience = (1 << 4),
        ZeroHealth = (1 << 5),
        ZeroConcentration = (1 << 6),
        StackOnCondition = (1 << 7)
    }
}
