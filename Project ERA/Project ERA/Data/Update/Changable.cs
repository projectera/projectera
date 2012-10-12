using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using ProjectERA.Services.Data;
using ERAUtils.Logger;

namespace ProjectERA.Data.Update
{
    /// <summary>
    /// 
    /// </summary>
    public class Changable : IApplyable
    {
        private ConcurrentQueue<Action> _changes = new ConcurrentQueue<Action>();
        private Boolean _isChangeRegistered;

        /// <summary>
        /// Applies all registered changes
        /// </summary>
        public void Apply()
        {
            Action change;
            _isChangeRegistered = false;

            while (_changes.TryDequeue(out change))
                change.Invoke();
        }

        [Obsolete("Try using this.AddChange(() => { variable = 5; }); instead of creating a new change object.")]
        internal void AddChange(IApplyable change)
        {
            AddChange(() => { change.Apply(); });
        }

        /// <summary>
        /// Enqueues a new change and registers itself to be updated at the end of the draw
        /// </summary>
        /// <param name="change">The change to be enqueued</param>
        internal void AddChange(Action change)
        {
            if (!_isChangeRegistered)
            {
                _isChangeRegistered = true;
                DataManager.QueueChange(this);
            }

            _changes.Enqueue(change);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        [Obsolete("Try using this.AddChange(() => { variable = 5; }); instead of creating a new change object.")]
        internal void AddChange(String property, Object value)
        {
            #pragma warning disable 0618
            AddChange(new Change(this, property, value));
            #pragma warning restore 0618
        }
    }
}
