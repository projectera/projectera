using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    [Flags]
    internal enum GuildPermissionGroup : byte
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
