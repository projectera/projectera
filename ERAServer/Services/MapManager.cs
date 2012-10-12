using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using ERAServer.Data;
using ERAServer.Data.AI;
using System.Threading.Tasks;
using System.Threading;
using ERAUtils;
using System.Collections;
using ERAServer.Services.Listeners;

namespace ERAServer.Services
{
    internal static class MapManager
    {
        private static Dictionary<ObjectId, Map> _maps;
        private static Dictionary<ObjectId, Tileset> _tilesets;
        private static ConcurrentQueue<Task> _tasks;
        private static ConcurrentDictionary<Int64, ConcurrentQueue<Task>> _actions;
        private static Int32 _runningCount;
        private static Int64 _timeframe;
        private static ReaderWriterLockSlim _lock;
        private static AutoResetEvent _timeframeStartPulser;

        private const Int32 PulseTime = 500;
        private const Int32 Concurrency = 5;

        /// <summary>
        /// 
        /// </summary>
        public static void Initialize()
        {
            _maps = new Dictionary<ObjectId, Map>();
            _tilesets = new Dictionary<ObjectId, Tileset>();
            _tasks = new ConcurrentQueue<Task>();
            _actions = new ConcurrentDictionary<Int64, ConcurrentQueue<Task>>();
            _lock = new ReaderWriterLockSlim();
            _timeframeStartPulser = new AutoResetEvent(false);
            _runningCount = 0;
            _timeframe = 1;

            while (_tasks.Count < Concurrency)
                _tasks.Enqueue(Task.Factory.StartNew(() => { }));
        }

        /// <summary>
        /// 
        /// </summary>
        public static Int64 CurrentTimeFrame
        {
            get 
            {
                try
                {
                    _lock.EnterReadLock();
                    return _timeframe; 
                }
                finally 
                {
                    _lock.ExitReadLock();
                }
            }
            private set 
            {
                _lock.EnterWriteLock();
                _timeframe = value;
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Int64 NextTimeFrame
        {
            get { return Interlocked.Read(ref _timeframe) + 1; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public static void QueueAction(Action action)
        {
            QueueAction(NextTimeFrame, action);
        }

        /// <summary>
        /// Queues a new action
        /// </summary>
        /// <param name="action"></param>
        public static void QueueAction(Int64 timeframe, Action action)
        {
            // If there are actions left, we have to queue this item
            Task task;
            if (CurrentTimeFrame < timeframe || _actions[timeframe].Count > 0)
            {
                
                _actions.GetOrAdd(timeframe, 
                    new Lazy<ConcurrentQueue<Task>>(() => new ConcurrentQueue<Task>()).Value).Enqueue(
                    new Task(action));

                return;
            }

            // If there are tasks available
            if (_tasks.TryDequeue(out task))
            {
                // Start the task
                Interlocked.Increment(ref _runningCount);
                task.ContinueWith(t => action).ContinueWith(t => OnTaskFree(task));
                return;
            }

            // Queue action
            _actions.GetOrAdd(timeframe,
                    new Lazy<ConcurrentQueue<Task>>(() => new ConcurrentQueue<Task>()).Value).Enqueue(
                    new Task(action));
        }

        /// <summary>
        /// On Task free start queued actions
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private static void OnTaskFree(Task task)
        {
            Task result;
            Int64 timeFrame = CurrentTimeFrame;
            if (_actions[timeFrame].TryDequeue(out result))
            {
                task.ContinueWith(t => result);
            }
            else
            {
                _tasks.Enqueue(task);

                if (Interlocked.Decrement(ref _runningCount) == 0)
                    AdvanceTimeFrame(timeFrame);
            }
        }

        /// <summary>
        /// Move to the next time frame
        /// </summary>
        private static void AdvanceTimeFrame(Int64 snapshot)
        {
            _lock.EnterWriteLock();
            if (_timeframe == snapshot)
            {
                if (Interlocked.Increment(ref _timeframe) == snapshot + 1)
                {
                    ConcurrentQueue<Task> value;

                    _actions.TryRemove(snapshot, out value);
                    if (value != null)
                        _actions.AddOrUpdate(_timeframe, value, (c, d) => { return d; });

                    _timeframeStartPulser.Set();
                }
            }
            _lock.ExitWriteLock();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeFrame"></param>
        public static void WaitAllTimeFrame(Int64 timeFrame)
        {
            try
            {
                WaitStartTimeFrame(timeFrame);
                if (_actions.ContainsKey(timeFrame))
                    Task.WaitAll(_actions[timeFrame].ToArray());
            }
            catch (NullReferenceException)
            {

            }
            catch (ArgumentException)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeFrame"></param>
        public static void WaitAnyTimeFrame(Int64 timeFrame)
        {
            try
            {
                WaitStartTimeFrame(timeFrame);
                if (_actions.ContainsKey(timeFrame))
                    Task.WaitAny(_actions[timeFrame].ToArray());
            }
            catch (NullReferenceException)
            {

            }
            catch (ArgumentException)
            {

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeFrame"></param>
        public static void WaitStartTimeFrame(Int64 timeFrame)
        {
            while (CurrentTimeFrame < timeFrame)
                _timeframeStartPulser.WaitOne(PulseTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeFrame"></param>
        public static void WaitEndTimeFrame(Int64 timeFrame)
        {
            while (timeFrame > CurrentTimeFrame)
                _timeframeStartPulser.WaitOne(PulseTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="interactable"></param>
        public static void InteractableJoined(ObjectId map, Interactable interactable)
        {
            VerifyMapRunning(map);
            lock (_maps)
            {
                System.Threading.Thread.MemoryBarrier();
                _maps[map].AddInteractable(interactable);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interactable"></param>
        public static void InteractableLeft(Interactable interactable)
        {
            VerifyMapRunning(interactable.MapId);
            lock (_maps)
            {
                System.Threading.Thread.MemoryBarrier();
                _maps[interactable.MapId].RemoveInteractable(interactable);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        private static void VerifyMapRunning(ObjectId map)
        {
            if (!_maps.ContainsKey(map)) //TODO: check supposed to run
                throw new Exception("Map doesn't run on this server");
        }

        /// <summary>
        /// Starts running map with id
        /// </summary>
        /// <param name="map">Map id</param>
        public static void StartRunningMap(ObjectId map)
        {
            if (_maps.ContainsKey(map) == false)
            {
                _maps.Add(map, Map.GetBlocking(map));
                Servers.BroadcastRunningMap(map);
            }
        }

        /// <summary>
        /// Gets interactables on map wit id
        /// </summary>
        /// <param name="map">Map Id</param>
        internal static List<ObjectId> GetInteractables(ObjectId map)
        {
            VerifyMapRunning(map);
            return _maps[map].GetInteractables();
        }

        /// <summary>
        /// Writes the mapdata of map with id to msg
        /// </summary>
        /// <param name="id">Map id</param>
        /// <param name="getMsg">Message to write to</param>
        internal static void WriteMapData(ObjectId id, Lidgren.Network.NetOutgoingMessage getMsg)
        {
            Map data;
            if (!_maps.TryGetValue(id, out data))
            {
                data = Map.GetBlocking(id);
            }

            getMsg.Write(data.Id.ToByteArray()); //12
            getMsg.Write(data.TilesetId.ToByteArray()); //24
            getMsg.Write(data.RegionId.ToByteArray()); //36
            getMsg.Write(data.Name ?? String.Empty);
            getMsg.Write((Int32)data.Type); // 40

            if (data.Settings == null)
            {
                getMsg.Write(String.Empty); 
                getMsg.Write((Byte)0); 
                getMsg.Write(String.Empty);
            }
            else
            {
                getMsg.Write(data.Settings.FogAssetName ?? String.Empty);
                getMsg.Write(data.Settings.FogOpacity); // 41+
                getMsg.Write(data.Settings.PanormaAssetName ?? String.Empty);
            }

            getMsg.Write(data.Width); // 43
            getMsg.Write(data.Height); // 45
            for (Int32 i = 0; i < data.Data.Length; i++)
            {
                for (Int32 j = 0; j < data.Data[i].Length; j++)
                {
                    getMsg.Write(data.Data[i][j][0]); // 3 * width * height * 2 
                    getMsg.Write(data.Data[i][j][1]); // w = 65, h = 65
                    getMsg.Write(data.Data[i][j][2]); // 25.350 bytes
                }
            }
            getMsg.Write(data.Version); // 49 + data + names

        }

        /// <summary>
        /// Gets current map version of map with id
        /// </summary>
        /// <param name="id">Map id</param>
        /// <returns>Current map version</returns>
        internal static UInt32 GetMapVersion(ObjectId id)
        {
            Map data;
            if (!_maps.TryGetValue(id, out data))
            {
                data = Map.GetBlocking(id);
                _maps.Add(id, data);
            }

            return data.Version;
        }

        /// <summary>
        /// Gets map hashcode of map with id
        /// </summary>
        /// <param name="id">Map id</param>
        /// <returns>Map hashcode</returns>
        internal static Int32 GetMapHashCode(ObjectId id)
        {
            Map data;
            if (!_maps.TryGetValue(id, out data))
            {
                data = Map.GetBlocking(id);
                _maps.Add(id, data);
            }

            return data.GetHashCode();
        }

        /// <summary>
        /// Writes the tilesetdata of tileset with id to msg
        /// </summary>
        /// <param name="id">Tileset id</param>
        /// <param name="msg">Message to write to</param>
        internal static void WriteTilesetData(ObjectId id, Lidgren.Network.NetOutgoingMessage msg)
        {
            Tileset data;
            if (!_tilesets.TryGetValue(id, out data))
            {
                data = Tileset.GetBlocking(id);
                data.Normalize();
                _tilesets.Add(id, data);
            }

            msg.Write(data.Id.ToByteArray());
            msg.Write(data.Name ?? String.Empty);
            msg.Write(data.AssetName ?? String.Empty);
            msg.Write(data.Tiles);

            msg.Write((Byte)data.AutotileAssetNames.Count);
            foreach (String autotile in data.AutotileAssetNames)
                msg.Write(autotile ?? String.Empty);
            Int32 ba = 0;
            for (Int32 i = 0; i < data.AutotileAssetNames.Count; i++)
                ba |= (1 << i) * (data.AutotileAnimationFlags == null ? 0 : data.AutotileAnimationFlags[i] ? 1 : 0);
            msg.Write((Byte)ba);

            msg.Write(data.Passages.Length);
            for (Int32 j = 0; j < data.Passages.Length; j++)
            {
                msg.Write(data.Passages[j]);
                msg.Write(data.Priorities[j]);
                msg.Write(data.Flags[j]);
                msg.Write(data.Tags[j]);
            }

            msg.Write(data.Version);
        }

        /// <summary>
        /// Gets tileset current version of tileset with id
        /// </summary>
        /// <param name="id">Tileset id</param>
        /// <returns>Current tileset version</returns>
        internal static UInt32 GetTilesetVersion(ObjectId id)
        {
            Tileset data;
            if (!_tilesets.TryGetValue(id, out data))
            {
                data = Tileset.GetBlocking(id);
                data.Normalize();
            }

            return data.Version;
        }

        /// <summary>
        /// Gets tileset hashcode of tileset with id
        /// </summary>
        /// <param name="id">Tileset id</param>
        /// <returns>Tileset hashcode</returns>
        internal static Int32 GetTilesetHashCode(ObjectId id)
        {
            Tileset data;
            if (!_tilesets.TryGetValue(id, out data))
            {
                data = Tileset.GetBlocking(id);
                data.Normalize();
            }

            return data.GetHashCode();
        }

        /// <summary>
        /// Gets tileset id bound to map with mapId
        /// </summary>
        /// <param name="mapId">Map id</param>
        /// <returns>Tileset Id</returns>
        internal static ObjectId GetTilesetId(ObjectId mapId)
        {
            Map data;
            if (!_maps.TryGetValue(mapId, out data))
            {
                data = Map.GetBlocking(mapId);
            }

            return data.TilesetId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vTilesetId2"></param>
        internal static Tileset GetTilesetData(ObjectId id)
        {
            Tileset data;
            if (!_tilesets.TryGetValue(id, out data))
            {
                data = Tileset.GetBlocking(id);
                data.Normalize();
            }

            return data;
        }
    }
}
