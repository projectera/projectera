using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.IO;
using ERAUtils.Logger;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using ProjectERA.Services.Data.Storage;
using System.Threading;

namespace ProjectERA.Services.Display
{

    /// <summary>
    /// The Texure Manager enables saving and loading dynamically created textures
    /// to the isolated data storage. This is done by registering the calling type
    /// and arguments used to create the texture and storing these in a dictionairy
    /// 
    /// The Manager can also be used to load static textures, defaulting to the
    /// machine storage and falling back on the titlecontainer if neccesairy. This
    /// makes graphics updating during runtime much easier.
    /// </summary>
    public class TextureManager : Microsoft.Xna.Framework.GameComponent
    {
        private Dictionary<Int64, Object[]> _dictionairy;
        private LinkedTextureHashMap<String> _staticCache;
        private LinkedTextureHashMap<String> _dynamicCache;
        private FileManager _fileManager;

        private readonly String[] extensions = new String[] { ".png", ".jpg" };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game to bind to</param>
        public TextureManager(Microsoft.Xna.Framework.Game game)
            : base(game)
        {
            _dictionairy = new Dictionary<Int64, Object[]>();
            _staticCache = new LinkedTextureHashMap<String>(128);
            _dynamicCache = new LinkedTextureHashMap<String>(128);

            this.Game.Services.AddService(this.GetType(), this);
        }

        /// <summary>
        /// Initialize manager
        /// </summary>
        public override void Initialize()
        {
            _fileManager = (FileManager)this.Game.Services.GetService(typeof(FileManager));
            if (_fileManager == null)
                throw new InvalidOperationException("No file manager found.");

            this.LoadTextureList();
            this.Enabled = false;

            base.Initialize();
        }

        /// <summary>
        /// Texture exists in Isolated Storage
        /// </summary>
        /// <param name="type">Request class Type</param>
        /// <param name="data">Data params</param>
        /// <remarks>Only looks up dictionairy, not storage container</remarks>
        /// <returns></returns>
        internal Boolean TextureExists(Type type, Object[] data)
        {
            return _dictionairy.ContainsKey(FindKey(type, data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal Boolean TextureCached(String path)
        {
            return _staticCache.ContainsKey(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal Boolean TextureCached(String path, Type type, Object[] data)
        {
            Int64 key = FindKey(type, data);
            String cacheKey = String.Format(@"{0}\{1}", path, key);
            return _dynamicCache.ContainsKey(cacheKey);
        }

        /// <summary>
        /// Finds the key for a type and data
        /// </summary>
        /// <param name="type">Request class Type</param>
        /// <param name="data">Data params</param>
        /// <returns>Key for existant data, or empty slot for data</returns>
        private Int64 FindKey(Type type, Object[] data)
        {
            Object[] found_data;
            Int64 hash_code = GetHashCode(type, data);

            lock (_dictionairy)
            {
                _dictionairy.TryGetValue(hash_code, out found_data);

                while (found_data != null)
                {
                    // Type match and data length match
                    if (type.Equals(found_data[0]) && found_data.Length == data.Length + 1)
                    {
                        Boolean data_match = true;

                        // All data match
                        for (Int32 i = 0; i < data.Length; i++)
                        {
                            if (!data[i].Equals(found_data[i + 1]))
                            {
                                data_match = false;
                                break;
                            }
                        }

                        if (data_match)
                            return hash_code;
                    }
                    _dictionairy.TryGetValue(++hash_code, out found_data);
                }
            }

            return hash_code;
        }

        /// <summary>
        /// Saves a dynamic texture
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <param name="type">Calling type</param>
        /// <param name="data">Calling arguments</param>
        /// <param name="texture">Created texture</param>
        internal void SaveDynamicTexture(String path, Texture2D texture, Type type, Object[] data)
        {
            // Save type to data
            Object[] save_data = new Object[data.Length + 1];
            save_data[0] = type;
            data.CopyTo(save_data, 1);

            // Get key and add save_data
            lock (_dictionairy)
            {
                Int64 key = FindKey(type, data);
                String cacheKey = String.Format(@"{0}\{1}", path, key);

                if (!_dictionairy.ContainsKey(key))
                    _dictionairy.Add(key, save_data);

                // Poll until device is ready
                while (_fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).IsReady == false)
                    Thread.Sleep(10);

                String md5 = String.Empty;

                // Saves file to isolated machine
                _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Save(path, key + ".png", (stream) => 
                    {
                        md5 = ProjectERA.Data.Asset.FileMD5(stream);
                        // When stream is loaded
                        texture.SaveAsPng(stream, texture.Width, texture.Height);
                    });

                // Create path
                Logger.Info(new String[] { "Saved texture (p:[::MACHINE::]", path, @"\", key.ToString(), ".png/t:", type.ToString(), ")" });

                if (_dynamicCache.ContainsKey(cacheKey))
                    if (_dynamicCache.Remove(cacheKey))
                        Logger.Info(String.Format(@"Deleted texture (p:[::CACHE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));

                if (_dynamicCache.Enqueue(cacheKey, texture, md5))
                {
                    Logger.Info(String.Format(@"Cached texture (p:[::CACHE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                }
            }

            // Save texture list
            SaveTextureList();
        }

        /// <summary>
        /// Loads a dynamic texture
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <param name="type">Calling type</param>
        /// <param name="data">Calling arguments</param>
        /// <returns></returns>
        internal Texture2D LoadDynamicTexture(String path, Type type, Object[] data)
        {
            Int64 key = FindKey(type, data);
            String cacheKey = String.Format(@"{0}\{1}", path, key);
            Texture2D result = null;

            if (_dynamicCache.ContainsKey(cacheKey) && !_dynamicCache[cacheKey].Item1.IsDisposed)
            {
                Logger.Info(String.Format(@"Loaded texture (p:[::CACHE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                _dynamicCache.Reference(cacheKey);
                return _dynamicCache[cacheKey].Item1;
            }

            String md5 = String.Empty;

            // Load texture from file
            try
            {
                // Poll until device is ready
                while (_fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).IsReady == false)
                    Thread.Sleep(10);

                if (_fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).FileExists(path, key + ".png"))
                {

                    // Loads file from isolated machine
                    _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Load(path, key + ".png", (stream) =>
                    {
                        // When stream is loaded
                        md5 = ProjectERA.Data.Asset.FileMD5(stream);
                        result = Texture2D.FromStream(this.Game.GraphicsDevice, stream);
                    });
                    Logger.Info(String.Format(@"Loaded texture (p:[::MACHINE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                }
                else
                {
                    Logger.Warning(String.Format(@"Texture was not found (p:[::MACHINE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                    _dictionairy.Remove(key);

                    SaveTextureList();
                }
               
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Warning(String.Format(@"Texture was not found (p:[::MACHINE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                _dictionairy.Remove(key);

                SaveTextureList();
            }
            catch (FileNotFoundException)
            {
                Logger.Warning(String.Format(@"Texture was not found (p:[::MACHINE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                _dictionairy.Remove(key);

                SaveTextureList();
            }

            if (result != null)
            {
                if (_dynamicCache.Enqueue(cacheKey, result, md5))
                {
                    Logger.Info(String.Format(@"Cached texture (p:[::CACHE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                    _dynamicCache.Reference(cacheKey);
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal Int32 ReleaseDynamicTexture(String path, Type type, Object[] data)
        {
            Int64 key = FindKey(type, data);
            String cacheKey = String.Format(@"{0}\{1}", path, key);
            return _dynamicCache.Dereference(cacheKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal Boolean UnloadDynamicTexture(String path, Type type, Object[] data)
        {
            Int64 key = FindKey(type, data);
            String cacheKey = String.Format(@"{0}\{1}", path, key);

            // From cache
            if (_dynamicCache.TryDispose(cacheKey))
            {
                Logger.Info(String.Format(@"Deleted texture (p:[::CACHE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
                return true;
            }
            else
            {
                Logger.Warning(String.Format(@"Texture (p:[::CACHE::]{0}\{1}.png/t:{2}) is still in use!", path, key.ToString(), type.ToString()));
                return false;
            }
        }

        /// <summary>
        /// Delete a dynamic texture
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <param name="type">Calling type</param>
        /// <param name="data">Calling arguments</param>
        internal void DeleteDynamicTexture(String path, Type type, Object[] data)
        {
            Int64 key = FindKey(type, data);
            String cacheKey = String.Format(@"{0}\{1}", path, key);
            _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Delete(path, key + ".png");
            _dictionairy.Remove(key);

            Logger.Info(String.Format(@"Deleted texture (p:[::MACHINE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));

            if (_dynamicCache.TryDispose(cacheKey))
            {
                Logger.Info(String.Format(@"Deleted texture (p:[::CACHE::]{0}\{1}.png/t:{2})", path, key.ToString(), type.ToString()));
            }
            else
            {
                Logger.Warning(String.Format(@"Texture (p:[::CACHE::]{0}\{1}.png/t:{2}) is still in use!", path, key.ToString(), type.ToString()));
            }
        }

        /// <summary>
        /// Saves a static texture to machine storage
        /// </summary>
        /// <param name="path"></param>
        /// <param name="texture"></param>
        public void SaveStaticTexure(String path, Texture2D texture)
        {
            String assetName = path;
            if (assetName.LastIndexOf(".") != -1)
            {
                assetName = assetName.Remove(assetName.LastIndexOf("."));
            }

            // Remove Content Dir
            path = path.Replace("Content/", "").Replace("Content\\", "");

            // Create path
            if (path.LastIndexOf(".") == -1)
                path += ".png";


            String md5 = String.Empty;

            // Create Directory
            _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Save(".", path, (stream) =>
            {
                md5 = ProjectERA.Data.Asset.FileMD5(stream);
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            });

            // Remove from cache
            if (_staticCache.ContainsKey(assetName))
            {
                _staticCache.Remove(assetName);
            }

            // Remove from cache (without content)
            assetName = assetName.Replace("Content/", "").Replace("Content\\", "");
            if (_staticCache.ContainsKey(assetName))
            {
                _staticCache.Remove(assetName);
            }

            _staticCache.Enqueue(assetName, texture, md5);
            Logger.Info(String.Format("Saved texture (p:[::MACHINE::]{0})", path));
            Logger.Info(String.Format("Cached texture (p:[::CACHE::]{0})", assetName));
        }

        /// <summary>
        /// Loads a static texture from either machine storage or titlecontainer 
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <param name="contentManager">ContentManager to use if not in machine storage</param>
        /// <returns></returns>
        public Texture2D LoadStaticTexture(String path, ContentManager contentManager, out String md5)
        {
            Tuple<Texture2D, String> result = null;
            String assetName = path;
            
            md5 = String.Empty;
            
            // From cache
            if (_staticCache.TryGetValue(assetName, out result) && !result.Item1.IsDisposed)
            {
                Logger.Info(String.Format("Loaded texture (p:[::CACHE::]{0})", path));
                _staticCache.Reference(assetName);
                md5 = result.Item2;
                return result.Item1;
            }
            else
            {
                if (assetName.IndexOf(".") != -1)
                {
                    assetName = assetName.Remove(assetName.LastIndexOf("."));
                    if (_staticCache.TryGetValue(assetName, out result))
                    {
                        Logger.Info(String.Format("Loaded texture (p:[::CACHE::]{0})", path));
                        _staticCache.Reference(assetName);
                        md5 = result.Item2;
                        return result.Item1;
                    }
                }
            }

            Boolean found = false;
            IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine);

            // Complete path for assets available in machine store
            if (contentManager != null)
            {
                if (storage.FileExists(".", path))
                {
                    found |= true;

                    if (assetName.Contains('.'))
                        assetName = assetName.Remove(assetName.LastIndexOf("."));
                }
                else
                {
                    for (Int32 i = 0; i < extensions.Length; i++)
                    {
                        if (storage.FileExists(".", path + extensions[i]))
                        {
                            path = assetName + extensions[i];
                            found |= true;
                            break;
                        }
                    }
                }
            }

            // Machine load (overrides title container)
            if (found || contentManager == null)
            {
                if (storage.FileExists(".", path.Replace("Content/", "").Replace("Content\\", "")))
                {
                    String __md5 = null;
                    storage.Load(".", path.Replace("Content/", "").Replace("Content\\", ""), (stream) =>
                        {
                            __md5 = ProjectERA.Data.Asset.FileMD5(stream);
                            result = new Tuple<Texture2D, String>(Texture2D.FromStream(this.Game.GraphicsDevice, stream), __md5);
                            
                        });

                    md5 = __md5;
                }
            }
            
            if (result != null)
            {
                Logger.Info(String.Format("Loaded texture (p:[::MACHINE::]{0})", path));

                // Cache file
                _staticCache.Enqueue(assetName.Replace("Content/", "").Replace("Content\\", ""), result);
                _staticCache.Reference(assetName);
                Logger.Info(String.Format("Cached texture (p:[::CACHE::]/{0})", path));
            }
            else
            {
                try
                {
                    for (Int32 i = 0; i < extensions.Length; i++)
                    {
                        if (assetName.EndsWith(extensions[i]))
                        {
                            assetName = assetName.Remove(assetName.LastIndexOf(extensions[i]));
                            break;
                        }
                    }

                    // ContentManager load (overrides default titleContainer load)
                    try
                    {
                        if (contentManager != null)
                            lock (contentManager)
                            {
                                result = new Tuple<Texture2D, String>(contentManager.Load<Texture2D>(assetName.Replace("Content/", "").Replace("Content\\", "")), String.Empty);
                            }
                    }
                    catch (ContentLoadException)
                    {
                        result = null;
                    }

                    // TitleContainer load
                    if (result == null)
                    {
                        String __md5 = String.Empty;
                        _fileManager.GetStorageDevice(FileLocationContainer.Title).Load(".", path, (stream) =>
                            {
                                __md5 = ProjectERA.Data.Asset.FileMD5(stream);
                                result = new Tuple<Texture2D, string>(Texture2D.FromStream(this.Game.GraphicsDevice, stream), __md5);
                            });

                        md5 = __md5;
                    }
                }
                catch (FileNotFoundException)
                {
                    Logger.Error(String.Format("Unable to load texture (p:[::ANY::]{0})", path));
                    return null;
                }

                Logger.Info(String.Format("Loaded texture (p:[::TITLE::]/{0})", path));
            }

            return result.Item1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contentManager"></param>
        /// <returns></returns>
        public Texture2D LoadStaticTexture(String path, ContentManager contentManager)
        {
            String dummy;
            return LoadStaticTexture(path, contentManager, out dummy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal Texture2D LoadStaticTexture(String path)
        {
            String dummy;
            return LoadStaticTexture(path, null, out dummy);
        }

        /// <summary>
        /// Loads a static texture from either machine storage or titlecontainer
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <returns></returns>
        internal Texture2D LoadStaticTexture(String path, out String md5)
        {
            return LoadStaticTexture(path, null, out md5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal Int32 DeleteStaticTexture(String path)
        {
            Int32 instances = 0;
            String assetName = path;

            // From cache
            if (_staticCache.TryDispose(assetName))
            {
                Logger.Info(String.Format("Deleted texture (p:[::CACHE::]{0})", path));
                _staticCache[path].Item1.Dispose();
                _staticCache.Remove(path);
                instances++;
            }
            else
            {
                Logger.Warning(String.Format(@"Texture (p:[::CACHE::]{0}) is still in use!", path));
            }

            // Remove extension and check cache again
            if (assetName.IndexOf(".") != -1)
            {
                assetName = assetName.Remove(assetName.LastIndexOf("."));
                if (_staticCache.TryDispose(assetName))
                {
                    Logger.Info(String.Format("Deleted texture (p:[::CACHE::]{0})", path));
                    _staticCache[path].Item1.Dispose();
                    _staticCache.Remove(path);
                    instances++;
                }
                else
                {
                    Logger.Warning(String.Format(@"Texture (p:[::CACHE::]{0}) is still in use!", path));
                }
            }

            IStorageDevice storage = _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine);

            // Check for with extension
            if (storage.FileExists(".", path))
            {
                storage.Delete(".", path);
                instances++;
                Logger.Info(String.Format("Deleted texture (p:[::MACHINE::]{0})", path));
            }
            
            // Delete all known extensions
            for (Int32 i = 0; i < extensions.Length; i++)
            {
                if (storage.FileExists(".", path + extensions[i]))
                {
                    path = assetName + extensions[i];
                    storage.Delete(".", path);
                    instances++;
                    Logger.Info(String.Format("Deleted texture (p:[::MACHINE::]{0})", path));
                }

                if (storage.FileExists(".", path.Replace("Content/", "").Replace("Content\\", "")))
                {
                    storage.Delete(".", path);
                    instances++;
                    Logger.Info(String.Format("Deleted texture (p:[::MACHINE::]{0})", path));
                }
            }

            return instances;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal Int32 ReleaseStaticTexture(String path)
        {
            return _staticCache.Dereference(path);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal Boolean UnloadStaticTexture(String path)
        {
            Boolean unloaded = true;
            String assetName = path;

            // From cache
            if (_staticCache.ContainsKey(assetName))
            {
                if (_staticCache.TryDispose(assetName))
                {
                    Logger.Info(String.Format("Deleted texture (p:[::CACHE::]{0})", path));
                }
                else
                {
                    Logger.Warning(String.Format(@"Texture (p:[::CACHE::]{0}) is still in use!", path));
                    unloaded = false;
                }
            }

            // Remove extension and check cache again
            if (assetName.IndexOf(".") != -1)
            {
                assetName = assetName.Remove(assetName.LastIndexOf("."));
                if (_staticCache.ContainsKey(assetName))
                {
                    if (_staticCache.TryDispose(assetName))
                    {
                        Logger.Info(String.Format("Deleted texture (p:[::CACHE::]{0})", path));
                    }
                    else
                    {
                        Logger.Warning(String.Format(@"Texture (p:[::CACHE::]{0}) is still in use!", path));
                        unloaded = false;
                    }
                }
            }

            return unloaded;
        }

        /// <summary>
        /// Gets hash code for type and arguments
        /// </summary>
        /// <param name="type">Calling type</param>
        /// <param name="data">Calling arguments</param>
        /// <returns></returns>
        internal Int64 GetHashCode(Type type, Object[] data)
        {
            Int64 large = type.ToString().GetHashCode() * 7919;
            Int32[] primes = Primes;

            for (Int32 i = 0; i < data.Length; i++)
            {
                large ^= primes[i % primes.Length] * data[i].ToString().GetHashCode();
            }

            if (large == 0)
                large = -1;

            return large;
        }

        /// <summary>
        /// Constant Prime numbers used for hashcode creation
        /// </summary>
        private Int32[] Primes
        {
            get { return new Int32[] { 7877, 7759, 7687, 7591, 7537, 7459, 7333, 7237, 7151, 7039,
                                       6967, 6871, 6793, 6703, 6619, 6547, 6421, 6337, 6269, 6197 }; }
        }

        /// <summary>
        /// Save dynamic texture list
        /// </summary>
        internal void SaveTextureList()
        {
            lock(_dictionairy)
            {
                // Settings for the writer
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                // Save the list
                _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Save("Data", "TextureList.xml", (stream) => 
                    {
                        using (XmlWriter writer = XmlWriter.Create(stream, settings))
                        {
                            IntermediateSerializer.Serialize(writer, _dictionairy, @"Data\TextureList.xml");
                        }
                    });

                Logger.Info(new StringBuilder("Texture list was saved to [::MACHINE::] (c:").Append(_dictionairy.Count).Append(")").ToString());
            }
        }

        /// <summary>
        /// Load dynamic texture list
        /// </summary>
        internal void LoadTextureList()
        {
            lock (_dictionairy)
            {
                if (_fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).FileExists("Data", "TextureList.xml"))
                {
                    Boolean errorThrown = false;
                    _fileManager.GetStorageDevice(FileLocationContainer.IsolatedMachine).Load("Data", "TextureList.xml", (stream) => {  
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            try
                            {
                                _dictionairy = IntermediateSerializer.Deserialize<Dictionary<Int64, Object[]>>(reader, @"Data\TextureList.xml");
                            }
                            catch (InvalidContentException)
                            {
                                Logger.Warning("Texture List corrupt");
                                errorThrown = true;
                            }
                        } 
                    });

                    if (errorThrown)
                    {
                        SaveTextureList();
                    }
                    else
                    {
                        // Log read
                        Logger.Info(new StringBuilder("Texture list was loaded from [::MACHINE::] (c:").Append(_dictionairy.Count).Append(")").ToString());
                    }
                    
                }
                else
                {
                    // Log read
                    Logger.Info("No texture list was found.");

                    // Create list
                    SaveTextureList();
                }
            }
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        /// <param name="staticSize"></param>
        /// <param name="staticSizeInBytes"></param>
        /// <param name="staticCapacityInBytes"></param>
        /// <param name="dynamicSize"></param>
        /// <param name="dynamicSizeInBytes"></param>
        /// <param name="dynamicCapacityInBytes"></param>
        internal void GetCacheStatistics(out Int64 staticSize, out Int64 staticSizeInBytes, out Int64 staticCapacityInBytes, 
            out int staticReferenceCount, out int staticDereferenceCount, out int staticEnqueueCount, out int staticRemoveCount,
            out int staticDisposeCount, out int staticCapacityLimitExceededCount,
            out Int64 dynamicSize, out Int64 dynamicSizeInBytes, out Int64 dynamicCapacityInBytes,
            out int dynamicReferenceCount, out int dynamicDereferenceCount, out int dynamicEnqueueCount, out int dynamicRemoveCount,
            out int dynamicDisposeCount, out int dynamicCapacityLimitExceededCount)
        {
            staticSize = _staticCache.Size;
            staticSizeInBytes = _staticCache.SizeInBytes;
            staticCapacityInBytes = _staticCache.CapacityInBytes;

            dynamicSize = _dynamicCache.Size;
            dynamicSizeInBytes = _dynamicCache.SizeInBytes;
            dynamicCapacityInBytes = _dynamicCache.CapacityInBytes;

            staticReferenceCount = 0;
            staticReferenceCount = 0;
            staticDereferenceCount = 0;
            staticEnqueueCount = 0;
            staticRemoveCount = 0;
            staticDisposeCount = 0;
            staticCapacityLimitExceededCount = 0;
            
            dynamicReferenceCount = 0;
            dynamicDereferenceCount = 0;
            dynamicEnqueueCount = 0; 
            dynamicRemoveCount = 0;
            dynamicDisposeCount = 0; 
            dynamicCapacityLimitExceededCount = 0;

            #if DEBUG || STATISTICS
            staticReferenceCount = _staticCache.CountReferences;
            staticDereferenceCount = _staticCache.CountDereferences;
            staticEnqueueCount = _staticCache.CountEnqueues;
            staticRemoveCount = _staticCache.CountRemoves;
            staticDisposeCount = _staticCache.CountDisposes;
            staticCapacityLimitExceededCount = _staticCache.CountCapacityExceeded;

            dynamicReferenceCount = _dynamicCache.CountReferences;
            dynamicDereferenceCount = _dynamicCache.CountDereferences;
            dynamicEnqueueCount = _dynamicCache.CountEnqueues;
            dynamicRemoveCount = _dynamicCache.CountRemoves;
            dynamicDisposeCount = _dynamicCache.CountDisposes;
            dynamicCapacityLimitExceededCount = _dynamicCache.CountCapacityExceeded;
            #endif
        }
    }
}
