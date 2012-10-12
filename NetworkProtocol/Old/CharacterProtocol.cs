using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectERA.Protocols
{
    /// <summary>
    /// 
    /// Range: 0-32
    /// </summary>
    public enum CharacterProtocol : byte
    {
        /// <summary>
        /// 0: Invalid
        /// </summary>
        Invalid = 0,
        
        /// <summary>
        /// 1: Get Character Data
        /// </summary>
        Get = 1,

        /// <summary>
        /// 2: Get Short Character List Data
        /// </summary>
        GetList = 2,

        /// <summary>
        /// 3: Get Body
        /// </summary>
        GetBody = 3,

        /// <summary>
        /// 4: Get Basic Stats
        /// </summary>
        GetStats = 4,

        /// <summary>
        /// 5: Get Items
        /// </summary>
        GetInventoryItems = 5,

        /// <summary>
        /// 6: Get Weapons
        /// </summary>
        GetInventoryWeapons = 6,

        /// <summary>
        /// 7: Get Armors
        /// </summary>
        GetInventoryArmors = 7,
        
        /// <summary>
        /// 8: Get Inventories
        /// </summary>
        GetInventories = 8,

        /// <summary>
        /// 9: Get Unlocked Skills
        /// </summary>
        GetSkills = 9,

        /// <summary>
        /// 10: Get Synths
        /// </summary>
        GetSynths = 10,

        /// <summary>
        /// 11: Get Location
        /// </summary>
        GetLocation = 11,

        /// <summary>
        /// 12: Get Visual Props
        /// </summary>
        GetVisualProps = 12,

        /// <summary>
        /// 13 : Get Current Equipment
        /// </summary>
        GetEquipment = 13,

        /// <summary>
        /// 20: Start Playing with Character
        /// </summary>
        Pick = 20,

        /// <summary>
        /// 21: Start Character Creation
        /// </summary>
        StartCreation = 21,

        /// <summary>
        /// 22: Finish Character Creation
        /// </summary>
        EndCreation = 22,

        /// <summary>
        /// 23: Cancel Character Creation
        /// </summary>
        CancelCreation = 23,

        /// <summary>
        /// 24: Remove Character
        /// </summary>
        Delete = 24,

        /// <summary>
        /// 25: Stop Character
        /// </summary>
        StopCharacter = 25,
    }
}
