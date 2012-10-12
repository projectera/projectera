using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    [Flags]
    public enum ItemFlags : ushort
    {
        None = 0,

        /// <summary>
        /// If set, item can not be dropped
        /// </summary>
        NoDrop  = (1 << 0),

        /// <summary>
        /// If set, item can not be traded
        /// </summary>
        NoTrade = (1 << 1),

        /// <summary>
        /// If set, item can not be sold
        /// </summary>
        NoSell  = (1 << 2),

        /// <summary>
        /// If set, item can not be stored
        /// </summary>
        NoStore = (1 << 3),

        /// <summary>
        /// If set, item can not be consumed (default for equipment)
        /// </summary>
        NoConsume = (1 << 4),

        /// <summary>
        /// If set, item can not be used in crafting
        /// </summary>
        NoCraft = (1 << 5),

        /// <summary>
        /// If set, item has no actions (key item)
        /// </summary>
        NoActions = NoDrop | NoTrade | NoSell | NoStore | NoConsume | NoCraft,

        /// <summary>
        /// 
        /// </summary>
        Special = (1 << 7),

        /// <summary>
        /// Equipment Only
        /// </summary>
        Locked  = (1 << 8),

        /// <summary>
        /// Equipment Only
        /// </summary>
        DefaultLocked = (1 << 9),
        
    }
}
