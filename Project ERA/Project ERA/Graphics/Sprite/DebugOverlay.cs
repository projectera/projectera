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

namespace ProjectERA.Graphics.Sprite
{
    /// <summary>
    /// The Debug Overlay component obtains data from the map and player when DEBUG compilation
    /// symbols are set and displays this on the map. Opposed to the framerate, this will be
    /// printed on screenshots taken.
    /// </summary>
    internal class DebugOverlay : DrawableComponent
    {
        private SpriteFont _debugFont;
        private SpriteBatch _spriteBatch;
        private MapData _mapData;
        private TextureManager _tm;

        /// <summary>
        /// Loads all managed content
        /// </summary>
        /// <param name="contentManager">Contentmanager to load to</param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            _spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            _debugFont = contentManager.Load<SpriteFont>("Common/defaultFont");
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
        public DebugOverlay(Game game, Camera3D camera, MapData mapData)
            :base(game, camera)
        {
            _mapData = mapData;
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

            _tm = (TextureManager)this.Game.Services.GetService(typeof(TextureManager));
        }

        private Stopwatch _stopwatch = new Stopwatch();

        Boolean _left, _right, _up, _down;
        Int32 _oldX, _oldY;

        /// <summary>
        /// Frame draw
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        /// <param name="drawTransparent">draws transparant portions</param>
        internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime, bool drawTransparent)
        {
            if (drawTransparent)
                return;

            _stopwatch.Start();

            this.GraphicsDevice.Clear(new Color(0, 0, 0, 50));

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_debugFont, "DebugOverlay active", Vector2.UnitX * 11 + Vector2.UnitY * 31, Color.Black);
            _spriteBatch.DrawString(_debugFont, "DebugOverlay active", Vector2.UnitX * 10 + Vector2.UnitY * 30, Color.White);

            InteractableAppearance appearance = DataManager.LocalAvatar.Appearance;

            // Player orientation
            Int32 startY = 60;
            _spriteBatch.DrawString(_debugFont, "MapX: " + DataManager.LocalAvatar.MapX, Vector2.UnitX * 11 + Vector2.UnitY * (startY + 1), Color.Black);
            _spriteBatch.DrawString(_debugFont, "MapY: " + DataManager.LocalAvatar.MapY, Vector2.UnitX * 61 + Vector2.UnitY * (startY + 1), Color.Black);
            _spriteBatch.DrawString(_debugFont, "MapX: " + DataManager.LocalAvatar.MapX, Vector2.UnitX * 10 + Vector2.UnitY * (startY), Color.White);
            _spriteBatch.DrawString(_debugFont, "MapY: " + DataManager.LocalAvatar.MapY, Vector2.UnitX * 60 + Vector2.UnitY * (startY), Color.White);
            _spriteBatch.DrawString(_debugFont, "MapDir: " + (Direction)appearance.MapDir, Vector2.UnitX * 11 + Vector2.UnitY * (startY + 11), Color.Black);
            _spriteBatch.DrawString(_debugFont, "MapDir: " + (Direction)appearance.MapDir, Vector2.UnitX * 10 + Vector2.UnitY * (startY + 10), Color.White);
            _spriteBatch.DrawString(_debugFont, "AniFrame: " + appearance.AnimationFrame, Vector2.UnitX * 91 + Vector2.UnitY * (startY + 11), Color.Black);
            _spriteBatch.DrawString(_debugFont, "AniFrame: " + appearance.AnimationFrame, Vector2.UnitX * 90 + Vector2.UnitY * (startY + 10), Color.White);

            // Passability
            startY = 90;

            if (_oldX != DataManager.LocalAvatar.MapX || _oldY != DataManager.LocalAvatar.MapY)
            {
                _oldX = DataManager.LocalAvatar.MapX;
                _oldY = DataManager.LocalAvatar.MapY;
                _left = GameAvatar.IsPassable(DataManager.LocalAvatar, _mapData, Direction.West);
                _right = GameAvatar.IsPassable(DataManager.LocalAvatar, _mapData, Direction.East);
                _up = GameAvatar.IsPassable(DataManager.LocalAvatar, _mapData, Direction.North);
                _down = GameAvatar.IsPassable(DataManager.LocalAvatar, _mapData, Direction.South);
            }

            _spriteBatch.DrawString(_debugFont, "Left: "  + (_left ? "passable" : "blocked"), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 1), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Right: " + (_right  ? "passable" : "blocked"), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 11), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Up: "    + (_up ? "passable" : "blocked"), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 21), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Down: "  + (_down ? "passable" : "blocked"), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 31), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Left: "  + (_left  ? "passable" : "blocked"), Vector2.UnitX * 10 + Vector2.UnitY * (startY), Color.White);
            _spriteBatch.DrawString(_debugFont, "Right: " + (_right  ? "passable" : "blocked"), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 10) , Color.White);
            _spriteBatch.DrawString(_debugFont, "Up: "    + (_up ? "passable" : "blocked"), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 20), Color.White);
            _spriteBatch.DrawString(_debugFont, "Down: "  + (_down ? "passable" : "blocked"), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 30), Color.White);

            // Flags
            startY = 140;
            _spriteBatch.DrawString(_debugFont, "Flags: " + (_mapData.IsBush(DataManager.LocalAvatar.MapX, DataManager.LocalAvatar.MapY) ? "bush " : "") + (_mapData.IsCounter(DataManager.LocalAvatar.MapX, DataManager.LocalAvatar.MapY) ? "counter " : ""), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 11), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Flags: " + (_mapData.IsBush(DataManager.LocalAvatar.MapX, DataManager.LocalAvatar.MapY) ? "bush " : "") + (_mapData.IsCounter(DataManager.LocalAvatar.MapX, DataManager.LocalAvatar.MapY) ? "counter " : ""), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 10), Color.White);

            // Camera
            startY = 170;
            _spriteBatch.DrawString(_debugFont, "ShakeAmount: " + this.Camera.ShakeAmount, Vector2.UnitX * 11 + Vector2.UnitY * (startY + 11), Color.Black);
            _spriteBatch.DrawString(_debugFont, "ShakeAmount: " + this.Camera.ShakeAmount, Vector2.UnitX * 10 + Vector2.UnitY * (startY + 10), Color.White);
            _spriteBatch.DrawString(_debugFont, "ZoomAmount: " + this.Camera.Zoom + " > " + this.Camera.ZoomTo, Vector2.UnitX * 11 + Vector2.UnitY * (startY + 21), Color.Black);
            _spriteBatch.DrawString(_debugFont, "ZoomAmount: " + this.Camera.Zoom + " > " + this.Camera.ZoomTo, Vector2.UnitX * 10 + Vector2.UnitY * (startY + 20), Color.White);
            _spriteBatch.DrawString(_debugFont, "Focus: " + this.Camera.Focus.Position.ToString(), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 31), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Focus: " + this.Camera.Focus.Position.ToString(), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 30), Color.White);
            _spriteBatch.DrawString(_debugFont, "Position: " + this.Camera.Position.ToString(), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 41), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Position: " + this.Camera.Position.ToString(), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 40), Color.White);
            _spriteBatch.DrawString(_debugFont, "Resolution: " + this.Camera.Resolution.ToString(), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 51), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Resolution: " + this.Camera.Resolution.ToString(), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 50), Color.White);
            _spriteBatch.DrawString(_debugFont, "FocusSpeed: " + this.Camera.MaxFocusSpeed, Vector2.UnitX * 11 + Vector2.UnitY * (startY + 61), Color.Black);
            _spriteBatch.DrawString(_debugFont, "FocusSpeed: " + this.Camera.MaxFocusSpeed, Vector2.UnitX * 10 + Vector2.UnitY * (startY + 60), Color.White);
            _spriteBatch.DrawString(_debugFont, "ZoomSpeed: " + this.Camera.MaxZoomSpeed, Vector2.UnitX * 11 + Vector2.UnitY * (startY + 71), Color.Black);
            _spriteBatch.DrawString(_debugFont, "ZoomSpeed: " + this.Camera.MaxZoomSpeed, Vector2.UnitX * 10 + Vector2.UnitY * (startY + 70), Color.White);

            Int64 staticSize, staticSizeInBytes, staticCapacityInBytes, dynamicSize, dynamicSizeInBytes, dynamicCapacityInBytes;
            Int32 sref, dref, sderef, dderef, senq, denq, srem, drem, sdis, ddis, slim, dlim;
            _tm.GetCacheStatistics(out staticSize, out staticSizeInBytes, out staticCapacityInBytes, out sref, out sderef, out senq, out srem, out sdis, out slim,
                out dynamicSize, out dynamicSizeInBytes, out dynamicCapacityInBytes, out dref, out dderef, out denq, out drem, out ddis, out dlim);
            _spriteBatch.DrawString(_debugFont, "Static Cache: " + staticSize + " items", Vector2.UnitX * 11 + Vector2.UnitY * (startY + 91), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Static Cache: " + staticSize + " items", Vector2.UnitX * 10 + Vector2.UnitY * (startY + 90), Color.White);
            _spriteBatch.DrawString(_debugFont, Math.Round(staticSizeInBytes / 1024 / 1024f, 2).ToString() + " MB / " + Math.Round(staticCapacityInBytes / 1024 / 1024f, 2), Vector2.UnitX * 11 + Vector2.UnitY * (startY + 101), Color.Black);
            _spriteBatch.DrawString(_debugFont, Math.Round(staticSizeInBytes / 1024 / 1024f, 2).ToString() + " MB / " + Math.Round(staticCapacityInBytes / 1024 / 1024f, 2), Vector2.UnitX * 10 + Vector2.UnitY * (startY + 100), Color.White);
            _spriteBatch.DrawString(_debugFont, "Dynamic Cache: " + dynamicSize + " items", Vector2.UnitX * 11 + Vector2.UnitY * (startY + 111), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Dynamic Cache: " + dynamicSize + " items", Vector2.UnitX * 10 + Vector2.UnitY * (startY + 110), Color.White);
            _spriteBatch.DrawString(_debugFont, Math.Round(dynamicSizeInBytes / 1024 / 1024f, 2).ToString() + " MB / " + Math.Round(dynamicCapacityInBytes / 1024 / 1024f, 2) + " MB", Vector2.UnitX * 11 + Vector2.UnitY * (startY + 121), Color.Black);
            _spriteBatch.DrawString(_debugFont, Math.Round(dynamicSizeInBytes / 1024 / 1024f, 2).ToString() + " MB / " + Math.Round(dynamicCapacityInBytes / 1024 / 1024f, 2) + " MB", Vector2.UnitX * 10 + Vector2.UnitY * (startY + 120), Color.White);

            _spriteBatch.DrawString(_debugFont, "Ref/Def [S: " + sref + "/" + sderef + "] [D: " + dref + "/" + dderef + "]", Vector2.UnitX * 11 + Vector2.UnitY * (startY + 131), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Enq/Rem [S: " + senq + "/" + srem + "] [D: " + denq + "/" + drem + "]", Vector2.UnitX * 11 + Vector2.UnitY * (startY + 141), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Dispose [S: " + sdis + "] [D: " + ddis + "]", Vector2.UnitX * 11 + Vector2.UnitY * (startY + 151), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Cap.lim [S: " + slim + "] [D: " + dlim + "]", Vector2.UnitX * 11 + Vector2.UnitY * (startY + 161), Color.Black);
            _spriteBatch.DrawString(_debugFont, "Ref/Def [S: " + sref + "/" + sderef + "] [D: " + dref + "/" + dderef + "]", Vector2.UnitX * 10 + Vector2.UnitY * (startY + 130), Color.White);
            _spriteBatch.DrawString(_debugFont, "Enq/Rem [S: " + senq + "/" + srem + "] [D: " + denq + "/" + drem + "]", Vector2.UnitX * 10 + Vector2.UnitY * (startY + 140), Color.White);
            _spriteBatch.DrawString(_debugFont, "Dispose [S: " + sdis + "] [D: " + ddis + "]", Vector2.UnitX * 10 + Vector2.UnitY * (startY + 150), Color.White);
            _spriteBatch.DrawString(_debugFont, "Cap.lim [S: " + slim + "] [D: " + dlim + "]", Vector2.UnitX * 10 + Vector2.UnitY * (startY + 160), Color.White);

            
            _stopwatch.Stop();

            Double elapsed = _stopwatch.ElapsedTicks;

            _stopwatch.Restart();

            Color color = elapsed > 1500 ? (elapsed > 3000 ? Color.Red : Color.Lerp(Color.Red, Color.LightGreen, 1 - ((Single)(elapsed - 1500) / (3000 - 1500f)))) : Color.LightGreen;            

            _spriteBatch.DrawString(_debugFont, "Overlay penalty: " + elapsed + " ticks", Vector2.UnitX * 11 + Vector2.UnitY * 41, Color.Black);
            _spriteBatch.DrawString(_debugFont, "Overlay penalty: " + elapsed + " ticks", Vector2.UnitX * 10 + Vector2.UnitY * 40, color);
            _spriteBatch.End();

            _stopwatch.Stop();
        }

        /// <summary>
        /// Frame update
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        internal override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
        }
    }
}
