using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectERA.Services.Data;
using ProjectERA.Data.Update;
using ProjectERA.Graphics.Sprite;
using System.Threading.Tasks;
using ProjectERA.Data.Enum;

namespace ProjectERA.Screen
{
    internal class DummyScreen : ProjectERA.Services.Display.GameScreen
    {
        private TileMap _tilemap;
        private MapData _mapData;
        //private DefaultRendertarget _mapRT;

        /// <summary>
        /// 
        /// </summary>
        internal DummyScreen()
        {
            // Set the transition time
            this.TransitionOnTime = TimeSpan.FromSeconds(1);
            this.TransitionOffTime = TimeSpan.FromSeconds(1);

            // Set the type
            this.IsPopup = false;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Initialize()
        {
            this.Camera = new ProjectERA.Services.Display.Camera3D();
            this.Camera.Initialize();
 
            base.Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            // Safe contentmanager
            base.LoadContent(contentManager);

            // Get the data
            using (ProjectERA.Services.Data.MapManager mapManager = (ProjectERA.Services.Data.MapManager)this.Game.Services.GetService(typeof(ProjectERA.Services.Data.MapManager)))
            {
                /*Int32 size = 180;

                UInt16[][][] tiledata = new UInt16[size][][];

                for (Int32 i = 0; i < size; i++)
                {
                    tiledata[i] = new UInt16[size][];
                    for (Int32 j = 0; j < size; j++)
                    {
                        tiledata[i][j] = new UInt16[3];
                        tiledata[i][j][0] = 50;
                        tiledata[i][j][1] = 500;
                        tiledata[i][j][2] = 60;
                    }
                }

                ProjectERA.Services.Data.MapData.GenerateFrom(2, 0, tiledata, 1).Save(); */

                mapManager.FillMapObjects(1, out _mapData, out _tilemap);

            }

            // Initialize Tilemap
            _tilemap.Initialize();
            _tilemap.Camera = this.Camera;

            _motionBlurRenderTarget = new MotionBlurRenderTarget(this.Game, this.Camera, this.ScreenManager.ScreenWidth, this.ScreenManager.ScreenHeight);
            _motionBlurRenderTarget.Initialize();
            _motionBlurRenderTarget.LoadContent(this.ContentManager);
            _motionBlurRenderTarget.Disable();

            // Add as component;
            _motionBlurRenderTarget.AddComponent(_tilemap);
            _motionBlurRenderTarget.AddComponent(this.Camera);

            //_mapRT.AddComponent(_tilemap);

            // Create a Cam
            this.Camera.Position = new Vector3(20, 20, 0);
            this.Camera.ViewableArea = new Microsoft.Xna.Framework.Rectangle(0, 0, _tilemap.Width, _tilemap.Height);
           
            // Load the graphics
            _tilemap.LoadContent(_mapData);
            _tilemap.CreateTileMap(_mapData);

            // Create player [temp]
            if (DataManager.LocalPlayer == null)
            {
                DataManager.LocalPlayer = new Data.Player();
                DataManager.LocalAvatar = new Data.Interactable();
                DataManager.LocalAvatar.AddComponent(new Data.InteractableAppearance());
                DataManager.LocalAvatar.Appearance.MapX = 20;
                DataManager.LocalAvatar.Appearance.MapY = 30;
                DataManager.LocalAvatar.Appearance.MapDir = (Byte)Data.Enum.Direction.East;
                DataManager.LocalAvatar.StateFlags |= InteractableStateFlags.Moving;
                DataManager.LocalAvatar.AddComponent(new Data.InteractableMovement());
                DataManager.LocalAvatar.Movement.MoveSpeed = 3;
                DataManager.LocalAvatar.Movement.MoveFrequency = 3;
                DataManager.LocalAvatar.StateFlags |= InteractableStateFlags.Visible;
            }

            _avatars = new List<Graphics.Sprite.Interactable>();
            _avatars.Add(new Interactable(this.Game, ref _screenCamera, DataManager.LocalAvatar));

            this.Camera.Focus = _avatars[0];
            this.Camera.JumpToFocus();

            DataManager.LocalSprite = _avatars[0];

            for (Int32 i = 0; i < _avatars.Count; i++)
            {
                _avatars[i].Initialize();
                _avatars[i].LoadContent(_tilemap.ContentManager);

                _motionBlurRenderTarget.AddComponent(_avatars[i]);
            }

            _interactables = new List<Graphics.Sprite.Interactable>();

            // Create static monster
            Data.Interactable interactableSource = new Data.Interactable();
            interactableSource.Id = new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            interactableSource.Name = "Sahagin 1";
            interactableSource.StateFlags |= InteractableStateFlags.Visible | InteractableStateFlags.Moving;
            Data.InteractableAppearance appearance = new Data.InteractableAppearance();
            appearance.AddPart(Data.InteractableBodyPart.Generate(BodyPart.Asset, @"059-Aquatic01", 255, 0, 0));
            appearance.MapId =  new Byte[12];
            appearance.MapX = 15;
            appearance.MapY = 20;
            appearance.MapDir = (Byte)Data.Enum.Direction.West;
            interactableSource.AddComponent(appearance);
            interactableSource.Initialize(_mapData);
            _interactables.Add(new Graphics.Sprite.Interactable(this.Game, ref _screenCamera, interactableSource));

            Lidgren.Network.NetRandom netRandom = new Lidgren.Network.NetRandom();
            
            // "Stress" test

            // NOTE: Bottleneck is in the LINQ??? Should be cached, because can  be updated on move. Is cheaper than rebuilding list everytime.

            String[] file = new String[] { @"059-Aquatic01", "purlplemonsterqo0", "redmonsteraa4", "Runes" };
            Data.InteractableMovement movement;

            while (_interactables.Count < 100)
            {
                Int32 x = netRandom.Next(_tilemap.Width);
                Int32 y = netRandom.Next(_tilemap.Height);

                while (!_mapData.IsPassable(x, y, 0))
                {
                    x = netRandom.Next(_tilemap.Width);
                    y = netRandom.Next(_tilemap.Height);
                }

                // Create stopmotion monster
                try
                {
                    interactableSource = new Data.Interactable();
                    interactableSource.Id = new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (Byte)(_interactables.Count + 5) };
                    interactableSource.Name = "Random " + _interactables.Count;
                    interactableSource.StateFlags |= InteractableStateFlags.Visible | InteractableStateFlags.Moving;
                    appearance = new Data.InteractableAppearance();
                    appearance.AddPart(Data.InteractableBodyPart.Generate(BodyPart.Asset, file[_interactables.Count%file.Length], 255, 0, 0));
                    appearance.MapId = new Byte[12];
                    appearance.MapX = x;
                    appearance.MapY = y;
                    appearance.MapDir = (Byte)((1 + netRandom.Next(4)) * 2);
                    interactableSource.AddComponent(appearance);
                    movement = new Data.InteractableMovement();
                    movement.StopFrequency = (Byte)netRandom.Next(4);
                    interactableSource.AddComponent(movement);
                    interactableSource.Initialize(_mapData);
                    _interactables.Add(new Graphics.Sprite.Interactable(this.Game, ref _screenCamera, interactableSource));
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            // Create passable monster
            /*interactableSource = new Data.Interactable(new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 }, 0, "Sahagin 3", @"059-Aquatic01", InteractableFlags.Visible | InteractableFlags.Moving | InteractableFlags.Through,
                Data.Enum.InteractableType.None, new Byte[12], 20, 25, (Byte)Data.Enum.Direction.North, 0, 3, 3, 0);
            interactableSource.Initialize(_mapData);
            _interactables.Add(new Graphics.Sprite.Interactable(this.Game, ref _screenCamera, interactableSource));

            // Create invisible impassable monster
            interactableSource = new Data.Interactable(new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4 }, 0, "Sahagin 3", @"059-Aquatic01", InteractableFlags.Moving,
                Data.Enum.InteractableType.None, new Byte[12], 21, 25, (Byte)Data.Enum.Direction.North, 0, 3, 3, 0);
            interactableSource.Initialize(_mapData);
            _interactables.Add(new Graphics.Sprite.Interactable(this.Game, ref _screenCamera, interactableSource));*/

            for (Int32 i = 0; i < _interactables.Count; i++)
            {
                _interactables[i].Initialize();
                _interactables[i].LoadContent(_tilemap.ContentManager);
                _motionBlurRenderTarget.AddComponent(_interactables[i]);
 
            }

            DebugOverlay overlay = new DebugOverlay(this.Game, this.Camera, _mapData);
            overlay.Initialize();
            overlay.LoadContent(contentManager);
            this.ScreenRendertarget.AddComponent(overlay);

            _screenRenderTarget.AddRendertarget(_motionBlurRenderTarget);

            ProjectERA.Services.Network.Protocols.Protocol mapProtocol;
            if (this.NetworkManager.TryGetProtocol(typeof(ProjectERA.Services.Network.Protocols.Map), out mapProtocol))
            {
                ((ProjectERA.Services.Network.Protocols.Map)mapProtocol).OnInteractableJoined += new EventHandler(DummyScreen_OnInteractableJoined);
                ((ProjectERA.Services.Network.Protocols.Map)mapProtocol).OnInteractableLeft +=new EventHandler(DummyScreen_OnInteractableLeft); 
            }
            else
            {
                throw new InvalidOperationException("No Map protocol was found!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DummyScreen_OnInteractableLeft(object sender, EventArgs e)
        {
            ProjectERA.Data.Interactable i = sender as ProjectERA.Data.Interactable;

            if (true)
            {
                Interactable removee = _interactables.Find(a => a.SourceId == i.Id);
                if (removee != null)
                    _interactables.Remove(removee);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DummyScreen_OnInteractableJoined(object sender, EventArgs e)
        {
            ProjectERA.Data.Interactable i = sender as ProjectERA.Data.Interactable;

            if (true)//i.Appearance.MapId == _mapData.MapId)
            {
                // Add to mapdata.interactables and set blocking
                i.Initialize(_mapData);

                // Add sprite
                _interactables.Add(new Graphics.Sprite.Interactable(this.Game, ref _screenCamera, i));
            }
        }

        List<Graphics.Sprite.Interactable> _avatars;
        List<Graphics.Sprite.Interactable> _interactables;

        MotionBlurRenderTarget _motionBlurRenderTarget;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        internal override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {          
            // Update (critical section) input
            if (!otherScreenHasFocus && !coveredByOtherScreen)
                UpdateInput(gameTime);

#if NOMULTITHREAD
            // Also edits shakeAmount! TODO: NEED SOME EDIT
            this.ScreenRendertarget.Update(gameTime);
#endif

            // Update ScreenManager
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected void UpdateInput(GameTime gameTime)
        {
            // CAMERA UPDATE SECTION
            // Updates the camera movement by adding a fourth tile to the current position focus.

            if (!DataManager.LocalAvatar.StateFlags.HasFlag(InteractableStateFlags.Moving) || 
                    (Math.Abs(DataManager.LocalSprite.Position.X - DataManager.LocalAvatar.Appearance.MapX) < .5 &&
                     Math.Abs(DataManager.LocalSprite.Position.Y - (DataManager.LocalAvatar.Appearance.MapY + .5f)) < .5))
            {
                Boolean isMoving = false;
                Int32 xDestination = DataManager.LocalAvatar.Appearance.MapX;
                Int32 yDestination = DataManager.LocalAvatar.Appearance.MapY;

                Boolean left = InputManager.Keyboard.IsKeyDown(Keys.Left);
                Boolean right = InputManager.Keyboard.IsKeyDown(Keys.Right);

                if (InputManager.Keyboard.IsKeyDown(Keys.Up))
                {
                    if (right)
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, _mapData, Direction.NorthEast);
                        xDestination++;
                        yDestination++;
                    }
                    else if (left)
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, _mapData, Direction.NorthWest);
                        xDestination--;
                        yDestination++;
                    }
                    else
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMoveUp(DataManager.LocalAvatar, _mapData);
                        yDestination++;
                    }
                } else if (InputManager.Keyboard.IsKeyDown(Keys.Down)) {
                    if (right)
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, _mapData, Direction.SouthEast);
                        xDestination++;
                        yDestination--;
                    }
                    else if (left)
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMove(DataManager.LocalAvatar, _mapData, Direction.SouthWest);
                        xDestination--;
                        yDestination--;
                    }
                    else
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMoveDown(DataManager.LocalAvatar, _mapData);
                        yDestination--;
                    }
                } else {
                    if (left)
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMoveLeft(DataManager.LocalAvatar, _mapData);
                        xDestination--;
                    }
                    else if (right)
                    {
                        isMoving = ProjectERA.Logic.GameAvatar.TryMoveRight(DataManager.LocalAvatar, _mapData);
                        xDestination++;
                    }
                }

                if (isMoving)
                {
                    DataManager.LocalAvatar.AddChange(() => { DataManager.LocalAvatar.StateFlags |= InteractableStateFlags.Moving; });
                    ProjectERA.Services.Network.Protocols.Player.RequestMovement(xDestination, yDestination);
                }
            }

            // Zoom In
            if (InputManager.Keyboard.IsKeyReleased(Keys.OemPlus))
            {
                //this.Camera.AddChange(new Change<Single>(this.Camera, "Zoom", this.Camera.Zoom * 2));
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

            if (InputManager.Keyboard.IsKeyTriggerd(Keys.Space))
                //this.Camera.AddChange(new Change<Single>(this.Camera, "ShakeAmount", this.Camera.ShakeAmount * 1.5f));
                this.Camera.ShakeAmount *= 1.5f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {

#if NOMULTITHREAD
            this.ScreenRendertarget.Render(gameTime);
#else
            //this.Game.GraphicsDevice.SetRenderTarget(rt);
            //this.Game.GraphicsDevice.Clear(Color.Black);          
            
            this.ScreenRendertarget.RunExclusiveCycle(gameTime);
            //this.Game.GraphicsDevice.SetRenderTarget(null);

            //fx.Parameters["screen"].SetValue(rt);
            //fx.Parameters["speed"].SetValue(Vector2.Zero);

          
            //this.ScreenManager.SpriteBatch.Begin();//, fx);
            //this.ScreenManager.SpriteBatch.Draw(rt, new Rectangle(0, 0, 1280, 720), Color.White);
            //this.ScreenManager.SpriteBatch.End();
            
#endif
           
            // Update drawing with fading
            if (this.IsTransitioning)
                // Draw the black fading graphic
                ScreenManager.FadeBackBufferToBlack((Byte)(255 - TransitionAlpha));

            base.Draw(gameTime);
        }
    }
}
