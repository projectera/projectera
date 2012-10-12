using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data.Enum
{
    [Flags]
    public enum DataLoadResult : byte
    {
        None         = 0,
        FromTitle    = (1 << 0),
        Invalid      = (1 << 1),
        NotFound     = (1 << 2),
        NotValidated = (1 << 3),
        WithWarnings = (1 << 4),
        Downloaded   = (1 << 5),
    }
}
