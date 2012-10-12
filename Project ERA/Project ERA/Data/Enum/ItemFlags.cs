using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data.Enum
{
    [Flags]
    public enum ItemFlags : ushort
    {
        None = 0,
        NoDrop = 1,
        NoTrade = 2,
        NoSell = 4,
        NoStore = 8,

        NoActions = NoDrop | NoTrade | NoSell | NoStore,

        Special = 16,

        // EQUIPMENT Only
        Locked = 256,
        DefaultLocked = 512,
        
    }
}
