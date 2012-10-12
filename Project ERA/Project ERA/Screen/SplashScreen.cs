using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ProjectERA.Services.Display;

namespace ProjectERA.Screen
{
    /// <summary>
    /// 
    /// </summary>
    internal class SplashScreen : GameScreen
    {
        #region Fields
        private Texture2D[] _textures;
        private String[] _splashes;
        private String _contentPath;
        private Int32 _currentSplash = 0;

        private readonly TimeSpan _splashTime = TimeSpan.FromSeconds(2);
        private Double _splashPosition = 0;
        #endregion

        /// <summary>
        /// Gets or Sets the current Splash index
        /// </summary>
        private Int32 CurrentSplash
        {
            get
            {
                return _currentSplash;
            }
            set
            {
                // If value Exceeds availeble
                if (value >= _splashes.Length)
                {
                    _hasCalledExited = true;
                    this.ExitScreen();
                }
                else
                    // or set the currentSplash
                    _currentSplash = value;
            }

        }

        /// <summary>
        /// Splashscreen Constructor
        /// <param name="contentDirectory">Content Directory Path</param>
        /// <param name="splashes">Splashes assetnames</param>
        /// </summary>
        internal SplashScreen(String contentDirectory, params String[] splashes)
        {
            // Set the transition time
            this.TransitionOnTime = TimeSpan.FromSeconds(1);
            this.TransitionOffTime = TimeSpan.FromSeconds(1);

            // Set the type
            this.IsPopup = false;

            // Save the splashes asset names and path;
            _splashes = splashes;
            _contentPath = contentDirectory;
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="contentManager">ContentManager to load to</param>
        internal override void LoadContent(ContentManager contentManager)
        {
            // Save the Screenmanager and set the Content path;
            base.LoadContent(contentManager);
            ContentManager.RootDirectory = _contentPath;

            // Iteration Optimisation
            Int32 size = _splashes.Length;

            // Error handling
            if (size == 0)
            {
                // DEBUG: Possible Error, no input was found
                Debug.WriteLine("SplashScreen: Argument input was zero in length. Did you miss any input?", "Possible Error");
                // Kill transitions
                TransitionOffTime = TransitionOnTime = TimeSpan.Zero;
                // Exit this Screen
                this.ExitScreen();
            }

            // Create textures Array
            _textures = new Texture2D[size];

            // DEBUG: Notify Start Loading
            Debug.WriteLine(new StringBuilder("SplashScreen: Loading ").Append(size).Append(" items").ToString(), "Content Loaded");
            // DEBUG: Indent for Readability
            Debug.Indent();

            // Load the Textures
            for (Int32 count = 0; count < size; count++)
            {
                // Load the Texture
                _textures[count] = this.ScreenManager.TextureManager.LoadStaticTexture(_splashes[count], this.ContentManager);

                //this.ScreenManager.TextureManager.SaveStaticTexure(_contentPath + "/" + _splashes[count], _textures[count]);
            }

            // DEBUG: Outdent
            Debug.Unindent();
            // DEBUG: Notify End of Loading
            Debug.WriteLine("SplashScreen: Loading Completed", "Content Loaded");
        }

        internal override void UnloadContent()
        {
            base.UnloadContent();

            for (Int32 count = 0; count < _textures.Length; count++)
            {
                this.ScreenManager.TextureManager.ReleaseStaticTexture(_splashes[count]);
            }
        }

        /// <summary>
        /// Draw content
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        internal override void Draw(GameTime gameTime)
        {

            // If fading out
            if (CurrentSplash >= 1 && _splashPosition >= ((CurrentSplash) * (_splashTime.TotalMilliseconds + TransitionOnTime.TotalMilliseconds +
                TransitionOffTime.TotalMilliseconds) - TransitionOffTime.TotalMilliseconds) && _splashPosition < (CurrentSplash) * (_splashTime.TotalMilliseconds + TransitionOnTime.TotalMilliseconds +
                TransitionOffTime.TotalMilliseconds))
            {
                // Draw the `previous` splash on the Screen
                ScreenManager.SpriteBatch.Begin();
                ScreenManager.SpriteBatch.Draw(_textures[CurrentSplash - 1], ScreenManager.ScreenCenter, new Rectangle(0, 0, _textures[CurrentSplash - 1].Width, _textures[CurrentSplash - 1].Height), Color.White, 0f, new Vector2(_textures[CurrentSplash - 1].Width / 2, _textures[CurrentSplash - 1].Height / 2), 1f, SpriteEffects.None, 0f);
                ScreenManager.SpriteBatch.End();

                // Draw the black fading graphic
                ScreenManager.FadeBackBufferToBlack((Byte)(255 - (((CurrentSplash) * (_splashTime.TotalMilliseconds + TransitionOnTime.TotalMilliseconds +
                TransitionOffTime.TotalMilliseconds) - TransitionOffTime.TotalMilliseconds) - _splashPosition) / TransitionOffTime.TotalMilliseconds * 255));
            }
            // if fading in
            else if (_splashPosition >= ((CurrentSplash) * (_splashTime.TotalMilliseconds + TransitionOnTime.TotalMilliseconds +
                TransitionOffTime.TotalMilliseconds)) && _splashPosition < ((CurrentSplash + 1) * (_splashTime.TotalMilliseconds + TransitionOnTime.TotalMilliseconds +
                TransitionOffTime.TotalMilliseconds) - TransitionOffTime.TotalMilliseconds - _splashTime.TotalMilliseconds))
            {
                // Draw the `current` splash on the Screen
                ScreenManager.SpriteBatch.Begin();
                ScreenManager.SpriteBatch.Draw(_textures[CurrentSplash], ScreenManager.ScreenCenter, new Rectangle(0, 0, _textures[CurrentSplash].Width, _textures[CurrentSplash].Height), Color.White, 0f, new Vector2(_textures[CurrentSplash].Width / 2, _textures[CurrentSplash].Height / 2), 1f, SpriteEffects.None, 0f);
                ScreenManager.SpriteBatch.End();

                // Draw the black fading graphic
                ScreenManager.FadeBackBufferToBlack((Byte)((((CurrentSplash + 1) * (_splashTime.TotalMilliseconds + TransitionOnTime.TotalMilliseconds +
                TransitionOffTime.TotalMilliseconds) - TransitionOffTime.TotalMilliseconds - _splashTime.TotalMilliseconds) - _splashPosition) / TransitionOffTime.TotalMilliseconds * 255));
            }
            // if waiting
            else
            {
                // Draw the `current` splash on the Screen
                ScreenManager.SpriteBatch.Begin();
                ScreenManager.SpriteBatch.Draw(_textures[CurrentSplash], ScreenManager.ScreenCenter, new Rectangle(0, 0, _textures[CurrentSplash].Width, _textures[CurrentSplash].Height), Color.White, 0f, new Vector2(_textures[CurrentSplash].Width / 2, _textures[CurrentSplash].Height / 2), 1f, SpriteEffects.None, 0f);
                ScreenManager.SpriteBatch.End();
            }

            // Update drawing with fading
            if (IsTransitioning)
                // Draw the black fading graphic
                ScreenManager.FadeBackBufferToBlack((Byte)(255 - TransitionAlpha));

        }

        /// <summary>
        /// Update content
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        /// <param name="otherScreenHasFocus">Program InActive flag</param>
        /// <param name="coveredByOtherScreen">Screen InActive flag</param>
        internal override void Update(GameTime gameTime, Boolean otherScreenHasFocus, Boolean coveredByOtherScreen)
        {
            // If Screen is Active and not Transitioning
            if (this.IsActive && !this.IsTransitioning && !otherScreenHasFocus)
                // Jump to next if current needs to fade
                if (_splashPosition > ((CurrentSplash + 1) * (_splashTime.TotalMilliseconds + TransitionOnTime.TotalMilliseconds +
                    TransitionOffTime.TotalMilliseconds) - TransitionOffTime.TotalMilliseconds))
                    CurrentSplash = CurrentSplash + 1;

            // If this screen is focussed
            if (this.IsActive && !otherScreenHasFocus && !coveredByOtherScreen)
                // Keep counting splashposition
                _splashPosition += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Update Transitions
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void HandleInput()
        {
            // On trigger Enter: Speed up fading
            if (InputManager.Keyboard.IsKeyTriggerd(Keys.Enter) && _splashPosition < (CurrentSplash + 1) * (_splashTime + TransitionOnTime + TransitionOffTime).TotalMilliseconds - TransitionOffTime.TotalMilliseconds && _splashPosition > (CurrentSplash) * (_splashTime + TransitionOnTime + TransitionOffTime).TotalMilliseconds + TransitionOnTime.TotalMilliseconds)
            {
                _splashPosition = (CurrentSplash + 1) * (_splashTime + TransitionOnTime + TransitionOffTime).TotalMilliseconds - TransitionOffTime.TotalMilliseconds;

            }
            // On hitting Escape: Exit this screen
            if (InputManager.Keyboard.IsKeyTriggerd(Keys.Escape))
            {
                _hasCalledExited = true;
                ExitScreen();
            }
        }
    }
}
