using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    public enum AssetOperationResult : byte
    {
        Ok = 0,

        /// <summary>
        /// File already exists
        /// </summary>
        AlreadyExists = 1,

        /// <summary>
        /// File or Chunk was not found
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// Physical file is in use
        /// </summary>
        InUse = 3,

        /// <summary>
        /// Chunk is missing
        /// </summary>
        InComplete = 4,

        /// <summary>
        /// 
        /// </summary>
        TimedOut = 5,


    }
}
