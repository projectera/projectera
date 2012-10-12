using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace ERAUtils
{
    /// <summary>
    /// The Pool generic is used as datastructure to initialize a number
    /// of object of the generic type and keep them in memory. The reason
    /// can be either or both that the initialization of these types are
    /// extensive or use a lot of memory. 
    /// </summary>
    /// <remarks>The generic type needs a constructor with no arguments.</remarks>
    /// <typeparam name="T">The type of objects the pool will hold.</typeparam>
    public static class Pool<T>
        where T : IResetable, new()
    {
        private const Single GrowthOnEmpty = 1.5f;
        private const Single AutoFill = 0.5f;

        #region Private fields

        private static Int32 _capacity = 10;
        private static Queue<T> _pool;

        #endregion

        /// <summary>
        /// Pool Items Limit
        /// </summary>
        public static Int32 Capacity
        {
            get { return _capacity; }
            set { _capacity = value; }
        }

        /// <summary>
        /// Current number of pool items
        /// </summary>
        public static Int32 Count
        {
            get
            {
                if (_pool == null)
                    Initialize();

                return _pool.Count; 
            }
        }

        /// <summary>
        /// Initialize Pool
        /// </summary>
        public static void Initialize()
        {
            // Debug
            Logger.Logger.Debug(new StringBuilder("Pool Fill (t:").Append(typeof(T).Name).Append("/c:").Append(Pool<T>.Capacity).Append(")").ToString());
            // Create variable if needed
            if (_pool == null)
                _pool = new Queue<T>(); 

            // Fill Pool (1-time time consuming)
            Int32 count = (Int32)((Pool<T>.Capacity * AutoFill) - _pool.Count);
            while (count-- > 0)
                _pool.Enqueue(new T());
        }

        /// <summary>
        /// Initialize with Capacity
        /// </summary>
        /// <param name="capacity">Capacity of Pool</param>
        public static void Initialize(Int32 capacity)
        {
            Logger.Logger.Debug("Pool (t:" + typeof(T).Name + "/c:" + capacity.ToString() + ") manual initialization.");
        
            Pool<T>.Capacity = capacity;
            Pool<T>.Initialize();
        }

        private static Object _fillerLock = new Object();

        /// <summary>
        /// Fetches a T
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When failsafe is disabled and pool was not initialized.</exception>
        public static T Fetch()
        {
            if (_pool == null)
            {
                if (System.Threading.Monitor.TryEnter(_fillerLock))
                {
                    if (_pool == null)
                    {
                        Logger.Logger.Error("Pool (t:" + typeof(T).Name + ") not initialized!");

#if DEBUG && NOFAILSAFE
                        throw (new NullReferenceException("Pool was not initialized"));
                    }
                }
#else
                        Logger.Logger.Notice("Failsafe is late-initializing the pool");
                        Pool<T>.Initialize();
                    }

                    System.Threading.Monitor.Exit(_fillerLock);
                }
#endif
            }

            // Fetch
            T result;

            try
            {
                result = _pool.Dequeue(); //(out result))
                return result;
            }
            catch (InvalidOperationException)
            {
                if (System.Threading.Monitor.TryEnter(_fillerLock))
                {
                    if (_pool.Count == 0) //.IsEmpty)
                    {
                    
                            Logger.Logger.Notice(new StringBuilder("Pool (t:").Append(typeof(T)).Append(") was empty.").ToString());
                            Pool<T>.Capacity = (Int32)(Pool<T>.Capacity * GrowthOnEmpty);
                            Pool<T>.Initialize();
                        
                    }
                    System.Threading.Monitor.Exit(_fillerLock);
                }
                else
                {
                    Logger.Logger.Warning(new StringBuilder("Pool (t:").Append(typeof(T)).Append(") item could not be fetched.").ToString());
                }
                return Fetch();
            }
        }

        /// <summary>
        /// Recycles a T into the pool
        /// </summary>
        /// <param name="expired"></param>
        public static void Recycle(T expired)
        {
            if (expired == null)
                return;

            if (Pool<T>.Count < Pool<T>.Capacity)
            {
                expired.Clear();
                _pool.Enqueue(expired);
            }
            else
            {
                Logger.Logger.Debug(new StringBuilder("Pool (t:").Append(typeof(T)).Append(") at capacity (c:").Append(Pool<T>.Capacity).Append(")").ToString());
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IResetable
    {
        void Clear();
    }
}

