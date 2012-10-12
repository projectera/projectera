using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Data;
using ProjectERA.Graphics;

namespace ProjectERA.Graphics.Sprite
{
    internal class SplattedTile
    {
        internal Boolean IsVertical { get; set; }
        internal Tile[] Tiles { get; set; }

        /// <summary>
        /// This is the bottom left corner position
        /// </summary>
        internal Vector3 Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal Boolean IsSemiTransparent
        {
            get
            {
                Boolean isSemi = false;
                Boolean isOpaque = false;
                for (Int32 i = 0; i < _numTiles; i++)
                {
                    isSemi |= this.Tiles[i].IsSemiTransparent;
                    isOpaque |= this.Tiles[i].IsOpaque;
                }

                return isSemi && !isOpaque;
            }
        }

        protected Int32 _numTiles = 0; 

        /// <summary>
        /// Creates a new Splatted tile at a specified position
        /// </summary>
        /// <param name="position">The bottom left corner position</param>
        /// <param name="vertical">If the quad is horizontal or vertical</param>
        internal SplattedTile(Vector3 position, Boolean vertical)
        {
            Vector3 tilePosition = Vector3.Zero;
            tilePosition.X = position.X;
            tilePosition.Y = position.Y + position.Z;
            tilePosition.Z = position.Z;

            this.Position = tilePosition;

            this.IsVertical = vertical;
            //if (IsVertical)
            //    Position = new Vector3(Position.X, Position.Y + 1, Position.Z);
            this.Tiles = new Tile[3];
        }

        /// <summary>
        /// Adds a texture to be splatted on the current quad
        /// </summary>
        /// <param name="tile">The tile to be splatted</param>
        internal void AddTile(Tile tile)
        {
            this.Tiles[_numTiles++] = tile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="start"></param>
        /// <param name="firstIndice"></param>
        internal void SetIndices(Int32[] arr, Int32 start, Int32 firstIndice)
        {
            Int32[] indices = new Int32[] { 2, 3, 0, 0, 1, 2 };
            for (Int32 i = 0; i < 6; i++)
                arr[start + i] = firstIndice + indices[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="start"></param>
        internal void SetVertexes(SplattedTileVertex[] arr, Int32 start)
        {
            Vector3[] offsets  = new Vector3[] { Vector3.Zero, Vector3.Right, Vector3.Right + (this.IsVertical ? Vector3.Forward : Vector3.Up), (this.IsVertical ? Vector3.Forward : Vector3.Up) };
            Vector2[] texOffset = new Vector2[] { Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY };

            for (int i = 0; i < 4; i++)
            {
                SetVertex(out arr[start + i]);
                arr[start + i].vertexPosition = Position + offsets[i];
                if (this.Tiles[0] != null)
                    arr[start + i].texturePos1 += texOffset[i] * this.Tiles[0].TextureWidth;
                if (this.Tiles[1] != null)
                    arr[start + i].texturePos2 += texOffset[i] * this.Tiles[1].TextureWidth;
                if (this.Tiles[2] != null)
                    arr[start + i].texturePos3 += texOffset[i] * this.Tiles[2].TextureWidth;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertex"></param>
        protected void SetVertex(out SplattedTileVertex vertex)
        {
            vertex = new SplattedTileVertex();
            vertex.textures = (Byte)_numTiles;
            //if (!this.IsVertical)
            //    vertex.textures = 0;
            if (_numTiles == 0)
                return;
            if (this.Tiles[0].IsAutotile)
                if (this.Tiles[0].IsAnimated)
                    vertex.texture1 = 2;
                else
                    vertex.texture1 = 1;
            else
                vertex.texture1 = 0;
            vertex.texturePos1 = this.Tiles[0].TexturePos;
            if (_numTiles == 1)
                return;
            if (this.Tiles[1].IsAutotile)
                if (this.Tiles[1].IsAnimated)
                    vertex.texture2 = 2;
                else
                    vertex.texture2 = 1;
            else
                vertex.texture2 = 0;
            vertex.texturePos2 = this.Tiles[1].TexturePos;
            if (_numTiles == 2)
                return;
            if (this.Tiles[2].IsAutotile)
                if (this.Tiles[2].IsAnimated)
                    vertex.texture3 = 2;
                else
                    vertex.texture3 = 1;
            else
                vertex.texture3 = 0;
            vertex.texturePos3 = this.Tiles[2].TexturePos;
        }
    }
}
