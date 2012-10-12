using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Services.Data
{
    internal delegate void IntegerEventHandler(object sender, IntegerEventArgs e);

    /// <summary>
    /// IntegerEventArgs with one Int32 value
    /// </summary>
    internal class IntegerEventArgs : EventArgs
    {
        /// <summary>
        /// New status
        /// </summary>
        public readonly Int32 Value;

        public IntegerEventArgs(Int32 value)
            : base()
        {
            this.Value = value;
        }

    }
}
