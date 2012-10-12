using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Display;
using System.Threading.Tasks;

namespace ProjectERA.Graphics
{
    /// <summary> 
    /// A base class for rendering to a RenderTarget2D // TODO REMOVE LOCKS
    /// </summary>
    internal class DefaultRendertarget : DrawableComponent
    {
        /// <summary>
        /// The width of the RenderTarget2D
        /// </summary>
        public Int32 Width { get; protected set; }
        /// <summary>
        /// The height of the RenderTarget2D
        /// </summary>
        public Int32 Height { get; protected set; }

        protected List<DefaultRendertarget> _childRendertargets;
        protected List<DrawableComponent> _childComponents;
        protected RenderTarget2D _renderTarget;

        /// <summary>
        /// Creates a new DefaultRendertarget with a width and a height.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="camera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public DefaultRendertarget(Game game, Camera3D camera, Int32 width, Int32 height)
            : base(game, camera)
        {
           this.Width = width;
           this.Height = height;
        }

        /// <summary>
        /// Adds a DrawableComponent to be drawn on this RenderTarget
        /// </summary>
        /// <param name="component">The component to be added</param>
        internal void AddComponent(DrawableComponent component)
        {
            lock (_childComponents)
            {
                _childComponents.Add(component);
            }
        }

        /// <summary>
        /// Removes a DrawableComponent from the draw list
        /// </summary>
        /// <param name="component">The component to be removed</param>
        internal void RemoveComponent(DrawableComponent component)
        {
            lock (_childComponents)
            {
                _childComponents.Remove(component);
            }
        }

        /// <summary>
        /// Adds a new DefaultRendertarget to be drawn on this RenderTarget
        /// </summary>
        /// <param name="target">The DefaultRendertarget to be added</param>
        internal void AddRendertarget(DefaultRendertarget target)
        {
            lock (_childRendertargets)
            {
                _childRendertargets.Add(target);
            }
        }

        /// <summary>
        /// Removes a DefaultRendertarget from the draw list
        /// </summary>
        /// <param name="target">The DefaultRendertarget to be removed</param>
        internal void RemoveRendertarget(DefaultRendertarget target)
        {
            lock (_childRendertargets)
            {
                _childRendertargets.Remove(target);
            }
        }

        /// <summary>
        /// Initalizes the DefaultRendertarget
        /// </summary>
        internal override void Initialize()
        {
            _renderTarget = new RenderTarget2D(this.GraphicsDevice, this.Width, this.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);
            _childRendertargets = new List<DefaultRendertarget>();
            _childComponents = new List<DrawableComponent>();

            this.IsOccludable = false;
            this.IsSemiTransparant = false;

            this.IsVisible = true;
            this.IsEnabled = true;
        }

        /// <summary>
        /// Loads content for all child components
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            foreach (DefaultRendertarget target in _childRendertargets)
                target.LoadContent(contentManager);

            foreach (DrawableComponent component in _childComponents)
                component.LoadContent(contentManager);
        }

        /// <summary>
        /// Unloads content for all child nodes
        /// </summary>
        internal override void UnloadContent()
        {
            foreach (DefaultRendertarget target in _childRendertargets)
                target.UnloadContent();
  
            foreach (DrawableComponent component in _childComponents)
                component.UnloadContent();

            // Dispose rendertarget
            if (_renderTarget != null && !_renderTarget.IsDisposed)
                _renderTarget.Dispose();

            if (rt_batch != null && !rt_batch.IsDisposed)
                rt_batch.Dispose();
        }

        /// <summary>
        /// Renders the components to the Rendertarget
        /// </summary>
        /// <param name="gameTime"></param>
        internal virtual void Render(GameTime gameTime)
        {
            foreach (DefaultRendertarget target in _childRendertargets)
                target.Render(gameTime);

            this.GraphicsDevice.SetRenderTarget(_renderTarget);

            this.GraphicsDevice.Clear(Color.Transparent);

            foreach (DrawableComponent component in _childComponents)
                if (component.IsVisible && (!component.IsOccludable || component.IsInView(Camera.Bound)))
                    component.Draw(gameTime, false);

            foreach (DefaultRendertarget target in _childRendertargets)
                if (target.IsVisible && (!target.IsOccludable || target.IsInView(Camera.Bound)))
                    target.Draw(gameTime, false);

            foreach (DrawableComponent component in _childComponents)
                if (component.IsVisible && (!component.IsOccludable || component.IsInView(Camera.Bound)))
                    if (component.IsSemiTransparant)
                        component.Draw(gameTime, true);

            foreach (DefaultRendertarget target in _childRendertargets)
                if (target.IsVisible && (!target.IsOccludable || target.IsInView(Camera.Bound)))
                    if (target.IsSemiTransparant)
                        target.Draw(gameTime, true);
        
            this.GraphicsDevice.SetRenderTarget(null);
        }

        SpriteBatch rt_batch;

        /// <summary>
        /// Draws the current Rendertarget
        /// </summary>
        /// <param name="gameTime">The elapsed time</param>
        /// <param name="drawTransparent">True if semi-transparent pixels should be drawn</param>
        internal override void Draw(GameTime gameTime, bool drawTransparent)
        {
            rt_batch = rt_batch ?? new SpriteBatch(this.Game.GraphicsDevice);
            rt_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            rt_batch.Draw(_renderTarget, _renderTarget.Bounds, Color.White);
            rt_batch.End();
        }

        /// <summary>
        /// Updates all child components
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Update(GameTime gameTime)
        {
            Task[] tasks = new Task[_childRendertargets.Count + _childComponents.Count];

            Int32 i = 0;

            foreach (DefaultRendertarget target in _childRendertargets)
            {
                DefaultRendertarget threadSafeTarget = target;
                tasks[i++] = Task.Factory.StartNew(() => { if (threadSafeTarget != null && threadSafeTarget.IsEnabled) threadSafeTarget.Update(gameTime); }, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent);
            }
 
            foreach (DrawableComponent component in _childComponents)
            {
                DrawableComponent threadSafeComponent = component;
                tasks[i++] = Task.Factory.StartNew(() => { if (threadSafeComponent != null && threadSafeComponent.IsEnabled) threadSafeComponent.Update(gameTime); }, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent);
            }

            //Task.WaitAll(tasks);
        }
    }
}
