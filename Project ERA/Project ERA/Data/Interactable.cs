using System;
using System.Collections.Generic;
using ERAUtils;
using System.Threading.Tasks;
using ERAUtils.Enum;
using ProjectERA.Data.Update;
using ProjectERA.Services.Data;

namespace ProjectERA.Data
{
    [Serializable]
    internal partial class Interactable : Changable, IResetable
    {
        private Boolean _blockingFlag;
        private Queue<DialogueMessage> _messageQueue;
        
        /// <summary>
        /// Interactable ID
        /// </summary>
        public MongoObjectId Id
        {
            get;
            set;
        }

        /// <summary>
        /// Interactable Name
        /// </summary>
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Map Id
        /// </summary>
        public MongoObjectId MapId
        {
            get;
            set;
        }

        /// <summary>
        /// Map X coord
        /// </summary>
        public Int32 MapX
        {
            get;
            set;
        }

        /// <summary>
        /// Map Y coord
        /// </summary>
        public Int32 MapY
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal Queue<DialogueMessage> MessageQueue
        {
            get
            {
                return _messageQueue;
            }
            set
            {
                _messageQueue = value;
            }
        }

        /// <summary>
        /// Interactable State Flags
        /// </summary>
        public InteractableStateFlags StateFlags
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsBlocking
        {
            get { return !StateFlags.HasFlag(InteractableStateFlags.Through) && _blockingFlag; }
            set { _blockingFlag = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsTransparantBlocking
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte PassagesBits
        {
            get;
            set;
        }

        /// <summary>
        /// Holds the Interactable Components
        /// </summary>
        /// <remarks>Key is String because JSON requires Dictionairies to have String Keys</remarks>
        public Dictionary<String, IInteractableComponent> Components
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean HasAppearance
        {
            get { return this.Components.ContainsKey(typeof(InteractableAppearance).Name); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean HasMovement
        {
            get { return this.Components.ContainsKey(typeof(InteractableMovement).Name); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean HasBattler
        {
            get { return this.Components.ContainsKey(typeof(InteractableBattler).Name); }
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractableAppearance Appearance
        {
            get { return GetComponent(typeof(InteractableAppearance)) as InteractableAppearance; }
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractableMovement Movement
        {
            get { return GetComponent(typeof(InteractableMovement)) as InteractableMovement; }
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractableBattler Battler
        {
            get { return GetComponent(typeof(InteractableBattler)) as InteractableBattler; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public Interactable()
        {
            this.Components = new Dictionary<String, IInteractableComponent>();
            this.MessageQueue = new Queue<DialogueMessage>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapData"></param>
        internal void Initialize(MapData mapData)
        {
            SetBlocking(mapData.TilesetData);
            mapData.Interactables.Add(this.Id, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tilesetData"></param>
        internal void SetBlocking(Services.Data.TilesetData tilesetData)
        {
            this.IsTransparantBlocking = this.HasAppearance && tilesetData.Priorities[this.Appearance.TileId] == 0 && tilesetData.Flags != null && ((TilesetFlags)tilesetData.Flags[this.Appearance.TileId]).HasFlag(TilesetFlags.TransparantPassability);
            this.IsBlocking |= (this.HasAppearance && (this.Appearance.Count != 0));
            this.IsBlocking |= (this.HasAppearance && tilesetData.Flags != null && ((TilesetFlags)tilesetData.Flags[this.Appearance.TileId]).HasFlag(TilesetFlags.Blocking));
            this.PassagesBits = this.HasAppearance ? tilesetData.Passages[this.Appearance.TileId] : (Byte)0;
        }

        /// <summary>
        /// Add an AI component
        /// </summary>
        /// <param name="component"></param>
        internal IInteractableComponent AddComponent(IInteractableComponent component)
        {
            this.Components.Add(component.GetType().Name, component);
            return component;
        }

        /// <summary>
        /// Gets an AI component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        internal IInteractableComponent GetComponent(Type component)
        {
            IInteractableComponent result;

            if (this.Components.TryGetValue(component.Name, out result))
                return result;

            return null;
        }

        /// <summary>
        /// Clears for pool
        /// </summary>
        public void Clear()
        {
            this.Id = MongoObjectId.Empty;

            // Expires all components. Some will be collected by pools, some will
            // expire unmanaged resources. 
            foreach (IInteractableComponent component in this.Components.Values)
                component.Expire();

            this.Components.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        internal void AddMessage(String message)
        {
            System.Threading.Interlocked.CompareExchange(ref _messageQueue, new Queue<DialogueMessage>(), null);
            this.MessageQueue.Enqueue(DialogueMessage.Generate(message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal Boolean RemoveMessage(out String message)
        {
            if (this.MessageQueue.Count == 0)
            {
                message = String.Empty;
                return false;
            }

            DialogueMessage dm = this.MessageQueue.Dequeue();

            if (dm == null)
            {
                message = String.Empty;
                return false;
            }

            message = dm.Contents;
            return true;
     
        }
    }
}
