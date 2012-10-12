using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;

namespace ProjectERA.Data
{
    internal class ItemBag : IResetable
    {
        
        // DISCUSS: Arrays or dictionairy?
        private Item[] _items;
        private Byte _capacity;

        /// <summary>
        /// 
        /// </summary>
        public ItemBag()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity"></param>
        internal ItemBag(Byte capacity)
        {
            Initialize(capacity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        internal void Initialize(Byte capacity)
        {
            _capacity = capacity;

            _items = new Item[_capacity];
            for (Byte i = 0; i < _capacity; i++)
                _items[i] = Item.EmptyItem;
        }

        // DISCUSS: Functions/Properties?

        /// <summary>
        /// Clears the bag
        /// </summary>
        public void Clear()
        {
            foreach(Item item in _items)
                if (item.Equals(Item.EmptyItem) == false)
                    Pool<Item>.Recycle(item);

            _items = new Item[_capacity];
            for (Byte i = 0; i < _capacity; i++)
                _items[i] = Item.EmptyItem;
        }
        
        /// <summary>
        /// There is one+ item in the bag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal Boolean HasItem(Item item)
        {
            for (Byte i = 0; i < _capacity; i++)
                if (_items[i].Equals(item))
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
                if (_items[i].Equals(item))
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
                if (_items[i].Equals(item))
                    count[i] = true;

            return count;
        }

        /// <summary>
        /// Tries storing item in this bag
        /// </summary>
        /// <param name="item">item to store</param>
        /// <returns>Succession flag</returns>
        internal Boolean Store(Item item)
        {
            for (Byte i = 0; i < _capacity; i++)
                if (_items[i].Equals(Item.EmptyItem))
                    return Store(item, i);

            return false;
        }

        /// <summary>
        /// Tries storing item in this bag
        /// </summary>
        /// <param name="item">item to store</param>
        /// <param name="slot">slot to store to</param>
        /// <returns>Succession flag</returns>
        internal Boolean Store(Item item, Byte slot)
        {
            if (_items[slot].Equals(Item.EmptyItem))
            {
                _items[slot] = item;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to withdraw item from a bag
        /// </summary>
        /// <param name="item">item to withdraw</param>
        /// <returns>Withdrawn item</returns>
        internal Item Withdraw(Item item)
        {
            for (Byte i = 0; i < _capacity; i++)
                if (_items[i].Equals(item))
                {
                    Item result = Withdraw(item, i);
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
        internal Item Withdraw(Item item, Byte slot)
        {
            if (_items[slot].Equals(item))
            {
                return _items[slot];
            }

            return null;
        }
    }
}
