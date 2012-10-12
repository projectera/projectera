using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using ProjectERA.Protocols;
using ProjectERA.Data.Update;
using System.Collections;

namespace ProjectERA.Data
{
    [Serializable]
    internal class InteractableBattler : Changable, IInteractableComponent, IResetable
    {
        /// <summary>
        /// Class ID
        /// </summary>
        public Int32 ClassId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Race ID
        /// </summary>
        public Int32 RaceId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Magic Number
        /// </summary>
        public Int32 MagicNumber
        {
            get;
            private set;
        }
           
        /// <summary>
        /// Number of Experience Points
        /// </summary>
        public Int32 ExperiencePoints
        {
            get;
            internal set;
        }

        /// <summary>
        /// Number of Additional Points
        /// </summary>
        public Int32 AdditionalPoints
        {
            get;
            internal set;
        }

        /// <summary>
        /// Percentage of Health
        /// </summary>
        public Double HealthPoints
        {
            get;
            internal set;
        }

        /// <summary>
        /// Percentage of Concentration
        /// </summary>
        public Double ConcentrationPoints
        {
            get;
            internal set;
        }

        /// <summary>
        /// Equipment List
        /// </summary>
        public List<Data.Equipment> Equipment
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns current level
        /// </summary>
        public Int32 Level
        {
            get { return 1;  }
        }

        /// <summary>
        /// Returns level progress
        /// </summary>
        public Double LevelProgress
        {
            get { return 0.62f; }
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractableBattler()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public InteractableBattler(Lidgren.Network.NetIncomingMessage msg)
            : this()
        {
            Unpack(msg);
        }

        /// <summary>
        /// Expires this battler
        /// </summary>
        public void Expire()
        {
            Pool<InteractableBattler>.Recycle(this);
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return this.Equipment.GetEnumerator();
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
        public void Unpack(Lidgren.Network.NetIncomingMessage msg)
        {
            this.ClassId = msg.ReadInt32();
            this.RaceId = msg.ReadInt32();
            this.MagicNumber = msg.ReadInt32();
            
            this.HealthPoints = ERAUtils.BitManipulation.DecompressUnitDouble(msg.ReadUInt32(24), 24);
            this.ConcentrationPoints = ERAUtils.BitManipulation.DecompressUnitDouble(msg.ReadUInt32(24), 24);
            this.ExperiencePoints = msg.ReadInt32();
            this.AdditionalPoints = msg.ReadInt32();

            this.Equipment = new List<Equipment>(msg.ReadInt32());
            for (Int32 i = 0; i < this.Equipment.Capacity; i++)
            {
                this.Equipment.Add(Data.Equipment.Unpack(msg));
            }
        }
    }
}
