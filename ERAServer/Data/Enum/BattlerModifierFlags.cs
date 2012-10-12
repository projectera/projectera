using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    [Flags]
    public enum BattlerModifierFlags : byte
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        State = (1 << 0),

        /// <summary>
        /// 
        /// </summary>
        Buff = (1 << 1),

        /// <summary>
        /// 
        /// </summary>
        NoResistance = (1 << 2),

        /// <summary>
        /// 
        /// </summary>
        NoEvading = (1 << 3),

        /// <summary>
        /// 
        /// </summary>
        NoExperience = (1 << 4),

        /// <summary>
        /// 
        /// </summary>
        ZeroHealth = (1 << 5),

        /// <summary>
        /// 
        /// </summary>
        ZeroConcentration = (1 << 6),

        /// <summary>
        /// 
        /// </summary>
        StackOnCondition = (1 << 7)
    }
}
