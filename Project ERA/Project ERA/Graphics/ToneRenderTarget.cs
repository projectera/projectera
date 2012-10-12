using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ProjectERA.Services.Display;

namespace ProjectERA.Graphics
{
    internal class ToneRenderTarget : DefaultRendertarget
    {
        private Effect _toneFx;
        private Vector4 _tone;
        
        /// <summary>
        /// 
        /// </summary>
        public Vector4 Tone
        {
            get { return _tone; }
            set 
            { 
                _tone = value;
                if (_toneFx != null)
                {
                    _toneFx.Parameters["Tone"].SetValue(_tone);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="camera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ToneRenderTarget(Game game, Camera3D camera, Int32 width, Int32 height)
            : base(game, camera, width, height)
        {
            this.Tone = Color.Transparent.ToVector4();
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Initialize()
        {
            base.Initialize();
        }

        SpriteBatch _sb;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(ContentManager contentManager)
        {
            _toneFx = contentManager.Load<Effect>("Shaders/Tone").Clone();
            _toneFx.CurrentTechnique = _toneFx.Techniques["Technique1"];
            this.Tone = this.Tone; // set in effect

            _sb = new SpriteBatch(this.GraphicsDevice);

            base.LoadContent(contentManager);
        }

        internal override void UnloadContent()
        {
            _toneFx.Dispose();
            _sb.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal override void Draw(GameTime gameTime, bool drawTransparent)
        {
            if (drawTransparent)
                return;
            this.Tone = this.Tone;
            _sb.Begin(0, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, _toneFx);

            _sb.Draw(_renderTarget, new Rectangle(-1, 1, 2, -2), Color.White);

            _sb.End();
        }

        internal void Disable()
        {
            //this.IsEnabled = false;
        }
    }
}
