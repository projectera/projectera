using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

namespace Camera
{
    /// <summary>
    /// 
    /// </summary>
    public class Camera3D
    {
        public Game Game { get; protected set; }
        public IFocusable Focus { get; set; }
        public Matrix Projection { get; protected set; }
        public Single MoveSpeed { get; set; }

        protected BoundingBox _bound;
        protected Vector3 _position;
        protected Single _zoom;
        protected Rectangle _safeZone;
        protected Point _resolution;
        protected Single _focusEdge;
        protected Vector3 _shakeOffset;
        protected NetRandom _random;
        protected Rectangle _viewableArea;
        protected Rectangle _safePositions;

        public float ShakeAmount { get; set; }

        /// <summary>
        /// The speed at which the interpolation is done.
        /// </summary>
        public Single MaxFocusSpeed { get; set; }

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
                _safePositions.Inflate((Int32)(-this._resolution.X / this.Zoom / 2), (Int32)(-this._resolution.Y / this.Zoom / 2));
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
                {
                    //this.Position += new Vector2(1024/2, 768/2);
                    _zoom = value;

                }

                _safePositions = _viewableArea;
                _safePositions.Inflate((int)(-this._resolution.X / this.Zoom / 2), (int)(-this._resolution.Y / this.Zoom / 2));

                UpdateProjection();
                UpdateBounds();
            }
        }

        /// <summary>
        /// Update bounds 
        /// </summary>
        protected void UpdateBounds()
        {
            float aspectRatio = _resolution.Y / (float)_resolution.X;
            Int32 range = (Int32)Math.Ceiling(this.Zoom / 2);
            Rectangle rect = new Rectangle((int)this._position.X, (int)this._position.Y, 0, 0);
            rect.Inflate(range, (int)(range * aspectRatio + 0.5));
            _bound = new BoundingBox(new Vector3(rect.Left, rect.Top, 0), new Vector3(rect.Right, rect.Bottom, 0));
        }

        /// <summary>
        /// Update projection when position or zoom changes
        /// </summary>
        protected void UpdateProjection()
        {
            float x = (float)Math.Round((_position.X + _shakeOffset.X) * this.Zoom) / this.Zoom;
            float y = (float)Math.Round((_position.Y + _shakeOffset.Y) * this.Zoom) / this.Zoom;

            Matrix translation = Matrix.CreateTranslation(-x, -y, 0);
            Matrix obliqueProjection = new Matrix( 1,  0,  0,  0,
                                                   0,  1,  1,  0, 
                                                   0, -1,  0,  0, 
                                                   0,  0,  0,  1);

            
            Matrix orthographic = Matrix.CreateOrthographicOffCenter(-_resolution.X / this.Zoom / 2, _resolution.X / this.Zoom / 2, _resolution.Y / this.Zoom / 2, -_resolution.Y / this.Zoom / 2, -10000, 10000);
            Matrix shakeRotation = Matrix.CreateRotationZ(_shakeOffset.Z > 0.01?_shakeOffset.Z / 20:0);
            this.Projection = translation * obliqueProjection * orthographic * shakeRotation;
            
        }

        /// <summary>
        /// Jump to focus
        /// </summary>
        public void JumpToFocus()
        {
            this.Position = this.Focus.Position;
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            // Update Position if we follow something
            if (this.Focus != null)
            {
                Vector2 focusPos = new Vector2(Focus.Position.X, Focus.Position.Y + Focus.Position.Z);
                Vector2 delta = focusPos - new Vector2(this.Position.X, this.Position.Y);

                delta *= this.MaxFocusSpeed;

                Vector3 newPosition = new Vector3(this.Position.X + delta.X * (float)gameTime.ElapsedGameTime.TotalSeconds, this.Position.Y + delta.Y * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);

                // Clamp if neccecary
                if (_viewableArea != Rectangle.Empty)
                {
                    newPosition.X = MathHelper.Clamp(newPosition.X, _safePositions.Left, _safePositions.Right);
                    newPosition.Y = MathHelper.Clamp(newPosition.Y, _safePositions.Top, _safePositions.Bottom);
                }

                
                this.Position = newPosition;
            }

            _shakeOffset *= 1 - (Single)Math.Min(gameTime.ElapsedGameTime.TotalSeconds * 0.8, 0.8);
            if (ShakeAmount > 1)
            {
                ShakeAmount *= 1 - (Single)Math.Min(gameTime.ElapsedGameTime.TotalSeconds * 2, 0.95);
                _shakeOffset = new Vector3(_shakeOffset.X + (float)(2 * _random.NextDouble() - 1) * ShakeAmount, _shakeOffset.Y + (float)(2 * _random.NextDouble() - 1) * ShakeAmount, _shakeOffset.Z + (float)(2 * _random.NextDouble() - 1) * ShakeAmount);
                _shakeOffset *= (Single)gameTime.ElapsedGameTime.TotalSeconds;
                
                UpdateProjection();
            }

        }

        /// <summary>
        /// Initializes to default values
        /// </summary>
        public void Initialize()
        {
            _resolution = new Point(1024, 768);
            _zoom = 32f;
            _position = Vector3.Zero;
            _bound = new BoundingBox(new Vector3(-16, -16, 0), new Vector3(16, 16, 0));
            _random = new NetRandom();
            _shakeOffset = Vector3.Zero;

            this.MaxFocusSpeed = 0.6f;
            this.MoveSpeed = 1;
            this.Focus = new PositionFocus(new Vector3(16, 16, 0));
            this.ShakeAmount = 1;

            UpdateBounds();
            UpdateProjection();
        }
    }
}
