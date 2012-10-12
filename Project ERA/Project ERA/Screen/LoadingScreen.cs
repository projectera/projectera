using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Threading;
using System.Text;
using ProjectERA.Services.Display;

namespace ProjectERA.Screen
{
    /// <summary>
    /// The Loadingscreen provides the methods to nicely load any data while
    /// showing a loading animation and progressbar.
    /// </summary>
    internal class LoadingScreen : Services.Display.GameScreen
    {
        private LoadingContent _loadingContent;
        private Single _loadingProgress;
        private Single _realLoadingProgress;
        private String _backgroundAsset;
        private String _colorSuffix;
        private Boolean _canCallExit;
        private Texture2D _textureBackground;
        private Texture2D _textureBarBack;
        private Texture2D _textureBarFront;
        private Texture2D _textureActiveSpin;
        private Texture2D _textureSpinBackground;
        private List<LoadingPop> _loadingPops;
        private Double _previousPopTime;

        internal static String NextBackground = "load_dc_beach";
        internal static String NextColor = "brown";

        private List<String> _randomMizedList = new List<String> {
            "Powering underwater pumps", "Dividing Luck and Happiness","Populating villages", 
            "Crowding markets", "Hiding chests", "Spreading diseases", "Occupying unoccupied area\'s", 
            "Drying deserts", "Curing incurable illnesses", "Triangulating position markers", 
            "Calculating vegetation growth", "Configuring fog densities", "Fine tuning audio output", 
            "Compiling monster data", "Digitalizing analog time works", "Simulating doom scenario\'s", 
            "Stimulating global warming", "Flattening 3D worlds", "Extrapolating grass growth curves", 
            "Reversing tree deterioration", "Composing music algorithms", "Arranging orchestras", 
            "Forecasting Weather","Removing monster waste products", "Reviving dead undead", 
            "Crowding markets", "Arresting shoplifters", "Hiding chests", "Stocking inventories", 
            "Synthesizing Synths", "Polishing polished weaponry", "Analyzing friendly area\'s", 
            "Deciphering decryption methods", "Initializing heroic charismatics", "Write dialogs",
            "Defining definitions", "Randomizing randomizers", "Seeding vegetation", "Spoiling rich kids",
            "Inherit inheritances", "Calibrating personality matrices" };

        private Queue<String> _randomUsedQueue;

        /// <summary>
        /// Enumeration for data type loading
        /// </summary>
        private enum LoadingContent
        {
            ServerData = 0,
            MapData,
        }

        /// <summary>
        /// Gets the Current Progress
        /// </summary>
        internal Single Progress
        {
            get
            {
                return _loadingProgress;
            }
        }

        /// <summary>
        /// Sets the current progress + value
        /// </summary>
        /// <param name="value">value to add</param>
        public void ProgressBy(Single value)
        {
            Single snapshot, updated;
            do {
                // Atomic operation, snapshot
                snapshot = _loadingProgress;

                // Compute the new value
                updated = MathHelper.Clamp(snapshot + value, 0, 1);

                // CompareExchange compares totalValue to initialValue. If
                // they are not equal, then another thread has updated the
                // running total since this loop started. CompareExchange
                // does not update totalValue. CompareExchange returns the
                // contents of totalValue, which do not equal initialValue,
                // so the loop executes again.
            } while (snapshot != Interlocked.CompareExchange(ref _loadingProgress, updated, snapshot));

            // Pop a new text from the list
            Pop(_randomMizedList[new Random().Next(_randomMizedList.Count)]);

            // Reaching 100% means terminition is possible
            if (_loadingProgress == 1f)
            {
                _canCallExit = true;
                _hasCalledExited = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="maxparts"></param>
        public void ProgressBy(Single parts, Single maxparts)
        {
            ProgressBy(parts / maxparts);
        }

        /// <summary>
        /// 
        /// </summary>
        public void FinishProgress()
        {
            ProgressBy(1f - Progress);
        }

        /// <summary>
        /// 
        /// </summary>
        internal LoadingScreen()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Constructor with set Background [popup/blue/mapdata]
        /// </summary>
        /// <param name="backgroundAsset">Background to Load</param>
        internal LoadingScreen(String backgroundAsset)
            : this(backgroundAsset, "blue")
        {
        }

        /// <summary>
        /// Constructor with set Background and Color Scheme [popup/mapdata]
        /// </summary>
        /// <param name="backgroundAsset">Background to Load</param>
        /// <param name="colorSuffix">Color to Use</param>
        internal LoadingScreen(String backgroundAsset, String colorSuffix)
            : base()
        {
            // Make this a popup
            this.IsPopup = true;

            // Set the Background
            _backgroundAsset = backgroundAsset;
            // Set the Color
            _colorSuffix = colorSuffix;
            // Set the Content according to Background Status
            _loadingContent = (_backgroundAsset == System.String.Empty ? LoadingContent.ServerData : LoadingContent.MapData);
        }


        /// <summary>
        /// Constructor as Screen [screen]
        /// </summary>
        /// <param name="backgroundAsset">Background to Load</param>
        /// <param name="colorSuffix">Color to Use</param>
        /// <param name="nextScreen">Next Screen after Loading</param>
        internal LoadingScreen(String backgroundAsset, String colorSuffix, Services.Display.GameScreen nextScreen)
            : base()
        {
            // This is not a popup
            this.IsPopup = false;
            // Mark screen for next in queue
            this.Next = nextScreen;

            // Load the Background
            _backgroundAsset = backgroundAsset;
            // Set the Color
            _colorSuffix = colorSuffix;
            // Set the Content according to background
            _loadingContent = (_backgroundAsset == System.String.Empty ? LoadingContent.ServerData : LoadingContent.MapData);
        }

        /// <summary>
        /// Initialize this Screen
        /// </summary>
        internal override void Initialize()
        {
            // Set the transition Times
            this.TransitionOnTime = TimeSpan.FromSeconds(1);
            this.TransitionOffTime = TimeSpan.FromSeconds(1);

            // Used Queue
            _randomUsedQueue = new Queue<String>();
            // Set the Popcolor to blue
            PopColor = new Color(18, 124, 233, 255);
            // Visible Progress sticks to Actual Progress
            _realLoadingProgress = this.Progress;
            // Create the loadingpops list
            _loadingPops = new List<LoadingPop>();

            // Initialize Base
            base.Initialize();

            // Break branch on color sceme string
            switch (_colorSuffix)
            {
               
                // Brown
                case "brown":
                    PopColor = new Color(153, 89, 12, 255);
                    break;

                default: // case "blue":
                    PopColor = new Color(18, 124, 233, 255);
                    break;
            }

            // LoadingContent Captures all input
            this.IsCapturingInput = true;
        }

        /// <summary>
        /// Load all Content
        /// </summary>
        /// <param name="contentManager">Content Manager to load to</param>
        internal override void LoadContent(ContentManager contentManager)
        {
            // Save the ContentManager
            base.LoadContent(contentManager);

            _textureBackground = this.ScreenManager.TextureManager.LoadStaticTexture(new StringBuilder(@"Graphics\Backgrounds\").Append((_backgroundAsset == String.Empty ? "loading" : _backgroundAsset)).ToString(), this.ContentManager);
            _textureBarBack = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Pictures\loading_back_" + _colorSuffix, this.ContentManager);
            _textureBarFront = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Pictures\loading_front_" + _colorSuffix, this.ContentManager);
            _textureActiveSpin = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Pictures\loading_icon_" + _colorSuffix, this.ContentManager);
            _textureSpinBackground = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Pictures\loading_icon_background", this.ContentManager);
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void UnloadContent()
        {
            lock (_randomMizedList)
            {
                base.UnloadContent();
            }
        }

        /// <summary>
        /// Draw to Screen
        /// </summary>
        /// <param name="gameTime">Snapshot of Timing Values</param>
        internal override void Draw(GameTime gameTime)
        {
            // Ugly Locking until barrier is in place
            lock (_randomMizedList)
            {
                if (this.ContentManager != null)
                {
                    try
                    {
                        // Open pipe for drawing data
                        this.ScreenManager.SpriteBatch.Begin();

                        // Draw all textures
                        this.ScreenManager.SpriteBatch.Draw(_textureBackground, new Vector2(this.ScreenManager.ScreenWidth - _textureBackground.Width, this.ScreenManager.ScreenHeight - _textureBackground.Height) / 2, new Rectangle(0, 0, _textureBackground.Width, _textureBackground.Height), Color.White, 0, Vector2.Zero, (Single)Math.Max(((this.ScreenManager.ScreenWidth) / _textureBackground.Width), ((this.ScreenManager.ScreenHeight) / _textureBackground.Height)), SpriteEffects.None, 1);
                        this.ScreenManager.SpriteBatch.Draw(_textureBarFront, new Vector2(this.ScreenManager.ScreenWidth - _textureBarFront.Width, this.ScreenManager.ScreenHeight - _textureBarFront.Height) / 2, Color.White);
                        // Draw the bar
                        this.ScreenManager.SpriteBatch.Draw(_textureBarBack,
                            new Rectangle(251 + (Int32)(297 * (_realLoadingProgress) + (this.ScreenManager.ScreenWidth - _textureBackground.Width) / 2), (Int32)(301 + (Single)(this.ScreenManager.ScreenHeight - _textureBackground.Height) / 2), (Int32)(297 * (1 - _realLoadingProgress)), 18),
                            new Rectangle(251 + (Int32)(297 * (_realLoadingProgress)), 301, (Int32)(297 * (1 - _realLoadingProgress)), 18),
                            Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                        // Draw Status Indicator
                        this.ScreenManager.SpriteBatch.Draw(_textureSpinBackground, new Vector2(this.ScreenManager.ScreenWidth - (800 - 775), this.ScreenManager.ScreenHeight - (640 - 620)), new Rectangle(0, 0, 24, 24),
                            Color.White, 0f, new Vector2(_textureActiveSpin.Width / 2, _textureActiveSpin.Height / 2), 1f, SpriteEffects.None, 0);
                        // Draw the actual spinning indicator
                        this.ScreenManager.SpriteBatch.Draw(_textureActiveSpin, new Vector2(this.ScreenManager.ScreenWidth - (800 - 775), this.ScreenManager.ScreenHeight - (640 - 620)), new Rectangle(0, 0, 24, 24),
                            Color.White, (Single)(gameTime.TotalGameTime.TotalSeconds * MathHelper.Pi * 2),
                            new Vector2(_textureActiveSpin.Width / 2, _textureActiveSpin.Height / 2), 1f, SpriteEffects.None, 0);

                        LoadingPop[] i_loadingPops;

                        // For All LoadingPopps
                        lock (_loadingPops)
                        {
                            i_loadingPops = new LoadingPop[_loadingPops.Count];
                            // Copy them
                            _loadingPops.CopyTo(i_loadingPops);
                        }
                        // And Draw the copies
                        foreach (LoadingPop i_loadingPop in i_loadingPops)
                            i_loadingPop.Draw();

                        // Flush Pipe to screen
                        this.ScreenManager.SpriteBatch.End();
                    }
                    catch (ObjectDisposedException)
                    {
                        this.ScreenManager.SpriteBatch.End();
                    }
                }
            }

            // Fade Buffer if needed
            if (this.IsTransitioning || this.IsActive == false)
                this.ScreenManager.FadeBackBufferToBlack((Byte)(255 - TransitionAlpha));

            // Draw Base
            base.Draw(gameTime);
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of Timing Values</param>
        /// <param name="otherScreenHasFocus">otherGame.IsActive</param>
        /// <param name="coveredByOtherScreen">otherScreen.IsActive</param>
        internal override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If sufficient time has passed fading in and
            // visual progress doesn't match actual progress...
            if (this.TransitionAlpha > 125 && _realLoadingProgress < this.Progress)
                _realLoadingProgress = MathHelper.Clamp((Single)(_realLoadingProgress + Math.Max((this.Progress - _realLoadingProgress) * gameTime.ElapsedGameTime.TotalMilliseconds / 120, 0.00001)), 0, 1);
            // ...increase the visual progress value
            //_realLoadingProgress = MathHelper.Clamp(_realLoadingProgress + Math.Max(((this.Progress - _realLoadingProgress) / 20), 0.001f), 0, 1);

            // If not already exiting and exiting is allowed and done loading...
            if (!this.IsExiting && _canCallExit && _realLoadingProgress == 1f)
                // ...exit
                ExitScreen();

            LoadingPop[] i_loadingPops;

            // For All LoadingPopps
            lock (_loadingPops)
            {
                i_loadingPops = new LoadingPop[_loadingPops.Count];
                // Copy them
                _loadingPops.CopyTo(i_loadingPops);
            }

            // And iterate trough them
            foreach (LoadingPop i_loadingPop in i_loadingPops)
            {
                // Update the loadingpop
                i_loadingPop.Update(gameTime);
                // If it is no longer visible...
                if (i_loadingPop.IsVisible == false)
                    lock (_loadingPops)
                    {
                        // ...remove it from the lists
                        _loadingPops.Remove(i_loadingPop);
                    }
            }

            _previousPopTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Base Update
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Set PopColor for Loading pops
        /// </summary>
        internal Color PopColor
        {
            set { LoadingPop.BaseColor = value; }
        }

        /// <summary>
        /// Pop a text on screen
        /// </summary>
        /// <param name="text">text to pop</param>
        internal void Pop(String text)
        {
            if (Monitor.TryEnter(_loadingPops))
            {
                if (_loadingPops != null && (_loadingPops.Count == 0 || _previousPopTime >= 500f))
                {
                    if (_randomUsedQueue == null)
                        _randomUsedQueue = new Queue<String>();
                    _randomUsedQueue.Enqueue(text);
                    _randomMizedList.Remove(text);
                    _previousPopTime = 0;
                    _loadingPops.Add(new LoadingPop(text, ScreenManager));

                    if (_randomUsedQueue.Count >= _randomMizedList.Count)
                        lock (_randomMizedList)
                            _randomMizedList.Add(_randomUsedQueue.Dequeue());
                }

                Monitor.Exit(_loadingPops);
            }
        }
    }

    /// <summary>
    /// LoadingPop class is a helper class to show loading popup text during loading
    /// </summary>
    internal class LoadingPop
    {
        #region Fields
        private String _loadingText;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Single _lifeTime;
        private Vector2 _position;
        #endregion

        /// <summary>
        /// Constant: Max Life time of the pops
        /// </summary>
        private const Single c_MaxLifeTime = 2000f;

        /// <summary>
        /// Get the current Position
        /// </summary>
        private Vector2 Position
        {
            get
            {
                return _position - new Vector2(0f, (Single)Math.Sqrt(_lifeTime / c_MaxLifeTime) * 30);
            }
        }

        /// <summary>
        /// BaseColor [aka 100% color] of all pops
        /// </summary>
        internal static Color BaseColor = new Color(18, 124, 233, 255);

        /// <summary>
        /// Get the Current Color
        /// </summary>
        private Color Color
        {
            get
            {
                return new Color(BaseColor.ToVector4() * ((Single)(c_MaxLifeTime - _lifeTime) / c_MaxLifeTime));
            }
        }

        /// <summary>
        /// Visiblility Flag
        /// </summary>
        internal Boolean IsVisible
        {

            get { return (_lifeTime < c_MaxLifeTime); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">Text to pop</param>
        /// <param name="spriteFont">Font to use</param>
        /// <param name="spriteBatch">Spritebatch to draw to</param>
        internal LoadingPop(String text, ScreenManager screenManager)
        {
            // Set all Variables
            _loadingText = text;
            _spriteFont = screenManager.SpriteFonts["Default"];
            _spriteBatch = screenManager.SpriteBatch;

            // Set [Random] x position
            _position = Vector2.Zero;
            _position.X = (Single)(251 + (Randomizer.NextDouble() * 297 - (Int32)_spriteFont.MeasureString(text).X / 2) + (screenManager.ScreenWidth - 800) / 2); //- (Int32)_spriteFont.MeasureString(text).X /// 2
            _position.Y = 287 + (screenManager.ScreenHeight - 640) / 2;
        }

        private static Lidgren.Network.NetRandom Randomizer = new Lidgren.Network.NetRandom();

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of Timing Values</param>
        internal void Update(GameTime gameTime)
        {
            // Add time to lifetime
            _lifeTime += (Single)(gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        /// <summary>
        /// Draw to screen
        /// </summary>
        internal void Draw()
        {
            // If still visible
            if (this.IsVisible)
            {
                // ...draw the poptext
                //_spriteBatch.DrawString(_spriteFont, _loadingText, Position + Vector2.One, new Color(0,0,,(Byte)(255 * (MaxLifeTime - _lifeTime)/_lifeTime)));
                _spriteBatch.DrawString(_spriteFont, _loadingText, this.Position + Vector2.One, new Color(0, 0, 0, Color.A));
                _spriteBatch.DrawString(_spriteFont, _loadingText, this.Position, Color);

            }
        }
    }
}
