using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using ERAUtils;
using ProjectERA.Protocols;
using MongoDB.Bson.Serialization.Attributes;
using ERAUtils.Enum;

namespace ERAServer.Data.AI
{
    [Serializable]
    internal class InteractableBattler : InteractableComponent, IResetable
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public Int32 ClassId
        {
            get { return Class.BlueprintId; }
        }

        [BsonRequired]
        public InteractableClass Class
        {
            get;
            internal set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public Int32 RaceId
        {
            get;
            internal set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public Int32 MagicNumber
        {
            get;
            internal set;
        }

        /// <summary>
        /// Number of Experience Points
        /// </summary>
        [BsonRequired]
        public Int32 ExperiencePoints
        {
            get;
            internal set;
        }

        /// <summary>
        /// Number of Additional Points
        /// </summary>
        [BsonRequired]
        public Int32 AdditionalPoints
        {
            get;
            internal set;
        }

        /// <summary>
        /// Percentage of Health
        /// </summary>
        [BsonRequired]
        public Double HealthPoints
        {
            get;
            internal set;
        }

        /// <summary>
        /// Percentage of Concentration
        /// </summary>
        [BsonRequired]
        public Double ConcentrationPoints
        {
            get;
            internal set;
        }


        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public InteractableEquipment[] Equipment
        {
            get;
            internal set;
        }


        /// <summary>
        /// Generates a new battler
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static InteractableBattler Generate(Int32 classId, Int32 raceId)
        {
            InteractableBattler result = new InteractableBattler();
            result.MagicNumber = Lidgren.Network.NetRandom.Instance.NextInt();
            result.Class = InteractableClass.Generate(classId); 
            result.RaceId = raceId;
            result.Equipment = new InteractableEquipment[System.Enum.GetValues(typeof(EquipmentPart)).Length];

            result.ConcentrationPoints = 1f;
            result.HealthPoints = 1f;
            result.AdditionalPoints = 0;
            result.ExperiencePoints = 0;

            return result;
        }

        /// <summary>
        /// Generates a new battler
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static InteractableBattler Generate(Int32 classId, Int32 raceId, InteractableEquipment single)
        {
            InteractableBattler result = Generate(classId, raceId);
            result.Equip(single);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        internal static InteractableBattler Generate(Interactable root, Int32 classId, Int32 raceId)
        {
            InteractableBattler result = Generate(classId, raceId);
            result.Root = root;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        internal static InteractableBattler Generate(Interactable root, Int32 classId, Int32 raceId, InteractableEquipment single)
        {
            InteractableBattler result = Generate(classId, raceId, single);
            result.Root = root;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        internal Boolean Equip(InteractableEquipment equipment)
        {
            Data.Blueprint.Equipment blueprint = Data.Blueprint.Equipment.GetBlocking(equipment.BlueprintId);
            EquipmentPart part = blueprint.Part;

            lock (Equipment)
            {
                if (this.Equipment[(Byte)part] == null)
                {
                    this.Equipment[(Byte)part] = equipment;
                    if (this.Root != null)
                    {
                        (this.Root.GetComponent(typeof(InteractableAppearance)) as InteractableAppearance).AddPart(
                            blueprint.ItemType == ItemType.Weapon ?
                                InteractableBodyPart.GenerateWeapon(blueprint.Id) :
                                InteractableBodyPart.GenerateArmor(blueprint.Id, blueprint.Part)
                            );

                        this.Root.Put();

                        // broadcast TODO
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        internal Boolean Unequip(InteractableEquipment equipment)
        {
            Data.Blueprint.Equipment blueprint = Data.Blueprint.Equipment.GetBlocking(equipment.BlueprintId);
            EquipmentPart part = blueprint.Part;

            lock (Equipment)
            {
                if (this.Equipment[(Byte)part] != null)
                {
                    if (this.Equipment[(Byte)part].IsLocked)
                    {
                        return false;
                    }

                    if (this.Root != null)
                    {
                        InteractableBodyPart ibPart = null;
                        if (blueprint.ItemType == ItemType.Weapon)
                        {
                            ibPart = (this.Root.GetComponent(typeof(InteractableAppearance)) as InteractableAppearance).RemoveWeapon(blueprint.Id);
                        }
                        else if (blueprint.ItemType == ItemType.Armor || blueprint.ItemType == ItemType.Shield)
                        {
                            ibPart = (this.Root.GetComponent(typeof(InteractableAppearance)) as InteractableAppearance).RemoveArmor(blueprint.Id);
                        }

                        this.Root.Put();

                        if (ibPart != null)
                        {
                            // broadcast TODO
                        }
                    }

                    this.Equipment[(Byte)part] = null;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Expires this battler
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
            foreach (var item in this.Equipment)
                item.Clear();

            this.Equipment = new InteractableEquipment[this.Equipment.Length];

            this.Class = null;
            this.RaceId = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        internal override void Pack(ref Lidgren.Network.NetOutgoingMessage msg)
        {
            // Write header
            msg.WriteRangedInteger(0, (Int32)InteractableAction.Max, (Int32)InteractableAction.Battler);

            // Write data
            msg.Write(this.ClassId);
            msg.Write(this.RaceId);
            msg.Write(this.MagicNumber);

            msg.Write(ERAUtils.BitManipulation.CompressUnitDouble(this.HealthPoints, 24), 24);
            msg.Write(ERAUtils.BitManipulation.CompressUnitDouble(this.ConcentrationPoints, 24), 24);
            msg.Write(this.ExperiencePoints);
            msg.Write(this.AdditionalPoints);

            msg.Write(this.Equipment.Count(a => a != null));
            foreach (var equipment in this.Equipment)
            {
                if (equipment != null)
                {
                    equipment.Pack(ref msg);
                }
            }
        }
    }
}
