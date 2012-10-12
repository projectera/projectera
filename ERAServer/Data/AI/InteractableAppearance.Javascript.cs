using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.AI
{
    internal partial class InteractableAppearance : InteractableComponent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void AddPart(Interactable me, params Object[] args)
        {
            // First get component
            InteractableComponent result = me.GetComponent(typeof(InteractableAppearance));

            if (result == null)
                result = me.AddComponent(InteractableAppearance.Generate());

            // Now add part
            ((InteractableAppearance)result).AddPart(InteractableBodyPart.Generate(0, null, 0, 0, 0)); //javascript args
        }
    }
}
