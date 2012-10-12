using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ERAUtils.Enum;
using ERAServer.Data.Blueprint;
using Lidgren.Network;

namespace ERAServer.Data
{
    [BsonDiscriminator(RootClass=true)]
    [BsonKnownTypes(typeof(InteractableConsumable), typeof(InteractableEquipment))]
    internal class InteractableItem : IResetable
    {
        public static readonly InteractableItem EmptyItem = new InteractableItem();

        /// <summary>
        /// InteractableItem Id
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Database (Blueprint) Item Id
        /// </summary>
        [BsonRequired]
        public Int32 BlueprintId
        {
            get;
            private set;
        }

        /// <summary>
        /// Interactable id traded/obtained/looted
        /// </summary>
        public ObjectId OriginalInteractableId
        {
            get;
            protected set;
        }

        /// <summary>
        /// Price upon buy
        /// </summary>
        [BsonRequired]
        public Double PriceBought
        {
            get;
            private set;
        }

        /// <summary>
        /// Item Flags
        /// </summary>
        [BsonRequired]
        public ItemFlags ItemFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Item Type
        /// </summary>
        [BsonRequired]
        public ItemType ItemType
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates an Interactable Item
        /// </summary>
        /// <param name="item">Blueprint id</param>
        /// <returns></returns>
        internal static InteractableItem Generate(Int32 item)
        {
            Item blueprint = Item.GetBlocking(item) ?? Item.Empty;
            if (blueprint == null || blueprint.Id.Equals(ObjectId.Empty))
                throw new Exception("No such blueprint to generate InteractableItem from.");

            return Generate(blueprint);
        }

        /// <summary>
        /// Generates an Interactable Item
        /// </summary>
        /// <param name="item">Blueprint id</param>
        /// <param name="source">Source</param>
        /// <returns></returns>
        internal static InteractableItem Generate(Int32 item, ObjectId source)
        {
            Item blueprint = Item.GetBlocking(item) ?? Item.Empty;
            if (blueprint == null || blueprint.Id.Equals(ObjectId.Empty))
                throw new Exception("No such blueprint to generate InteractableItem from.");

            return Generate(blueprint, source);
        }

        /// <summary>
        /// Generates an Interactable Item
        /// </summary>
        /// <param name="item">Blueprint</param>
        /// <returns></returns>
        internal static InteractableItem Generate(Item item)
        {
            InteractableItem result = new InteractableItem();
            return Generate(item, result);
        }

        /// <summary>
        /// Generates an Interactable Item
        /// </summary>
        /// <param name="item">Blueprint</param>
        /// <param name="source">Source</param>
        /// <returns></returns>
        internal static InteractableItem Generate(Item item, ObjectId source)
        {
            InteractableItem result = Generate(item);
            result.OriginalInteractableId = source;

            return result;
        }

        /// <summary>
        /// Generates an Interactable Item, sets the values on the result
        /// </summary>
        /// <param name="item">Blueprint</param>
        /// <param name="result">Destination</param>
        protected static InteractableItem Generate(Item item, InteractableItem result)
        {
            result.Id = ObjectId.GenerateNewId();
            result.BlueprintId = item.Id;
            result.ItemFlags = item.ItemFlags;
            result.ItemType = item.ItemType;

            return result;
        }

        /// <summary>
        /// Transfers item (sets bought price and source)
        /// </summary>
        /// <param name="price">price transfer</param>
        /// <param name="source">origin</param>
        internal void Transfer(Double price, ObjectId source)
        {
            this.PriceBought = price;
            this.OriginalInteractableId = source;
        }

        /// <summary>
        /// Clears this instance
        /// </summary>
        public void Clear()
        {
            this.Id = ObjectId.Empty;
            this.BlueprintId = 0;
            this.OriginalInteractableId = ObjectId.Empty;
            this.PriceBought = 0;
            this.ItemFlags = ItemFlags.None;
            this.ItemType = ItemType.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public virtual NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
        {
            msg.Write(this.Id.ToByteArray());
            if (this is InteractableEquipment)
                msg.Write(Blueprint.Equipment.GetBlocking(this.BlueprintId).Id);
            else
                msg.Write(Blueprint.Item.GetBlocking(this.BlueprintId).Id);
            msg.Write(this.OriginalInteractableId.ToByteArray()); // 36
            msg.Write(Convert.ToInt32(this.PriceBought)); // 40
            msg.Write((UInt16)this.ItemFlags); // 42
            msg.Write((Byte)this.ItemType); // 43
            return msg;
        }
    }
}
