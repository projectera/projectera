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
using ProjectERA.Data.Update;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;


namespace ProjectERA.Services.Data
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    internal partial class DataManager : Microsoft.Xna.Framework.GameComponent
    {
        private static ConcurrentQueue<IApplyable> _changes;
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        internal DataManager(Game game)
            : base(game)
        {
            Initialize();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            this.Enabled = false;

            //SynchronisationSetup();
            //PlayersSetup();
            _changes = new ConcurrentQueue<IApplyable>();

            base.Initialize();
        }

        /// <summary>
        /// Queues a new change to be integrated
        /// </summary>
        /// <param name="change">The Change to be integrated</param>
        internal static void QueueChange(IApplyable change)
        {
            _changes.Enqueue(change);
        }

        /// <summary>
        /// Integrates all the changes that were queued and waits for them to finish
        /// </summary>
        internal static void IntegrateChanges()
        {
           /* Boolean hasChangesLock = false;
            try
            {
                Monitor.TryEnter(_changes, ref hasChangesLock);
                if (hasChangesLock)
                {*/
              
                // Start queuing
                //List<Task> _currentChanges = new List<Task>();

                IApplyable[] changes;
                lock (_changes)
                {
                    changes = _changes.ToArray();
                    IApplyable dummy;
                    while(_changes.TryDequeue(out dummy))
                    {

                    }
                }

                Parallel.ForEach(changes, a => a.Apply());
                    
                /*while (_changes.TryDequeue(out change))
                {
                    // Copies change (thread safety)
                    IApplyable thisChange = change;
                    _currentChanges.Add(Task.Factory.StartNew(() => { thisChange.Apply(); }, TaskCreationOptions.PreferFairness));
                }

                if (!_changes.IsEmpty)
                    _currentChanges.Add(Task.Factory.StartNew(() => { IntegrateChanges(); }, TaskCreationOptions.PreferFairness));

                // Wait for all changes to be applied
                Task.WaitAll(_currentChanges.ToArray()); */
                 
            /*    }
            }
            finally
            {
                if (hasChangesLock)
                    Monitor.Exit(_changes);
            }*/

        }
    }
}
