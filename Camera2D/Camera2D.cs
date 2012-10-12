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

namespace Camera
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera2D : Microsoft.Xna.Framework.GameComponent
    {
        #region Fields
        
        private Vector3 _position;

        #endregion

        #region Properties

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public float Rotation { get; set; }
        public Vector2 Origin { get; set; }
        public float Scale { get; set; }
        public Vector2 ScreenCenter { get; protected set; }
        public Matrix Transform { get; set; }
        public IFocusable Focus { get; set; }
        public float MoveSpeed { get; set; }
        public Rectangle Bounds { get; set; }

        #endregion

        public Camera2D(Game game)
            : base(game)
        {
            this.Bounds = new Rectangle(0, 0, this.Game.GraphicsDevice.Viewport.Width, this.Game.GraphicsDevice.Viewport.Height);

        }

        public Camera2D(Game game, Rectangle bounds)
            : base(game)
        {
            this.Bounds = bounds;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {

            // Get the Screen Center
            this.ScreenCenter = new Vector2((this.Bounds.Width) / 2f, (this.Bounds.Height)/ 2f);
           
            // Set Basic Values
            this.Scale = 1;
            this.MoveSpeed = 1.25f;
            this.Position = new Vector3((Int32)(this.ScreenCenter.X / 32) * 32, (Int32)(this.ScreenCenter.Y / 32) * 32, 0);
            this.Focus = new PositionFocus(this.Position);
            this.Origin = this.ScreenCenter / this.Scale;

            base.Initialize();
        }

        /// <summary>
        /// Directly Move to Focused Point
        /// </summary>
        public void JumpToFocus()
        {
            _position.X = Focus.Position.X;
            _position.Y = Focus.Position.Y;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Create the Transform used by any spritebatch process
            Transform = Matrix.Identity *
                        Matrix.CreateTranslation(-(Int32)Position.X + this.Bounds.X, -(Int32)Position.Y + this.Bounds.Y, 0) *
                        Matrix.CreateRotationZ(Rotation) *
                        Matrix.CreateTranslation((Int32)Origin.X, (Int32)Origin.Y, 0) *
                        Matrix.CreateScale(new Vector3(Scale, Scale, Scale));

            // Set the Origin
            Origin = ScreenCenter / Scale;

            // Move the Camera to the position that it needs to go
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update Position
            _position.X = _position.X + (Focus.Position.X - Position.X) * MoveSpeed * delta;
            _position.Y = _position.Y + (Focus.Position.Y - Position.Y) * MoveSpeed * delta;

            // Update Base
            base.Update(gameTime);
        }

        /// <summary>
        /// Determines whether the target is in view given the specified position.
        /// This can be used to increase performance by not drawing objects
        /// directly in the viewport
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texture">The texture.</param>
        /// <returns>
        ///     <c>true</c> if [is in view] [the specified position]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInView(Vector2 position, Texture2D texture)
        {
            // If the object is not within the horizontal bounds of the screen
            if ((position.X + texture.Width) < (Position.X - Origin.X) || (position.X) > (Position.X + Origin.X))
                return false;

            // If the object is not within the vertical bounds of the screen
            if ((position.Y + texture.Height) < (Position.Y - Origin.Y) || (position.Y) > (Position.Y + Origin.Y))
                return false;

            // In View
            return true;
        }
    }

    public class PositionFocus : IFocusable
    {
        public Vector3 Position
        {
            get;
            private set;
        }

        public PositionFocus(Vector3 focus)
        {
            this.Position = focus;
        }

        /*public PositionFocus(Vector2 focus)
        {
            this.Position = new Vector3(focus.X, focus.Y, 0);
        }*/
    }
}
