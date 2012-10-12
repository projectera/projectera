using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Display;
using ProjectERA.Services.Data;
using ProjectERA.Graphics;
using Microsoft.Xna.Framework;
using ProjectERA.Graphics.Sprite;
using Microsoft.Xna.Framework.Content;
using ERAUtils.Enum;
using Microsoft.Xna.Framework.Input;
using ERAUtils.Logger;
using Lidgren.Network;
using ERAUtils;
using ProjectERA.Data.Enum;
using ProjectERA.Services;

namespace ProjectERA.Screen
{
    internal class PlayingScreen : GameScreen
    {
        private DefaultRendertarget _motionBlurRenderTarget;
        private ToneRenderTarget _toneRenderTarget;
        private DefaultRendertarget _bloomRenderTarget;
        private DefaultRendertarget _hudRenderTarget;

        private HeadsUpDisplay _hud;

#if DEBUG
        private DefaultRendertarget _debugRenderTarget;
        private DefaultRendertarget _editorRenderTarget;
#endif

        /// <summary>
        /// 
        /// </summary>
        public PlayingScreen()
        {
            // Set the transition time
            this.TransitionOnTime = TimeSpan.FromSeconds(1);
            this.TransitionOffTime = TimeSpan.FromSeconds(1);

            // Set the type
            this.IsPopup = false;
        }

        /// <summary>
        /// Initializes playin screen
        /// </summary>
        internal override void Initialize()
        {
            if (DataManager.LocalCamera == null)
            {
                DataManager.LocalCamera = new Camera3D();
                DataManager.LocalCamera.Initialize();
            }

            if (DataManager.LocalSprites == null)
                DataManager.LocalSprites = new List<Graphics.Sprite.Interactable>();

            this.Camera = DataManager.LocalCamera;

            tic = new Services.Input.TextInputComponent(Game, 255);
            tic.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// Loads all managed content
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(ContentManager contentManager)
        {
            // Safe contentmanager
            base.LoadContent(contentManager);

            // Get the data
            using (MapManager mapManager = (MapManager)this.Game.Services.GetService(typeof(MapManager)))
            {
                TileMap tileMap;
                MapData mapData;
                mapManager.FillMapObjects(ProjectERA.Services.Network.Protocols.Map.Id, out mapData, out tileMap);

                if (DataManager.LocalMapData == null || DataManager.LocalTileMap  == null ||
                    mapData.MapId.Equals(DataManager.LocalMapData.MapId) == false)
                {
                    // Save and initialize
                    DataManager.LocalMapData = mapData;
                    DataManager.LocalTileMap = tileMap;
                    DataManager.LocalCamera.Initialize();
                    DataManager.LocalTileMap.Initialize();

                    // Create a Cam
                    DataManager.LocalTileMap.Camera = DataManager.LocalCamera;
                    DataManager.LocalCamera.ViewableArea = new Rectangle(0, 0, DataManager.LocalTileMap.Width, DataManager.LocalTileMap.Height);

                    // Load the graphics
                    DataManager.LocalTileMap.LoadContent(DataManager.LocalMapData);
                    DataManager.LocalTileMap.CreateTileMap(DataManager.LocalMapData);

                    // Logging
                    Logger.Info(String.Format("Updated DataManager with mapData and tilemap (map: {0})", MapData.GetFileName(DataManager.LocalMapData.MapId, DataManager.LocalMapData.InnerMapId)));
                }

                // Set interactables
                DataManager.LocalMapData.Interactables = Services.Network.Protocols.Map.Interactables;
            }
            
            // Motion blur filter
            _motionBlurRenderTarget = GameSettings.MotionBlurEnabled ?
                new MotionBlurRenderTarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight) :
                new DefaultRendertarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight);
            _motionBlurRenderTarget.Initialize();
            _motionBlurRenderTarget.LoadContent(this.ContentManager);

            // Things to blur
            _motionBlurRenderTarget.AddComponent(DataManager.LocalTileMap);
            _motionBlurRenderTarget.AddComponent(DataManager.LocalCamera);

            // Enable map to receive interactables
            ProjectERA.Services.Network.Protocols.Map.OnInteractableJoined += new EventHandler(Map_OnInteractableJoined);
            ProjectERA.Services.Network.Protocols.Map.OnInteractableLeft += new EventHandler(Map_OnInteractableLeft);

            Data.Interactable spriteSource;
            if (!DataManager.LocalMapData.Interactables.TryGetValue(DataManager.LocalAvatar.Id, out spriteSource))
            {
                DataManager.LocalMapData.Interactables.Add(DataManager.LocalAvatar.Id, DataManager.LocalAvatar);
            }

            // Interactables (non-static map objects)
            lock(DataManager.LocalMapData.Interactables)
            {
                foreach (Data.Interactable interactable in DataManager.LocalMapData.Interactables.Values)
                {
                    Logger.Info("Interactable sprite will be created [" + interactable.Id.ToString() + "] while loadContent");

                    // Create sprite
                    Interactable interactableSprite = new Interactable(this.Game, ref _screenCamera, interactable);
                    interactableSprite.Initialize();
                    interactableSprite.LoadContent(DataManager.LocalTileMap.ContentManager);
                    
                    // Add sprite
                    DataManager.LocalSprites.Add(interactableSprite);
                    _motionBlurRenderTarget.AddComponent(interactableSprite);

                    if (interactable.Id.Equals(DataManager.LocalAvatar.Id))
                    {
                        DataManager.LocalSprite = interactableSprite;
                        DataManager.LocalCamera.Focus = DataManager.LocalSprite;
                    }
                }
            }

            // Intialize tone
            _toneRenderTarget = new ToneRenderTarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight);
            _toneRenderTarget.Initialize();
            _toneRenderTarget.LoadContent(this.ContentManager);
            _toneRenderTarget.Tone = new Vector4(25f / 255, 15f / 255, -15f / 255, 0);
            // The tone should draw the blurred render target
            _toneRenderTarget.AddRendertarget(_motionBlurRenderTarget);

            // Initialize bloom
            _bloomRenderTarget = GameSettings.BloomEnabled ?
                new BloomRendertarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight) :
                new DefaultRendertarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight);
            _bloomRenderTarget.Initialize();
            _bloomRenderTarget.LoadContent(this.ContentManager);
            // The bloom should draw the toned blurred render target
            _bloomRenderTarget.AddRendertarget(_toneRenderTarget);

            // The screen should draw the bloomed toned blurred render target
            this.ScreenRendertarget.AddRendertarget(_bloomRenderTarget);

            // Initialize hud and stuff
            _hudRenderTarget = new DefaultRendertarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight);
            _hudRenderTarget.Initialize();

            // Add screens to the hud rt
            _hud = new HeadsUpDisplay(this.Game, this.Camera, DataManager.LocalAvatar);
            _hud.Initialize();

            _hudRenderTarget.AddComponent(_hud);
            _hudRenderTarget.LoadContent(this.ContentManager);

            this.ScreenRendertarget.AddRendertarget(_hudRenderTarget);

#if DEBUG
            _editorRenderTarget = new DefaultRendertarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight);
            _editorRenderTarget.Initialize();
            _editorRenderTarget.IsVisible = false;
            _editorRenderTarget.IsEnabled = false;

            EditorOverlay editor = new EditorOverlay(this.Game, this.Camera, DataManager.LocalMapData, DataManager.LocalTileMap);
            editor.Initialize();
            _editorRenderTarget.AddComponent(editor);
            

            _debugRenderTarget = new DefaultRendertarget(this.Game, this.Camera, 200, 340);
            _debugRenderTarget.Initialize();
            _debugRenderTarget.IsVisible = false;
            _debugRenderTarget.IsEnabled = false;

            // Create an overlay
            DebugOverlay overlay = new DebugOverlay(this.Game, this.Camera, DataManager.LocalMapData);
            overlay.Initialize();

            // Add after blur to draw
            _debugRenderTarget.AddComponent(overlay);

            // Add to rendertarget
            _debugRenderTarget.LoadContent(contentManager);
            _editorRenderTarget.LoadContent(contentManager);
            this.ScreenRendertarget.AddRendertarget(_editorRenderTarget);
            this.ScreenRendertarget.AddRendertarget(_debugRenderTarget);

#endif

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_OnInteractableJoined(Object sender, EventArgs e)
        {
            Logger.Info("Interactable sprite will be created [" + ((Data.Interactable)sender).Id.ToString() + "] event");

            Interactable sprite = new Interactable(this.Game, ref _screenCamera, (Data.Interactable)sender);
            sprite.Initialize();
            sprite.LoadContent(DataManager.LocalTileMap.ContentManager);

            // Add sprite
            DataManager.LocalSprites.Add(sprite);
            _motionBlurRenderTarget.AddComponent(sprite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_OnInteractableLeft(Object sender, EventArgs e)
        {

            Logger.Info("Interactable sprite will be removed [" + ((Data.Interactable)sender).Id.ToString() + "]");

            Interactable sprite = DataManager.LocalSprites.Find(a => a.SourceId.Equals(((Data.Interactable)sender).Id));
            DataManager.LocalSprites.Remove(sprite);
            _motionBlurRenderTarget.RemoveComponent(sprite);
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        internal override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Update ScreenManager
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        ProjectERA.Services.Input.TextInputComponent tic;

        /// <summary>
        /// Updates user input
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void HandleInput()
        {
            // CHAT TODO HACK TEMP SECTION CHAT
            if (tic.Enabled)
            {
                tic.HandleInput();

                if (InputManager.Keyboard.IsKeyReleased(Keys.Enter))
                {
                    tic.Enabled = false;
                    String text = tic.Text;
                    tic.Text = String.Empty;
                    tic.Enabled = true;

                    if (String.IsNullOrWhiteSpace(text) == false)
                        ProjectERA.Services.Network.Protocols.Interactable.Message(text);

                    Logger.Debug(String.Format("Send message {0} at {1}", text, DateTime.Now.ToShortTimeString()));
                }
            }

            // HUD UPDATE SECTION
            _hud.HandleInput();

            // CAMERA UPDATE SECTION
            // Updates the camera movement by adding a fourth tile to the current position focus.
            if (_hud.MenuActive == false && (
                    !DataManager.LocalAvatar.StateFlags.HasFlag(InteractableStateFlags.Moving) ||
                    (Math.Abs(DataManager.LocalSprite.Position.X - DataManager.LocalAvatar.MapX) < .5 &&
                     Math.Abs(DataManager.LocalSprite.Position.Y - (DataManager.LocalAvatar.MapY + .5f)) < .5))
                )
            {
                Boolean isMoving = false;
                Int32 xDestination = DataManager.LocalAvatar.MapX;
                Int32 yDestination = DataManager.LocalAvatar.MapY;
                Byte dDestination = DataManager.LocalAvatar.Appearance.MapDir;

                Boolean left = InputManager.Keyboard.IsKeyDown(Keys.Left);
                Boolean right = InputManager.Keyboard.IsKeyDown(Keys.Right);

                if (InputManager.Keyboard.IsKeyDown(Keys.Up))
                {
                    if (right)
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, DataManager.LocalMapData, Direction.NorthEast);
                        isMoving = direction != Direction.None;

                        if (direction == Direction.NorthEast || direction == Direction.East)
                            xDestination++;
                        if (direction == Direction.NorthEast || direction == Direction.North)
                            yDestination--;
                    }
                    else if (left)
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, DataManager.LocalMapData, Direction.NorthWest);
                        isMoving = direction != Direction.None;
                            
                        if (direction == Direction.NorthWest || direction == Direction.West)
                            xDestination--;
                        if (direction == Direction.NorthWest || direction == Direction.North)
                            yDestination--;
                        
                    }
                    else
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMoveUp(DataManager.LocalAvatar, DataManager.LocalMapData);
                        isMoving = direction != Direction.None;
                        if (direction == Direction.North)
                            yDestination--;
                    }

                    dDestination = (Byte)Direction.North;
                }
                else if (InputManager.Keyboard.IsKeyDown(Keys.Down))
                {
                   if (right)
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, DataManager.LocalMapData, Direction.SouthEast);
                        isMoving = direction != Direction.None;

                        if (direction == Direction.SouthEast || direction == Direction.East)
                            xDestination++;
                        if (direction == Direction.SouthEast || direction == Direction.South)
                            yDestination++;
                    }
                    else if (left)
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, DataManager.LocalMapData, Direction.SouthWest);
                        isMoving = direction != Direction.None;
                            
                        if (direction == Direction.SouthWest || direction == Direction.West)
                            xDestination--;
                        if (direction == Direction.SouthWest || direction == Direction.South)
                            yDestination++;
                        
                    }
                    else
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMoveDown(DataManager.LocalAvatar, DataManager.LocalMapData);
                        isMoving = direction != Direction.None;
                        if (direction == Direction.South)
                            yDestination++;
                    }

                    dDestination = (Byte)Direction.South;
                }
                else
                {
                    if (left)
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMoveLeft(DataManager.LocalAvatar, DataManager.LocalMapData);
                        isMoving = direction != Direction.None;
                        if (direction == Direction.West)
                            xDestination--;

                        dDestination = (Byte)Direction.West;
                    }
                    else if (right)
                    {
                        Direction direction = ProjectERA.Logic.GameAvatar.TryMoveRight(DataManager.LocalAvatar, DataManager.LocalMapData);
                        isMoving = direction != Direction.None;
                        if (direction == Direction.East)
                            xDestination++;

                        dDestination = (Byte)Direction.East;
                    }
                }

                if (isMoving)
                {
                    DataManager.LocalAvatar.AddChange(() => { DataManager.LocalAvatar.StateFlags |= InteractableStateFlags.Moving; });
                    ProjectERA.Services.Network.Protocols.Player.RequestMovement(xDestination, yDestination, dDestination);
                }
            }

            // Zoom In
            if (InputManager.Keyboard.IsKeyReleased(Keys.OemPlus))
            {
                this.Camera.ZoomTo = this.Camera.ZoomTo * 2;

                if (!InputManager.Keyboard.IsKeyDown(Keys.LeftShift))
                    this.Camera.JumpToZoom();
            }

            // Zoom out
            if (InputManager.Keyboard.IsKeyReleased(Keys.OemMinus))
            {
                this.Camera.ZoomTo = this.Camera.ZoomTo / 2;

                if (!InputManager.Keyboard.IsKeyDown(Keys.LeftShift))
                    this.Camera.JumpToZoom();
            }

#if DEBUG
            // Shake
            if (InputManager.Keyboard.IsKeyTriggerd(Keys.Space))
            {
                this.Camera.ShakeAmount *= 1.5f;
            }

            _debugRenderTarget.IsVisible = InputManager.Keyboard.IsKeyDown(Keys.F11);
            _debugRenderTarget.IsEnabled = _debugRenderTarget.IsVisible;

            if (InputManager.Keyboard.IsKeyTriggerd(Keys.F10))
            {
                _editorRenderTarget.IsVisible = !_editorRenderTarget.IsVisible;
                _editorRenderTarget.IsEnabled = _editorRenderTarget.IsVisible;
            }
#endif
        }

        /// <summary>
        /// Draws a frame
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            this.ScreenRendertarget.RunExclusiveCycle(gameTime);

            // Update drawing with fading
            if (this.IsTransitioning)
                // Draw the black fading graphic
                this.ScreenManager.FadeBackBufferToBlack((Byte)(255 - TransitionAlpha));

            base.Draw(gameTime);
        }
    }
}
