using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectERA.Protocols
{
    /// <summary>
    /// 
    /// Range: 0-7
    /// </summary>
    public enum DataIntegrityProtocol : byte
    {
        /// <summary>
        /// 0: Invalid
        /// </summary>
        Invalid = 0,
        
        /// <summary>
        /// 1: 
        /// </summary>
        Tileset = 1,

        /// <summary>
        /// 2: 
        /// </summary>
        Map = 2,

        /// <summary>
        /// 
        /// </summary>
        Items = 3,

        /// <summary>
        /// 
        /// </summary>
        Weapons = 4,

        /// <summary>
        /// 
        /// </summary>
        Armors = 5,

        /// <summary>
        /// 
        /// </summary>
        Skills = 6,

    }
}
