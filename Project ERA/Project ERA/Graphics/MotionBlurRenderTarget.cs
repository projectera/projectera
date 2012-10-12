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
    internal class MotionBlurRenderTarget : DefaultRendertarget
    {
        Effect _motionBlurFx;
        IndexBuffer _indexBuffer;
        VertexBuffer _vertexBuffer;

        public MotionBlurRenderTarget(Game game, Camera3D camera, Int32 width, Int32 height)
            : base(game, camera, width, height)
        {

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
            _motionBlurFx = contentManager.Load<Effect>("Shaders/MotionBlur").Clone();
            _motionBlurFx.CurrentTechnique = _motionBlurFx.Techniques["Technique1"];
          
            _indexBuffer = new IndexBuffer(this.GraphicsDevice, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.WriteOnly);
            _vertexBuffer = new VertexBuffer(this.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            
            _sb = new SpriteBatch(this.GraphicsDevice);

            Int32[] indices = new Int32[6];
            SetIndices(indices, 0, 0);
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            SetVertexes(vertices);

            _vertexBuffer.SetData<VertexPositionTexture>(vertices);
            _indexBuffer.SetData<Int32>(indices);

            base.LoadContent(contentManager);
        }

        internal override void UnloadContent()
        {
            base.UnloadContent();

            if (_sb != null && !_sb.IsDisposed)
                _sb.Dispose();

            _indexBuffer.Dispose();
            _vertexBuffer.Dispose();
            _motionBlurFx.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="start"></param>
        /// <param name="firstIndice"></param>
        internal void SetIndices(Int32[] arr, Int32 start, Int32 firstIndice)
        {
            Int32[] indices = new Int32[] { 2, 3, 0, 0, 1, 2 };
            for (Int32 i = 0; i < 6; i++)
                arr[start + i] = firstIndice + indices[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="start"></param>
        internal void SetVertexes(VertexPositionTexture[] arr)
        {
            Vector3[] offsets = new Vector3[] { Vector3.Zero, Vector3.Right, Vector3.Right + Vector3.Up, Vector3.Up };
            Vector2[] texOffset = new Vector2[] { Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY };

            for (int i = 0; i < 4; i++)
            {
                arr[i].Position = offsets[i];
                arr[i].TextureCoordinate = texOffset[i];
            }
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

            this.GraphicsDevice.SetVertexBuffer(_vertexBuffer);

            _motionBlurFx.Parameters["screen"].SetValue(_renderTarget);

            Vector2 speed = Vector2.Zero;
            speed.X = this.Camera.ShakeDifference.X / 1280;
            speed.Y = this.Camera.ShakeDifference.Y / 786;
            _motionBlurFx.Parameters["speed"].SetValue(speed);

            _motionBlurFx.Parameters["rotation"].SetValue(this.Camera.ShakeDifference.Z/512);

            _sb.Begin(0, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, _motionBlurFx);

            _sb.Draw(_renderTarget, new Rectangle(-1, 1, 2, -2), Color.White);

            _sb.End();
            
            /*using (System.IO.IsolatedStorage.IsolatedStorageFileStream fs = new System.IO.IsolatedStorage.IsolatedStorageFileStream(@"test.png", System.IO.FileMode.OpenOrCreate))
                _renderTarget.SaveAsPng(fs, 1280, 768);

            foreach (EffectPass pass in be.CurrentTechnique.Passes)//_motionBlurFx.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                GraphicsDevice.Indices = _indexBuffer;

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 8);
            }*/
        }

        internal void Disable()
        {
            //this.IsEnabled = false;
        }
    }
}
