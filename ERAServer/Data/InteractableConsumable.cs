using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading;
using MongoDB.Bson;
using ERAServer.Data.Blueprint;
using ERAUtils;

namespace ERAServer.Data
{
    [Serializable]
    internal class InteractableConsumable : InteractableItem
    {
        /// <summary>
        /// Uses left
        /// </summary>
        [BsonRequired]
        public Byte Uses
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates an Interactable Consumable
        /// </summary>
        /// <param name="item">Blueprint id</param>
        /// <returns></returns>
        internal static new InteractableConsumable Generate(Int32 item)
        {
            Consumable blueprint = Consumable.GetBlocking(item) ?? Consumable.Empty;
            if (blueprint == null || blueprint.Id.Equals(ObjectId.Empty))
                throw new Exception("No such blueprint to generate InteractableItem from.");

            return Generate(blueprint);
        }

        /// <summary>
        /// Generates an Interactable Equipment
        /// </summary>
        /// <param name="item">Blueprint id</param>
        /// <param name="source">Source</param>
        /// <returns></returns>
        internal static new InteractableConsumable Generate(Int32 item, ObjectId source)
        {
            Consumable blueprint = Consumable.GetBlocking(item) ?? Consumable.Empty;
            if (blueprint == null || blueprint.Id.Equals(ObjectId.Empty))
                throw new Exception("No such blueprint to generate InteractableItem from.");

            return Generate(blueprint, source);
        }

        /// <summary>
        /// Generates an Interactable Equipment
        /// </summary>
        /// <param name="item">Blueprint</param>
        /// <returns></returns>
        internal static InteractableConsumable Generate(Consumable item)
        {
            InteractableConsumable result = new InteractableConsumable();
            InteractableItem.Generate(item, result);
            result.Uses = item.Uses;

            return result;
        }

        /// <summary>
        /// Generates an Interactable Equipment
        /// </summary>
        /// <param name="item">Blueprint</param>
        /// <param name="source">Source</param>
        /// <returns></returns>
        internal static InteractableConsumable Generate(Consumable item, ObjectId source)
        {
            InteractableConsumable result = Generate(item);
            result.OriginalInteractableId = source;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Consume()
        {
            Uses--; // TODO Threadsafety
        }

        internal void Stack(InteractableConsumable consumable)
        {
            if (!consumable.Equals(this))
                throw new ArgumentException("Can only stack consumables that have the same flags and id", "consumable");

            Byte move = (Byte)Math.Min(255 - consumable.Uses, this.Uses);
            consumable.Uses += move;
            this.Uses -= move;
        }
    }
}
