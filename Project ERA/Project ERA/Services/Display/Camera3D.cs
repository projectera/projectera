using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;
using ProjectERA.Data.Update;
using ProjectERA.Graphics;

namespace ProjectERA.Services.Display
{
    /// <summary>
    /// 
    /// </summary>
    internal class Camera3D : DrawableComponent
    {
        
        public IFocusable Focus { get; set; }
        public Matrix Projection { get; protected set; }
        public Single MoveSpeed { get; set; }

        #region Protected members
        protected BoundingBox _bound;
        protected Vector3 _position;
        protected Single _zoom;
        protected Single _zoomTo;
        //protected Rectangle _safeZone;
        protected Point _resolution;
        //protected Single _focusEdge;
        protected Vector3 _shakeOffset;
        protected NetRandom _random;
        protected Rectangle _viewableArea;
        protected Rectangle _safePositions;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public Single ShakeAmount { get; set; }

        /// <summary>
        /// The speed at which the interpolation is done.
        /// </summary>
        public Single MaxFocusSpeed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Rectangle ViewableArea
        {
            get
            {
                return _viewableArea;
            }
            set
            {
                _viewableArea = value;
                _safePositions = value;
                _safePositions.Inflate((Int32)(-_resolution.X / this.Zoom / 2), (Int32)(-_resolution.Y / this.Zoom / 2));
            }
        }

        /// <summary>
        /// The screen resolution
        /// </summary>
        public Point Resolution
        {
            get
            {
                return _resolution;
            }
            set
            {
                _resolution = value;
                UpdateBounds();
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets the Bounds for this camera
        /// </summary>
        public BoundingBox Bound
        {
            get
            {
                return _bound;
            }

            set
            {
                _bound = value;
            }
        }

        /// <summary>
        /// Gets or Sets the position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets or Sets the zoom
        /// </summary>
        public Single Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (value != 0)
                    _zoom = value;

                _safePositions = _viewableArea;
                _safePositions.Inflate((Int32)(-_resolution.X / this.Zoom / 2), (Int32)(-_resolution.Y / this.Zoom / 2));

                UpdateProjection();
                UpdateBounds();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Single ZoomTo
        {
            get
            {
                return _zoomTo;
            }
            set
            {
                if (value != 0)
                    _zoomTo = value;
            }
        }

        /// <summary>
        /// Update bounds 
        /// </summary>
        protected void UpdateBounds()
        {
            Single aspectRatio = _resolution.Y / (Single)_resolution.X;
            Int32 range = (Int32)Math.Ceiling(this.Zoom / 2);
            Rectangle rect = new Rectangle((Int32)this._position.X, (int)this._position.Y, 0, 0);
            rect.Inflate(range, (int)(range * aspectRatio + 0.5));

            // Bound is used in Render (= Draw)
            this.AddChange(() => { this.Bound = new BoundingBox(new Vector3(rect.Left, rect.Top, 0), new Vector3(rect.Right, rect.Bottom, 0)); });
        }

        /// <summary>
        /// Update projection when position or zoom changes
        /// </summary>
        protected void UpdateProjection()
        {
            UpdateProjection(_position, _shakeOffset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newShakeOffset"></param>
        protected void UpdateProjection(Vector3 newPosition, Vector3 newShakeOffset)
        {
            //newShakeOffset.X = 0;
            //newShakeOffset.Y = 0;

            Single x = (Single)Math.Round((newPosition.X + (newShakeOffset.X * (this.Zoom / 32))) * this.Zoom) / this.Zoom;
            Single y = (Single)Math.Round((newPosition.Y + (newShakeOffset.Y * (this.Zoom / 32))) * this.Zoom) / this.Zoom;

            Matrix translation = Matrix.CreateTranslation(-x, -y, 0);
            Matrix obliqueProjection = new Matrix( 1, 0, 0, 0,
                                                   0, 1, 1, 0,
                                                   0, -1, 0, 0,
                                                   0, 0, 0, 1);

            Matrix taper = Matrix.Identity; /* new Matrix(1, 0, 0, 0,
                                      0, 1, 0, 0.1f,
                                      0, 0, 1, 0,
                                      0, 0, 0, 1);*/


            Matrix orthographic = Matrix.CreateOrthographicOffCenter(-_resolution.X / this.Zoom / 2, _resolution.X / this.Zoom / 2, _resolution.Y / this.Zoom / 2, -_resolution.Y / this.Zoom / 2, -10000, 10000);
            Matrix shakeRotation = Matrix.CreateRotationZ(Math.Abs(newShakeOffset.Z) > 0.01 ? newShakeOffset.Z / 20 * (this.Zoom/32) : 0);

            // Projection is used in Draw/Render
            this.AddChange(() => { this.Projection = translation * obliqueProjection * shakeRotation * orthographic * taper; });
        }

        public Vector3 ShakeDifference { get; set; }

        /// <summary>
        /// Jump to focus
        /// </summary>
        public void JumpToFocus()
        {
            this.Position = this.Focus.Position;
        }

        /// <summary>
        /// 
        /// </summary>
        public void JumpToZoom()
        {
            this.Zoom = this.ZoomTo;
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        internal override void Update(GameTime gameTime)
        {

            Vector3 newPosition = _position;
            Vector2 deltaPosition = Vector2.Zero;

            // Update Position if we follow something
            if (this.Focus != null)
            {
                /*Vector2 focusPos = new Vector2(this.Focus.Position.X, this.Focus.Position.Y + this.Focus.Position.Z);
                Vector2 delta = focusPos - new Vector2(this.Position.X, this.Position.Y);

                delta *= this.MaxFocusSpeed;*/

                newPosition = new Vector3(
                    MathHelper.Lerp(this.Position.X, this.Focus.Position.X, this.MaxFocusSpeed * (Single)gameTime.ElapsedGameTime.TotalSeconds), 
                    MathHelper.Lerp(this.Position.Y, this.Focus.Position.Y + this.Focus.Position.Z, this.MaxFocusSpeed * (Single)gameTime.ElapsedGameTime.TotalSeconds), 
                    0);

                // Clamp if neccecary
                if (_viewableArea != Rectangle.Empty)
                {
                    newPosition.X = MathHelper.Clamp(newPosition.X, _safePositions.Left + (_resolution.X % 32 / this.Zoom), _safePositions.Right - (_resolution.X % 32 / this.Zoom));
                    newPosition.Y = MathHelper.Clamp(newPosition.Y, _safePositions.Top + (_resolution.Y % 32 / this.Zoom), _safePositions.Bottom - (_resolution.Y % 32 / this.Zoom)); 
                }

                deltaPosition.X = newPosition.X - _position.X;
                deltaPosition.Y = newPosition.Y - _position.Y;

                /*if (Math.Abs(deltaPosition.X) < 0.005f)
                {
                    deltaPosition.X = 0;
                    newPosition.X = MathHelper.Clamp(this.Focus.Position.X, _safePositions.Left + (_resolution.X % 32 / this.Zoom), _safePositions.Right - (_resolution.X % 32 / this.Zoom));
                }

                if (Math.Abs(deltaPosition.Y) < 0.005f)
                {
                    deltaPosition.Y = 0;
                    newPosition.Y = MathHelper.Clamp(this.Focus.Position.Y, _safePositions.Top + (_resolution.Y % 32 / this.Zoom), _safePositions.Bottom - (_resolution.Y % 32 / this.Zoom));
                }*/

                // Position is NOT used in draw/render
                this.Position = newPosition;
            }

            Vector3 newShakeOffset = _shakeOffset * (1 - (Single)Math.Min(gameTime.ElapsedGameTime.TotalSeconds * 0.8, 0.8));
            
            // Shake to
            if (this.ShakeAmount > 1)
            {
                Single newShakeAmount = MathHelper.Clamp(this.ShakeAmount * (1 - (Single)Math.Min(gameTime.ElapsedGameTime.TotalSeconds * 2, 0.95)), 1, 675);

                Vector3 delta = Vector3.Zero;
                delta.X = (Single)(2 * _random.NextDouble() - 1) * newShakeAmount;
                delta.Y = (Single)(2 * _random.NextDouble() - 1) * newShakeAmount;
                delta.Z = (Single)(2 * _random.NextDouble() - 1) * newShakeAmount;

                newShakeOffset += delta;
                newShakeOffset *= 1 / 60f; //(Single)gameTime.ElapsedGameTime.TotalSeconds;

                delta.X += deltaPosition.X;
                delta.Y += deltaPosition.Y;
                delta *= this.Zoom / 32;
                 
                this.AddChange(() => { this.ShakeDifference = delta; });

                // ShakeAmount is NOT used in draw/Render
                this.ShakeAmount = newShakeAmount;

                UpdateProjection(newPosition, newShakeOffset);
            }
            else
            {
                Vector3 delta = Vector3.Zero;
                delta.X += deltaPosition.X;
                delta.Y += deltaPosition.Y;
                delta *= this.Zoom / 32;

                this.AddChange(() => { this.ShakeDifference = delta; });
            }

            // Zoom to
            if (this.ZoomTo != this.Zoom)
            {
                Single delta = (this.ZoomTo - this.Zoom) * this.MaxZoomSpeed;
                Single newZoom = this.Zoom + delta * (Single)gameTime.ElapsedGameTime.TotalSeconds;

                if (Math.Abs(delta) < 0.05f)
                    newZoom = this.ZoomTo;

                // Zoom is NOT used in any draw/render
                this.Zoom = newZoom;
            }

            _shakeOffset = newShakeOffset;
                
        }

        /// <summary>
        /// Initializes to default values
        /// </summary>
        internal override void Initialize()
        {
            _resolution = new Point(1280, 720);//736);//720);
            _zoom = 32f;
            _zoomTo = 32f;
            _position = Vector3.Zero;
            _bound = new BoundingBox(new Vector3(-16, -16, 0), new Vector3(16, 16, 0));
            _random = new NetRandom();
            _shakeOffset = Vector3.Zero;

            this.MaxFocusSpeed = 0.6f;
            this.MaxZoomSpeed = 1.5f;
            this.MoveSpeed = 1;
            this.Focus = new PositionFocus(new Vector3(16, 16, 0));
            this.ShakeAmount = 1;
            this.ShakeDifference = Vector3.Zero;

            UpdateBounds();
            UpdateProjection();
        }

        #region Drawable part
        internal override void Draw(GameTime gameTime, bool drawTransparent)
        {
            throw new NotImplementedException();
        }

        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            throw new NotImplementedException();
        }

        internal override void UnloadContent()
        {
            throw new NotImplementedException();
        }
        #endregion

        public Single MaxZoomSpeed { get; set; }
    }
}
