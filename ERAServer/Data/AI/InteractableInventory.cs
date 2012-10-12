using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAServer.Data.AI;
using ERAUtils;
using MongoDB.Bson;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace ERAServer.Data.AI
{
    [Serializable]
    internal class InteractableInventory : InteractableComponent, IResetable
    {
        /// <summary>
        /// Bags in inventory
        /// </summary>
        [BsonIgnore]
        public List<ItemBag> Bags
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates a new bag pool
        /// </summary>
        /// <returns></returns>
        internal static InteractableInventory Generate(ObjectId owner)
        {
            InteractableInventory result = new InteractableInventory();
            result.Bags = new List<ItemBag>(1);
            result.Bags.Add(ItemBag.Generate(owner, 9));

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        internal static Task<InteractableInventory> Get(ObjectId owner)
        {
            return Task<InteractableInventory>.Factory.StartNew(() => { return GetBlocking(owner); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        internal static InteractableInventory GetBlocking(ObjectId owner)
        {
            InteractableInventory result = new InteractableInventory();
            ItemBag[] bags = ItemBag.GetAllForInteractable(owner).Result;
            foreach (var bag in bags)
            {
                result.Bags.Add(bag);
            }

            return result; 
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Expire()
        {
            this.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        internal override void Pack(ref Lidgren.Network.NetOutgoingMessage msg)
        {
            msg.Write(this.Bags.Count);
            foreach (var bag in this.Bags)
                msg = bag.Pack(ref msg);
        }
    }
}
