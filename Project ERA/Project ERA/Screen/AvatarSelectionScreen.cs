using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Display;
using ProjectERA.Data;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectERA.Services.Network.Protocols;
using ERAUtils;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Graphics;

namespace ProjectERA.Screen
{
    internal class AvatarSelectionScreen : GameScreen
    {
        private Texture2D _backgroundTexture;
        private VariableLoadingPopup _popupScreen;
        private AvatarInformationWidget[] _informationWindows;

        private Task<Data.Player> _playerTask;
        private Task<Data.Interactable>[] _avatarTask;
        private Task _completionTask;

        /// <summary>
        /// Constructor
        /// </summary>
        internal AvatarSelectionScreen()
        {
            // Set the transition time
            this.TransitionOnTime = TimeSpan.FromSeconds(1);
            this.TransitionOffTime = TimeSpan.FromSeconds(1);

            // Set the type
            this.IsPopup = false;

            // Create popup
            _popupScreen = new VariableLoadingPopup();

        }

        /// <summary>
        /// Initialization
        /// </summary>
        internal override void Initialize()
        {
            // After initialize, input and network manager are set
            base.Initialize();
        }

        /// <summary>
        /// Load content
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            // Save contentManager
            base.LoadContent(contentManager);

            _backgroundTexture = this.ScreenManager.TextureManager.LoadStaticTexture(@"Graphics\Backgrounds\scene_background", this.ContentManager);
            FetchPlayer();

        }

        /// <summary>
        /// 
        /// </summary>
        internal override void UnloadContent()
        {
            base.UnloadContent();

            this.ScreenRendertarget.UnloadContent();

            this.ScreenManager.TextureManager.ReleaseStaticTexture(@"Graphics\Backgrounds\scene_background");

            this.ScreenManager.TextureManager.UnloadStaticTexture(@"Graphics\Interface\window_character_info");
            this.ScreenManager.TextureManager.UnloadStaticTexture(@"Graphics\Interface\window_character_overlay");
            this.ScreenManager.TextureManager.UnloadStaticTexture(@"Graphics\Interface\window_character_overlay_active");
        }

        /// <summary>
        /// Runs after screen is added to the screenmanager
        /// </summary>
        internal override void PostProcessing()
        {
            // Add screen (the popup)
            this.ScreenManager.AddScreen(_popupScreen);

        }
       
        /// <summary>
        /// Start Fetching Player
        /// </summary>
        private void FetchPlayer()
        {
            _avatarTask = null; done = false;
            
            Protocol protocol;
            this.NetworkManager.TryGetProtocol((Byte)Protocols.ClientProtocols.Player, out protocol);

            _playerTask = ((Services.Network.Protocols.Player)protocol).Get((player) =>
                { 
                    _popupScreen.FinishProgress(); 
                    FetchAvatar(_playerTask.Result); 
                }
             );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        private void FetchAvatar(Data.Player player)
        {
            if (player != null)
            {
                Int32 count = _playerTask.Result.Avatars.Count;
                _avatarTask = new Task<Data.Interactable>[count];
                if (_informationWindows == null)
                {
                    _informationWindows = new AvatarInformationWidget[count];
                    Vector2 spacing = Vector2.UnitX * 60;
                    Vector2 size = Vector2.UnitX * 200 + Vector2.UnitY * 140;
                    Vector2 center = new Vector2(1280 / 2, 720 / 2);

                    Protocol protocol;
                    this.NetworkManager.TryGetProtocol((Byte)Protocols.ClientProtocols.Interactable, out protocol);


                    for (Int32 j = 0; j < count; j++)
                    {
                        _informationWindows[j] = new AvatarInformationWidget(this.Game, protocol as ProjectERA.Services.Network.Protocols.Interactable, _playerTask.Result.Avatars[j].Id, center);
                       
                        if (j > 0)
                        {
                            for (Int32 k = j - 1; k >= 0; k--)
                                _informationWindows[k]._center -= (size.X * Vector2.UnitX + spacing) / 2;
                            _informationWindows[j]._center = _informationWindows[j - 1]._center + (spacing + size.X * Vector2.UnitX);
                        }
                    }

                    for (Int32 j = 0; j < count; j++)
                    {
                        _informationWindows[j].Initialize();
                        _informationWindows[j].LoadContent(this.ContentManager);

                        Int32 safej = j;
                        this.ScreenRendertarget.AddChange(() => this.ScreenRendertarget.AddComponent(_informationWindows[safej]));

                    }
                }
                else
                {
                    for (Int32 i = 0; i < _informationWindows.Length; i++)
                    {
                        Protocol protocol;
                        this.NetworkManager.TryGetProtocol((Byte)Protocols.ClientProtocols.Interactable, out protocol);

                        if (_informationWindows[i].Result == null || _informationWindows[i].Result.Id == MongoObjectId.Empty)
                            _informationWindows[i].Retry(protocol as ProjectERA.Services.Network.Protocols.Interactable, _playerTask.Result.Avatars[i].Id);
                    }
                }

                _avatarTask = _informationWindows.Select(a => a.Source).ToArray();

                _completionTask = Task.Factory.StartNew(() =>
                {
                    Task.WaitAll(_avatarTask);

                    if (_avatarTask.Length > cursor && _informationWindows[cursor].Result != null)
                    {
                        selectedId = _informationWindows[cursor].Result.Id;
                        foreach (var window in _informationWindows)
                        {
                            window.IsSelected = (window.Result.Id == selectedId);
                        }
                    }

                    System.Threading.Thread.MemoryBarrier();

                    done = true;
                });
            }
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        internal override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            this.ScreenRendertarget.Update(gameTime);

            // Update this
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Input Frame Renewal
        /// </summary>
        internal override void HandleInput()
        {
            if (done || (_playerTask.IsCompleted && _playerTask.Result == null))
            {
                if (this.InputManager.Keyboard.IsKeyReleased(Keys.Q))
                {
                    _popupScreen = new VariableLoadingPopup();
                    this.ScreenManager.AddScreen(_popupScreen);
                    FetchPlayer();
                }
            } 

            if (done && _avatarTask.Length > 0)
            {
                // Select previous
                if (this.InputManager.Keyboard.IsKeyTriggerd(Keys.Up) || this.InputManager.Keyboard.IsKeyTriggerd(Keys.Left))
                {

                    if (--cursor < 0)
                        cursor = _avatarTask.Length - 1;

                    if (_informationWindows[cursor].Result != null)
                    {
                        selectedId = _informationWindows[cursor].Result.Id;
                        foreach (var window in _informationWindows)
                        {
                            window.IsSelected = (window.Result.Id == selectedId);
                        }
                    }
                }

                // Select next
                if (this.InputManager.Keyboard.IsKeyTriggerd(Keys.Down) || this.InputManager.Keyboard.IsKeyTriggerd(Keys.Right))
                {
                    cursor = (cursor + 1) % _avatarTask.Length;

                    if (_informationWindows[cursor].Result != null)
                    {
                        selectedId = _informationWindows[cursor].Result.Id;
                        foreach (var window in _informationWindows)
                        {
                            window.IsSelected = (window.Result.Id == selectedId);
                        }
                    }
                }

                // Select or pick using mouse
                if (this.InputManager.Mouse.IsButtonReleased(Services.Input.MouseButton.Left))
                {
                    foreach (var window in _informationWindows)
                    {
                        Boolean pointing = this.InputManager.Mouse.IsOverObj(window.Surface);
                        if (!window.IsSelected && pointing)
                        {
                            for (Int32 i = 0; i < _informationWindows.Length; i++)
                            {
                                if (window == _informationWindows[i])
                                {
                                    window.IsSelected = true;
                                    if (window.Result != null)
                                    {
                                        selectedId = window.Result.Id;
                                        cursor = i;
                                    }
                                }
                                else
                                {
                                    _informationWindows[i].IsSelected = false;
                                }
                            }
                        }
                        else if (pointing)
                        {
                            this.NetworkManager.SetPlayerId(_playerTask.Result.UserId.Id);
                            ProjectERA.Services.Network.Protocols.Player.RequestPickAvatar(selectedId, (a) =>
                                {
                                    _hasCalledExited = true;
                                    this.Next = new PrePlayingScreen();
                                    this.Next.Next = new PlayingScreen();
                                    this.ExitScreen();
                                });
                        }
                    }
                }

                /*if (this.InputManager.Mouse.IsMoved)
                {
                    foreach (var window in _informationWindows)
                    {
                        if (!window.IsSelected && this.InputManager.Mouse.IsOverObj(window.Surface))
                        {
                            for (Int32 i = 0; i < _informationWindows.Length; i++)
                            {
                                if (window == _informationWindows[i])
                                {
                                    window.IsSelected = true;
                                    if (window.Result != null)
                                    {
                                        selectedId = window.Result.Id;
                                    }
                                }
                                else
                                {
                                    _informationWindows[i].IsSelected = false;
                                }   
                            }
                        }
                    }
                }*/
            }

            if (!this.IsExiting && this.InputManager.Keyboard.IsKeyReleased(Keys.Back))
            {
                _hasCalledExited = true;
                this.Next = new LoginScreen();
                this.ExitScreen();
                return;
            }

            if (!this.IsExiting && this.InputManager.Keyboard.IsKeyReleased(Keys.Escape))
            {
                this.ExitScreen();
                return;
            }

            if (!this.IsExiting && done && this.InputManager.Keyboard.IsKeyReleased(Keys.Enter))
            {
                this.NetworkManager.SetPlayerId(_playerTask.Result.UserId.Id);

                ProjectERA.Services.Network.Protocols.Player.RequestPickAvatar(selectedId, (a) =>
                    {
                        _hasCalledExited = true;
                        this.Next = new PrePlayingScreen();
                        this.Next.Next = new PlayingScreen();
                        this.ExitScreen();
                    });
                               
                return;
            }
        }

        Int32 cursor = 0;
        MongoObjectId selectedId = MongoObjectId.Empty;
        Boolean done;

        /// <summary>
        /// Draw Frame
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Spritebatch for background
            this.ScreenManager.SpriteBatch.Begin();
            this.ScreenManager.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            this.ScreenManager.SpriteBatch.End();

            // Render ScreenRendertarget
            this.ScreenRendertarget.Render(gameTime);

#if DEBUG
            // DEBUG DATA
            this.ScreenManager.SpriteBatch.Begin();

            // When playertask is created
            if (_playerTask != null)
                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Player Task Status: " + _playerTask.Status, new Vector2(20, 40), Color.White);

            // When it is completed
            if (_playerTask != null && _playerTask.IsCompleted)
            {
                if (_playerTask.Result != null)
                    this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Data Id: " + _playerTask.Result.UserId, new Vector2(20, 50), Color.White);

                this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Press Q to fetch again", new Vector2(20, 60), Color.White);

                // When avatarTask is created
                if (_avatarTask != null)
                {
                    this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Avatar Task Status: " + _avatarTask.Where((a) => a == null || !a.IsCompleted).Count() + " Running.", new Vector2(20, 70), Color.White);

                    Int32 counter = 0;
                    foreach (Task<Data.Interactable> finished in _avatarTask.Where((a) => a != null && a.IsCompleted))
                    {
                        if (finished.Result != null)
                            // When it is complteded
                            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Data Id: " + finished.Result.Id, new Vector2(20, 80 + counter * 10), Color.White);
                        counter++;
                    }

                    if (done)
                        this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Picking: " + selectedId, new Vector2(20, 90 + counter * 10), Color.White);
                    
                }
            }

            this.ScreenManager.SpriteBatch.End();
#endif



             // Draw the black fading graphic
            if (this.IsTransitioning)
                this.ScreenManager.FadeBackBufferToBlack((Byte)(255 - TransitionAlpha));
        }

        /// <summary>
        /// 
        /// </summary>
        private class AvatarInformationWidget : Widget
        {
            private VariableLoadingPopup _loadingPopup;
            public Vector2 _center;
            private Vector2 _topLeftCorner;
            
            private Texture2D _windowTexture;
            private Texture2D _windowOverlayTexture;
            private Texture2D _windowOverlayActiveTexture;
            private Texture2D _textSeperationLineTexture;
            private SpriteBatch _spriteBatch;
            private Graphics.Sprite.Interactable _interactable;

            private ScreenState _screenState;
            private TimeSpan _transitionTime = TimeSpan.FromSeconds(1);
            private Single _transitionPosition = 1;   

            private Task<Data.Interactable> _source;
            private Boolean _isSelected;
            private Single _timePassed;
            private Int32 _frame;

            private Data.BattlerClass _battlerClass;

            /// <summary>
            /// 
            /// </summary>
            internal Task<Data.Interactable> Source
            {
                get { return _source; }
                private set { _source = value; }
            }

            /// <summary>
            /// 
            /// </summary>
            internal Data.Interactable Result
            {
                get { return Source.Result; }
            }

            /// <summary>
            /// 
            /// </summary>
            internal Boolean IsSelected
            {
                get { return _isSelected; }
                set { _isSelected = value; }
            }

            /// <summary>
            /// Gets the current position of the screen transition, ranging
            /// from zero (fully active, no transition) to one (transitioned
            /// fully off to nothing).
            /// </summary>
            public Single TransitionPosition
            {
                get { return _transitionPosition; }
                protected set { _transitionPosition = value; }
            }

            /// <summary>
            /// Gets the current alpha of the screen transition, ranging
            /// from 255 (fully active, no transition) to 0 (transitioned
            /// fully off to nothing).
            /// </summary>
            internal Byte TransitionAlpha
            {
                get { return (Byte)(255 - TransitionPosition * 255); }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="game"></param>
            /// <param name="protocol"></param>
            /// <param name="source"></param>
            /// <param name="position"></param>
            internal AvatarInformationWidget(Game game, ProjectERA.Services.Network.Protocols.Interactable protocol, MongoObjectId source, Vector2 position)
                : base(game, null)
            {
                _center = position;
                _loadingPopup = new VariableLoadingPopup(_center, true);
                _loadingPopup.Exiting += new EventHandler(_loadingPopup_Exiting);
                _source = protocol.Get(source, (a) => { _loadingPopup.FinishProgress(); });

                this.ScreenManager.AddScreen(_loadingPopup);

                _screenState = ScreenState.WaitingForTransition;
            }

            internal void Retry(ProjectERA.Services.Network.Protocols.Interactable protocol, MongoObjectId source)
            {
                _loadingPopup = new VariableLoadingPopup(_center);
                _loadingPopup.Exiting += new EventHandler(_loadingPopup_Exiting);
                _source = protocol.Get(source, (a) => { _loadingPopup.FinishProgress(); });

                this.ScreenManager.AddScreen(_loadingPopup);

                _screenState = ScreenState.WaitingForTransition;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void _loadingPopup_Exiting(object sender, EventArgs e)
            {
 	            if (this.Result != null)
                {
                    //Task.Factory.StartNew(() =>
                    //{
                        Camera3D camera = new Camera3D();
                        camera.Initialize();
                        camera.ViewableArea = new Rectangle(0, 0, _windowTexture.Width, _windowTexture.Height);
                        camera.Position = new Vector3(_topLeftCorner, 0);
                        _interactable = new Graphics.Sprite.Interactable(this.Game, ref camera, Result);
                        _interactable.Initialize();
                        _interactable.LoadContent(this.ScreenManager.ContentManager);
                        _interactable.Position = Vector3.Zero;

                        _battlerClass = ProjectERA.Services.Data.ContentDatabase.GetBattlerClass(this.Result.Battler.ClassId);

                        System.Threading.Thread.MemoryBarrier();

                        _screenState = ScreenState.TransitionOn;
                    //});
                    
                 }
            }

            /// <summary>
            /// Initializes
            /// </summary>
            internal override void Initialize()
            {
                this.IsSemiTransparant = false; // we don't use an actual rendertarget
            }


            /// <summary>
            /// Frame draw
            /// </summary>
            /// <param name="gameTime"></param>
            /// <param name="drawTransparent"></param>
            internal override void Draw(GameTime gameTime, Boolean drawTransparent)
            {
                if (_screenState == ScreenState.TransitionOn || _screenState == ScreenState.TransitionOff || _screenState == ScreenState.Active)
                {
                    Color color = new Color(Color.White.ToVector4() * (1 - _transitionPosition));
                    Color colorGreen = new Color(color.ToVector4() * new Color(156, 234, 113).ToVector4());
                    Color colorBlack = new Color(color.ToVector4() * Color.Black.ToVector4());

                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                    _spriteBatch.Draw(_windowTexture, _center, _windowTexture.Bounds, color, 0f, new Vector2(_windowTexture.Width / 2f, _windowTexture.Height / 2f), 1f, SpriteEffects.None, 0);

                    if (this.IsSelected)
                    {
                        _spriteBatch.Draw(_windowOverlayActiveTexture, _topLeftCorner, color);
                    }
                    else
                    {
                        _spriteBatch.Draw(_windowOverlayTexture, _topLeftCorner, color);
                    }

                    if (Result != null)
                    {
                        _spriteBatch.DrawString(this.FontCollector["Names"], Result.Name, _topLeftCorner + Vector2.UnitX * (Single)Math.Ceiling((_windowTexture.Width - this.FontCollector["Names"].MeasureString(Result.Name).X) / 2f + 1) + Vector2.UnitY * 25, colorBlack);
                        _spriteBatch.DrawString(this.FontCollector["Names"], Result.Name, _topLeftCorner + Vector2.UnitX * (Single)Math.Ceiling((_windowTexture.Width - this.FontCollector["Names"].MeasureString(Result.Name).X) / 2f) + Vector2.UnitY * 24, color);
                        _spriteBatch.Draw(_textSeperationLineTexture, _topLeftCorner + (Vector2.UnitX * (Single)Math.Ceiling((_windowTexture.Width - _textSeperationLineTexture.Width) / 2f) + Vector2.UnitY * 53), color);
                        _spriteBatch.DrawString(this.FontCollector["Default"], _battlerClass.Name.ToString(), _topLeftCorner + (Vector2.UnitX * (Single)Math.Ceiling((_windowTexture.Width - _textSeperationLineTexture.Width) / 2f + 1) + Vector2.UnitY * 59), colorBlack);
                        _spriteBatch.DrawString(this.FontCollector["Default"], _battlerClass.Name.ToString(), _topLeftCorner + (Vector2.UnitX * (Single)Math.Ceiling((_windowTexture.Width - _textSeperationLineTexture.Width) / 2f) + Vector2.UnitY * 58), colorGreen);
                        _spriteBatch.DrawString(this.FontCollector["Default"], "lv. 1", _topLeftCorner + (Vector2.UnitX * (Single)Math.Ceiling(_windowTexture.Width - (_windowTexture.Width - _textSeperationLineTexture.Width) / 2f - this.FontCollector["Default"].MeasureString("lv. 1").X + 1) + Vector2.UnitY * 59), colorBlack);
                        _spriteBatch.DrawString(this.FontCollector["Default"], "lv. 1", _topLeftCorner + (Vector2.UnitX * (Single)Math.Ceiling(_windowTexture.Width - (_windowTexture.Width - _textSeperationLineTexture.Width) / 2f - this.FontCollector["Default"].MeasureString("lv. 1").X) + Vector2.UnitY * 58), colorGreen);
                    }
                    else
                    {
                        _spriteBatch.DrawString(this.FontCollector["Names"], "Failed to load", _topLeftCorner + Vector2.UnitX * (Single)Math.Ceiling((_windowTexture.Width - this.FontCollector["Names"].MeasureString("Failed to load").X) / 2f + 1) + Vector2.UnitY * 25, colorBlack);
                        _spriteBatch.DrawString(this.FontCollector["Names"], "Failed to load", _topLeftCorner + Vector2.UnitX * (Single)Math.Ceiling((_windowTexture.Width - this.FontCollector["Names"].MeasureString("Failed to load").X) / 2f) + Vector2.UnitY * 24, color);
                    }

                    if (_interactable != null && _interactable._interactableTexture != null)
                    {
                        _spriteBatch.Draw(_interactable._interactableTexture, 
                            _topLeftCorner + new Vector2((_windowTexture.Width - _interactable._interactableTexture.Width / 4) / 2, _windowTexture.Height - 20 - _interactable._interactableTexture.Height / 4),
                            new Rectangle(this.IsSelected ? _frame * _interactable._interactableTexture.Width / 4 : 0, 0, _interactable._interactableTexture.Width / 4, _interactable._interactableTexture.Height / 4), color);
                    }


                    _spriteBatch.End();
                    /*if (_interactable != null)
                        _interactable.Draw(gameTime, drawTransparent); // TODO Het werkt nog niet... ik mis blijkbaar wat -_-*/
                }
            }

            /// <summary>
            /// Frame update
            /// </summary>
            /// <param name="gameTime"></param>
            internal override void Update(GameTime gameTime)
            {
                if (_interactable != null)
                {
                    _interactable.Camera.Update(gameTime);

                    if (this.IsSelected)
                    {
                        _timePassed += (Single)gameTime.ElapsedGameTime.TotalSeconds;
                        //_interactable.Update(gameTime);
                        if (_timePassed > .2f)
                        {
                            _frame = (_frame + 1) % 4;
                            _timePassed -= .2f;
                        }
                    }
                    else
                    {
                        _timePassed = 0;
                        _frame = 0;
                    }
                }

                // When needed, transit
                if (_screenState == ScreenState.TransitionOn || _screenState == ScreenState.TransitionOff)
                {
                    if (_screenState == ScreenState.TransitionOn)
                    {
                        // If transitioning on
                        if (!UpdateTransition(gameTime, _transitionTime, -1))
                        {
                            // finished!
                            _screenState = ScreenState.Active;
                        }

                    }
                    else
                    {
                        // If transitioning off
                        if (!UpdateTransition(gameTime, _transitionTime, 1))
                        {
                            // Transition finished!
                            _screenState = ScreenState.Hidden;
                        }
                    }
                }
            }

            /// <summary>
            /// Loads alll content
            /// </summary>
            /// <param name="contentManager"></param>
            internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
            {
                _spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
                _windowTexture = this.TextureManager.LoadStaticTexture(@"Graphics\Interface\window_character_info", contentManager);
                _windowOverlayTexture = this.TextureManager.LoadStaticTexture(@"Graphics\Interface\window_character_overlay", contentManager);
                _windowOverlayActiveTexture = this.TextureManager.LoadStaticTexture(@"Graphics\Interface\window_character_overlay_active", contentManager);
                _textSeperationLineTexture = this.TextureManager.LoadStaticTexture(@"Graphics\Interface\text_seperation_line", contentManager);

                _topLeftCorner = Vector2.UnitX * (Single)Math.Ceiling(_center.X - _windowTexture.Width / 2f) + Vector2.UnitY * (Single)Math.Ceiling(_center.Y - _windowTexture.Height / 2f);

                this.Surface = new Rectangle((Int32)_topLeftCorner.X, (Int32)_topLeftCorner.Y, _windowTexture.Width, _windowTexture.Height);
            }

            /// <summary>
            /// Unloads all unmanaged content
            /// </summary>
            internal override void UnloadContent()
            {
                this.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\text_seperation_line");
                this.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\window_character_info");
                this.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\window_character_overlay");
                this.TextureManager.ReleaseStaticTexture(@"Graphics\Interface\window_character_overlay_active");

                if (_interactable != null)
                    _interactable.UnloadContent();

                _spriteBatch.Dispose();
            }

            /// <summary>
            /// Helper for updating the screen transition position.
            /// </summary>
            private bool UpdateTransition(GameTime gameTime, TimeSpan time, Int32 direction)
            {
                // How much should we move by?
                Single transitionDelta;

                // Update delay

                if (time == TimeSpan.Zero)
                    transitionDelta = 1;
                else
                    transitionDelta = (Single)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                               time.TotalMilliseconds);

                // Update the transition position.
                _transitionPosition += transitionDelta * direction;


                // Did we reach the end of the transition?
                if ((_transitionPosition <= 0) || (_transitionPosition >= 1))
                {
                    _transitionPosition = MathHelper.Clamp(_transitionPosition, 0, 1);
                    return false;
                }
                // Otherwise we are still busy transitioning.
                return true;
            }

            internal override void HandleInput()
            {
                throw new NotImplementedException();
            }
        }
    }
}
