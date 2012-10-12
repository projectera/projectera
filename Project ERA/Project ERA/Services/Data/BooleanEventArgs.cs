using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Services.Data
{
    internal delegate void BooleanEventHandler(object sender, BooleanEventArgs e);

    /// <summary>
    /// Event arguments with one boolean value
    /// </summary>
    internal class BooleanEventArgs : EventArgs
    {
        /// <summary>
        /// New status
        /// </summary>
        public readonly Boolean Value;

        public BooleanEventArgs(Boolean value)
            : base()
        {
            this.Value = value;
        }

    }
}
