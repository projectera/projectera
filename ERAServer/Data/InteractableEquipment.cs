using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using MongoDB.Bson;
using ERAServer.Data.Blueprint;
using MongoDB.Driver;
using ERAUtils.Enum;

namespace ERAServer.Data
{
    [Serializable]
    internal class InteractableEquipment : InteractableItem
    {
        /// <summary>
        /// Integrity of the Weapon
        /// </summary>
        public Double Integrity 
        {
            get;
            set;
        }

        /// <summary>
        /// When true, can not be unequipped
        /// </summary>
        internal Boolean IsLocked
        {
            get { return this.ItemFlags.HasFlag(ItemFlags.Locked); }
        }

        /// <summary>
        /// Generates an Interactable Equipment
        /// </summary>
        /// <param name="item">Blueprint id</param>
        /// <returns></returns>
        internal static new InteractableEquipment Generate(Int32 item)
        {
            Equipment blueprint = Equipment.GetBlocking(item) ?? Equipment.Empty;
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
        internal static new InteractableEquipment Generate(Int32 item, ObjectId source)
        {
            Equipment blueprint = Equipment.GetBlocking(item) ?? Equipment.Empty;
            if (blueprint == null || blueprint.Id.Equals(ObjectId.Empty))
                throw new Exception("No such blueprint to generate InteractableItem from.");

            return Generate(blueprint, source);
        }

        /// <summary>
        /// Generates an Interactable Equipment
        /// </summary>
        /// <param name="item">Blueprint</param>
        /// <returns></returns>
        internal static InteractableEquipment Generate(Equipment item)
        {
            InteractableEquipment result = new InteractableEquipment();
            InteractableItem.Generate(item, result);
            result.Integrity = 1f;

            return result;
        }

        /// <summary>
        /// Generates an Interactable Equipment
        /// </summary>
        /// <param name="item">Blueprint</param>
        /// <param name="source">Source</param>
        /// <returns></returns>
        internal static InteractableEquipment Generate(Equipment item, ObjectId source)
        {
            InteractableEquipment result = Generate(item);
            result.OriginalInteractableId = source;

            return result;
        }

        /// <summary>
        /// Processes when equiped
        /// </summary>
        internal void Equip()
        {
            if (this.ItemFlags.HasFlag(ItemFlags.DefaultLocked))
                Lock();
        }

        /// <summary>
        /// Locks equipment
        /// </summary>
        internal void Lock()
        {
            this.ItemFlags |= ItemFlags.Locked;
        }

        /// <summary>
        /// Unlocks equipment
        /// </summary>
        internal void Unlock()
        {
            this.ItemFlags &= ~ItemFlags.Locked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override Lidgren.Network.NetOutgoingMessage Pack(ref Lidgren.Network.NetOutgoingMessage msg)
        {
            // First pack item stuff
            base.Pack(ref msg); // 43

            msg.Write(Convert.ToInt32(this.Integrity)); // 45
            return msg;
        }
    }
}
