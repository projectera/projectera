using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using ProjectERA.Protocols;
using ProjectERA.Data.Update;

namespace ProjectERA.Data
{
    [Serializable]
    internal class InteractableMovement : Changable, IInteractableComponent, IResetable
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
        public InteractableMovement()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public InteractableMovement(Lidgren.Network.NetIncomingMessage msg)
        {
            Unpack(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Expire()
        {
            Pool<InteractableMovement>.Recycle(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.MoveFrequency = 0;
            this.MoveSpeed = 0;
            this.StopFrequency = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void Unpack(Lidgren.Network.NetIncomingMessage msg)
        {
            this.MoveFrequency = msg.ReadByte();
            this.StopFrequency = msg.ReadByte();
            this.MoveSpeed = msg.ReadByte();
        }
    }
}
