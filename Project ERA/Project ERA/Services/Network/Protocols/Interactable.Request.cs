using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Lidgren.Network;
using ERAUtils;
using ProjectERA.Protocols;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Interactable : Protocol
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ProjectERA.Data.Interactable> Get(MongoObjectId id, Action<ProjectERA.Data.Interactable> ResultAction)
        {
            InteractableRequest req = new InteractableRequest(id, ResultAction);

            // Queue the request
            QueueAction(() =>
            {
                // If in storage
                InteractableRequest interactableRequest = GeneralCache<MongoObjectId, InteractableRequest>.QueryCache(id);
                if (interactableRequest != null && interactableRequest.Task != null && interactableRequest.Task.Task != null && interactableRequest.Task.Task.Result != null)
                {
                    // Result directly
                    req.Result = interactableRequest.Result;
                }
                else
                {
                    // Create the outgoing request Message
                    NetOutgoingMessage msg = OutgoingMessage(InteractableAction.Get, 12);
                    msg.Write(id.Id);
                    this.Connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                    // Add it to outstanding Requests
                    _outstandingInteractableRequests.Enqueue(id, req);

                    // Start the retrieval timer
                    req.TimeOut = new Timer((object state) =>
                    {
                        // On timeout, set the result to none
                        req.Result = null;

                        // Queue removal of request
                        QueueAction(() =>
                        {
                            _outstandingInteractableRequests.Remove(id);
                        });

                    }, null, Interactable.InteractableRequestTimeout, System.Threading.Timeout.Infinite);
                }
            });

            // Return TaskCompletionSource
            return req.Task.Task;
        }


        /// <summary>
        /// 
        /// </summary>
        private class InteractableRequest : ICacheable<MongoObjectId>
        {
            /// <summary>
            /// Interactable Id
            /// </summary>
            public MongoObjectId Key { get; set; }

            // internal Type

            /// <summary>
            /// Task that will yield Player object
            /// </summary>
            internal TaskCompletionSource<ProjectERA.Data.Interactable> Task { get; private set; }

            /// <summary>
            /// Action that will run upon completion
            /// </summary>
            internal Action<ProjectERA.Data.Interactable> Action { get; private set; }

            /// <summary>
            /// Retrieval Timer
            /// </summary>
            internal Timer TimeOut { get; set; }

            /// <summary>
            /// Creation of Request
            /// </summary>
            internal DateTime Creation { get; private set; }

            /// <summary>
            /// Creates a new DataStoreRequest
            /// </summary>
            /// <param name="key">The key to request</param>
            public InteractableRequest(MongoObjectId key)
            {
                this.Key = key;
                this.Task = new TaskCompletionSource<ProjectERA.Data.Interactable>();
                this.Creation = DateTime.Now;
            }

             /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public InteractableRequest(MongoObjectId key, Action<ProjectERA.Data.Interactable> action)
                : this(key)
            {
                this.Action = action;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="player"></param>
            internal ProjectERA.Data.Interactable Result
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
