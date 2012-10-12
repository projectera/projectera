using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;

namespace ERAServer.Data.AI
{
    internal class InteractableHumanBattler : InteractableBattler, IResetable
    {
        /// <summary>
        /// 
        /// </summary>
        public Byte ClassId
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte RaceId
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static InteractableHumanBattler Generate()
        {
            InteractableHumanBattler result = Pool<InteractableHumanBattler>.Fetch();
            InteractableBattler.Generate(result);

            // Random initialization? Or direct fetch from different shizzle? 

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public new void Clear()
        {
            base.Clear();
        }
    }
}
