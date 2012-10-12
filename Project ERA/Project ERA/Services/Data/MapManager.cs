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
using ProjectERA.Graphics.Sprite;
using ProjectERA.Services.Data.Storage;
using ERAUtils;
using System.Threading.Tasks;

namespace ProjectERA.Services.Data
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    internal class MapManager : Microsoft.Xna.Framework.GameComponent
    {

        #region Private Members

        private TilesetManager _tilesetManager;
        private FileManager _fileManager;
        private Dictionary<String, MapData> _dataCache;
        private Dictionary<String, TileMap> _graphicsCache;

        #endregion

        internal FileManager FileManager
        {
            get { return _fileManager; }
            set { _fileManager = value; }
        }

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="game">Game to bind to</param>
        internal MapManager(Game game)
            : base(game)
        {
            // Add as service
            this.Game.Services.AddService(this.GetType(), this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // Save tileset Manager
            _tilesetManager = (TilesetManager)this.Game.Services.GetService(typeof(TilesetManager));
            if (_tilesetManager == null)
                new InvalidOperationException("No tileset manager.");

            _fileManager = (FileManager)this.Game.Services.GetService(typeof(FileManager));
            if (_fileManager == null)
                new InvalidOperationException("No file manager.");

            // Create Dictionaries
            _dataCache = new Dictionary<String, MapData>();
            _graphicsCache = new Dictionary<String, TileMap>();

            // Disable updating
            this.Enabled = false;

            base.Initialize();
        }

        /// <summary>
        /// Fill Map Referenced out objects with data
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <param name="mapData">Map Data [out]</param>
        /// <param name="mapGraphics">Map Graphics [out]</param>
        internal void FillMapObjects(MongoObjectId mapId, out MapData mapData, out TileMap mapGraphics)
        {
            mapData = GetMapData(mapId);
            mapGraphics = ((mapData.TilesetData.AssetName != null) ? GetMapGraphics(mapId) : new TileMap(this.Game, mapData));
        }

        /// <summary>
        /// Fill Map Referenced out objects with data
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <param name="innerId">Inner ID</param>
        /// <param name="mapData">Map Data [out]</param>
        /// <param name="mapGraphics">Map Graphics [out]</param>
        internal void FillMapObjects(MongoObjectId mapId, Byte innerId, out MapData mapData, out TileMap mapGraphics)
        {
            mapData = GetMapData(mapId, innerId);
            mapGraphics = ((mapData.TilesetData.AssetName != null) ? GetMapGraphics(mapId, innerId) : new TileMap(this.Game, mapData));
        }

        /// <summary>
        /// Get Map Data from ID
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <returns>Cached MapData</returns>
        private MapData GetMapData(MongoObjectId mapId)
        {
            return GetMapData(mapId, 0);
        }

        /// <summary>
        /// Get Map Data from ID
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <param name="innerId">Inner ID</param>
        /// <returns>Cached MapData</returns>
        private MapData GetMapData(MongoObjectId mapId, Byte innerId)
        {
            MapData mapData;
            if (_dataCache.TryGetValue(mapId.ToString() + "-" + innerId.ToString(), out mapData))
            {
                return mapData;
            }
            else
            {
                return CacheMapData(mapId, innerId);
            }
        }

        /// <summary>
        /// Cache Map Data by ID
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <returns>Cached MapData</returns>
        private MapData CacheMapData(MongoObjectId mapId)
        {
            return CacheMapData(mapId, 0);
        }

        /// <summary>
        /// Cache Map Data by ID
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <param name="innerId">Inner ID</param>
        /// <returns>Cached MapData</returns>
        private MapData CacheMapData(MongoObjectId mapId, Byte innerId)
        {
            MapData mapData = new MapData(mapId, innerId, _tilesetManager, _fileManager);
            _dataCache[mapId.ToString() + " - " + innerId] = mapData;

            return _dataCache[mapId.ToString() + " - " + innerId];
        }

        /// <summary>
        /// Cache Map Data by ID
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <returns>Cached MapData</returns>
        private TileMap GetMapGraphics(MongoObjectId mapId)
        {
            return GetMapGraphics(mapId, 0);
        }

        /// <summary>
        /// Get Map Graphics
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <param name="innerId">Inner ID</param>
        /// <returns>Cached MapGraphics</returns>
        private TileMap GetMapGraphics(MongoObjectId mapId, Byte innerId)
        {
            TileMap mapGraphics;
            if (_graphicsCache.TryGetValue(mapId.ToString() + "-" + innerId.ToString(), out mapGraphics))
            {
                return mapGraphics;
            }
            else
            {
                return CacheMapGraphics(mapId, innerId);
            }
        }

        /// <summary>
        /// Cache Map Graphics
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <returns>Cached Map Graphics</returns>
        private TileMap CacheMapGraphics(MongoObjectId mapId)
        {
            return CacheMapGraphics(mapId, 0);
        }

        /// <summary>
        /// Cache Map Graphics
        /// </summary>
        /// <param name="mapId">Map ID</param>
        /// <param name="innerId">Inner ID</param>
        /// <returns>Cached Map Graphics</returns>
        private TileMap CacheMapGraphics(MongoObjectId mapId, Byte innerId)
        {
            // Create MapGraphics
            TileMap mapGraphics = new TileMap(this.Game, GetMapData(mapId, innerId));
            // Safe Texture
            mapGraphics.TilesetTexture = _tilesetManager.FetchTilesetGraphic(GetMapData(mapId, innerId).TilesetId);

            _graphicsCache[mapId.ToString() + "-" + innerId.ToString()] = mapGraphics;

            return _graphicsCache[mapId.ToString() + "-" + innerId.ToString()];
        }

        /// <summary>
        /// Reload a certain tileset
        /// </summary>
        /// <param name="tsId">Tileset ID</param>
        internal void ReloadTilesetData(MongoObjectId tsId)
        {
            _tilesetManager.ReloadTilesetData(tsId);
        }

        /// <summary>
        /// Reload a certain map
        /// </summary>
        /// <param name="mapId">Map Id</param>
        internal void ReloadMapData(MongoObjectId mapId)
        {
            CacheMapData(mapId);
            CacheMapGraphics(mapId);
        }

        /// <summary>
        /// Reload a certain map
        /// </summary>
        /// <param name="mapId">Map Id</param>
        /// <param name="innerId">Inner id</param>
        internal void ReloadMapData(MongoObjectId mapId, Byte innerId)
        {
            CacheMapData(mapId, innerId);
            CacheMapGraphics(mapId, innerId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="networkManager"></param>
        internal Task<Int32> ValidateData(MongoObjectId id, Network.NetworkManager networkManager, out MongoObjectId tilesetId)
        {
            Services.Network.Protocols.Protocol protocol;
            if (networkManager.TryGetProtocol(typeof(Services.Network.Protocols.Map), out protocol))
            {
                return ((Services.Network.Protocols.Map)protocol).ValidateData(id, (a) => { }, out tilesetId);
            }
            else
            {
                throw new InvalidOperationException("Map protocol not found!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="networkManager"></param>
        /// <returns></returns>
        internal Task<Int32> ValidateGraphics(MongoObjectId id, Network.NetworkManager networkManager)
        {
            Services.Network.Protocols.Protocol protocol;
            if (networkManager.TryGetProtocol(typeof(Services.Network.Protocols.Map), out protocol))
            {
                return ((Services.Network.Protocols.Map)protocol).ValidateGraphics(id, (a) => { });
            }
            else
            {
                throw new InvalidOperationException("Map protocol not found!");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="networkManager"></param>
        internal Task<MapData> UpdateMap(MongoObjectId id, Network.NetworkManager networkManager)
        {
            Services.Network.Protocols.Protocol protocol;
            if (networkManager.TryGetProtocol(typeof(Services.Network.Protocols.Map), out protocol))
            {
                return ((Services.Network.Protocols.Map)protocol).RequestMap(id, (mapData) => { });
            }
            else
            {
                throw new InvalidOperationException("Map protocol not found!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="networkManager"></param>
        internal Task<TilesetData> UpdateTileset(MongoObjectId id, Network.NetworkManager networkManager)
        {
            Services.Network.Protocols.Protocol protocol;
            if (networkManager.TryGetProtocol(typeof(Services.Network.Protocols.Map), out protocol))
            {
                return ((Services.Network.Protocols.Map)protocol).RequestTileset(id, (tilesetData) => { });
            }
            else
            {
                throw new InvalidOperationException("Map protocol not found!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal Task<Boolean> RequestInteractables(MongoObjectId id, Network.NetworkManager networkManager)
        {
            Services.Network.Protocols.Protocol protocol;
            if (networkManager.TryGetProtocol(typeof(Services.Network.Protocols.Map), out protocol))
            {
                return ((Services.Network.Protocols.Map)protocol).RequestInteractables(id, (succeeded) => { });
            }
            else
            {
                throw new InvalidOperationException("Map protocol not found!");
            }
        }
    }
}
