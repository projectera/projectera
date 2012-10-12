using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Display;
using ProjectERA.Services.Input;

namespace ProjectERA.Graphics.Sprite
{
    internal partial class HeadsUpDisplay : DrawableComponent
    {
        private readonly String _assetPortretBackground = @"Graphics\Interface\hud_portret_background";
        private readonly String _assetPortretShadow = @"Graphics\Interface\hud_portret_shadow";
        private readonly String _assetPortretExperience = @"Graphics\Interface\hud_portret_experience_noglow";
        private readonly String _assetPortretExperiencePoint = @"Graphics\Interface\hud_portret_experience_point";
        private readonly String _assetPortretExperienceEffect = @"Shaders\SpriteBatchPieShader";
        private readonly String _assetMenuBackground = @"Graphics\Interface\hud_menu_background";
        private readonly String _assetMenuShadow = @"Graphics\Interface\hud_portret_menushadow";
        private readonly String _assetMenuCursor = @"Graphics\Interface\hud_menu_highlight";
        
        private readonly Vector2 _positionNotification = new Vector2(220, 10);
        private readonly Vector2 _positionPortret = new Vector2(60, 60);
        private readonly Vector2 _positionExperienceCenter = new Vector2(60 + 120 / 2, 60 + 120 / 2);
        private readonly Vector2 _positionMenu = new Vector2(80, 150);
        private readonly Int32[] _positionMenuCursor = new Int32[] { 18, 38, 68, 88, 108, 128, 158 };

        private readonly Int32 _spacingNotification = 10;

        /// <summary>
        /// Enumeration for the MenuOptions
        /// </summary>
        internal enum MenuOption : byte
        {
            Character   = 0,
            Inventory   = 1,
            Abilities   = 2,
            Talents     = 3,
            Widgets     = 4,
            Options     = 5,
            Logout      = 6
        }

        #region Private Fields

        /// <summary>
        /// The interactable used to display the portret head
        /// </summary>
        private Interactable _portretHead;
        /// <summary>
        /// The widgetback for the portret
        /// </summary>
        private Texture2D _portretBackground;

        /// <summary>
        /// The Shadow for the portret
        /// </summary>
        private Texture2D _portretShadow;

        /// <summary>
        /// The Experience Circle
        /// </summary>
        private Texture2D _portretExperience;

        /// <summary>
        /// The Experience Point
        /// </summary>
        private Texture2D _portretExperiencePoint;

        /// <summary>
        /// The Experience Effect
        /// </summary>
        private Effect _experienceEffect;

        /// <summary>
        /// The widgetback for the portret
        /// </summary>
        private Texture2D _menuBackground;

        /// <summary>
        /// The shadow for the portret
        /// </summary>
        private Texture2D _menuShadow;

        /// <summary>
        /// The cursor highlighter for the menu
        /// </summary>
        private Texture2D _menuCursor;

        /// <summary>
        /// Current displaying percentage (opposed to actual percentage)
        /// </summary>
        private Single _displayPercentage;

        /// <summary>
        /// Current displaying menu cursor position
        /// </summary>
        private Vector2 _displayPositionMenuCursor;

        /// <summary>
        /// Previous frame angle (for Experience endpoint)
        /// </summary>
        private Double _oldAngle;

        /// <summary>
        /// Position for the Experience Point
        /// </summary>
        private Vector2 _positionExperiencePoint;

#endregion

        #region Properties
        
        /// <summary>
        /// DataSource for the Display
        /// </summary>
        private Data.Interactable DataSource
        {
            get;
            set;
        }

        /// <summary>
        /// Notifications showing
        /// </summary>
        private Queue<String> Notifications
        {
            get;
            set;
        }

        /// <summary>
        /// Default HUD Font
        /// </summary>
        private SpriteFont Font
        {
            get;
            set;
        }

        /// <summary>
        /// Default HUD Spritebatch
        /// </summary>
        private SpriteBatch SpriteBatch
        {
            get;
            set;
        }

        /// <summary>
        /// Reference to TextureManager
        /// </summary>
        private TextureManager TextureManager
        {
            get;
            set;
        }

        /// <summary>
        /// Reference to InputManager
        /// </summary>
        private InputManager InputManager
        {
            get;
            set;
        }

        /// <summary>
        /// Created Widgets
        /// </summary>
        private HashSet<Widget> Widgets
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the menu state
        /// </summary>
        internal Boolean MenuActive
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Menu cursor index
        /// </summary>
        internal Int32 MenuCursorIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the menu toggle ability
        /// </summary>
        internal Boolean MenuLocked
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Created the Heads Up Display
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="camera">Camera to use</param>
        /// <param name="interactable">Datasource</param>
        public HeadsUpDisplay(Microsoft.Xna.Framework.Game game, Services.Display.Camera3D camera, Data.Interactable interactable)
            :base(game, camera)
        {
            this.DataSource = interactable;
            this.Notifications = new Queue<String>();

            Services.Network.Protocols.Map.OnNotification += new EventHandler(MapProtocol_OnNotification);

            _portretHead = new Sprite.Interactable(game, ref camera, interactable);
        }

        /// <summary>
        /// Injects a new Notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MapProtocol_OnNotification(Object sender, EventArgs e)
        {
            this.AddChange(() => { 
                this.Notifications.Enqueue((String)sender); 
                if (this.Notifications.Count > 10) 
                    this.Notifications.Dequeue(); 
            });
        }

        /// <summary>
        /// Initializes HUD
        /// </summary>
        internal override void Initialize()
        {
            this.IsSemiTransparant = true;

            this.TextureManager = (TextureManager)this.Game.Services.GetService(typeof(TextureManager));
            this.InputManager = (InputManager)this.Game.Services.GetService(typeof(InputManager));
            this.Widgets = new HashSet<Widget>();
            
            // Add Widgets
            this.Widgets.Add(new HeadsUpDisplay.StatusWidget(this.Game, this.Camera, this.DataSource));

            // Initialize local
            _portretHead.Initialize();
            
            _displayPercentage = 0.10f;
            _displayPositionMenuCursor = _positionMenu + Vector2.UnitY * _positionMenuCursor[this.MenuCursorIndex];
            _oldAngle = 0;

            // Initialize widgets
            foreach (Widget widget in Widgets)
            {
                widget.Initialize();
            }
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            this.Font = ((ProjectERA.Services.Data.FontCollector)Game.Services.GetService(typeof(ProjectERA.Services.Data.FontCollector)))["Notifications"];
            this.SpriteBatch = new SpriteBatch(this.Game.GraphicsDevice);

            _portretBackground = TextureManager.LoadStaticTexture(_assetPortretBackground, contentManager);
            _portretShadow = TextureManager.LoadStaticTexture(_assetPortretShadow, contentManager);
            _portretExperience = TextureManager.LoadStaticTexture(_assetPortretExperience, contentManager);
            _portretExperiencePoint = TextureManager.LoadStaticTexture(_assetPortretExperiencePoint, contentManager);
            _menuBackground = TextureManager.LoadStaticTexture(_assetMenuBackground, contentManager);
            _menuShadow = TextureManager.LoadStaticTexture(_assetMenuShadow, contentManager);
            _menuCursor = TextureManager.LoadStaticTexture(_assetMenuCursor, contentManager);

            _portretHead.LoadContent(contentManager);
            Rectangle sourceRect = _portretHead.SourceRect;
            sourceRect.Height = 14;
            _portretHead.SourceRect = sourceRect;

            _experienceEffect = contentManager.Load<Effect>(_assetPortretExperienceEffect);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, this.Camera.Resolution.X, this.Camera.Resolution.Y, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            _experienceEffect.Parameters["World"].SetValue(Matrix.Identity);
            _experienceEffect.Parameters["View"].SetValue(Matrix.Identity);
            _experienceEffect.Parameters["Projection"].SetValue(halfPixelOffset * projection);
            _experienceEffect.Parameters["Percentage"].SetValue(0.62f);
            _experienceEffect.CurrentTechnique = _experienceEffect.Techniques["NonPremultiplied"];

            //this.TextureManager.SaveStaticTexure("test.png", _menuBackground);

            foreach (Widget widget in this.Widgets)
            {
                widget.LoadContent(contentManager);
            }
            
        }

        /// <summary>
        /// Unloads all content
        /// </summary>
        internal override void UnloadContent()
        {
            this.SpriteBatch.Dispose();

            this.TextureManager.ReleaseStaticTexture(_assetPortretBackground);
            this.TextureManager.ReleaseStaticTexture(_assetPortretShadow);
            this.TextureManager.ReleaseStaticTexture(_assetPortretExperience);
            this.TextureManager.ReleaseStaticTexture(_assetPortretExperiencePoint);
            this.TextureManager.ReleaseStaticTexture(_assetMenuBackground);
            this.TextureManager.ReleaseStaticTexture(_assetMenuShadow);
            this.TextureManager.ReleaseStaticTexture(_assetMenuCursor);

            _portretHead.UnloadContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime, bool drawTransparent)
        {
            if (!drawTransparent)
                return;

            this.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            // Creates a new vector
            Vector2 position = Vector2.One * _positionNotification;

            // Show notifications
            foreach (String s in this.Notifications)
            {
                this.SpriteBatch.DrawString(Font, s, position + Vector2.UnitY + Vector2.UnitX, Color.Black);
                this.SpriteBatch.DrawString(Font, s, position, Color.White);
                position += Vector2.UnitY * _spacingNotification;
            }

            // Draw menu
            if (this.MenuActive)
            { 
                this.SpriteBatch.Draw(_menuBackground, _positionMenu, Color.White);
                this.SpriteBatch.Draw(_menuCursor, _displayPositionMenuCursor, Color.White);
            }

            // Draw portret
            this.SpriteBatch.Draw(this.MenuActive ? _menuShadow : _portretShadow, _positionPortret + Vector2.One * 6, Color.White);
            this.SpriteBatch.Draw(_portretBackground, _positionPortret, Color.White);

            this.SpriteBatch.End();


            this.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, _experienceEffect);
            this.SpriteBatch.Draw(_portretExperience, _positionPortret, Color.White);
            this.SpriteBatch.End();

            this.SpriteBatch.Begin();

            // Draw Experience end
            this.SpriteBatch.Draw(_portretExperiencePoint, _positionExperiencePoint + _positionExperienceCenter, Color.White);

            // Draw name
            String name = DataSource.Name;
            Vector2 center = Font.MeasureString(name);
            center.X = (Int32)(center.X / 2);
            center.Y = (Int32)(center.Y / 2);
            this.SpriteBatch.DrawString(Font, name, _positionPortret + Vector2.UnitX * (_portretBackground.Width / 2) + Vector2.UnitY * 71, Color.Black, 0, center, 1, SpriteEffects.None, 0);
            this.SpriteBatch.DrawString(Font, name, _positionPortret + Vector2.UnitX * ((_portretBackground.Width / 2) - 1) + Vector2.UnitY * 70, Color.White, 0, center, 1, SpriteEffects.None, 0);

            // Draw level
            String level = String.Format("lvl. {0}", DataSource.Battler.Level);
            center = Font.MeasureString(level);
            center.X = (Int32)(center.X / 2);
            center.Y = (Int32)(center.Y / 2);
            this.SpriteBatch.DrawString(Font, level, _positionPortret + Vector2.UnitX * (_portretBackground.Width / 2) + Vector2.UnitY * 86, Color.Black, 0, center, 1, SpriteEffects.None, 0);
            this.SpriteBatch.DrawString(Font, level, _positionPortret + Vector2.UnitX * ((_portretBackground.Width / 2) - 1) + Vector2.UnitY * 85, Color.LightGreen, 0, center, 1, SpriteEffects.None, 0);

            // Draw head
            this.SpriteBatch.Draw(_portretHead._interactableTexture, _positionPortret + Vector2.One * 30 + Vector2.UnitX * 16, new Rectangle(0, 0, _portretHead._interactableTexture.Width / 4, 30), Color.White);
            this.SpriteBatch.End();

            foreach (Widget widget in this.Widgets)
            {
                if (widget.IsVisible)
                    widget.Draw(gameTime, drawTransparent);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // Update Experience
            Double PI2 = Math.PI * 2;
            Double angle = PI2 * (_displayPercentage - 0.25f);
#if DEBUG
            Single somePercentage = InputManager.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) ? 0.10f : (Single)DataSource.Battler.LevelProgress;
#else
            Single somePercentage = (Single)DataSource.Battler.LevelProgress;
#endif
 
            if (_displayPercentage != somePercentage || _displayPercentage == 1f || _oldAngle != angle)
            {
                Single newPercentage = _displayPercentage;

                // Reset display if level up
                if (newPercentage == 1f)
                    newPercentage = 0;

                // If level up, first fill circle
                if (somePercentage < newPercentage)
                    somePercentage = 1f;

                // Lerp between current and goto
                newPercentage = MathHelper.Lerp(newPercentage, somePercentage, (Single)(gameTime.ElapsedGameTime.TotalSeconds * 2));
                
                // Set current to goto if close
                if (somePercentage - newPercentage < 0.001f)
                    newPercentage = somePercentage;

                // Calculate endpoint position
                Vector2 newPositionExperiencePoint = _positionExperiencePoint;
                newPositionExperiencePoint.X = (Single)Math.Cos(angle) * (92 / 2) - 2;
                newPositionExperiencePoint.Y = (Single)Math.Sin(angle) * (92 / 2) - 2;

                // Changes
                this.AddChange(() => _positionExperiencePoint = newPositionExperiencePoint);
                this.AddChange(() => _displayPercentage = newPercentage);
                this.AddChange(() => _experienceEffect.Parameters["Percentage"].SetValue(newPercentage));
                this.AddChange(() => _oldAngle = angle);
            }

            // Update menu cursor;
            Vector2 somePositionMenuCursor = _positionMenu + Vector2.UnitY * _positionMenuCursor[this.MenuCursorIndex];
            if (_displayPositionMenuCursor != somePositionMenuCursor)
            {
                // Lerp between current and goto
                Vector2 newPosition = _displayPositionMenuCursor;
                newPosition.Y = MathHelper.Lerp(_displayPositionMenuCursor.Y, somePositionMenuCursor.Y, (Single)(gameTime.ElapsedGameTime.TotalSeconds * 16));

                // Set current to goto if close
                if (Math.Abs(somePositionMenuCursor.Y - newPosition.Y) < 0.001f)
                    newPosition = somePositionMenuCursor;

                // Changes
                this.AddChange(() => _displayPositionMenuCursor = newPosition);
            }


            // Update Widgets
            foreach (Widget widget in this.Widgets)
            {
                if (widget.IsEnabled)
                    widget.Update(gameTime);
            }
        }

        /// <summary>
        /// Handles HUD input
        /// </summary>
        internal void HandleInput()
        {
            // Toggle menu
            if (InputManager.Keyboard.IsKeyTriggerd(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                ToggleMenu();
                return;
            }

            // Left button
            if (InputManager.Mouse.IsButtonReleased(MouseButton.Left))
            {
                // Move cursor or fire action
                for (int i = 0; i < 7; i++)
                {
                    if (InputManager.Mouse.IsOverObj(_positionMenu + Vector2.UnitY * _positionMenuCursor[i], _menuCursor.Bounds))
                    {
                        if (i == this.MenuCursorIndex)
                            FireAction();
                        else
                            this.MenuCursorIndex = i;

                        return;
                    }
                }

                // Toggle menu
                if (InputManager.Mouse.IsOverObj(_positionPortret, _portretBackground.Bounds))
                {
                    ToggleMenu();
                    return;
                }
            }

            // Update Widgets
            foreach (Widget widget in this.Widgets)
            {
                if (widget.IsEnabled)
                    widget.HandleInput();
            }

            if (this.MenuActive == false)
                return;

            // Move cursor down
            if (InputManager.Keyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                this.MenuCursorIndex = (this.MenuCursorIndex + 1) % 7;
                return;
            }

            // Move cursor up
            if (InputManager.Keyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                this.MenuCursorIndex = this.MenuCursorIndex - 1;
                if (this.MenuCursorIndex < 0)
                    this.MenuCursorIndex = 6;
                return;
            }

            // Fire action
            if (InputManager.Keyboard.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                FireAction();
                return;
            }
        }

        /// <summary>
        /// Fires the current cursor action
        /// </summary>
        internal void FireAction()
        {
            FireAction(this.MenuCursorIndex);
        }

        /// <summary>
        /// Fires an action
        /// </summary>
        /// <param name="option"></param>
        internal void FireAction(MenuOption option)
        {
            switch (option)
            {
                case MenuOption.Character:
                    break;
                case MenuOption.Inventory:
                    break;
                case MenuOption.Abilities:
                    break;
                case MenuOption.Talents:
                    break;
                case MenuOption.Widgets:
                    break;
                case MenuOption.Options:
                    break;
                case MenuOption.Logout:
                    this.Game.Exit();
                    break;
            }

            CloseMenu(true);
        }

        /// <summary>
        /// Fires an action
        /// </summary>
        /// <param name="index"></param>
        internal void FireAction(Int32 index)
        {
            FireAction((MenuOption)index);
        }

        /// <summary>
        /// Locks menu
        /// </summary>
        internal void LockMenu()
        {
            this.MenuLocked = true;
        }

        /// <summary>
        /// Toggles menu
        /// </summary>
        internal void ToggleMenu()
        {
            ToggleMenu(0);
        }

        /// <summary>
        /// Toggles menu and sets cursor
        /// </summary>
        internal void ToggleMenu(Int32 index)
        {
            if (this.MenuLocked)
                return;

            this.MenuActive = !this.MenuActive;
            this.MenuCursorIndex = index;

            // Move the _displayPosition of the MenuCursor
            _displayPositionMenuCursor = _positionMenu + Vector2.UnitY * _positionMenuCursor[this.MenuCursorIndex];
        }

        /// <summary>
        /// Hides the menu, overrides MenuLocked
        /// </summary>
        internal void HideMenu()
        {
            CloseMenu(false);
        }

        /// <summary>
        /// Hides the menu
        /// </summary>
        /// <param name="respectLock">if true, respects MenuLocked</param>
        internal void CloseMenu(Boolean respectLock)
        {
            if (respectLock && this.MenuLocked)
                return;

            this.MenuActive = false;
        }


        /// <summary>
        /// Opens the menu, overrides MenuLocked
        /// </summary>
        internal void OpenMenu()
        {
            OpenMenu(false);
        }

        /// <summary>
        /// Opens the menu
        /// </summary>
        /// <param name="respectLock">if true, respects MenuLocked</param>
        internal void OpenMenu(Boolean respectLock)
        {
            if (respectLock && this.MenuLocked)
                return;

            this.MenuActive = true;

            // Move the _displayPosition of the MenuCursor
            _displayPositionMenuCursor = _positionMenu + Vector2.UnitY * _positionMenuCursor[this.MenuCursorIndex];
        }

        /// <summary>
        /// Unlocks menu
        /// </summary>
        internal void UnlockMenu()
        {
            this.MenuLocked = false;
        }
    }
}
