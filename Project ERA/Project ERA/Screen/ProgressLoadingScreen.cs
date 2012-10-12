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
using Lidgren.Network;

namespace ProjectERA.Screen
{
    /// <summary>
    /// The ProgressLoadingScreen provides the methods to nicely load any data while
    /// showing a loading animation and progressbar.
    /// </summary>
    internal partial class ProgressLoadingScreen : Services.Display.GameScreen
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
        private Texture2D _textureLoadingText;
        private Texture2D _textureActiveSpin;
        private Texture2D _textureSpinBackground;
        private List<LoadingPop> _loadingPops;
        private Double _previousPopTime;
        private Double _nextStalePopAt;

        internal static String NextBackground = "load_dc_beach";
        internal static String NextColor = "brown";

        private const Single MinimumStalePopTime = .5f;//1f;
        private const Single RandomStalePopTime = .5f; //2.5f;
        private Queue<String> _randomUsedQueue;
        private List<String> _randomMizedList = LoadingPopText.List;

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
            lock (_randomMizedList)
                Pop(_randomMizedList[NetRandom.Instance.Next(_randomMizedList.Count)]);
        }

        /// <summary>
        /// Progresses by part/maxparts * 100%
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="maxparts"></param>
        public void ProgressBy(Single parts, Single maxparts)
        {
            ProgressBy(parts / maxparts);
        }

        /// <summary>
        /// Finish Progress by making it 100%
        /// </summary>
        public void FinishProgress()
        {
            ProgressBy(1f - Progress);

            // Reaching 100% means terminition is possible
            // Note: even if other thread decreases progress
            // this screen will finish and exit!
            _canCallExit = true;
            _hasCalledExited = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal ProgressLoadingScreen()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Constructor with set Background [popup/blue/mapdata]
        /// </summary>
        /// <param name="backgroundAsset">Background to Load</param>
        internal ProgressLoadingScreen(String backgroundAsset)
            : this(backgroundAsset, "blue")
        {
        }

        /// <summary>
        /// Constructor with set Background and Color Scheme [popup/mapdata]
        /// </summary>
        /// <param name="backgroundAsset">Background to Load</param>
        /// <param name="colorSuffix">Color to Use</param>
        internal ProgressLoadingScreen(String backgroundAsset, String colorSuffix)
            : base()
        {
            // Make this a popup
            this.IsPopup = true;

            // Set the Background
            _backgroundAsset = backgroundAsset;
            // Set the Color
            _colorSuffix = colorSuffix;
            // Set the Content according to Background Status
            _loadingContent = (String.IsNullOrEmpty(_backgroundAsset) ? LoadingContent.ServerData : LoadingContent.MapData);
        }


        /// <summary>
        /// Constructor as Screen [screen]
        /// </summary>
        /// <param name="backgroundAsset">Background to Load</param>
        /// <param name="colorSuffix">Color to Use</param>
        /// <param name="nextScreen">Next Screen after Loading</param>
        internal ProgressLoadingScreen(String backgroundAsset, String colorSuffix, Services.Display.GameScreen nextScreen)
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
            _loadingContent = (String.IsNullOrEmpty(_backgroundAsset) ? LoadingContent.ServerData : LoadingContent.MapData);
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
            PopColor = new Color(82, 116, 184, 255); //new Color(18, 124, 233, 255);
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

            _textureBackground = this.ScreenManager.TextureManager.LoadStaticTexture(new StringBuilder(@"Graphics\Backgrounds\").Append((String.IsNullOrEmpty(_backgroundAsset) ? "scene_background" : _backgroundAsset)).ToString(), this.ContentManager);
            _textureBarBack = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\bar_loading_background", this.ContentManager);
            _textureBarFront = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\bar_loading_" + _colorSuffix, this.ContentManager);
            _textureLoadingText = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Interface\text_loading", this.ContentManager);
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

                this.ScreenManager.TextureManager.ReleaseStaticTexture(new StringBuilder(@"Graphics\Backgrounds\").Append((String.IsNullOrEmpty(_backgroundAsset) ? "scene_background" : _backgroundAsset)).ToString());
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\bar_loading_background");
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\bar_loading_" );
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\text_loading");
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Pictures\loading_icon_" + _colorSuffix);
                this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Pictures\loading_icon_background");
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
                        this.ScreenManager.SpriteBatch.Draw(_textureBackground, Vector2.Zero, Color.White);
                        this.ScreenManager.SpriteBatch.Draw(_textureBarBack, new Vector2(444-5, 356-5), Color.White);
                        // Draw the bar
                        this.ScreenManager.SpriteBatch.Draw(_textureBarFront,
                            new Rectangle(444, 356, (Int32)(392 * (_realLoadingProgress)), 6),
                            new Rectangle(0, 0, (Int32)(392 * (_realLoadingProgress)), 6),
                            Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                        
                        // Draw the text
                        Single y = (this.IsTransitioning && !this.IsExiting) ? (8 * Math.Min(1, (this.TransitionAlpha / 255f) * 2)): 8;
                        this.ScreenManager.SpriteBatch.Draw(_textureLoadingText, new Vector2(444 - 5, 356 + y), new Color(Color.White.ToVector4() * (this.IsExiting ? 1 : this.TransitionAlpha / 255f)));

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

            if (this.TransitionAlpha > 125)
            {
                // Atomic read, so we can now freely use progressNow
                Single progressNow = this.Progress;

                // ...increase the visual progress value
                _realLoadingProgress = MathHelper.Lerp(_realLoadingProgress, progressNow, (Single)(gameTime.ElapsedGameTime.TotalMilliseconds / 120));

                // ...and snap if close
                if (Math.Abs(_realLoadingProgress - progressNow) < 0.00001)
                    _realLoadingProgress = progressNow;
            }

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


            if (_previousPopTime > _nextStalePopAt)
            {
                lock (_randomMizedList)
                    Pop(_randomMizedList[NetRandom.Instance.Next(_randomMizedList.Count)]);
                _nextStalePopAt = (NetRandom.Instance.NextSingle() * RandomStalePopTime + MinimumStalePopTime) * 1000;
            }

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
            // If can't get lock, someone is popping, so it would have failed on inner check anyway
            if (Monitor.TryEnter(_loadingPops))
            {
                // Pop when allowed
                if (_loadingPops != null && (_loadingPops.Count == 0 || _previousPopTime >= 500f))
                {
                    List<String> isUse = _loadingPops.ConvertAll<String>((pop) => { return pop.ToString(); });

                    if (_randomUsedQueue == null)
                        _randomUsedQueue = new Queue<String>();
                    _randomUsedQueue.Enqueue(text);
                    _randomMizedList.Remove(text);

                    // We don't want 2 of the same visible at the same time
                    while (isUse.Contains(text) && _randomMizedList.Count > 0)
                    {
                        text = _randomMizedList[0];
                        _randomMizedList.RemoveAt(0);
                        _randomUsedQueue.Enqueue(text);
                    }

                    _previousPopTime = 0;
                    _loadingPops.Add(new LoadingPop(text, ScreenManager));

                    // Restore some items to the randomizd list
                    while (_randomUsedQueue.Count >= (_randomMizedList.Count + _randomUsedQueue.Count) / 2)
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
            _position.X = (Single)(444 - (Int32)_spriteFont.MeasureString(text).X / 2 + (NetRandom.Instance.NextDouble() * 392));
            _position.Y = 356 - _spriteFont.MeasureString(text).Y;
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
 	         return this.IsVisible ? _loadingText : String.Empty;
        }
    }
}
