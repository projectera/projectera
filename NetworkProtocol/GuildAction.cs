using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Protocols
{
    /// <summary>
    /// 
    /// </summary>
    public enum GuildAction : byte
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Max = 4,

        /// <summary>
        /// 1: Gets Guild Data
        /// </summary>
        Get = 1,

        /// <summary>
        /// 2: Passivly Updating Guild Data (invalidates cache)
        /// </summary>
        Update = 2,


    }
}
