using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ProjectERA.Services.Display;

namespace ProjectERA.Graphics.Sprite
{
    internal class Message : DrawableComponent
    {
        #region Fields
        private String _message;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Single _lifeTime;
        private Vector2 _position;
        #endregion

        /// <summary>
        /// Constant: Max Life time of the pops
        /// </summary>
        private const Single c_MaxLifeTime = 5000f;

        /// <summary>
        /// Get the current Position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        /// <summary>
        /// BaseColor [aka 100% color] of all pops
        /// </summary>
        internal static Color BaseColor = Color.White; //18, 124, 233, 255);
        internal static Color BaseShadowColor = Color.Black;

        /// <summary>
        /// Get the Current Color
        /// </summary>
        private Color Color
        {
            get
            {
                return BaseColor; //new Color(BaseColor.ToVector4() * ((Single)(c_MaxLifeTime - _lifeTime) / c_MaxLifeTime));
            }
        }

        /// <summary>
        /// Get the Current Color
        /// </summary>
        private Color ShadowColor
        {
            get
            {
                return BaseShadowColor;//new Color(BaseShadowColor.ToVector4() * ((Single)(c_MaxLifeTime - _lifeTime) / c_MaxLifeTime));
            }
        }

        /// <summary>
        /// Visiblility Flag
        /// </summary>
        internal new Boolean IsVisible
        {

            get { return (base.IsVisible && _lifeTime < c_MaxLifeTime); }
        }

        public Message(String message, ScreenManager screenManager)
        {
            // Set all Variables
            _message = message;
            _spriteFont = screenManager.SpriteFonts["Default"];
            _spriteBatch = screenManager.SpriteBatch;

            // Set position
            _position = Vector2.Zero;
        }

        public Message(String message)
        {
            _message = message;
            _position = Vector2.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Initialize()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void UnloadContent()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime, bool drawTransparent)
        {
            // If still visible
            if (this.IsVisible && !drawTransparent)
            {
                // ...draw the poptext
                _spriteBatch.DrawString(_spriteFont, _message, this.Position + Vector2.One, ShadowColor);
                _spriteBatch.DrawString(_spriteFont, _message, this.Position, Color);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // Add time to lifetime
            this.AddChange(() => _lifeTime += (Single)(gameTime.ElapsedGameTime.TotalMilliseconds));
        }
    }
}
