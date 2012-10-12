using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectERA.Services.Data;
using ProjectERA.Graphics;
using ProjectERA.Services.Display;
using ERAUtils.Logger;
using ProjectERA.Services.Data.Storage;
using ERAUtils;

namespace ProjectERA.Graphics.Sprite
{
    internal class TileMap : DrawableComponent
    {

        #region Private fields
        private MongoObjectId _mapId;
        private MapData _lastMapData;

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Int32 _normalCount;
        private Int32 _transparentCount;

        private Effect _splattedTileEffect;
        private RasterizerState _rasterizerState;
        private DepthStencilState _solidDepthState;
        private DepthStencilState _alphaDepthState;

        private Texture2D _panoramaTexture;
        private Texture2D _tilesetTexture;
        private Texture2D[] _autotileTextures;
        private Texture2D _generatedTexture;
        private Texture2D _fogTexture;

        private String _tilesetAssetName;
        private String[] _autotileAssetNames;

        private ContentManager _contentManager;
        private TextureManager _textureManager;
        private Boolean _skipLoading;
        private Vector2 _cameraPosition = Vector2.Zero;

        private Int32 _width, _heigth;
        #endregion

        #region Properties
        /// <summary>
        /// Tileset Texture
        /// </summary>
        internal Texture2D TilesetTexture
        {
            set { _tilesetTexture = value; }
        }

        /// <summary>
        /// Map Width
        /// </summary>
        internal Int32 Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Map Height
        /// </summary>
        internal Int32 Height
        {
            get { return _heigth; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal ContentManager ContentManager
        {
            get { return _contentManager; }
            set { _contentManager = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="mapData">Map Data to use</param>
        internal TileMap(Game game, MapData mapData)
            : base(game, null)
        {
            _mapId = mapData.MapId;
            _width = mapData.Width;
            _heigth = mapData.Height;
            _tilesetAssetName = mapData.TilesetData.AssetName;
            _autotileAssetNames = mapData.TilesetData.AutotileAssetNames.ToArray();

            this.Game.GraphicsDevice.DeviceLost += new EventHandler<EventArgs>(GraphicsDevice_DeviceLost);
            this.Game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            if (_lastMapData != null)
            {
                this.AddChange(() =>
                {
                    _generatedTexture.Dispose();
                    _generatedTexture = null;
                    CreateTileMap(_lastMapData);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GraphicsDevice_DeviceLost(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Initialize Tilemap
        /// </summary>
        internal override void Initialize()
        {
            _textureManager = (TextureManager)this.Game.Services.GetService(typeof(TextureManager));
            if (_textureManager == null)
                throw new InvalidOperationException("No texture manager.");

            _rasterizerState = new RasterizerState();
            _rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            
            _solidDepthState = new DepthStencilState();
            _solidDepthState.DepthBufferEnable = true;
            _solidDepthState.DepthBufferWriteEnable = true;

            _alphaDepthState = new DepthStencilState();
            _alphaDepthState.DepthBufferEnable = true;
            _alphaDepthState.DepthBufferWriteEnable = false;

            this.IsOccludable = false;
            this.IsSemiTransparant = true;

            this.IsVisible = true;
        }

        /// <summary>
        /// Loads all content for this GameComponent
        /// </summary>
        internal void LoadContent(MapData mapData)
        {
            if (!_skipLoading)
            {
                // CONTENT LOADING
                // For each Tilemap a seperate Contentmanager is created, so that
                // when a tilemap needs disposing, the contents of the contentmanager
                // can safely be purged without risking to lose other content still o
                // screen.

                _skipLoading = true;
                _contentManager = new ContentManager(this.Game.Services, "Content");

                // Load required graphics
                if (_tilesetAssetName != null && _tilesetTexture == null)
                    _tilesetTexture = _textureManager.LoadStaticTexture(@"Graphics\Tilesets\" + _tilesetAssetName, _contentManager);
                   
                _autotileTextures = new Texture2D[_autotileAssetNames.Length];
                for (Byte i_pointer = 0; i_pointer < _autotileAssetNames.Length; i_pointer++)
                {
                    _autotileTextures[i_pointer] = _textureManager.LoadStaticTexture(@"Graphics\Autotiles\" + _autotileAssetNames[i_pointer], _contentManager);
                    mapData.TilesetData.AutotileAnimationFlags.Add(_autotileTextures[i_pointer].Width > 128);
                }

                // Additional Textures only if required
                if (mapData.MapSettings != null)
                {
                    if (mapData.MapSettings.FogAssetName != null)
                        _fogTexture = _textureManager.LoadStaticTexture(@"Graphics\Fogs\" + mapData.MapSettings.FogAssetName, _contentManager);

                    if (mapData.MapSettings.PanormaAssetName != null)
                        _panoramaTexture = _textureManager.LoadStaticTexture(@"Graphics\Panormas\" + mapData.MapSettings.PanormaAssetName, _contentManager);
                }

#if !DEBUG || SAVEGENERATEDTEXTURES
                if (_textureManager.TextureExists(this.GetType(), _autotileAssetNames.ToArray()))
                {
                    _generatedTexture = _textureManager.LoadDynamicTexture(@"Graphics\Tilesets\Dynamic", this.GetType(), _autotileAssetNames.ToArray());
                }
#endif

                // Load Effect
                _splattedTileEffect = _contentManager.Load<Effect>(@"Shaders\SplattedTileShader");

                // Set world matrix and default animation offset
                _splattedTileEffect.Parameters["World"].SetValue(Matrix.Identity);
                _splattedTileEffect.Parameters["animOffset"].SetValue(new Vector2(1 / 24f, 1 / 56f));

                // Final Projection (with half pixel offset
                _splattedTileEffect.Parameters["Projection"].SetValue(this.Camera.Projection);
            }

            //base.LoadContent();
        }

        /// <summary>
        /// Creates the tilemap
        /// </summary>
        internal void CreateTileMap(MapData mapData)
        {
            // Sets
            _lastMapData = mapData;

            // Generates autotile texture
            if (_generatedTexture == null)
                this.GenerateAutotileTexture(mapData);

            // Calculate Flags
            if (mapData.TilesetData.NeedComputation)
                this.ComputeOpaquenessTiles(mapData);

            // Save tilesets
            _splattedTileEffect.Parameters["staticTiles"].SetValue(_tilesetTexture);
            _splattedTileEffect.Parameters["autoTiles"].SetValue(_generatedTexture);

            // Create tiles
            List<SplattedTile> normalTiles = new List<SplattedTile>();
            List<SplattedTile> transparentTiles = new List<SplattedTile>();

            for (UInt16 x = 0; x < _width;  x++)
            {
                for (UInt16 y = 0; y < _heigth; y++)
                {
                    SplattedTile currentQuad = null;
                    Int32 currentHeight = -100;

                    for (Byte l = 0; l < MapData.Layers; l++)
                    {
                        // Check if tileId is invalid
                        if (mapData.TileData[x][y][l] == 0)
                            continue;

                        UInt16 tileId = mapData.TileData[x][y][l];
                        Byte priority = mapData.TilesetData.Priorities[tileId];
                        if (priority > currentHeight)
                        {
                            currentHeight = priority;
                            if (currentQuad != null)
                            {
                                if (currentQuad.IsSemiTransparent)
                                    transparentTiles.Add(currentQuad);
                                else
                                    normalTiles.Add(currentQuad);
                            }

                            Vector3 position = Vector3.Zero;
                            position.X = x;
                            position.Y = y;
                            position.Z = priority;
                            currentQuad = new SplattedTile(position, priority != 0);
                        }

                        Tile currentTile = new Tile(tileId, mapData.TilesetData);

                        currentQuad.AddTile(currentTile);
                    }

                    if (currentQuad != null)
                    {
                        if (currentQuad.IsSemiTransparent)
                            transparentTiles.Add(currentQuad);
                        else
                            normalTiles.Add(currentQuad);
                    }
                }
            }

            // Create Vertices and indices
            _vertexBuffer = new VertexBuffer(this.GraphicsDevice, typeof(SplattedTileVertex), normalTiles.Count * 4 + transparentTiles.Count * 4, BufferUsage.WriteOnly);
            SplattedTileVertex[] vertices = new SplattedTileVertex[normalTiles.Count * 4 + transparentTiles.Count * 4];
            _indexBuffer = new IndexBuffer(this.GraphicsDevice, IndexElementSize.ThirtyTwoBits, normalTiles.Count * 6 + transparentTiles.Count * 6, BufferUsage.WriteOnly);
            Int32[] indices = new Int32[normalTiles.Count * 6 + transparentTiles.Count * 6];

            Int32 pos = 0;
            foreach (SplattedTile sp in normalTiles)
            {
                sp.SetVertexes(vertices, pos * 4);
                sp.SetIndices(indices, pos * 6, pos * 4);
                pos++;
            }

            _normalCount = pos;

            foreach (SplattedTile sp in transparentTiles)
            {
                sp.SetVertexes(vertices, pos * 4);
                sp.SetIndices(indices, pos * 6, pos * 4);
                pos++;
            }

            _transparentCount = pos - _normalCount + 1;

            // Store vertices and indices on gpu
            _vertexBuffer.SetData<SplattedTileVertex>(vertices);
            _indexBuffer.SetData<Int32>(indices);
        }

        /// <summary>
        /// Generates autotile texture for this map
        /// </summary>
        internal void GenerateAutotileTexture(MapData mapData)
        {
            // GENERATION OF LARGE AUTOTILETEXTURE
            // Second  all the generated textures are drawn to a specific texture 
            // which is used to draw the autotiles later on

            // Create a spritebatch
            using (SpriteBatch sb = new SpriteBatch(this.Game.GraphicsDevice))
            {
                // Create the texture target
                RenderTarget2D rt = new RenderTarget2D(sb.GraphicsDevice, 768, 1792, false, SurfaceFormat.Color, DepthFormat.None);
                sb.GraphicsDevice.SetRenderTarget(rt);

                // Start spritebatch
                sb.Begin();
                GraphicsDevice.Clear(Color.Transparent);

#if !DEBUG || SAVEGENERATEDTEXTURES
                Boolean _loaded = false;

                if (_generatedTexture != null)
                {
                    sb.Draw(_generatedTexture, new Rectangle(0, 0, 768, 1792), Color.White);
                    _loaded = true;
                }
                else
                {
#endif
                    Logger.Info(new String[] { "Generating Graphics.Sprite.Tilemap texture (t:", mapData.TilesetId.ToString(), ")" });
                    
                    // Draw all tiles
                    for (Int32 i = 0; i < (384 - 48); i++)
                    {
                        // Skip ids if no texture
                        if ((i / 48) >= _autotileTextures.Length || _autotileTextures[i / 48] == null)
                            continue;

                        // Get texture
                        Texture2D texture = _autotileTextures[i / 48];
                        Byte frames = (Byte)Math.Min(4, (texture.Width / (texture.Height == 32 ? 32 : 96)));

                        // If autotile single tile animated (like single flower patches)
                        if (texture.Height == 32)
                        {
                            for (Int32 f = 0; f < frames; f++)
                            {
                                // Create a rect for each frame and draw it 
                                Rectangle srcRect = new Rectangle(f * 32, 0, 32, 32);
                                sb.Draw(texture, new Rectangle((i / 56) * (32 * 4) + srcRect.X, ((i % 56) * 32), 32, 32), srcRect, Color.White);
                            }
                        }
                        else
                        {
                            for (Int32 f = 0; f < frames; f++)
                            {
                                // Get the tiles from the Autotiles Array (tile_id corresponds
                                // to the autotile graphic, so left_top is different to center
                                Byte[] tiles = new Byte[4];
                                for (Int32 k = 0; k < 4; k++)
                                {
                                    tiles[k] = Tile.AutoTileIndex[((i + 48) % 48) >> 3, ((i + 48) % 48) & 7, k];
                                }

                                // Draw 16 by 16 combinations
                                for (Int32 j = 0; j < 4; j++)
                                {
                                    // Get tilposition (to get from the graphic)
                                    Byte tilePosition = (Byte)(tiles[j] - 1);
                                    Rectangle srcRect = new Rectangle(tilePosition % 6 * 16 + f * 96, tilePosition / 6 * 16, 16, 16);

                                    // Draw to rendertarget
                                    sb.Draw(texture, new Rectangle((i / 56) * (32 * 4) + j % 2 * 16 + 32 * f,
                                        ((i % 56) * 32) + j / 2 * 16, 16, 16), srcRect, Color.White);
                                }
                            }
                        }
                    }
#if !DEBUG || SAVEGENERATEDTEXTURES
                }
#endif
                // Flush to rendertarget
                sb.End();

                // Reset device
                sb.GraphicsDevice.SetRenderTarget(null);

                // Save generated texture in memory
                _generatedTexture = rt;

#if !DEBUG || SAVEGENERATEDTEXTURES
                if (!_loaded)
                {
                    _textureManager.SaveDynamicTexture(@"Graphics\Tilesets\Dynamic", rt, this.GetType(), _autotileAssetNames.ToArray());
                }

                // Load so at invalidation texture is not lost
                _generatedTexture = _textureManager.LoadDynamicTexture(@"Graphics\Tilesets\Dynamic", this.GetType(), _autotileAssetNames.ToArray());
#else
                _textureManager.SaveDynamicTexture(@"Graphics\Tilesets\Dynamic", rt, this.GetType(), _autotileAssetNames.ToArray());
                _generatedTexture = _textureManager.LoadDynamicTexture(@"Graphics\Tilesets\Dynamic", this.GetType(), _autotileAssetNames.ToArray());
                _textureManager.DeleteDynamicTexture(@"Graphics\Tilesets\Dynamic", this.GetType(), _autotileAssetNames.ToArray());
#endif
            }
        }

        /// <summary>
        /// Computes the opaqueness for all tiles
        /// </summary>
        internal void ComputeOpaquenessTiles(MapData mapData)
        {
            // COMPUTE OPAQUENESS TILES
            // To compute these boolean values, one for the tile being
            // completely opaque, one for the tile having some transparant
            // pixels, the color data is iterated from both autotile texture
            // as tileset texture. On animated tiles, the complete animation
            // is looked up.
            ERAUtils.Logger.Logger.Info(new String[] { "Computing Graphics.Sprite.Tilemap opaqueness of tiles (i:", mapData.TilesetId.ToString(), ")" });

            // First do the autotiles
            Color[] colors1D = new Color[_generatedTexture.Width * _generatedTexture.Height];
            _generatedTexture.GetData(colors1D);

            // Get animation flags
            Boolean[] animationFlags = new Boolean[7];
            mapData.TilesetData.AutotileAnimationFlags.ToArray().CopyTo(animationFlags, 0);

            for (int i = 48; i < 384; i++)
            {
                Boolean isOpaque = true;
                Boolean hasSemiTransparant = false;

                // If no more textures, stop looking up autotiles
                if (_autotileTextures.Length < (i / 48 - 1))
                    break;

                // Get lookup width
                Int32 width = animationFlags[i / 48 - 1] ? 4 * 32 : 32;

                // Color check
                for (int j = (i - 48) / 56 * (32 * 4); j < (i - 48) / 56 * (32 * 4) + width; j++)
                {
                    for (int k = (i - 48) % 56 * 32; k < (i - 48) % 56 * 32 + 32; k++)
                    {
                        // Flip switch if some pixel is not opaque
                        if (isOpaque && colors1D[j + k * 768].A != 255)
                            isOpaque = false;

                        // Flip switch if some pixel is semitransparant
                        if (!hasSemiTransparant && colors1D[j + k * 768].A > 0 && colors1D[j + k * 768].A < 255)
                            hasSemiTransparant = true;

                        // If both switches where flipped, stop looking
                        if (hasSemiTransparant && !isOpaque)
                            break;
                    }

                    // If both switches where flipped, stop looking
                    if (hasSemiTransparant && !isOpaque)
                        break;
                }

                // Save switch values
                mapData.TilesetData.OpaqueTiles[i] = isOpaque;
                mapData.TilesetData.SomeSemiTransparantTiles[i] = hasSemiTransparant;
            }

            // Now for the tileset
            colors1D = new Color[_tilesetTexture.Width * _tilesetTexture.Height];
            _tilesetTexture.GetData(colors1D);

    

            // Iterate through all tile ids
            for (int i = 0; i < Math.Min(mapData.TilesetData.Passages.Length - 384, _tilesetTexture.Width / 32 * _tilesetTexture.Height / 32); i++)
            {
                Boolean isOpaque = true;
                Boolean hasSemiTransparant = false;
                
#if DEBUG
                Int32 a = 0;
                Color[] colorsHere = new Color[32 * 32];
#endif

                for (int j = ((i % 8) * 32 + (i / 512) * 256); j < ((i % 8) * 32 + (i / 512) * 256 + 32); j++)
                {
                    for (int k = (i % 512) / 8 * 32; j + k * _tilesetTexture.Width < colors1D.Length && k < (i % 512) / 8 * 32 + 32; k++)// k++) // k < ... + 32
                    {
                        if (isOpaque && colors1D[j + k * _tilesetTexture.Width].A != 255)
                            isOpaque = false;

                        if (!hasSemiTransparant && colors1D[j + k * _tilesetTexture.Width].A > 0 && colors1D[j + k * _tilesetTexture.Width].A < 255)
                            hasSemiTransparant = true;

                        //if (hasSemiTransparant && !isOpaque)
                        //    break;
#if DEBUG
                        colorsHere[a++] =  colors1D[j + k * _tilesetTexture.Width];
#endif
                    }

                    //if (hasSemiTransparant && !isOpaque)
                    //    break;

                }

                // Save Data
                mapData.TilesetData.OpaqueTiles[i + 384] = isOpaque;
                mapData.TilesetData.SomeSemiTransparantTiles[i + 384] = hasSemiTransparant;

                //RenderTarget2D rt = new RenderTarget2D(this.GraphicsDevice, 32, 32);
                //rt.SetData<Color>(colorsHere);
                //_textureManager.SaveStaticTexure(@"Graphics\Tilesets\DEBUG_" + i.ToString() + (isOpaque ? "-O" : "") + (hasSemiTransparant ? "-h" : ""), rt);
            }

#if !DEBUG || SAVEGENERATEDNUMBERS
            FileManager fileManager = (FileManager)this.Game.Services.GetService(typeof(FileManager));
            mapData.TilesetData.Save(fileManager);
            ERAUtils.Logger.Logger.Info(new String[] { "Saved Graphics.Sprite.Tilemap computed opaqueness of tiles (i:", mapData.TilesetId.ToString(), ")" });
#endif
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal void Draw(GameTime gameTime)
        {
            //this.Draw(gameTime, false);
            //this.Draw(gameTime, true);
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        internal override void LoadContent(ContentManager contentManager)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void UnloadContent()
        {
            lock (_contentManager)
            {
                _contentManager.Unload();
                _contentManager.Dispose();
                _skipLoading = false;

                if (_tilesetTexture != null)
                    _textureManager.ReleaseStaticTexture(@"Graphics\Tilesets\" + _tilesetAssetName);

                if (_generatedTexture != null)
                    _textureManager.ReleaseDynamicTexture(@"Graphics\Tilesets\Dynamic", this.GetType(), _autotileAssetNames.ToArray());

                for (Byte i_pointer = 0; i_pointer < _autotileAssetNames.Length; i_pointer++)
                {
                     if (_autotileTextures != null && _autotileTextures[i_pointer] != null)
                        _textureManager.ReleaseStaticTexture(@"Graphics\Autotiles\" + _autotileAssetNames[i_pointer]);
                }

                if (_fogTexture != null && String.IsNullOrWhiteSpace(_fogTexture.Name) == false)
                    _textureManager.ReleaseStaticTexture(@"Graphics\Fogs\" + _fogTexture.Name);

                if (_panoramaTexture != null && String.IsNullOrWhiteSpace(_panoramaTexture.Name) == false)
                    _textureManager.ReleaseStaticTexture(@"Graphics\Panormas\" + _panoramaTexture.Name);

                _indexBuffer.Dispose();
                _vertexBuffer.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="drawTransparent"></param>
        internal override void Draw(GameTime gameTime, Boolean drawTransparent)
        {
            // Set graphicsdevice
            GraphicsDevice.RasterizerState = _rasterizerState;

            if (!drawTransparent)
            {
                GraphicsDevice.DepthStencilState = _solidDepthState;

                // Set current view
                _splattedTileEffect.Parameters["View"].SetValue(Matrix.Identity);
                _splattedTileEffect.Parameters["Projection"].SetValue(this.Camera.Projection);

                // Set current animation parameters
                _splattedTileEffect.Parameters["frame"].SetValue(gameTime.TotalGameTime.Seconds % 4);
                _splattedTileEffect.Parameters["nextframe"].SetValue((gameTime.TotalGameTime.Seconds + 1) % 4);
                Single blend = (Single)(gameTime.TotalGameTime.TotalSeconds - Math.Floor(gameTime.TotalGameTime.TotalSeconds));
                _splattedTileEffect.Parameters["frameBlend"].SetValue(blend);

                _splattedTileEffect.CurrentTechnique = _splattedTileEffect.Techniques["Solid"];
            }
            else
            {
                GraphicsDevice.DepthStencilState = _alphaDepthState;

                _splattedTileEffect.CurrentTechnique = _splattedTileEffect.Techniques["Alpha"];
            }

            // Draw passes
            foreach (EffectPass pass in _splattedTileEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                GraphicsDevice.Indices = _indexBuffer;
                
                if (!drawTransparent)
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (_normalCount + _transparentCount) * 4, 0, _normalCount * 2);
                else
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (_normalCount + _transparentCount) * 4, _normalCount * 6, _transparentCount * 2);
            }

           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p_2"></param>
        /// <param name="p_4"></param>
        /// <param name="p_3"></param>
        internal void SetTile(int p, int p_2, int p_4, int p_3)
        {
            Int32 id = Math.Min(_lastMapData.TilesetData.Priorities.Length - 1, p_3 % (_lastMapData.TilesetData.Priorities.Length - 1));
            _lastMapData.TileData[p][p_2][p_4] = (UInt16)(id < 48 ? 0 : Math.Max(48, id));

            AddChange(() =>
            {
                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();
                CreateTileMap(_lastMapData);
            });
            
        }
    }
}
