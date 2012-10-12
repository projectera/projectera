using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Logger
{
    public enum Severity : byte
    {
        None    = 255,
        Fatal   = 200,
        Error   = 150,
        Warning = 50,
        Notice  = 30,
        Info    = 20,
        Debug   = 10,
        Verbose = 0,
    }
}
