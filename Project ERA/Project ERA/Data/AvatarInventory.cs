using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using ERAUtils.Logger;

namespace ProjectERA.Data
{
    /// <summary>
    /// TODO: functions for resizing/dropping/adding bags
    /// TODO: Threadsafety
    /// </summary>
    internal class AvatarInventory : IResetable
    {
        private const Byte DefaultBagCapacity = 15;
        private List<ItemBag> _internalBags;

        /// <summary>
        /// Constructor
        /// </summary>
        public AvatarInventory()
        {
            _internalBags = new List<ItemBag>(5);

            ItemBag bag = Pool<ItemBag>.Fetch();
            bag.Initialize(DefaultBagCapacity);
            _internalBags.Add(bag);
        }

        /// <summary>
        /// Tries storing the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal Boolean Store(Item item)
        {
            try
            {
                for (Byte i = 0; i < _internalBags.Count; i++)
                    if (_internalBags[i].HasItem(Item.EmptyItem))
                        if (Store(item, i))
                            return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                // Threading: collection changed during iteration
                Logger.Verbose("Number of bags changed during iteration");
            }

            return false;
        }

        /// <summary>
        /// Tries storing item in the bag
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bag"></param>
        /// <remarks>item reference no longer usable after storage</remarks>
        /// <returns></returns>
        internal Boolean Store(Item item, Byte bag)
        {
            lock (_internalBags[bag])
                return _internalBags[bag].Store(item);
        }

        /// <summary>
        /// Tries withdrawing item from any bag
        /// </summary>
        /// <param name="item">Item to withdraw</param>
        /// <returns>Withdrawn item</returns>
        internal Item Withdraw(Item item)
        {
            try
            {
                for (Byte i = 0; i < _internalBags.Count; i++)
                    if (_internalBags[i].HasItem(item))
                    {
                        Item result = Withdraw(item, i);
                        if (result != null)
                            return result;
                    }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Threading: collection changed during iteration
                Logger.Verbose("Number of bags changed during iteration");
            }

            return null;
        }

        /// <summary>
        /// Tries transfering item fromBag toBag
        /// </summary>
        /// <param name="item">item to transfer</param>
        /// <param name="fromBag">source</param>
        /// <param name="toBag">destination</param>
        /// <returns>Sucession flag</returns>
        internal Boolean Transfer(Item item, Byte fromBag, Byte toBag)
        {
            // Get item from bag
            Item withdrawn = Withdraw(item, fromBag);
            // If couldn't get, transfer failed
            if (withdrawn == null)
                return false;
            // Store item to bag
            if (Store(withdrawn, toBag))
                return true;
            // If storing failed, try to restore
            if (!Store(withdrawn, fromBag))
                // Restoring failed, try to restore to another bag
                if (!Store(withdrawn))
                    throw new Exception("Item lost in transfer");
            // Transfer failed
            return false;
        }

        /// <summary>
        /// Tries withdrawing item from the bag
        /// </summary>
        /// <param name="item">Item to withdraw</param>
        /// <param name="bag">Bag to withdraw from</param>
        /// <returns>Sucession flag</returns> 
        internal Item Withdraw(Item item, Byte bag)
        {
            lock(_internalBags[bag])
                return _internalBags[bag].Withdraw(item);
        }

        /// <summary>
        /// Clears all items from all bags
        /// </summary>
        public void Clear()
        {
            foreach (ItemBag bag in _internalBags)
                Pool<ItemBag>.Recycle(bag);
            _internalBags.Clear();

            ItemBag somebag = Pool<ItemBag>.Fetch();
            somebag.Initialize(DefaultBagCapacity);
            _internalBags.Add(somebag);
        }
    }
}
