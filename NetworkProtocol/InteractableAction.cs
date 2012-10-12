using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Protocols
{
    public enum InteractableAction : byte  // DISUCSS: Get, GetList/GetUser, GetActive (= get0) ?
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Max = 31,

        /// <summary>
        /// 1: Get Interactable
        /// </summary>
        Get = 1,

        /// <summary>
        /// 2: Passivly Updating Interactable Data (invalidates cache)
        /// </summary>
        Update = 2,

        /// <summary>
        /// 5: Broadcasts a message
        /// </summary>
        Message = 5,

        /// <summary>
        /// 10: Appearance data
        /// </summary>
        Appearance = 10,
        
        /// <summary>
        /// 11: Add body part
        /// </summary>
        AppearanceAddBodyPart = 11,

        /// <summary>
        /// 12: Remove body part
        /// </summary>
        AppearanceRemoveBodyPart = 12,


        /// <summary>
        /// 20: Movement data
        /// </summary>
        Movement = 20,

        /// <summary>
        /// 21: Teleport data
        /// </summary>
        MovementTeleport = 21,


        /// <summary>
        /// 30: Battler data
        /// </summary>
        Battler = 30,
    }
}
