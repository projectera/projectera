using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum AssetType : ushort   // (1 << 15)
    {
        Invalid = 0,

        Autotile = (1 << 0),     // 1
        Animation = (1 << 1),     // 2
        Background = (1 << 2),     // 4
        Character = (1 << 3),     // 8
        Icon = (1 << 4),     // 16
        Interface = (1 << 5),     // 32
        Picture = (1 << 6),     // 64
        Tileset = (1 << 7),     // 128
        Splash = (1 << 8),     // 256

        Shader = (1 << 10),    // 1024
        Max = (1 << 15),
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RootType : ushort    // (1 << 15)
    {
        Invalid     = 0,

        // Graphics root
        Graphics    = AssetType.Autotile | AssetType.Animation | AssetType.Background | 
                      AssetType.Character | AssetType.Icon | AssetType.Interface | 
                      AssetType.Picture | AssetType.Splash | AssetType.Tileset,

        // No top root
        None        = AssetType.Shader,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class AssetPath
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns></returns>
        public static string Get(AssetType assetType)
        {
            String path = String.Empty;

            var rootTypes = System.Enum.GetValues(typeof(RootType));
            foreach (var rootType in rootTypes)
            {
                if (((RootType)rootType).HasFlag((RootType)assetType))
                {
                    if (!RootType.None.Equals(rootType))
                    {
                        path = rootType.ToString() + ".";
                    }

                    break;
                }
            }

            return path + assetType.ToString() + "s";
        }
    }
}
