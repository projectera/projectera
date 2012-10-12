using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.SRP6
{
    /// <summary>
    /// Exception while handshaking
    /// </summary>
    internal class HandShakeException : Exception
    {

        public HandShakeException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HandShakeException(String message)
            : base(message)
        {
        }
    }
}
