using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectERA.Graphics
{
    internal struct SplattedTileVertex : IVertexType
    {
        internal Vector3 vertexPosition;
        internal float textures;
        /// <summary>
        /// Texture 0 is static tiles
        /// Texture 1 is static autotiles
        /// Texture 2 is animated autotiles
        /// </summary>
        internal float texture1;
        internal float texture2;
        internal float texture3;
        internal Vector2 texturePos1;
        internal Vector2 texturePos2;
        internal Vector2 texturePos3;

        internal readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.PointSize, 0),
            new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(36, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
}