using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ERAServer.Data.Blueprint;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
using System.Collections;
using Lidgren.Network;
using ERAServer.Services;

namespace ERAServer.Data
{
    internal class ItemBag : IEnumerable
    {
        /// <summary>
        /// Item Bag Id
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Current owner (interactable Id)
        /// </summary>
        [BsonRequired]
        public ObjectId Owner
        {
            get;
            private set;
        }

        [BsonElement("Items")]
        private InteractableItem[] _items;

        [BsonElement("Capacity")]
        private Byte _capacity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity"></param>
        private ItemBag(Byte capacity)
        {
            _capacity = capacity;

            _items = new InteractableItem[_capacity];
            for (Byte i = 0; i < _capacity; i++)
                _items[i] = InteractableItem.EmptyItem;
        }

        /// <summary>
        /// Generates a new ItemBag with owner and capacity
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        internal static ItemBag Generate(ObjectId owner, Byte capacity)
        {
            ItemBag result = new ItemBag(capacity);
            result.Id = ObjectId.GenerateNewId();
            result.Owner = owner;

            return result;
        }

        /// <summary>
        /// Generates a new ItemBag with owner and capacity
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        internal static ItemBag Generate(Interactable owner, Byte capacity)
        {
            return Generate(owner.Id, capacity);
        }

        /// <summary>
        /// There is one+ item in the bag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal Boolean HasItem(Item item)
        {
            for (Byte i = 0; i < _capacity; i++)
                if (item.HasGenerated(_items[i]))
                    return true;

            return false;
        }

        /// <summary>
        /// There is one+ item in the bag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal Boolean HasItem(InteractableItem item)
        {
            for (Byte i = 0; i < _capacity; i++)
                if (item.Equals(_items[i]))
                    return true;

            return false;
        }

        /// <summary>
        /// Total number of item in the bag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal Int32 ItemCount(Item item)
        {
            Int32 count = 0;

            for (Byte i = 0; i < _capacity; i++)
                if (item.HasGenerated(_items[i]))
                    count++;

            return count;
        }

        /// <summary>
        /// Total number of item in the bag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal Int32 ItemCount(InteractableItem item)
        {
            Int32 count = 0;

            for (Byte i = 0; i < _capacity; i++)
                if (item.Equals(_items[i]))
                    count++;

            return count;
        }

        /// <summary>
        /// Total number of item per slot
        /// </summary>
        /// <param name="item">item to find</param>
        /// <returns>Array of quantities per slot</returns>
        internal Boolean[] FindItem(Item item)
        {
            Boolean[] count = new Boolean[_capacity];

            for (Byte i = 0; i < _capacity; i++)
                if (item.HasGenerated(_items[i]))
                    count[i] = true;

            return count;
        }

        /// <summary>
        /// Total number of item per slot
        /// </summary>
        /// <param name="item">item to find</param>
        /// <returns>Array of quantities per slot</returns>
        internal Boolean[] FindItem(InteractableItem item)
        {
            Boolean[] count = new Boolean[_capacity];

            for (Byte i = 0; i < _capacity; i++)
                if (item.Equals(_items[i]))
                    count[i] = true;

            return count;
        }

        /// <summary>
        /// Tries storing item in this bag
        /// </summary>
        /// <param name="item">item to store</param>
        /// <returns>Succession flag</returns>
        internal Boolean Store(InteractableItem item)
        {
            for (Byte i = 0; i < _capacity; i++)
                if (_items[i].Equals(InteractableItem.EmptyItem))
                    return Store(item, i);

            return false;
        }

        /// <summary>
        /// Tries storing item in this bag
        /// </summary>
        /// <param name="item">item to store</param>
        /// <param name="slot">slot to store to</param>
        /// <returns>Succession flag</returns>
        internal Boolean Store(InteractableItem item, Byte slot)
        {
            if (_items[slot].Equals(InteractableItem.EmptyItem))
            {
                _items[slot] = item;

                GetCollection().Update(Query.EQ("_id", this.Id), Update.Set("Items." + slot, item.ToBsonDocument<InteractableItem>()));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to withdraw item from a bag
        /// </summary>
        /// <param name="item">item to withdraw</param>
        /// <returns>Withdrawn item</returns>
        internal InteractableItem Withdraw(InteractableItem item)
        {
            for (Byte i = 0; i < _capacity; i++)
                if (_items[i].Equals(item))
                {
                    InteractableItem result = Withdraw(item, i);
                    if (result != null)
                        return result;
                }

            return null;
        }

        /// <summary>
        /// Tries to withdraw item from a bag
        /// </summary>
        /// <param name="item">item to withdraw</param>
        /// <param name="slot">slot to withdraw from</param>
        /// <returns>Withdrawn item</returns>
        internal InteractableItem Withdraw(InteractableItem item, Byte slot)
        {
            if (_items[slot].Equals(item))
            {
                GetCollection().Update(Query.EQ("_id", this.Id), Update.Unset("Items." + slot));
                InteractableItem removed = _items[slot];
                _items[slot] = null;
                return removed;
            }

            return null;
        }

        /// <summary>
        /// Gets an itembag from the db
        /// </summary>
        /// <param name="id">id of itembag to get</param>
        /// <returns></returns>
        public static Task<ItemBag> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets itemsbags form interactable from the db
        /// </summary>
        /// <param name="id">id of interactable to get bags for</param>
        /// <returns></returns>
        public static Task<ItemBag[]> GetAllForInteractable(ObjectId id)
        {
            return Task.Factory.StartNew(() =>
            {
                MongoCursor<ItemBag> cursor = GetCollection().Find(Query.EQ("Owner", id));
                return cursor.ToArray();
            });
        }

        /// <summary>
        /// Gets an itembag from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of itembag to get</param>
        /// <returns></returns>
        public static ItemBag GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as ItemBag;
        }

        /// <summary>
        /// Gets the players collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<ItemBag> GetCollection()
        {
            return DataManager.Database.GetCollection<ItemBag>("ItemBags");
        }

        /// <summary>
        /// Puts a player to the db
        /// </summary>
        public virtual void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts an item to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        public virtual SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<ItemBag>(this, safemode);
        }

        /// <summary>
        /// Clears the bag
        /// </summary>
        public void Clear()
        {
            foreach (InteractableItem item in _items)
                if (item.Equals(InteractableItem.EmptyItem) == false)
                    item.Clear();

            _items = new InteractableItem[_capacity];
            for (Byte i = 0; i < _capacity; i++)
                _items[i] = InteractableItem.EmptyItem;
        }

        /// <summary>
        /// Returns the Enumerator for the bag
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns the Enumerator for the Bag
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
        {
            msg.Write(this.Id.ToByteArray());
            msg.Write(this._capacity);
            foreach (var item in this._items)
            {
                msg.Write(item != null);
                if (item != null)
                {
                    msg = item.Pack(ref msg);
                }
            }

            return msg;
        }
    }
}
