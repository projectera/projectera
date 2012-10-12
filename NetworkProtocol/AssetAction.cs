using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Protocols
{
    public enum AssetAction : byte
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Max = 6,

        /// <summary>
        /// 1: Get Asset Info
        /// </summary>
        Get = 1,

        /// <summary>
        /// 2: Starts downloading asset
        /// </summary>
        Download = 2,

        /// <summary>
        /// 2: Gets Asset Chunk
        /// </summary>
        GetChunk = 3,

        /// <summary>
        /// 3: Validate Asset
        /// </summary>
        Validate = 4,

        /// <summary>
        /// 4: Update Asset
        /// </summary>
        Update = 5,

        /// <summary>
        /// 5: Distribute Asset to peer
        /// </summary>
        Distribute = 6,
    }
}
