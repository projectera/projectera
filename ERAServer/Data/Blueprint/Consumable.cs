using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ERAServer.Data.Blueprint
{
    [Serializable]
    internal class Consumable : Item
    {
        internal static new readonly Consumable Empty = new Consumable();

        /// <summary>
        /// Number of uses
        /// </summary>
        [BsonRequired]
        public Byte Uses
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public BattlerConsumable ConsumingModifier
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates a Database Item
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="iconAssetName">Icon Asset Name</param>
        /// <param name="price">Price</param>
        /// <param name="flags">Flags</param>
        /// <param name="consumingModifier">Interactable modifications on use</param>
        /// <returns>Generated item</returns>
        /// <remarks>Generates a ItemType.Item (Consumable). To Generate different types, call their generate methods</remarks>
        internal static Consumable Generate(String name, String iconAssetName, Double price, ItemFlags flags, Byte uses, BattlerConsumable consumingModifier)
        {
            return Generate(name, Description.Empty, iconAssetName, price, flags, uses, consumingModifier);
        }

        /// <summary>
        /// Generates a Database Item
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description"></param>
        /// <param name="iconAssetName">Icon Asset Name</param>
        /// <param name="price">Price</param>
        /// <param name="flags">Flags</param>
        /// <param name="consumingModifier">Interactable modifications on use</param>
        /// <returns>Generated item</returns>
        /// <remarks>Generates a ItemType.Item (Consumable). To Generate different types, call their generate methods</remarks>
        internal static Consumable Generate(String name, Description description, String iconAssetName, Double price, ItemFlags flags, Byte uses, BattlerConsumable consumingModifier)
        {
            Consumable result = new Consumable(); // Pool<Item>.Fetch();
            Item.Generate(name, description, iconAssetName, price, flags, result);

            result.ItemFlags &= ~ItemFlags.NoConsume; // Must be consumable
            result.Uses = uses;
            result.ConsumingModifier = consumingModifier;

            return result;
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static new Consumable GetBlocking(Int32 id)
        {
            return GetCollection().FindOneById(id) as Consumable;
        }
    }
}
