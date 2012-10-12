using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    [Flags]
    public enum TilesetFlags : byte
    {
        /// <summary>
        /// 0: 0000000
        /// </summary>
        None = 0,

        /// <summary>
        /// 1: 0000001
        /// </summary>
        Bush = (1 << 0),

        /// <summary>
        /// 2: 0000010
        /// </summary>
        Counter = (1 << 1),

        /// <summary>
        /// 4: 0000100
        /// </summary>
        Blocking = (1 << 2),

        /// <summary>
        /// 8: 0001000
        /// </summary>
        TransparantPassability = (1 << 3),

        /// <summary>
        /// 16: 0010000
        /// </summary>
        Poisonous = (1 << 4),

        /// <summary>
        /// 32: 0100000
        /// </summary>
        Hazardous = (1 << 5),

        /// <summary>
        /// 64: 1000000
        /// </summary>
        Healing = (1 << 6),
    }
}
