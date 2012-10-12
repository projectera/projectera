using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    [Flags]
    internal enum InteractableFlags : byte
    {
        None = 0,

        Through = (1 << 0),
        Visible = (1 << 1),
        Moving = (1 << 2),
        EventRunning = (1 << 3),
        Locked = (1 << 4),
        Bush
    }
}
