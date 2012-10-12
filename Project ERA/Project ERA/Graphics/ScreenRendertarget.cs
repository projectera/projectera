using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectERA.Services.Display;
using System.Threading.Tasks;

namespace ProjectERA.Graphics
{
    /// <summary>
    /// This renders components to the default screen instead of a Texture
    /// </summary>
    internal class ScreenRendertarget : DefaultRendertarget
    {
        /// <summary>
        /// Creates a new ScreenRendertarget
        /// </summary>
        /// <param name="game">The game it belongs to</param>
        /// <param name="camera">The current camera</param>
        public ScreenRendertarget(Game game, Camera3D camera)
            : base(game, camera, 0, 0) {}

        /// <summary>
        /// Initializes the ScreenRendertarget
        /// </summary>
        internal override void Initialize()
        {
            _childRendertargets = new List<DefaultRendertarget>();
            _childComponents = new List<DrawableComponent>();

            this.IsOccludable = false;
            this.IsSemiTransparant = false;
        }

        /// <summary>
        /// Renders the child components to screen
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Render(GameTime gameTime)
        {
            foreach (DefaultRendertarget target in _childRendertargets)
                if (target != null)
                    target.Render(gameTime);

            foreach (DrawableComponent component in _childComponents)
                if (component != null && component.IsVisible && (!component.IsOccludable || component.IsInView(Camera.Bound)))
                    component.Draw(gameTime, false);

            foreach (DefaultRendertarget target in _childRendertargets)
                if (target != null && target.IsVisible && (!target.IsOccludable || target.IsInView(Camera.Bound)))
                    target.Draw(gameTime, false);

            foreach (DrawableComponent component in _childComponents)
                if (component != null &&  component.IsVisible && (!component.IsOccludable || component.IsInView(Camera.Bound)))
                    if (component.IsSemiTransparant)
                        component.Draw(gameTime, true);
            
            foreach (DefaultRendertarget target in _childRendertargets)
                if (target != null && target.IsVisible && (!target.IsOccludable || target.IsInView(Camera.Bound)))
                    if (target.IsSemiTransparant)
                        target.Draw(gameTime, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal void RunExclusiveCycle(GameTime gameTime)
        {
#if NOMULTITHREAD
            throw new PlatformNotSupportedException("Can not run an exclusive cycle if threads are not enabled");
#else
            // Update on new Thread
            Task task = Task.Factory.StartNew(() => { this.Update(gameTime); }, TaskCreationOptions.PreferFairness);

            // Render on Main Thread
            this.Render(gameTime); 

            // Wait for both to finish
            task.Wait();
#endif
        }
    }
}
