using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Data.Update;
using System.Threading;

namespace ProjectERA.Graphics
{
    internal abstract class DrawableComponent : Changable, IGameComponent
    {
#if DEBUG
        private static Int32 _count = 0;

        /// <summary>
        /// Generated object id
        /// </summary>
        public Int32 Id { get; private set; }
#endif
        /// <summary>
        /// Game Reference
        /// </summary>
        public Game Game
        {
            get;
            protected set;
        }

        /// <summary>
        /// Camera Reference
        /// </summary>
        internal Camera3D Camera
        {
            get;
            set;
        }

        /// <summary>
        /// Has transparent pixels
        /// </summary>
        public Boolean IsSemiTransparant 
        { 
            get; 
            protected set; 
        }

        /// <summary>
        /// Can be removed from sight
        /// </summary>
        public Boolean IsOccludable
        {
            get;
            protected set;
        }

        /// <summary>
        /// Is visible (calls draw)
        /// </summary>
        public Boolean IsVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Is enabled (calls update)
        /// </summary>
        public Boolean IsEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Object Box
        /// </summary>
        public BoundingBox Box
        {
            get;
            protected set;
        }

        /// <summary>
        /// GraphicsDevice Reference
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get;
            protected set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>By default, component is not visible</remarks>
        internal DrawableComponent()
        {
            this.IsEnabled = true;
            this.IsVisible = false;
            this.IsOccludable = true;

#if DEBUG
            this.Id = Interlocked.Increment(ref _count);
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="camera">Camera to bind to</param>
        internal DrawableComponent(Game game, Camera3D camera)
        {
            this.Game = game;
            this.Camera = camera;
            this.GraphicsDevice = this.Game.GraphicsDevice;
            this.IsVisible = true;
            this.IsEnabled = true;
#if DEBUG
            this.Id = Interlocked.Increment(ref _count);
#endif
        }

        /// <summary>
        /// Loads content for this component
        /// </summary>
        /// <param name="contentManager"></param>
        internal abstract void LoadContent(ContentManager contentManager);

        /// <summary>
        /// Unloads content for this componetn
        /// </summary>
        internal abstract void UnloadContent();

        /// <summary>
        /// Initializes component
        /// </summary>
        internal abstract void Initialize();

        /// <summary>
        /// Frame Draw
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal abstract void Draw(GameTime gameTime, Boolean drawTransparent);

        /// <summary>
        /// Frame Update
        /// </summary>
        /// <param name="gameTime"></param>
        internal abstract void Update(GameTime gameTime);

        /// <summary>
        /// Is in view of screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        internal Boolean IsInView(BoundingBox screen)
        {
            if (this.IsVisible && !this.IsOccludable)
                return true;
            return this.IsVisible && screen.Intersects(this.Box);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        void IGameComponent.Initialize()
        {
            this.Initialize();
        }
    }
}
