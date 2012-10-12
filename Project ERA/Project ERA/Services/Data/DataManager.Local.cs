using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Display;

namespace ProjectERA.Services.Data
{
    internal partial class DataManager
    {
        private static ProjectERA.Data.Player _localPlayer;
        private static ProjectERA.Graphics.Sprite.Interactable _localSprite;

        /// <summary>
        /// 
        /// </summary>
        internal static ProjectERA.Data.Interactable LocalAvatar
        {
            get { return DataManager.LocalPlayer.ActiveAvatar; }
            set { DataManager.LocalPlayer.ActiveAvatar = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static ProjectERA.Data.Player LocalPlayer
        {
            get { return _localPlayer; }
            set { _localPlayer = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static ProjectERA.Graphics.Sprite.Interactable LocalSprite
        {
            get { return _localSprite; }
            set { _localSprite = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static List<ProjectERA.Graphics.Sprite.Interactable> LocalSprites
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal static MapData LocalMapData
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal static ProjectERA.Graphics.Sprite.TileMap LocalTileMap
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal static Camera3D LocalCamera
        {
            get;
            set;
        }
    }
}
