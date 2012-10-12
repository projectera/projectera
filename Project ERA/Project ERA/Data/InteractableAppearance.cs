using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using ERAUtils;
using ProjectERA.Protocols;
using ERAUtils.Enum;
using ProjectERA.Data.Update;

namespace ProjectERA.Data
{
    [Serializable]
    internal partial class InteractableAppearance : Changable, IInteractableComponent, IEnumerable, IResetable
    {
        private List<InteractableBodyPart> _bodyParts;

        [NonSerialized]
        private ReaderWriterLockSlim _rwLock;

        /// <summary>
        /// Returns the body part with type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal InteractableBodyPart this[BodyPart type]
        {
            get
            {
                try
                {
                    _rwLock.EnterReadLock();
                    return _bodyParts.First(bp => bp.Type == type);
                } finally {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                _rwLock.EnterWriteLock();
                if (_bodyParts.Any(bp => bp.Type == type))
                    _bodyParts[_bodyParts.FindIndex(bp => bp.Type == type)] = value;
                else
                    _bodyParts.Add(value);

                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns the body part at index
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal InteractableBodyPart this[Int32 index]
        {
            get 
            { 
                try
                {
                    _rwLock.EnterReadLock();
                    return _bodyParts[index]; 
                } finally {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            { 
                _rwLock.EnterWriteLock();
                _bodyParts[index] = value; 
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns the number of body parts
        /// </summary>
        internal Int32 Count
        {
            get { return _bodyParts.Count; }
        }

        /// <summary>
        /// Map Dir coord
        /// </summary>
        public Byte MapDir
        {
            get;
            set;
        }

        /// <summary>
        /// Initial Animation Frame
        /// </summary>
        public Byte AnimationFrame
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 TileId 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractableAppearance()
        {
            _rwLock = new ReaderWriterLockSlim();
            _bodyParts = new List<InteractableBodyPart>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public InteractableAppearance(Lidgren.Network.NetIncomingMessage msg)
            : this()
        {
            Unpack(msg);
        }

        /// <summary>
        /// Adds a part
        /// </summary>
        /// <param name="part"></param>
        internal void AddPart(InteractableBodyPart part)
        {
            _rwLock.EnterWriteLock();
            _bodyParts.Add(part);
            _rwLock.ExitWriteLock();

            Sort();
        }

        /// <summary>
        /// Removes a part
        /// </summary>
        /// <param name="part"></param>
        internal void RemovePart(InteractableBodyPart part)
        {
            _rwLock.EnterWriteLock();
            _bodyParts.Remove(part);
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Removes a part by type
        /// </summary>
        /// <param name="type"></param>
        internal void RemovePart(BodyPart type)
        {
            _rwLock.EnterWriteLock();
            InteractableBodyPart pending = _bodyParts.First(bp => bp.Type == type);
            _bodyParts.Remove(pending);
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="hash"></param>
        internal void RemovePart(BodyPart type, Int32 hash)
        {
            _rwLock.EnterWriteLock();
            InteractableBodyPart pending = _bodyParts.First(bp => bp.Type == type && bp.GetValueHashCode() == hash);
            _bodyParts.Remove(pending);
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Sorts the body parts by priority
        /// </summary>
        internal void Sort()
        {
 	        _bodyParts.Sort(InteractableBodyPart.CompareByPriority);
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator  GetEnumerator()
        {
            Sort();
            return _bodyParts.GetEnumerator();
        }

        /// <summary>
        /// Clears this component
        /// </summary>
        public void Clear()
        {
            _rwLock.EnterWriteLock();
            _bodyParts.Clear();
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Recycles this component and sub-components
        /// </summary>
        public void Expire()
        {
            Pool<InteractableAppearance>.Recycle(this);
        }

        /// <summary>
        /// 6
        /// </summary>
        /// <param name="msg"></param>
        public void Unpack(Lidgren.Network.NetIncomingMessage msg)
        {
            // Write map appearance
            this.MapDir = msg.ReadByte();
            this.AnimationFrame = msg.ReadByte();

            // Write avatar appearance
            lock (_bodyParts)
            {
                Int32 count = msg.ReadInt32();
                while(count-- > 0)
                {
                    InteractableBodyPart part = InteractableBodyPart.Unpack(msg);
                    if (part.Type.HasFlag(BodyPart.Tile))
                        this.TileId = part.IntegerValue ?? 0;

                    AddPart(part);   
                }
            }
        }
    }

}
