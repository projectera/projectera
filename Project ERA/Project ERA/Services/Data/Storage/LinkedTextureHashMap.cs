using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectERA.Services.Data.Storage
{
    public class LinkedTextureHashMap<K> : LinkedHashMap<K, Tuple<Texture2D, String>>
    {
        public Int64 SizeInBytes { get; protected set; }
        public Int64 CapacityInBytes { get; protected set; }

        private Dictionary<K, Int32> _cacheReferences;

#if DEBUG || STATISTICS
        public Int32 CountReferences { get; protected set; }
        public Int32 CountDereferences { get; protected set; }
        public Int32 CountEnqueues { get; protected set; }
        public Int32 CountDisposes { get; protected set; }
        public Int32 CountRemoves { get; protected set; }
        public Int32 CountCapacityExceeded { get; protected set; }
#endif

        /// <summary>
        /// Creates a new LinkedHashMap.
        /// </summary>
        /// <param name="capacity"></param>
        public LinkedTextureHashMap(Int32 capacity)
           : base()
        {
            if (capacity > 2048)
                throw new ArgumentOutOfRangeException("capacity", capacity, "Can not allocate more than 2048 MBytes at once");

            this.CapacityInBytes = capacity * 1024 * 1024;
            _cacheReferences = new Dictionary<K, Int32>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comparer"></param>
        public LinkedTextureHashMap(Int32 capacity, IEqualityComparer<K> comparer)
            : base(comparer)
        {
            this.CapacityInBytes = capacity;
            _cacheReferences = new Dictionary<K, Int32>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public new Boolean Enqueue(K key, Tuple<Texture2D, String> value)
        {
            return Enqueue(key, value.Item1, value.Item2);
        }

        /// <summary>
        /// Adds a value to the LinkedHashMap
        /// </summary>
        /// <param name="key">The key to be added</param>
        /// <param name="value">The value to be added</param>
        public Boolean Enqueue(K key, Texture2D value, String md5)
        {
            Boolean result = base.Enqueue(key, new Tuple<Texture2D, String>(value, md5));
            Int32 multiplier = 4;
            switch (value.Format)
            {
                case SurfaceFormat.Color:
                    multiplier = 4;
                    break;
            }

            SizeInBytes += (value.Width * value.Height * multiplier);

            #if DEBUG || STATISTICS
                CountEnqueues++;
            #endif

#if DEBUG
            if (SizeInBytes > CapacityInBytes)
            {
                lock (_cacheReferences)
                {
                    // Clean up
                    var keys = new K[_cacheReferences.Keys.Count];
                    _cacheReferences.Keys.CopyTo(keys, 0);

                    foreach (var cacheKey in keys) {
                        
                        if (TryDispose(cacheKey))
                        {
                            ERAUtils.Logger.Logger.Notice(String.Format("Removed {0} from cache because size exceeded capacity!", cacheKey));
                        }

                        if (SizeInBytes <= CapacityInBytes)
                            break;
                    }
                }

                if (SizeInBytes > CapacityInBytes)
                {
                    ERAUtils.Logger.Logger.Warning("Cache limit exceeded!");

                    #if DEBUG || STATISTICS
                        CountCapacityExceeded++;
                    #endif
                }
            }
#else
            ERAUtils.Logger.Logger.Warning("Cache limit exceeded!");
#endif

            return result;
        }

        /// <summary>
        /// Removes a certain key from the LinkedHashMap
        /// </summary>
        /// <param name="key">The key to be removed</param>
        public new Boolean Remove(K key)
        {
            Tuple<Texture2D, String> value;
            if (this.TryGetValue(key, out value))
            {
                Int32 multiplier = 4;
                switch (value.Item1.Format)
                {
                    case SurfaceFormat.Color:
                        multiplier = 4;
                        break;
                }

                SizeInBytes -= (value.Item1.Width * value.Item1.Height * multiplier);

                #if DEBUG || STATISTICS
                    CountRemoves++;
                #endif

                return base.Remove(key);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Boolean TryDispose(K key)
        {
            Tuple<Texture2D, String> value;
            if (this.TryGetValue(key, out value))
            {
                Int32 references;
                if (!_cacheReferences.TryGetValue(key, out references))
                    references = 0;

                if (references == 0)
                {
                    _cacheReferences.Remove(key);
                    Remove(key);
                    value.Item1.Dispose();

                    #if DEBUG || STATISTICS
                        CountDisposes++;
                    #endif

                    // Dispose was allowed
                    return true;
                }

                // References exist
                return false;
            }
            else
            {
                Int32 references;
                if (!_cacheReferences.TryGetValue(key, out references))
                    references = 0;

                if (references == 0)
                {
                    _cacheReferences.Remove(key);
                }
            }
            
            // Not existant
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Int32 Reference(K key)
        {
            Int32 value;
            if (!_cacheReferences.TryGetValue(key, out value))
                _cacheReferences.Add(key, 0);

            #if DEBUG || STATISTICS
                CountReferences++;
            #endif

            return ++_cacheReferences[key];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Int32 Dereference(K key)
        {
            Int32 value;
            if (!_cacheReferences.TryGetValue(key, out value))
                return 0;

            #if DEBUG || STATISTICS
                CountDereferences++;
            #endif

            return --_cacheReferences[key];
        }
    }
}
