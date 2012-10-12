using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading.Tasks;
using ProjectERA.Services.Data.Storage;
using System.Text;
using ERAUtils;
using ProjectERA.Data.Enum;

namespace ProjectERA.Services.Data
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TilesetManager : Microsoft.Xna.Framework.GameComponent
    {
        #region Private fields

        private Dictionary<String, TilesetData> _dataCache;
        private Dictionary<String, Texture2D> _graphicsCache;
        private ContentManager _contentManager;
        private Display.TextureManager _textureManager;
        private FileManager _fileManager;
        private String _path = ".";

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game to bind to</param>
        internal TilesetManager(Game game)
            : base(game)
        {
            this.Game.Services.AddService(this.GetType(), this);
        }

        /// <summary>
        /// Constructor with path
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="path">Path to load from</param>
        public TilesetManager(Game game, String path)
            : base(game)
        {
            _path = path;

            this.Game.Services.AddService(this.GetType(), this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // Disable updating
            this.Enabled = false;

            // Load ContentManager
            _contentManager = new ContentManager(this.Game.Services, _path + (_path.EndsWith("\\") ? "" : _path.EndsWith("/") ? "" : _path.Length > 0 ? "\\" : "") + "Content");

            _textureManager = (Display.TextureManager)this.Game.Services.GetService(typeof(Display.TextureManager));
            if (_textureManager == null)
                throw new InvalidOperationException("No texture manager.");

            _fileManager = (FileManager)this.Game.Services.GetService(typeof(FileManager));
            if (_fileManager == null)
                throw new InvalidOperationException("No file manager.");

            // Create Caches
            _dataCache = new Dictionary<String, TilesetData>();
            _graphicsCache = new Dictionary<String, Texture2D>();

            base.Initialize();
        }

        /// <summary>
        /// Returns a data object for a tileset
        /// </summary>
        /// <param name="tilesetId"></param>
        /// <returns></returns>
        internal TilesetData FetchTilesetData(MongoObjectId tilesetId)
        {
            TilesetData tilesetData;
            String tilesetIdString = tilesetId.ToString();
            if (_dataCache.TryGetValue(tilesetIdString.ToString(), out tilesetData))
            {
                return tilesetData;
            }
            else
            {
                return CacheTilesetData(tilesetId);
            }
        }

        /// <summary>
        /// Returns a graphic for a tileset
        /// </summary>
        /// <param name="tilesetId"></param>
        /// <returns></returns>
        internal Texture2D FetchTilesetGraphic(MongoObjectId tilesetId)
        {
            String tilesetIdString = tilesetId.ToString();
            return FetchTilesetGraphic(tilesetIdString.ToString());
        }

        internal Texture2D FetchTilesetGraphic(String tilesetId)
        {
            if (_dataCache[tilesetId].AssetName != null)
                return _graphicsCache[_dataCache[tilesetId].AssetName];
            return null;
        }

        /// <summary>
        /// Cache Tileset Data
        /// </summary>
        /// <param name="tilesetId">Id to Cache</param>
        /// <returns>TilesetData object for tilesetId</returns>
        private TilesetData CacheTilesetData(MongoObjectId tilesetId)
        {
            String tilesetIdString = tilesetId.ToString();
            TilesetData tilesetData = new TilesetData(tilesetId, _path, _fileManager);
            _dataCache[tilesetIdString] = tilesetData;

            if (tilesetData.AssetName != null && _graphicsCache.ContainsKey(tilesetData.AssetName) == false)
                _graphicsCache[tilesetData.AssetName] = _textureManager.LoadStaticTexture(@"Graphics\Tilesets\" + tilesetData.AssetName, _contentManager);

            return _dataCache[tilesetIdString];
        }

        /// <summary>
        /// Reload a certain tileset
        /// </summary>
        /// <param name="tilesetId"></param>
        internal void ReloadTilesetData(MongoObjectId tilesetId)
        {
            CacheTilesetData(tilesetId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Texture2D LoadGraphic(TilesetData source)
        {
            return _textureManager.LoadStaticTexture(_contentManager == null ? "" : _contentManager.RootDirectory + "\\" + @"Graphics\Tilesets\" + source.AssetName, _contentManager);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private TilesetData LoadTask(Object id)
        {
            TilesetData data = new TilesetData();
            data.TilesetId = (MongoObjectId)id;

            if (data.Exists(_fileManager))
            {
                data.Initialize(_fileManager);

                if (!data.LoadResult.HasFlag(DataLoadResult.NotFound) &&
                    !data.LoadResult.HasFlag(DataLoadResult.Invalid))
                    return data;

                data.Delete(_fileManager);
            }

            return null;
        }
        
    }
}
