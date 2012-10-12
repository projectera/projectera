using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Graphics;
using Microsoft.Xna.Framework;
using ProjectERA.Services.Display;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Data;
using ProjectERA.Services.Input;

namespace ProjectERA.Graphics
{
    internal abstract class Widget : DrawableComponent
    {
        /// <summary>
        /// ScreenManager reference
        /// </summary>
        protected ScreenManager ScreenManager
        {
            get;
            set;
        }

        /// <summary>
        /// TextureManager reference
        /// </summary>
        protected TextureManager TextureManager
        {
            get { return this.ScreenManager.TextureManager; }
        }
        
        /// <summary>
        /// InputManager reference
        /// </summary>
        protected InputManager InputManager
        {
            get { return this.ScreenManager.InputManager; }
        }

        /// <summary>
        /// FontCollector reference
        /// </summary>
        protected FontCollector FontCollector
        {
            get { return this.ScreenManager.SpriteFonts; }
        }

        /// <summary>
        /// Spritebatch
        /// </summary>
        protected SpriteBatch SpriteBatch
        {
            get;
            set;
        }

        /// <summary>
        /// Clickable Surface
        /// </summary>
        public Rectangle Surface
        {
            get;
            protected set;
        }

        /// <summary>
        /// Display Position
        /// </summary>
        public Vector2 Position
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        internal Widget(Game game, Camera3D camera)
            :base(game, camera)
        {
            this.ScreenManager = (ScreenManager)game.Services.GetService(typeof(ScreenManager));
            this.Surface = Rectangle.Empty;
            this.SpriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            this.IsSemiTransparant = true;
        }

        /// <summary>
        /// 
        /// </summary>
        internal abstract void HandleInput();
    }
}
