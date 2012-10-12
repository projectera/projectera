using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectERA.Services.Display
{
    /// <summary>
    /// This interface is used if the object is focusable. All focusable
    /// objects can be snapped to with an ICamera2D interface.
    /// </summary>
    public interface IFocusable
    {
        /// <summary>
        /// Gets the position to snap to
        /// </summary>
        Vector3 Position { get; }
    }
}
