using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Services.Display
{
    internal enum ScreenState
    {
        /// <summary>
        /// 
        /// </summary>
        Hidden,
        /// <summary>
        /// 
        /// </summary>
        TransitionOn,
        /// <summary>
        /// 
        /// </summary>
        TransitionOff,
        /// <summary>
        /// 
        /// </summary>
        Active,

        /// <summary>
        /// 
        /// </summary>
        WaitingForTransition
    }
}
