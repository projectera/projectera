using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectERA.Screen
{
    internal class VariableLoadingPopup : Services.Display.GameScreen
    {
        private Vector2 _screenPosition;
        private Action _cancelAction;
        private Boolean _canCallExit;
        private Texture2D _textureBackground;
        private Texture2D _textureForeground;
        private Texture2D _textureCancelText;
        private Texture2D _textureCancelTextX;
        private Texture2D _textureCancelTextX_inactive;

        public String DisplayText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsCancelEnabled
        {
            get { return _cancelAction != null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsCompleteTransitionEnforced
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsCancelActive
        {
            get
            {
                return this.InputManager.Mouse.IsOverObj(new Rectangle((Int32)_screenPosition.X, (Int32)_screenPosition.Y, 77, 77)) ||
                    this.InputManager.Mouse.IsOverObj(new Rectangle((Int32)_screenPosition.X + 77, (Int32)_screenPosition.Y + 35 - _textureCancelTextX.Height / 2, _textureCancelTextX.Width, _textureCancelTextX.Height));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void FinishProgress()
        {
            // Reaching 100% means terminition is possible
            // Note: even if other thread decreases progress
            // this screen will finish and exit!
            _canCallExit = (!IsCompleteTransitionEnforced || !this.IsTransitioning);
            _hasCalledExited = true;
        }

        /// <summary>
        /// 
        /// </summary>
        internal VariableLoadingPopup()
            : this(new Vector2(1280/2, 720/2))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onCancel"></param>
        internal VariableLoadingPopup(Action onCancel)
            : this()
        {
            _cancelAction = onCancel;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal VariableLoadingPopup(Vector2 position)
            : base()
        {
            // Make this a popup
            this.IsPopup = true;

            // Set position
            _screenPosition = position;
        }

        internal VariableLoadingPopup(Vector2 position, Action onCancel)
            : this(position)
        {
            _cancelAction = onCancel;
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isCompleteTransitionEnforced"></param>
        internal VariableLoadingPopup(Vector2 position, Boolean isCompleteTransitionEnforced)
            : this(position)
        {
            this.IsCompleteTransitionEnforced = isCompleteTransitionEnforced;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isCompleteTransitionEnforced"></param>
        internal VariableLoadingPopup(Action onCancel, Boolean isCompleteTransitionEnforced)
            : this(onCancel)
        {
            this.IsCompleteTransitionEnforced = isCompleteTransitionEnforced;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="onCancel"></param>
        /// <param name="isCompleteTransitionEnforced"></param>
        internal VariableLoadingPopup(Vector2 position, Action onCancel, Boolean isCompleteTransitionEnforced)
            : this(position)
        {
            _cancelAction = onCancel;
            this.IsCompleteTransitionEnforced = isCompleteTransitionEnforced;
        }


        /// <summary>
        /// 
        /// </summary>
        internal override void Initialize()
        {
            // Set the transition Times
            this.TransitionOnTime = TimeSpan.FromSeconds(.5f);
            this.TransitionOffTime = TimeSpan.FromSeconds(.5f);

            base.Initialize();

            this.IsCapturingInput = false;
            this.Position = _screenPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            // Set contentManager
            base.LoadContent(contentManager);

            _textureBackground = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\icon_loading_background_big", this.ContentManager);

            if (this.IsCancelEnabled)
            {
                _textureForeground = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\icon_loading_red_big", this.ContentManager);
                _textureCancelText = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\text_cancel_loading", this.ContentManager);
                _textureCancelTextX = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\text_cancel_loading_x", this.ContentManager);
                _textureCancelTextX_inactive = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\text_cancel_loading_x_inactive", this.ContentManager);
            }
            else
            {
                _textureForeground = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\icon_loading_blue_big", this.ContentManager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void UnloadContent()
        {
            base.UnloadContent();

            this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\icon_loading_background_big");

            if (this.IsCancelEnabled)
            {
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\icon_loading_red_big");
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\text_cancel_loading");
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\text_cancel_loading_x");
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\text_cancel_loading_x_inactive");
            }
            else
            {
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\icon_loading_blue_big");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Draw(GameTime gameTime)
        {
            this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            Color colorBackground = new Color(255, 255, 255, (Byte)(this.TransitionAlpha > 127 ? 255 : Math.Min(255, this.TransitionAlpha * 2)));
            Color colorForeground = new Color(255, 255, 255, (Byte)(this.TransitionAlpha < 127 ? 0 : Math.Min(255, (this.TransitionAlpha - 127) * 2)));

            // Bar
            this.ScreenManager.SpriteBatch.Draw(_textureBackground, _screenPosition, _textureBackground.Bounds, colorBackground, 0f,
                            new Vector2(_textureBackground.Width / 2f, _textureBackground.Height / 2f), 1f, SpriteEffects.None, 0);
            this.ScreenManager.SpriteBatch.Draw(_textureForeground, _screenPosition, _textureForeground.Bounds, colorForeground, -(Single)(gameTime.TotalGameTime.TotalSeconds * MathHelper.Pi * 2),
                            new Vector2(_textureForeground.Width / 2f, _textureForeground.Height / 2f), 1f, SpriteEffects.None, 0);

            // Display Text
            if (!String.IsNullOrWhiteSpace(DisplayText))
            {
                Vector2 size = this.ScreenManager.SpriteFonts["Framerate"].MeasureString(this.DisplayText);
                size = new Vector2(((Int32)size.X/2), ((Int32)size.Y/2));

                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Framerate"], this.DisplayText, _screenPosition + Vector2.One,
                    Color.Black * (colorForeground.A / 255f), 0, size, 1, SpriteEffects.None, 0);
                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Framerate"], this.DisplayText, _screenPosition,
                    colorForeground, 0, size, 1, SpriteEffects.None, 0);
            }

            // Cancel enabled
            if (this.IsCancelEnabled)
            {
                if (this.IsCancelActive)
                    this.ScreenManager.SpriteBatch.Draw(_textureCancelTextX, _screenPosition + new Vector2(_textureForeground.Width / 2f - _textureCancelTextX.Width / 2f, _textureForeground.Height / 2f - _textureCancelTextX.Height / 2f), colorForeground);
                else
                    this.ScreenManager.SpriteBatch.Draw(_textureCancelTextX_inactive, _screenPosition + new Vector2(_textureForeground.Width / 2f - _textureCancelTextX.Width / 2f, _textureForeground.Height / 2f - _textureCancelTextX.Height / 2f), colorForeground);

                this.ScreenManager.SpriteBatch.Draw(_textureCancelText, _screenPosition + new Vector2(_textureForeground.Width, _textureForeground.Height / 2f - _textureCancelText.Height / 2f), colorForeground);
            }

            this.ScreenManager.SpriteBatch.End();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        internal override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Update base
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (this.Position != _screenPosition)
            {
                _screenPosition.X = MathHelper.Lerp(_screenPosition.X, this.Position.X, (Single)Math.Min(1, gameTime.ElapsedGameTime.TotalSeconds * 50));
                _screenPosition.Y = this.Position.Y;
            }

            // If not already exiting and exiting is allowed and done loading...
            if (!this.IsExiting && _canCallExit) // && !this.IsTransitioning)
                // ...exit
                ExitScreen();

            if (!this.IsExiting && _hasCalledExited && !_canCallExit)
                if (!IsCompleteTransitionEnforced || !this.IsTransitioning)
                    ExitScreen();
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void HandleInput()
        {
            if (this.IsCancelEnabled == false)
                return;

            if (this.InputManager.Mouse.IsButtonReleased(Services.Input.MouseButton.Left) && this.IsCancelActive)
            {
                _cancelAction.Invoke();
                _canCallExit = true;
                _hasCalledExited = true;
            }
        }
    }
}
