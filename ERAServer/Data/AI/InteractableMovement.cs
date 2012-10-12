using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using ProjectERA.Protocols;

namespace ERAServer.Data.AI
{
    [Serializable]
    internal class InteractableMovement : InteractableComponent, IResetable
    {
        /// <summary>
        /// 
        /// </summary>
        public Byte MoveSpeed
        {
            get;
            set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Byte MoveFrequency
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte StopFrequency
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static InteractableMovement Generate()
        {
            InteractableMovement movement = new InteractableMovement();
            movement.MoveSpeed = 3;
            movement.MoveFrequency = 3;
            movement.StopFrequency = 0;
            return movement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        internal static InteractableMovement Generate(Interactable root)
        {
            InteractableMovement result = Generate();
            result.Root = root;
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
            this.MoveFrequency = 0;
            this.MoveSpeed = 0;
            this.StopFrequency = 0;
            this.Root = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        internal override void Pack(ref Lidgren.Network.NetOutgoingMessage msg)
        {
            // Write header
            msg.WriteRangedInteger(0, (Int32)InteractableAction.Max, (Int32)InteractableAction.Movement);

            // Write data
            msg.Write(this.MoveFrequency);
            msg.Write(this.StopFrequency);
            msg.Write(this.MoveSpeed);
        }
    }
}
