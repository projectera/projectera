using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ERAUtils.Logger;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectERA.Screen;
using ProjectERA.Services;
using ProjectERA.Services.Data;
using ProjectERA.Services.Display;
using ProjectERA.Services.Input;
using ProjectERA.Services.Network;
using ProjectERA.Services.Data.Storage;

namespace ProjectERA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    internal class Client : Microsoft.Xna.Framework.Game
    {
        #region Private fields
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private NetworkManager _networkManager;
        private ScreenManager _screenManager;
        private FileManager _fileManager;
        private TilesetManager _tilesetManager;
        private MapManager _mapManager;
        private DataManager _currentUserManager;
        private InputManager _inputManager;
        private TextureManager _textureManager;

        private TimeSpan _elapsedTime;
        private Int32 _frameRate, _frameCount;
        private SpriteFont _font;

        private Stopwatch _stopwatchTotal, _stopwatchUpdate, _stopwatchDraw;
        private Double _cycles = 0;
        #endregion

        #if DEBUG
            private const Double __MAXRUNTIME = 5;
        #endif

        #if !NOMULTITHREAD
            private Task _screenShotTask;
        #endif

        #region Properties

        /// <summary>
        /// Global SpriteBatch Exposed
        /// </summary>
        internal SpriteBatch SpriteBatch
        {
            get { return _spriteBatch; }
        }

        /// <summary>
        /// Global Graphics Device Exposed
        /// </summary>
        internal new GraphicsDevice GraphicsDevice
        {
            get { return _graphics.GraphicsDevice; }
        }

        /// <summary>
        /// Exposes ScreenManager
        /// </summary>
        internal ScreenManager ScreenManager
        {
            get { return _screenManager; }
            private set { _screenManager = value; }
        }

        /// <summary>
        /// Exposes InputManager
        /// </summary>
        internal InputManager InputManager
        {
            get { return _inputManager; }
            private set { _inputManager = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal FileManager FileManager
        {
            get { return _fileManager; }
            private set { _fileManager = value; }
        }

        /// <summary>
        /// Exposes Tilesetmanager
        /// </summary>
        internal TilesetManager TilesetManager
        {
            get { return _tilesetManager; }
            set { _tilesetManager = value; }
        }

        /// <summary>
        /// Exposes MapManager
        /// </summary>
        internal MapManager MapManager
        {
            get { return _mapManager; }
            set { _mapManager = value; }
        }

        /// <summary>
        /// Exposes CurrentUserManager
        /// </summary>
        internal DataManager CurrentUserManager
        {
            get { return _currentUserManager; }
            set { _currentUserManager = value; }
        }

        /// <summary>
        /// Exposes NetworkManager
        /// </summary>
        internal NetworkManager NetworkManager
        {
            get { return _networkManager; }
            private set { _networkManager = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        internal TextureManager TextureManager
        {
            get { return _textureManager; }
            private set { _textureManager = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        internal Client()
        {
            // LOGGING START
            Logger.Initialize(Severity.Debug, Severity.Debug);//GameSettings.LogSeverity, Severity.None);
            Logger.Info("Started Client");

            // COMPILATION SYMBOL CHECK
            // Logs the running compilation symbols
            #region Compilation Symbols
            StringBuilder symbols = new StringBuilder("Compiled with ");

            #if WINDOWS
                symbols.Append("WINDOWS ");
            #endif

            #if XBOX
                symbols.Append("XBOX ");
            #endif

            #if NOFAILSAFE 
                symbols.Append("NOFAILSAFE ");
            #endif

            #if NOMULTITHREAD
                symbols.Append("NOMULTITHREAD ");
            #endif

            #if SAVEGENERATEDTEXTURES
                symbols.Append("SAVEGENERATEDTEXTURES ");
            #endif

            #if SAVEGENERATEDNUMBERS
                symbols.Append("SAVEGENERATEDNUMBERS ");
            #endif

            #if DEBUG
                symbols.Append("DEBUG ");
            #endif

            #if TRACE
                symbols.Append("TRACE ");
            #endif

            Logger.Info(symbols.ToString());

            #endregion

            // ELEVATE PROCESS
            // Elevates the process' priority to above normal
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            Thread.CurrentThread.Name = "Main Thread";
            using (Process process = Process.GetCurrentProcess())
                process.PriorityClass = ProcessPriorityClass.AboveNormal;

            // GRAPHICS DEVICE AREA
            // Sets values for the graphicsdevice. As the ApplyChanges
            // function is called, these changed are processed.
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280; 
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.IsFullScreen = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.ApplyChanges();

            // GAME PROPERTIES AREA
            // Sets values for the properties of the game object (and
            // underlying ContentManager) directly processed.
            this.Content.RootDirectory = "Content";

            this.Window.Title = "[ERA] Epos of Realms and Alliances 1.0";
            this.Window.AllowUserResizing = false;
            this.IsFixedTimeStep = false; 
            this.IsMouseVisible = true;

            // DATABASE INITIALIZATION
            ContentDatabase.Initialize();

            // COMPONENT CREATION AREA
            // Creates components used in the game.
            this.InputManager = new InputManager(this);
            this.CurrentUserManager = new DataManager(this);
            this.NetworkManager = new NetworkManager(this);
            this.FileManager = new FileManager(this);
            this.TextureManager = new TextureManager(this);
            this.TilesetManager = new TilesetManager(this);     // NEEDS textureManager
            this.MapManager = new MapManager(this);             // NEEDS tilesetManager
            this.ScreenManager = new ScreenManager(this);       // NEEDS graphicsDevice, input-, network-, textureManager

            // EVENT HANDLING
            // Adds event hooks to game events
            this.Exiting += new EventHandler<EventArgs>(Client_Exiting);
       }

        /// <summary>
        /// Exiting Function
        /// </summary>
        /// <param name="sender">source</param>
        /// <param name="e">event arguments</param>
        private void Client_Exiting(object sender, EventArgs e)
        {
            _stopwatchTotal.Stop();

            Logger.Info("Closing Client");

            Logger.Info(new String[] { _cycles.ToString(), " cycles in ", Math.Round(_stopwatchTotal.ElapsedMilliseconds / 1000f, 2).ToString(), " seconds, that is on average ", Math.Round(_cycles / Math.Round(_stopwatchTotal.ElapsedMilliseconds / 1000f, 2), 0).ToString(), " fps "});
            Logger.Info(new String[] { Math.Round(_stopwatchDraw.ElapsedMilliseconds/_cycles, 4).ToString(), " ms/draw & ", 
                Math.Round(_stopwatchUpdate.ElapsedMilliseconds/_cycles, 4).ToString(), " ms/update & ",
                Math.Round(_stopwatchTotal.ElapsedMilliseconds / _cycles, 4).ToString(), " ms/cycle" });
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Read in game settings
            GameSettings.Initialize();

            // Components Activation
            this.Components.Add(_networkManager);
            this.Components.Add(_screenManager);
            this.Components.Add(_inputManager);
            this.Components.Add(_fileManager);
            this.Components.Add(_tilesetManager);
            this.Components.Add(_mapManager);
            this.Components.Add(_currentUserManager);
            this.Components.Add(_textureManager);

            // NOTE: First initialize all components before adding screens!
            base.Initialize();

            // Game Timeming
            _stopwatchTotal = Stopwatch.StartNew();
            _stopwatchUpdate = new Stopwatch();
            _stopwatchDraw = new Stopwatch();

#if DEBUG
                GameScreen currentScreen = new InitializationScreen();
                currentScreen.Next = new LoginScreen();
#else
                // Create the CurrentScreen and the NextScreen
                GameScreen currentScreen = new SplashScreen("Content\\Graphics\\Splashes\\", new String[] { "xna_logo", "splash_magic" });
                GameScreen nextScreen = new InitializationScreen();
                currentScreen.Next = nextScreen;
                nextScreen.Next = new LoginScreen();

                ContentDatabase.LoadAll();
                ContentDatabase.Populate();
            

#endif

            // Add the CurrentScreen to the manager
            this.ScreenManager.AddScreen(currentScreen);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = this.Content.Load<SpriteFont>(@"Common\defaultFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _stopwatchTotal.Start();
            _stopwatchUpdate.Start();

            // FRAMERATE AREA
            // This area is reserved to count the current frame rate.
            // Each second the framerate is updated (any more is not needed)

            // Add to ElapsedTime
            _elapsedTime += gameTime.ElapsedGameTime;
            // If More then one second passed
            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                // Set the FrameRate
                _frameRate = _frameCount;
                // Reset the FrameCount
                _frameCount = 0;
                // Remove this second
                _elapsedTime -= TimeSpan.FromSeconds(1);
            }

            // Update components
            base.Update(gameTime);

            // Stop Update counter
            _stopwatchUpdate.Stop();
            _stopwatchTotal.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool BeginDraw()
        {
            _stopwatchTotal.Start();
            _stopwatchDraw.Start();

            return base.BeginDraw();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear the graphics Device
            GraphicsDevice.Clear(Color.Black);
            
            // Update frame renewal
            base.Draw(gameTime);

            // FRAMERATE AREA
            // This area is reserved to count the current frame rate.
            // Each second the framerate is updated (more is not needed)
            _frameCount++;

            _spriteBatch.Begin();
            if (_networkManager.RoundTrip <= 0)
            {
                _spriteBatch.DrawString(_font, String.Format("Framerate: {0} f/s", _frameRate), Vector2.One * 11, Color.Black);
                _spriteBatch.DrawString(_font, String.Format("Framerate: {0} f/s", _frameRate), Vector2.One * 10, Color.White);
            } else {

                _spriteBatch.DrawString(_font, String.Format("Framerate: {0} f/s  Roundtrip: {1} ms", _frameRate, Math.Ceiling(_networkManager.RoundTrip * 1000)), Vector2.One * 11, Color.Black);
                _spriteBatch.DrawString(_font, String.Format("Framerate: {0} f/s  Roundtrip: {1} ms", _frameRate, Math.Ceiling(_networkManager.RoundTrip * 1000)), Vector2.One * 10, Color.White);
            }

            if (_networkManager.IsAuthenticated)
            {
                _spriteBatch.DrawString(_font, String.Format("Authenticated: {0}", _networkManager.Username), Vector2.One * 11 + Vector2.UnitY * 10, Color.Black);
                _spriteBatch.DrawString(_font, String.Format("Authenticated: {0}", _networkManager.Username), Vector2.One * 10 + Vector2.UnitY * 10, Color.White);
            }
            else
            {
                _spriteBatch.DrawString(_font, String.Format("Not authenticated"), Vector2.One * 11 + Vector2.UnitY * 10, Color.Black);
                _spriteBatch.DrawString(_font, String.Format("Not authenticated"), Vector2.One * 10 + Vector2.UnitY * 10, Color.White);
            }
            _spriteBatch.End();

            // SCREENSHOT AREA
            //  
            if (InputManager.Keyboard.IsKeyReleased(Keys.F12))
                Screenshot();
                //DataManager.QueueChange(new Data.Update.ApplyableAction(() => Screenshot()));
        }

        /// <summary>
        /// Takes a screenshot of the backbuffer
        /// </summary>
        /// <param name="device">Device with backbuffer</param>
        internal void Screenshot()
        {
            #if !NOMULTITHREAD
                if (_screenShotTask != null && (_screenShotTask.Status.HasFlag(TaskStatus.Running) || _screenShotTask.Status.HasFlag(TaskStatus.WaitingToRun)))
                    return;
            #endif

            Logger.Debug("Starting to capture screenshot.");

            // Get device backbuffer
            Byte[] screenData = new Byte[this.GraphicsDevice.PresentationParameters.BackBufferWidth * this.GraphicsDevice.PresentationParameters.BackBufferHeight * 4];
            this.GraphicsDevice.GetBackBufferData<Byte>(screenData);

            // Copy device data
            Int32 width = this.GraphicsDevice.PresentationParameters.BackBufferWidth;
            Int32 height = this.GraphicsDevice.PresentationParameters.BackBufferHeight;
            SurfaceFormat sf = this.GraphicsDevice.PresentationParameters.BackBufferFormat;
            

            #if !NOMULTITHREAD
                _screenShotTask = Task.Factory.StartNew(() => { 
            #endif
                    ERAUtils.BmpWriter bmpWriter = new ERAUtils.BmpWriter(width, height);

                    // Set texture data
                    using (Texture2D t2d = new Texture2D(this.GraphicsDevice, width, height, false, sf))
                    {
                        t2d.SetData<Byte>(screenData);

                        // Find filename
                        Int32 i = 0;
                        String date = DateTime.Now.ToShortDateString();
                        String name = "Screenshot_" + date + ".png";
                        IStorageDevice psd = FileManager.GetStorageDevice(FileLocationContainer.Player);
                        while (psd.IsReady == false)
                        {
                            System.Threading.Thread.Sleep(10);
                            System.Threading.Thread.MemoryBarrier();
                        }

                        while(psd.FileExists(".", name))
                        {
                            name = "Screenshot_" + date + (++i).ToString() + ".png";

                            while (psd.IsReady == false)
                            {
                                System.Threading.Thread.Sleep(10);
                                System.Threading.Thread.MemoryBarrier();
                            }
                        }

                        // Save Screenshot
                        FileManager.GetStorageDevice(FileLocationContainer.Player).Save(".", name, (stream) =>
                        {
                            bmpWriter.TextureToBmp(t2d, stream);
                        });

                        // Log it
                        Logger.Info(String.Format("Saved screenshot (f:{0}) to [::PUBLIC/USER::]", name));
                    }

            #if !NOMULTITHREAD
                });
            #endif

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void EndDraw()
        {
            _stopwatchDraw.Stop();

            //ThreadedEndDraw();     
       
            /*if (_endDrawThread != null)
            {
                while (_endDrawThread.Status != TaskStatus.RanToCompletion)
                    b.SpinOnce();
                
            }*/

            //_endDrawThread = Task.Factory.StartNew(() => { base.EndDraw(); });

            ThreadedEndDraw();

            // APPLY UPDATES
            DataManager.IntegrateChanges();

            // Add to cycles
            _cycles++;

            #if DEBUG
                if (_stopwatchTotal.Elapsed.TotalMinutes > __MAXRUNTIME )
                    this.Exit();
            #endif
            
            // Draw loop finished
            _stopwatchTotal.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ThreadedEndDraw()
        {
            _stopwatchDraw.Start();

            base.EndDraw();
          
            _stopwatchDraw.Stop();
        }
    }
}