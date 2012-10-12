using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using ProjectERA.Services.Display;
using ProjectERA.Services.Data;
using ProjectERA.Logic;
using ERAUtils.Enum;
using System.Timers;
using ProjectERA.Data;
using ProjectERA.Data.Enum;
using ProjectERA.Services.Input;

namespace ProjectERA.Graphics.Sprite
{
    /// <summary>
    /// The Editor Overlay component obtains data from the map and player when DEBUG compilation
    /// symbols are set and displays this on the map.
    /// </summary>
    internal class EditorOverlay : DrawableComponent
    {
        private SpriteFont _debugFont;
        private SpriteBatch _spriteBatch;
        private MapData _mapData;
        private TileMap _tileMap;
        private TextureManager _textureManager;
        private InputManager _inputManager;
        private Texture2D _textureSelectorOverlay;

        /// <summary>
        /// Loads all managed content
        /// </summary>
        /// <param name="contentManager">Contentmanager to load to</param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            _spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            _debugFont = contentManager.Load<SpriteFont>("Common/defaultFont");
            _textureSelectorOverlay = contentManager.Load<Texture2D>("Graphics/Interface/tsSelectorOverlay");
        }

        /// <summary>
        /// Unload all unmanaged content
        /// </summary>
        internal override void UnloadContent()
        {
            _spriteBatch.Dispose();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="camera">Camera to use</param>
        /// <param name="tileMap">Tilemap for data</param>
        public EditorOverlay(Game game, Camera3D camera, MapData mapData, TileMap tileMap)
            :base(game, camera)
        {
            _mapData = mapData;
            _tileMap = tileMap;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        internal override void Initialize()
        {
            // Make this visible and updatable
            this.IsOccludable = false;
            this.IsEnabled = true;
            this.IsVisible = true;

            _textureManager = (TextureManager)this.Game.Services.GetService(typeof(TextureManager));
            _inputManager = (InputManager)this.Game.Services.GetService(typeof(InputManager));
        }


        /// <summary>
        /// Frame draw
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        /// <param name="drawTransparent">draws transparant portions</param>
        internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime, bool drawTransparent)
        {
            if (drawTransparent)
                return;

            Int32 startY = 350;

            Int32 tileX = IsSelected ? Selected.X : (Int32)((Camera.Position.X - 20) * 32 + _inputManager.Mouse.X) / 32;
            Int32 tileY = IsSelected ? Selected.Y : (Int32)((Camera.Position.Y - 11.5f)  * 32 + _inputManager.Mouse.Y) / 32;

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_debugFont, "EditorOverlay active", Vector2.UnitX * 10 + Vector2.UnitY * startY, Color.White);
            if (_mapData.IsValid(tileX, tileY))
                _spriteBatch.DrawString(_debugFont, 
                    _mapData.TileData[tileX][tileY][0] + " " +
                    _mapData.TileData[tileX][tileY][1] + " " + 
                    _mapData.TileData[tileX][tileY][2], 
                    Vector2.UnitX * 10 + Vector2.UnitY * (startY + 10), 
                    Color.White);

            Vector2 destinationSelector = new Vector2(
                (IsSelected ? Selected.X - (Camera.Position.X - 20 ) : _inputManager.Mouse.X / 32) * 32 - (IsSelected ? 0 : (((Camera.Position.X - 19) * 32) % 32)), 
                (IsSelected ? Selected.Y - (Camera.Position.Y - 11.5f) : _inputManager.Mouse.Y / 32) * 32 - (IsSelected ? 11.25f *32 % 32:  (((Camera.Position.Y - 11.25f) * 32) % 32)));
            _spriteBatch.Draw(_textureSelectorOverlay, destinationSelector, Color.White);
            
            String destinationPosition = "X: " + tileX + "\n" +
                                         "Y: " + tileY + (IsSelected ?
                                         "Z: " + Layer : String.Empty);
            _spriteBatch.DrawString(_debugFont, destinationPosition, new Vector2(destinationSelector.X + 40, destinationSelector.Y), Color.Black);
            _spriteBatch.End();

        }

        /// <summary>
        /// Frame update
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        internal override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (_inputManager.Mouse.IsButtonPressed(MouseButton.Left))
            {
                Point current = new Point((Int32)((Camera.Position.X - 20) * 32 + _inputManager.Mouse.X) / 32,
                     (Int32)((Camera.Position.Y - 11.5f) * 32 + _inputManager.Mouse.Y) / 32);

                if (IsSelected && Layer == 2)
                {
                    IsSelected = false;
                }
                else if (!IsSelected || Selected != current)
                {
                    IsSelected = true; Layer = 0;
                    Selected = current;
                } else {
                    Layer++;
                }
            }

            if (IsSelected && _mapData.IsValid(Selected.X,Selected.Y)  && Math.Abs(_inputManager.Mouse.ScrollWheelChangeValue) > 0 && _inputManager.Mouse.ScrollWheelValue != _oldValue)
            {
                
                _oldValue = _inputManager.Mouse.ScrollWheelValue;
                _mapData.TileData[Selected.X][Selected.Y][Layer] = (UInt16)(_mapData.TileData[Selected.X][Selected.Y][Layer] + (_inputManager.Mouse.ScrollWheelChangeValue / 120));
                _tileMap.SetTile(Selected.X, Selected.Y, Layer, _mapData.TileData[Selected.X][Selected.Y][Layer]);

            }
            else if (IsSelected && _inputManager.Keyboard.IsKeyTriggerd(Microsoft.Xna.Framework.Input.Keys.Up) && _mapData.IsValid(Selected.X, Selected.Y))
            {
                _mapData.TileData[Selected.X][Selected.Y][Layer] = (UInt16)(_mapData.TileData[Selected.X][Selected.Y][Layer] + 1);
                _tileMap.SetTile(Selected.X, Selected.Y, Layer, _mapData.TileData[Selected.X][Selected.Y][Layer]);
            }
            else if (IsSelected && _inputManager.Keyboard.IsKeyTriggerd(Microsoft.Xna.Framework.Input.Keys.Down) && _mapData.IsValid(Selected.X, Selected.Y))
            {
                _mapData.TileData[Selected.X][Selected.Y][Layer] = (UInt16)(_mapData.TileData[Selected.X][Selected.Y][Layer] - 1);
                _tileMap.SetTile(Selected.X, Selected.Y, Layer, _mapData.TileData[Selected.X][Selected.Y][Layer]);
            }
        }

        private Int32 _oldValue = 0;
        public Boolean IsSelected { get; set; }
        public Point Selected { get; set; }
        public Int32 Layer { get; set; }
    }
}
