using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Graphics;
using ProjectERA.Data.Update;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ProjectERA.Services.Display;
using ERAUtils.Enum;
using ProjectERA.Services.Data;
using ProjectERA.Data;
using ERAUtils;
using ProjectERA.Data.Enum;

namespace ProjectERA.Graphics.Sprite
{
    internal class Interactable : DrawableComponent, IFocusable
    {
        #region Private fields
        protected Data.Interactable _source;

        public Texture2D _interactableTexture; // HACK
        private ContentManager _contentManager;
        private GraphicsDevice _graphicsDevice;

        protected VertexBuffer _vertexBuffer;
        protected InteractableVertex[] _vertices;
        protected IndexBuffer _indexBuffer;

        private Rectangle _sourceRect;
        private Data.Enum.Direction _direction;

        protected Effect _interactableEffect;

        private Byte _animationFrame;
        private Single _animationCount;
        private Single _stopCount;
        private Boolean _bushFlag;

        private List<InteractableAsset> _assets;
        private RenderTarget2D _renderTarget;
        #endregion

        /// <summary>
        /// TextureManager reference
        /// </summary>
        Services.Display.TextureManager _textureManager;

        /// <summary>
        /// 
        /// </summary>
        SpriteBatch _spriteBatch;

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Direction facing
        /// </summary>
        public Direction Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
                if (_interactableEffect != null)
                    _interactableEffect.Parameters["direction"].SetValue(((Byte)this.Direction) / 2 - 1);
            }
        }

        /// <summary>
        /// AnimationCount for this Sprite. 
        /// After a value of 6, frame progresses with 1
        /// </summary>
        public Single AnimationCount
        {
            get
            {
                return _animationCount;
            }
            set
            {
                _animationCount = value;

                if (this.AnimationCount > 6)
                {
                    _animationCount -= 6;

                    // If no stop animation, and stopped
                    if (_source.HasMovement && _source.Movement.StopFrequency == 0 && this.StopCount > 0)
                        // Return to original pattern
                        this.AnimationFrame = 0;
                    // If stop animation is ON when moving
                    else
                        // Update pattern
                        this.AnimationFrame++;
                }
            }
        }

        /// <summary>
        /// Stop Count (for stop animation)
        /// </summary>
        public Single StopCount
        {
            get
            {
                return _stopCount;
            }
            set
            {
                _stopCount = value;
            }
        }

        /// <summary>
        /// Animation frame
        /// </summary>
        public Byte AnimationFrame
        {
            get
            {
                return _animationFrame;
            }
            set
            {
                _animationFrame = (Byte)(value % 4);
                if (_interactableEffect != null)
                    _interactableEffect.Parameters["frame"].SetValue(this.AnimationFrame);
            }
        }

        /// <summary>
        /// Currently moving
        /// </summary>
        public Boolean IsMoving { get; set; }

        /// <summary>
        /// Currently on Bush tile
        /// </summary>
        public Boolean IsBush
        {
            get
            {
                return _bushFlag;
            }
            set
            {
                _bushFlag = value;
                if (_interactableEffect != null)
                    _interactableEffect.Parameters["bush"].SetValue(this.IsBush);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Rectangle SourceRect
        {
            get
            {
                return _sourceRect;
            }
            set
            {
                _sourceRect = value;

                if (_vertices != null)
                    SetVertexes(_vertices, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MongoObjectId SourceId
        {
            get { return _source.Id; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Message> Messages
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor for the interactable
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="camera">Camera to bind to</param>
        internal Interactable(Game game, ref Camera3D camera, Data.Interactable source)
            : base(game, camera)
        {
            _graphicsDevice = this.Game.GraphicsDevice;

            _source = source;

            if (source.HasAppearance)
            {
                Vector3 position = Vector3.Zero;
                position.X = source.MapX;
                position.Y = source.MapY - 0.5f;
                position.Z = 0;
                this.Direction = (Data.Enum.Direction)source.Appearance.MapDir;
                this.AnimationFrame = source.Appearance.AnimationFrame;
                this.Position = position;
                this.SourceRect = Rectangle.Empty;
                this.IsVisible = source.StateFlags.HasFlag(InteractableStateFlags.Visible);
                _assets = new List<InteractableAsset>();
            }

            this.Messages = new List<Message>();

            this.Game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            if (_contentManager != null)
                LoadContent(_contentManager);
        }


        /// <summary>
        /// Initialize all other objects required
        /// </summary>
        internal override void Initialize()
        {
            _textureManager = (Services.Display.TextureManager)this.Game.Services.GetService(typeof(Services.Display.TextureManager));

            if (_textureManager == null)
                throw new InvalidOperationException("No texture manager.");

            ScreenManager screenManager = (Services.Display.ScreenManager)this.Game.Services.GetService(typeof(Services.Display.ScreenManager));

            if (screenManager != null)
                _spriteBatch = screenManager.SpriteBatch;
        }

        /// <summary>
        /// Load managed resources
        /// </summary>
        /// <param name="contentManager">Content Manager to load from</param>
        internal override void LoadContent(ContentManager contentManager)
        {
            // Save manager
            _contentManager = contentManager;

            if (_source.HasAppearance == false)
                return;

            ///// LOAD TEXTURE
            String baseAsset = String.Empty;
            Byte opacity = 255, hue = 0;

            foreach (InteractableBodyPart bodypart in _source.Appearance)
            {
                switch (bodypart.Type)
                {
                    case BodyPart.Asset:
                        if (String.IsNullOrEmpty(baseAsset))
                        {
                            baseAsset = @"Graphics/Characters/" + bodypart.StringValue;
                            opacity = bodypart.Opacity;
                            hue = bodypart.Hue;
                            _assets.Add(new InteractableAsset(baseAsset, bodypart.Opacity, bodypart.Hue));
                        }
                        else
                            _assets.Add(new InteractableAsset(@"Graphics/Characters/" + bodypart.StringValue, bodypart.Opacity, bodypart.Hue));
                        break;
                    case BodyPart.Skin:
                        baseAsset = @"Graphics/Characters/Bodies/" + ContentDatabase.GetBodyAssetName(bodypart.ByteValue ?? 0);
                        opacity = bodypart.Opacity;
                        hue = bodypart.Hue;
                        _assets.Add(new InteractableAsset(@"Graphics/Characters/Bodies/" + ContentDatabase.GetBodyAssetName(bodypart.ByteValue ?? 0), bodypart.Opacity,bodypart.Hue));
                        break;
                    case BodyPart.Hair:
                        _assets.Add(new InteractableAsset(@"Graphics/Characters/Heads/" + ContentDatabase.GetHeadAssetName(bodypart.TupleLeftValue, bodypart.TupleRightValue), bodypart.Opacity,bodypart.Hue));
                        break;
                    case BodyPart.Eyes:
                        // TODO: head shader for eyes?
                        break;
                    case BodyPart.Tile:
                        // TODO: load tileset and tile
                        break;

                    case BodyPart.Weapon:
                        Equipment equipmentWeapon = ContentDatabase.GetEquipmentWeapon(bodypart.IntegerValue ?? ContentDatabase.DefaultWeaponAssetId);
                        if (equipmentWeapon != null && equipmentWeapon.EquipmentAssetName != null)
                            _assets.Add(new InteractableAsset(@"Graphics/Characters/Equipment/Weapon/" + equipmentWeapon.EquipmentAssetName, bodypart.Opacity,bodypart.Hue));
                        break;

                    case BodyPart.Armor:
                        Equipment equipmentArmor = ContentDatabase.GetEquipmentArmor(bodypart.IntegerValue ?? 0);
                        if (equipmentArmor != null && equipmentArmor.EquipmentAssetName != null)
                            _assets.Add(new InteractableAsset(@"Graphics/Characters/Equipment/" + equipmentArmor.Part.ToString() + "/" + equipmentArmor.EquipmentAssetName, (Byte)(bodypart.Opacity), bodypart.Hue));
                        break;

                    case BodyPart.Accessoiry:
                        Equipment equipmentAccessoiry = ContentDatabase.GetEquipmentAccessoiry(bodypart.IntegerValue ?? 0);
                        if (equipmentAccessoiry != null && equipmentAccessoiry.EquipmentAssetName != null)
                            _assets.Add(new InteractableAsset(@"Graphics/Characters/Equipment/Accessoiry/" + equipmentAccessoiry.EquipmentAssetName, bodypart.Opacity,bodypart.Hue));
                        break;

                    case BodyPart.KeyItem:
                        Equipment equipmentKeyItem = ContentDatabase.GetEquipmentKeyItem(bodypart.IntegerValue ?? 0);
                        if (equipmentKeyItem != null && equipmentKeyItem.EquipmentAssetName != null)
                            _assets.Add(new InteractableAsset(@"Graphics/Characters/Equipment/KeyItem/" + equipmentKeyItem.EquipmentAssetName, bodypart.Opacity, bodypart.Hue));
                        break;
                }
            }

#if !DEBUG || SAVEGENERATEDTEXTURES
            Boolean _loaded = false;

            if (_textureManager.TextureExists(this.GetType(), this.GetSpriteStoreData()))
            {
                Boolean error;

                do
                {
                    error = false;

                    try
                    {
                        _interactableTexture = _textureManager.LoadDynamicTexture(@"Graphics\Characters\Dynamic", this.GetType(), this.GetSpriteStoreData());
                    }
                    catch (System.IO.IOException)
                    {
                        error = true;
                        System.Threading.Thread.Sleep(100);
                    }
                } while (error);

                if (_interactableTexture != null)
                    _loaded = true;
            }

            if (!_loaded)
            {
#endif
                // Character base graphic
                _interactableTexture = _textureManager.LoadStaticTexture(String.IsNullOrEmpty(baseAsset) ? @"Graphics\Characters\Bodies\" + ContentDatabase.DefaultBody : baseAsset, _contentManager);
#if !DEBUG || SAVEGENERATEDTEXTURES
            }
#endif

            // Set sourcerect for vertextbuffer
            this.SourceRect = new Rectangle(0, 0, _interactableTexture.Width / 4, _interactableTexture.Height / 4);

            // Vertex and Index
            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(InteractableVertex), 20, BufferUsage.WriteOnly);
            _vertices = new InteractableVertex[4];
            _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.WriteOnly);
            Int32[] indices = new Int32[6];

            // Store indices on gpu
            SetIndices(indices, 0, 0);
            _indexBuffer.SetData<Int32>(indices);

            // Effect
            lock (_contentManager)
            {
                _interactableEffect = _contentManager.Load<Effect>(@"Shaders\InteractableShader").Clone();
                _interactableEffect.CurrentTechnique = _interactableEffect.Techniques["Technique1"];
            }

            Effect assetEffect = null;
            lock (_contentManager)
            {
                assetEffect = _contentManager.Load<Effect>(@"Shaders\InteractableAssetShader").Clone();
                assetEffect.CurrentTechnique = assetEffect.Techniques[0];
            }

            // Set world matrix and default animation offset
            _interactableEffect.Parameters["World"].SetValue(Matrix.Identity);
            _interactableEffect.Parameters["Projection"].SetValue(this.Camera.Projection);
            _interactableEffect.Parameters["View"].SetValue(Matrix.Identity);
            _interactableEffect.Parameters["frame"].SetValue(this.AnimationFrame);
            _interactableEffect.Parameters["direction"].SetValue(((Byte)this.Direction) / 2 - 1);
            _interactableEffect.Parameters["animOffset"].SetValue(new Vector2(1 / 4f, 1 / 4f));
            _interactableEffect.Parameters["bush"].SetValue(false); // HACK

#if !DEBUG || SAVEGENERATEDTEXTURES
            if (!_loaded)
            {
#endif
                // Load a spritebatch
                using (SpriteBatch spriteBatch = new SpriteBatch(_graphicsDevice))
                {
                    // Create a render target
                    _renderTarget = new RenderTarget2D(_graphicsDevice, _interactableTexture.Width, _interactableTexture.Height);

                    // Prepare
                    _graphicsDevice.SetRenderTarget(_renderTarget);
                    _graphicsDevice.Clear(Color.Transparent);

                    // Draw
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, assetEffect);

                    spriteBatch.Draw(_interactableTexture, new Rectangle(0, 0, _interactableTexture.Width, _interactableTexture.Height), new Color(hue / 255f, 0, 0, opacity / 255f));

                    Boolean baseAssetFound = false;
                    foreach (InteractableAsset asset in _assets)
                    {
                        if (baseAsset != null && asset.Asset.Equals(baseAsset))
                        {
                            baseAssetFound = true;
                            continue;
                        }

                        asset.LoadContent(_contentManager, _textureManager);
                        asset.Draw(spriteBatch);
                    }
                    spriteBatch.End();

                    // Re-set the graphicsdevice and save texture
                    _graphicsDevice.SetRenderTarget(null);
                    _interactableTexture = _renderTarget;

#if !DEBUG || SAVEGENERATEDTEXTURES
                    _textureManager.SaveDynamicTexture(@"Graphics\Characters\Dynamic", _renderTarget, this.GetType(), this.GetSpriteStoreData());

                    // Release used static textures
                    foreach (InteractableAsset asset in _assets)
                    {
                        asset.UnloadContent(_textureManager);
                    }

                    if (baseAssetFound == false && baseAsset != null)
                        _textureManager.ReleaseStaticTexture(baseAsset);

                    // Ensure invalidation doesn't make the character texture disappear
                    _interactableTexture = _textureManager.LoadDynamicTexture(@"Graphics\Characters\Dynamic", this.GetType(), this.GetSpriteStoreData());
#endif
                }

#if !DEBUG || SAVEGENERATEDTEXTURES
            }
#endif
            // Set texture
            if (!_interactableTexture.IsDisposed)
                _interactableEffect.Parameters["avatarTexture"].SetValue(_interactableTexture);
        }

        /// <summary>
        /// Set indices
        /// </summary>
        /// <param name="arr">Indice array</param>
        /// <param name="start">Starting index</param>
        /// <param name="firstIndice">Indice offset</param>
        internal void SetIndices(Int32[] arr, Int32 start, Int32 firstIndice)
        {
            Int32[] indices = new Int32[] { 2, 3, 0, 0, 1, 2 };
            for (Int32 i = 0; i < 6; i++)
                arr[start + i] = firstIndice + indices[i];
        }

        /// <summary>
        /// Set vertexes
        /// </summary>
        /// <param name="arr">Vertex array</param>
        /// <param name="start">Starting Index</param>
        internal void SetVertexes(InteractableVertex[] arr, Int32 start)
        {
            Vector3[] offsets = new Vector3[] { 
                new Vector3(0, 0, this.SourceRect.Height / 32f),  // SOURCERECT
                new Vector3(this.SourceRect.Width / 32f, 0, this.SourceRect.Height / 32f), 
                new Vector3(this.SourceRect.Width / 32f, 0, 0), 
                Vector3.Zero };

            Vector2[] texOffset = new Vector2[] { Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY };

            for (Int32 i = 0; i < 4; i++)
            {
                SetVertex(out arr[start + i]);
                arr[start + i].vertexPosition = this.Position + offsets[i] ;
                arr[start + i].texturePos1 += texOffset[i] / 4;
            }
        }

        /// <summary>
        /// Create Vertex
        /// </summary>
        /// <param name="vertex">New Vertex</param>
        private void SetVertex(out InteractableVertex vertex)
        {
            vertex = new InteractableVertex();
        }

        /// <summary>
        /// Get dynamic storage data
        /// </summary>
        /// <returns></returns>
        internal Object[] GetSpriteStoreData()
        {
            String[] result = new String[_assets.Count];

            for (Int32 i = 0; i < _assets.Count; i++)
                result[i] = _assets[i].Asset.Substring(_assets[i].Asset.LastIndexOf('/') + 1) + " [" + _assets[i].Opacity + ", " + _assets[i].Hue + "]";

            return result;
        }

        /// <summary>
        /// Unload all unmanaged resources
        /// </summary>
        internal override void UnloadContent()
        {
            _textureManager.ReleaseDynamicTexture(@"Graphics\Characters\Dynamic", this.GetType(), this.GetSpriteStoreData());
            
            if (_indexBuffer != null && !_indexBuffer.IsDisposed)
                _indexBuffer.Dispose();

            if (_interactableEffect != null && !_interactableEffect.IsDisposed)
                _interactableEffect.Dispose();

            if (_renderTarget != null && !_renderTarget.IsDisposed)
                _renderTarget.Dispose();

            if (_vertexBuffer != null && !_vertexBuffer.IsDisposed)
                _vertexBuffer.Dispose();
        }

        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal override void Draw(GameTime gameTime, Boolean drawTransparent)
        {
            if (_source.HasAppearance == false)
                return;

            // Just draw texture with sourcerect at Position   
            _interactableEffect.Parameters["Projection"].SetValue(this.Camera.Projection);

            // Set Vertex
            SetVertexes(_vertices, 0);
            _vertexBuffer.SetData<InteractableVertex>(_vertices);

            // Draw
            foreach (EffectPass pass in _interactableEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            ;
                this.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                this.GraphicsDevice.Indices = _indexBuffer;
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            // And Draw
            if (this.Messages.Count > 0)
            {
                Vector2 position = new Vector2((this.Position.X - this.Camera.Position.X) * 32 + (1280/2), (this.Position.Y - this.Camera.Position.Y) * 32 + 350 - this.SourceRect.Height);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, Matrix.Identity);
                foreach (Message message in this.Messages)
                {
                    message.Position = position;
                    message.Draw(gameTime, drawTransparent);
                    if (message.IsVisible)
                        position = position - (Vector2.UnitY * 20);
                }
                _spriteBatch.End();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        /// <param name="position"></param>
        internal void DrawAt(GameTime gameTime, Boolean drawTransparent, Vector2 position)
        {
            if (_source.HasAppearance == false)
                return;

            // Just draw texture with sourcerect at Position   
            _interactableEffect.Parameters["Projection"].SetValue(Matrix.Identity);

            Vector3 oldPosition = this.Position * Vector3.One;
            this.Position = new Vector3(position.X, position.Y, oldPosition.Z);

            // Set Vertex
            SetVertexes(_vertices, 0);
            _vertexBuffer.SetData<InteractableVertex>(_vertices);

            // Draw
            foreach (EffectPass pass in _interactableEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                this.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                this.GraphicsDevice.Indices = _indexBuffer;
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            this.Position = oldPosition;
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of Timing values</param>
        internal override void Update(GameTime gameTime)
        {
            Logic.GameInteractable.UpdateSprite(_source, this, gameTime);

            Boolean needRemoving = false;

            // And iterate trough them
            foreach (Message message in this.Messages)
            {
                // Update the loadingpop
                message.Update(gameTime);
                // If it is no longer visible...
                if (message.IsVisible == false)
                {
                    needRemoving = true;
                }
            }

            if (needRemoving)
            {
                // ...remove it from the lists
                this.AddChange(() =>
                {
                    Message[] duplicate = new Message[this.Messages.Count];
                    this.Messages.CopyTo(0, duplicate, 0, duplicate.Length);

                    foreach (Message message in duplicate)
                    {
                        if (!message.IsVisible)
                            this.Messages.Remove(message);
                    }
                });
            }
        }

        /// <summary>
        /// (Deprecated) Draw frame
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        internal void Draw(GameTime gameTime)
        {
            Draw(gameTime, false);
        }

        /// <summary>
        /// 
        /// </summary>
        private class InteractableAsset
        {
            private Texture2D _texture;

            /// <summary>
            /// 
            /// </summary>
            internal String Asset
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            internal Byte Opacity 
            {
                get; 
                private set; 
            }

            /// <summary>
            /// 
            /// </summary>
            internal Byte Hue 
            { 
                get; 
                private set; 
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="asset"></param>
            /// <param name="opacity"></param>
            /// <param name="hue"></param>
            public InteractableAsset(String asset, Byte opacity, Byte hue)
            {
                this.Asset = asset;

                this.Opacity = opacity;
                this.Hue = hue;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="cm"></param>
            /// <param name="tm"></param>
            public void LoadContent(ContentManager cm, TextureManager tm)
            {
                _texture = tm.LoadStaticTexture(Asset,cm);
                
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tm"></param>
            public void UnloadContent(TextureManager tm)
            {
                tm.ReleaseStaticTexture(Asset);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sb"></param>
            internal void Draw(SpriteBatch sb)
            {
                if (this.Asset.Substring(this.Asset.LastIndexOf("/") + 1) != String.Empty)
                {
                    sb.Draw(_texture, Vector2.Zero, new Color(this.Hue / 255f, 0, 0, this.Opacity / 255f)); 
                }
            }
        }
    }
}
