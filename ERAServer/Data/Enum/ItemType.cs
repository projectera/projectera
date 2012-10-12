using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    internal enum ItemType : byte
    {
        None = 0,
        Weapon = 1,
        Armor = 2,
        Shield = 3,
        Item = 4,
        Special = 5,
    }
}
