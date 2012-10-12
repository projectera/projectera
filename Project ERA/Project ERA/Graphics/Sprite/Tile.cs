using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Display;
using ProjectERA.Services.Data;

namespace ProjectERA.Graphics.Sprite
{
    internal class Tile
    {
        // This is the Autotile animation graphical index array. It contains numbers
        // that point to the graphic part of an animating autotile.
        internal static readonly Byte[, ,] AutoTileIndex = { 
            { {27, 28, 33, 34}, { 5, 28, 33, 34}, {27,  6, 33, 34}, { 5,  6, 33, 34}, 
              {27, 28, 33, 12}, { 5, 28, 33, 12}, {27,  6, 33, 12}, { 5,  6, 33, 12} }, 
            { {27, 28, 11, 34}, { 5, 28, 11, 34}, {27,  6, 11, 34}, { 5,  6, 11, 34},
              {27, 28, 11, 12}, { 5, 28, 11, 12}, {27,  6, 11, 12}, { 5,  6, 11, 12} },
            { {25, 26, 31, 32}, {25,  6, 31, 32}, {25, 26, 31, 12}, {25,  6, 31, 12},
              {15, 16, 21, 22}, {15, 16, 21, 12}, {15, 16, 11, 22}, {15, 16, 11, 12} },
            { {29, 30, 35, 36}, {29, 30, 11, 36}, { 5, 30, 35, 36}, { 5, 30, 11, 36},
              {39, 40, 45, 46}, { 5, 40, 45, 46}, {39,  6, 45, 46}, { 5,  6, 45, 46} },
            { {25, 30, 31, 36}, {15, 16, 45, 46}, {13, 14, 19, 20}, {13, 14, 19, 12},
              {17, 18, 23, 24}, {17, 18, 11, 24}, {41, 42, 47, 48}, { 5, 42, 47, 48} },
            { {37, 38, 43, 44}, {37,  6, 43, 44}, {13, 18, 19, 24}, {13, 14, 43, 44},
              {37, 42, 43, 48}, {17, 18, 47, 48}, {13, 18, 43, 48}, { 1,  2,  7,  8} }
        };

        #region Private fields



        #endregion

        #region Properties

        internal UInt16 TileId { get; set; }

        internal Boolean IsAutotile
        {
            get { return this.TileId < 384; }
        }

        internal Vector2 TextureWidth { get; set; }

        internal Boolean IsAnimated { get; set; }
        internal Boolean IsOpaque { get; set; }
        internal Boolean IsSemiTransparent { get; set; }
        internal Vector2 TexturePos { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tileId">TileId</param>
        /// <param name="position">Position in Grid</param>
        /// <param name="priority">Priority of tile</param>
        /// <param name="frames">Number of frames</param>
        internal Tile(UInt16 tileId, TilesetData tileset)
        {
            this.TileId = tileId;

            this.IsSemiTransparent = tileset.SomeSemiTransparantTiles[TileId];
            this.IsOpaque = tileset.OpaqueTiles[TileId];

            if (this.IsAutotile)
            {
                Int32 pointer = TileId / 48 - 1;
                this.IsAnimated = pointer < tileset.AutotileAnimationFlags.Count && tileset.AutotileAnimationFlags[pointer];
                Int32 width = 768;
                Int32 height = 1792;
                Int32 i = this.TileId - 48;
                Int32 posx = (i / 56) * (32 * 4) + 1;
                Int32 posy = (i % 56) * 32 + 1;

                this.TexturePos = new Vector2((Single)posx / width, (Single)posy / height);
                this.TextureWidth = new Vector2(31f / width, 31f / height);

                //this.TexturePos = this.TextureWidth * new Vector2(4, 0);
            }
            else
            {
                Int32 numtiles = tileset.Tiles;
                Int32 height = numtiles < 512 ? (numtiles / 8 + 1) * 32 : 2048;
                Int32 width = (numtiles / 512 + 1) * (8 * 32);

                this.TexturePos = new Vector2((Single)(((this.TileId - 384) % 8) * 32 + ((this.TileId - 384) / 512) * 256) / width + 0.5f / width,
                    (Single)(((this.TileId - 384) % 512) / 8 * 32) / height + 0.5f / height);

                this.TextureWidth = new Vector2((Single)(((this.TileId - 384) % 8) * 32 + ((this.TileId - 384) / 512) * 256 + 32) / width,
                    (Single)(((this.TileId - 384) % 512) / 8 * 32 + 32) / height) - this.TexturePos;
            }
        }
    }
}
