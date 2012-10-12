using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    [Flags]
    public enum GuildPermissionGroup : byte
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Pending = (1 << 0),

        /// <summary>
        /// 
        /// </summary>
        Member  = (1 << 1),

        /// <summary>
        /// 
        /// </summary>
        Founder = (1 << 7),
    }
}
