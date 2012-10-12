using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectERA.Protocols
{
    /// <summary>
    /// 
    /// Range: 0-32
    /// </summary>
    public enum ModerationProtocol : byte
    {
        /// <summary>
        /// 0: Invalid
        /// </summary>
        Invalid = 0,
        
        /// <summary>
        /// 1:
        /// </summary>
        TeleportSelf = 1,

        /// <summary>
        /// 2: 
        /// </summary>
        TeleportOther = 2,

        /// <summary>
        /// 3:
        /// </summary>
        Kick = 3,

        /// <summary>
        /// 4:
        /// </summary>
        KickWarn = 4,

        /// <summary>
        /// 5:
        /// </summary>
        KickBan = 5,

        /// <summary>
        /// 6:
        /// </summary>
        SpawnCreature = 6,

        /// <summary>
        /// : 
        /// </summary>
        SpawnChest = 7,

        /// <summary>
        /// : 
        /// </summary>
        RemedySelf = 8,

        /// <summary>
        /// : 
        /// </summary>
        RemedyOther = 9,

        /// <summary>
        /// : 
        /// </summary>
        DamageOther = 10,

        /// <summary>
        /// : 
        /// </summary>
        ReviveSelf = 11,

        /// <summary>
        /// : 
        /// </summary>
        ReviveOther = 12,

        /// <summary>
        /// : 
        /// </summary>
        SetSwitchSelf = 13,

        /// <summary>
        /// : 
        /// </summary>
        SetSwitchOther = 14,

        /// <summary>
        /// : 
        /// </summary>
        SetVariableSelf = 15,

        /// <summary>
        /// : 
        /// </summary>
        SetVariableOther = 16,

        /// <summary>
        /// : 
        /// </summary>
        SetBodySelf = 17,

        /// <summary>
        /// : 
        /// </summary>
        SetBodyOther = 18,

        /// <summary>
        /// : 
        /// </summary>
        SetEquipmentSelf = 19,

        /// <summary>
        /// : 
        /// </summary>
        SetEquipmentOther = 20,

        /// <summary>
        /// : 
        /// </summary>
        SetStatsSelf = 21,

        /// <summary>
        /// : 
        /// </summary>
        SetStatsOther = 22,

        /// <summary>
        /// : 
        /// </summary>
        InventoryOperationSelf = 23,

        /// <summary>
        /// : 
        /// </summary>
        InventoryOperationOther = 24,

        /// <summary>
        /// : 
        /// </summary>
        SpectateInstance = 25,
    }
}
