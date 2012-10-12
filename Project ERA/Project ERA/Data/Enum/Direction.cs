using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data.Enum
{
    public enum Direction : byte
    {
        None = 0,

        South = 2,
        West = 4,
        East = 6,        
        North = 8,

        SouthWest = 3,
        SouthEast = 5,
        NorthWest = 7,
        NorthEast = 9,

        All = 10,
    }
}
