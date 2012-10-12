using System;
using System.Collections.Generic;
using ERAUtils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Threading.Tasks;
using ERAUtils.Enum;
using ERAServer.Data.AI;
using ERAServer.Services;
using ERAServer.Scripts;
using ERAServer.Scripts.Packages;

namespace ERAServer.Data
{
    [Serializable]
    internal partial class Interactable : IResetable
    {
        /// <summary>
        /// Interactable ID
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Interactable Name
        /// </summary>
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Interactable State Flags
        /// </summary>
        [BsonRequired]
        public InteractableStateFlags StateFlags
        {
            get;
            private set;
        }

        /// <summary>
        /// Map Id
        /// </summary>
        [BsonRequired]
        public ObjectId MapId
        {
            get;
            private set;
        }

        /// <summary>
        /// Map X coord
        /// </summary>
        [BsonRequired]
        public Int32 MapX
        {
            get;
            set;
        }

        /// <summary>
        /// Map Y coord
        /// </summary>
        [BsonRequired]
        public Int32 MapY
        {
            get;
            set;
        }

        /// <summary>
        /// Holds the Interactable Components
        /// </summary>
        /// <remarks>Key is String because JSON requires Dictionairies to have String Keys</remarks>
        public Dictionary<String, InteractableComponent> Components
        {
            get;
            set;
        }

        /// <summary>
        /// Handles all incoming messages and controls the interactable
        /// </summary>
        [BsonRequired]
        public MessageController Controller { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Interactable()
        {
            this.Components = new Dictionary<String, InteractableComponent>();
            this.Controller = new MessageController();
        }

        /// <summary>
        /// Generates an interactable using a blueprint flags and type
        /// </summary>
        /// <param name="blueprint"></param>
        /// <returns></returns>
        internal static Interactable Generate(Interactable blueprint)
        {  

            return blueprint;
        }

        /// <summary>
        /// Add an AI component
        /// </summary>
        /// <param name="component"></param>
        internal InteractableComponent AddComponent(InteractableComponent component)
        {
            this.Components.Add(component.GetType().Name, component);
            return component;
        }

        /// <summary>
        /// Gets an AI component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        internal InteractableComponent GetComponent(Type component)
        {
            InteractableComponent result;

            if (this.Components.TryGetValue(component.Name, out result))
                return result;

            return null;
        }

        /// <summary>
        /// Gets a interactable from the db
        /// </summary>
        /// <param name="id">id of interactable to get</param>
        /// <returns></returns>
        internal static Task<Interactable> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets a interactable from the db
        /// </summary>
        /// <param name="name">name of interactable to get</param>
        /// <returns></returns>
        internal static Task<Interactable> Get(String name)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Gets a Interactable from the db,  blocks while retrieving
        /// </summary>
        /// <param name="id">id of Interactable to get</param>
        /// <returns></returns>
        internal static Interactable GetBlocking(ObjectId id)
        {
            Interactable result = GetCollection().FindOneById(id) as Interactable;
            foreach (var component in result.Components.Values)
                component.Root = result;

            result.Controller.Setup(result);
            return result;
        }

        /// <summary>
        /// Gets a Interactable from the db, blocks while retrieving
        /// </summary>
        /// <param name="name">name of Interactable to get</param>
        /// <returns></returns>
        internal static Interactable GetBlocking(String name)
        {
            return GetCollection().FindOneAs<Interactable>(Query.Matches("Name", new BsonRegularExpression("^(?i)" + name + "$"))) as Interactable;
        }

        /// <summary>
        /// Gets the players collection
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Interactable> GetCollection()
        {
            return DataManager.Database.GetCollection<Interactable>("Interactables");
        }

        /// <summary>
        /// Puts a Interactable to the db
        /// </summary>
        internal void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a Interactable to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Interactable>(this, safemode);
        }

        /// <summary>
        /// Clears for pool
        /// </summary>
        public void Clear()
        {
            this.Id = ObjectId.Empty;

            // Expires all components. Some will be collected by pools, some will
            // expire unmanaged resources. 
            foreach (InteractableComponent component in this.Components.Values)
                component.Expire();

            this.Components.Clear();
        }
    }
}
