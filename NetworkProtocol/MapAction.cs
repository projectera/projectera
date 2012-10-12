using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Protocols
{
    public enum MapAction : byte
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Max = 12,

        /// <summary>
        /// 1: Get MapData
        /// </summary>
        Get = 1,

        /// <summary>
        /// 2: Get TilesetData
        /// </summary>
        GetTilesetData = 2,

        /// <summary>
        /// 3: Get Tileset Graphic file
        /// </summary>
        //GetTilesetGraphic = 3,

        /// <summary>
        /// 4: Get Autotile Graphic file
        /// </summary>
        //GetAutotileGraphic = 4,

        /// <summary>
        /// 5: Get Interactables
        /// </summary>
        GetInteractables = 5,

        /// <summary>
        /// 6: Validate Data
        /// </summary>
        ValidateData = 6,

        /// <summary>
        /// 7: Validate Graphics
        /// </summary>
        ValidateGraphics = 7,

        /// <summary>
        /// 10: Interactable Joined
        /// </summary>
        InteractableJoined = 10,

        /// <summary>
        /// 11: Interactable Left
        /// </summary>
        InteractableLeft = 11,
    }
}
