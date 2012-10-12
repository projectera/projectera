using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Lidgren.Network;
using ProjectERA.Protocols;
using ERAUtils;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Player : Protocol
    {
        /// <summary>
        /// Pickavatar action
        /// </summary>
        /// <param name="selectedId">avatar to select</param>
        internal static void RequestPickAvatar(MongoObjectId selectedId, Action<MongoObjectId> resultAction)
        {
            // Mark map not loaded
            Map.Id = MongoObjectId.Empty;
            _pickAvatarAction = resultAction;

            // Send message
            NetOutgoingMessage msg = OutgoingMessage(PlayerAction.PickAvatar, 12);
            msg.Write(selectedId.Id);
            _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal static void RequestMovement(Int32 x, Int32 y, Byte d)
        {
            NetOutgoingMessage msg = OutgoingMessage(PlayerAction.RequestMovement, 9);
            msg.Write(x);
            msg.Write(y);
            msg.Write(d);
            _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// PlayerRequest class
        /// </summary>
        private class PlayerRequest : ICacheable<MongoObjectId>
        {
            /// <summary>
            /// Player Id
            /// </summary>
            public MongoObjectId Key { get; set; }

            /// <summary>
            /// Task that will yield Player object
            /// </summary>
            internal TaskCompletionSource<ProjectERA.Data.Player> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<ProjectERA.Data.Player> Action { get; private set; }

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
            public PlayerRequest(MongoObjectId key)
            {
                this.Key = key;
                this.Task = new TaskCompletionSource<ProjectERA.Data.Player>();
                this.Creation = DateTime.Now;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public PlayerRequest(MongoObjectId key, Action<ProjectERA.Data.Player> action)
                : this(key)
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            internal ProjectERA.Data.Player Result
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
        }
    }
}
