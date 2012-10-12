using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ERAUtils;

namespace ProjectERA.Services.Network.Protocols
{
    internal abstract partial class Protocol : IDisposable
	{
        internal const Int32 DataRequestTimeout = 1000 * 3;
        protected LinkedHashMap<MongoObjectId, DataRequest> _outstandingDataRequests;

        /// <summary>
        /// PlayerRequest class
        /// </summary>
        internal class DataRequest : ICacheable<MongoObjectId>
        {
            /// <summary>
            /// Player Id
            /// </summary>
            public MongoObjectId Key { get; set; }

            /// <summary>
            /// Task that will yield succes object
            /// </summary>
            internal TaskCompletionSource<Boolean> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<Boolean> Action { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            internal Boolean[] Parts { get; private set; }

            /// <summary>
            /// Retrieval Timer
            /// </summary>
            internal Timer TimeOut { get; set; }

            /// <summary>
            /// Creation Time of Request
            /// </summary>
            internal DateTime Creation { get; private set; }

            /// <summary>
            /// Creates a new DataStoreRequest
            /// </summary>
            /// <param name="key">The key to request</param>
            public DataRequest(MongoObjectId key)
            {
                this.Key = key;
                this.Task = new TaskCompletionSource<Boolean>();
                this.Creation = DateTime.Now;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public DataRequest(MongoObjectId key, Action<Boolean> action)
                : this(key)
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public DataRequest(Action<Boolean> action)
                : this(MongoObjectId.GenerateRandom())
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            internal Boolean Result
            {
                set
                {
                    if (!this.Task.TrySetResult(value))
                    {

                    }

                    // Kill timout
                    if (this.TimeOut != null)
                        this.TimeOut.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                    // Run action
                    if (this.Action != null)
                        this.Action.Invoke(value);

                    // Kill action
                    this.Action = null;
                }
                get
                {
                    return this.Task.Task.Result;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="part"></param>
            internal Boolean ReceivePartial(Int32 part, Int32 total)
            {
                lock (this)
                {
                    // Start parts
                    if (Parts == null)
                        Parts = new Boolean[total];

                    // Set part
                    Parts[part] = true;

                    // Received all
                    if (Parts.All(a => a == true))
                    {
                        this.Result = true;
                        return true;
                    }


                    return false;
                }
            }
        }
	}
}
