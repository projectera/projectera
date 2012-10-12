using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.SRP6
{
    /// <summary>
    /// SRPExceptions are thrown when there is an internal exception
    /// during processing data in the Secure Remote Password protocol.
    /// </summary>
    internal class SRPException : Exception
    {
        public SRPException(String message)
            :base(message)
        {

        }
    }
}
