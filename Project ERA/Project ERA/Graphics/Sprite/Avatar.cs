using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Data;
using ProjectERA.Data.Update;
using ProjectERA.Graphics;
using ProjectERA.Services.Display;
using ProjectERA.Data.Enum;

namespace ProjectERA.Graphics.Sprite
{
    internal class Avatar : Interactable, IFocusable
    {

        #region Private fields
        private ContentManager _contentManager;
        private GraphicsDevice _graphicsDevice;
        private TextureManager _textureManager;
        private List<String> _assets;

        private RenderTarget2D _renderTarget;
        #endregion

        /// <summary>
        /// Constructor for the avatar
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="camera">Camera to bind to</param>
        internal Avatar(Game game, ref Camera3D camera, ProjectERA.Data.InteractableAppearance source)
            : base(game, ref camera, source)
        {
            _graphicsDevice = this.Game.GraphicsDevice;
            _assets = new List<String>();

            this.Position = new Vector3(source.MapX, source.MapY - 0.5f, 0);
            this.Direction = (ProjectERA.Data.Enum.Direction)(source.MapDir);
        }

        /// <summary>
        /// Initialize all other objects required
        /// </summary>
        internal override void Initialize()
        {
            _textureManager = (Services.Display.TextureManager)this.Game.Services.GetService(typeof(Services.Display.TextureManager));

            if (_textureManager == null)
                throw new InvalidOperationException("No texture manager.");
        }

        /// <summary>
        /// Load managed resources
        /// </summary>
        /// <param name="contentManager">Content Manager to load from</param>
        internal override void LoadContent(ContentManager contentManager)
        {
            // Save manager
            _contentManager = contentManager;

            // Assets
            _assets.Add(@"Graphics/Characters/Heads/" + ContentDatabase.DefaultHead);
            _assets.Add(@"Graphics/Characters/Equipment/Bottom/stalkers_bottom");
            _assets.Add(@"Graphics/Characters/Equipment/Top/stalkers_top");
            _assets.Add(@"Graphics/Characters/Equipment/Weapon/stick_sword");

#if !DEBUG || SAVEGENERATEDTEXTURES
            Boolean _loaded = false;

            if (_textureManager.TextureExists(this.GetType(), this.GetSpriteStoreData())) 
            {
                _interactableTexture = _textureManager.LoadDynamicTexture(@"Graphics\Characters\Dynamic", this.GetType(), this.GetSpriteStoreData());
                    
                if (_interactableTexture != null)
                    _loaded = true;
            }

            if (!_loaded)
            {
#endif
                // Character base graphic
                _interactableTexture = _textureManager.LoadStaticTexture(@"Graphics\Characters\Bodies\" + ContentDatabase.DefaultBody, _contentManager);
#if !DEBUG || SAVEGENERATEDTEXTURES
            }
#endif

            // Set sourcerect for vertextbuffer
            this.SourceRect = new Rectangle(0, 0, _interactableTexture.Width / 4, _interactableTexture.Height / 4);
                
            // Vertex and Index
            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(AvatarVertex), 20, BufferUsage.WriteOnly);
            _vertices = new AvatarVertex[4];
            _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.WriteOnly);
            Int32[] indices = new Int32[6];
       
            // Store indices on gpu
            SetIndices(indices, 0, 0);
            _indexBuffer.SetData<Int32>(indices);

            // Effect
            _interactableEffect = _contentManager.Load<Effect>(@"Shaders\AvatarShader").Clone();
            _interactableEffect.CurrentTechnique = _interactableEffect.Techniques["Technique1"];

            // Set world matrix and default animation offset
            _interactableEffect.Parameters["World"].SetValue(Matrix.Identity);
            _interactableEffect.Parameters["Projection"].SetValue(this.Camera.Projection);
            _interactableEffect.Parameters["View"].SetValue(Matrix.Identity);
            _interactableEffect.Parameters["frame"].SetValue(this.AnimationFrame);
            _interactableEffect.Parameters["direction"].SetValue(((Byte)this.Direction) / 2 - 1);
            _interactableEffect.Parameters["animOffset"].SetValue(new Vector2(1 / 4f, 1 / 4f));

#if !DEBUG || SAVEGENERATEDTEXTURES
            if (!_loaded)
            {
#endif
                // Load a spritebatch
                using (SpriteBatch sb = new SpriteBatch(_graphicsDevice))
                {
                    // Create a render target
                    _renderTarget = new RenderTarget2D(_graphicsDevice, _interactableTexture.Width, _interactableTexture.Height);

                    // Prepare
                    _graphicsDevice.SetRenderTarget(_renderTarget);
                    _graphicsDevice.Clear(Color.Transparent);

                    // Draw
                    sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    sb.Draw(_interactableTexture, Vector2.Zero, Color.White);
                    foreach (String asset in _assets)
                    {
                        if (asset.Substring(asset.LastIndexOf('/') + 1) != String.Empty)
                            sb.Draw(_textureManager.LoadStaticTexture(asset, _contentManager), Vector2.Zero, Color.White);
                    }
                    sb.End();

                    // Re-set the graphicsdevice and save texture
                    _graphicsDevice.SetRenderTarget(null);
                    _interactableTexture = _renderTarget;

#if !DEBUG || SAVEGENERATEDTEXTURES
                    _textureManager.SaveDynamicTexture(@"Graphics\Characters\Dynamic", _renderTarget, this.GetType(), this.GetSpriteStoreData());

                    // Ensure invalidation doesn't make the character texture disappear
                    _interactableTexture = _textureManager.LoadDynamicTexture(@"Graphics\Characters\Dynamic", this.GetType(), this.GetSpriteStoreData());
#endif
                } 

#if !DEBUG || SAVEGENERATEDTEXTURES
            }
#endif
            // Set texture
            _interactableEffect.Parameters["avatarTexture"].SetValue(_interactableTexture);
        }

        /// <summary>
        /// Unload all unmanaged resources
        /// </summary>
        internal override void UnloadContent()
        {
            if (_renderTarget != null)
                _renderTarget.Dispose();
        }

       
        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of Timing values</param>
        internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Get dynamic storage data
        /// </summary>
        /// <returns></returns>
        internal Object[] GetSpriteStoreData()
        {
            String[] result = new String[_assets.Count + 1];
            
            result[0] = ContentDatabase.DefaultBody;
            
            for (Int32 i = 0; i < _assets.Count; i++)
                result[i + 1] = _assets[i].Substring(_assets[i].LastIndexOf('/') + 1);

            return result;
        }

        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal override void Draw(GameTime gameTime, Boolean drawTransparent)
        {

            // Just draw texture with sourcerect at Position   
            _interactableEffect.Parameters["Projection"].SetValue(this.Camera.Projection);

            // Set Vertex
            SetVertexes(_vertices, 0);
            _vertexBuffer.SetData<AvatarVertex>(_vertices);

            // Draw
            foreach (EffectPass pass in _interactableEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                this.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                this.GraphicsDevice.Indices = _indexBuffer;
                // Always draw transparant
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

        }
    }
}
