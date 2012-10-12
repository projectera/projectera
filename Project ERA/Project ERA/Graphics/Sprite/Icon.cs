using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Display;
using Microsoft.Xna.Framework.Content;
using ERAUtils.Enum;

namespace ProjectERA.Graphics.Sprite
{
    [Serializable]
    internal class Icon : DrawableComponent
    {

        private static readonly Rectangle SingleSize = new Rectangle(0, 0, 24, 24);

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public String AssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public Rectangle SourceRect
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal Vector2 Position 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// 
        /// </summary>
        internal Texture2D Texture
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private TextureManager TextureManager
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vx"></param>
        /// <returns></returns>
        internal static Icon Generate(IconsetVx vx)
        {
            Int32 index = (Int32)vx;

            Icon result = new Icon();
            result.AssetName = "iconset_vx";
            result.SourceRect = new Rectangle(index % 16 * 24, index / 16 * 24, 24, 24);
 
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iconAssetName"></param>
        /// <returns></returns>
        internal static Icon Generate(String iconAssetName)
        {
            Icon result = new Icon();
            result.SourceRect = Icon.SingleSize;
            result.AssetName = iconAssetName;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textureManager"></param>
        internal void Initialize(TextureManager textureManager)
        {
            this.TextureManager = textureManager;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Initialize()
        {
            throw new NotSupportedException("Call Initialize(TextureManager instead)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        internal void Initialize(Game game)
        {
            Initialize(game.Services.GetService(typeof(TextureManager)) as TextureManager);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textureManager"></param>
        /// <param name="contentManager"></param>
        internal override void LoadContent(ContentManager contentManager)
        {
            this.Texture = this.TextureManager.LoadStaticTexture(@"Graphics\Icons\" + this.AssetName, contentManager);
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void UnloadContent()
        {
            if (this.Texture != null)
                this.TextureManager.UnloadStaticTexture(@"Graphics\Icons\" + this.AssetName);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal override void Draw(GameTime gameTime, bool drawTransparent)
        {
            ///throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Update(GameTime gameTime)
        {
            ///throw new NotImplementedException();
        }
    }
}
