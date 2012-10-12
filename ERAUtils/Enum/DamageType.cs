using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    internal enum DamageType
    {
        None,
                               
        Piercing,           // attack -|-|>| body
        Slashing,           // attack -|>||| body
        Crushing,           // attack >||||| body

        Thermal,            // attack ~|~|~| body
        Electromagnetic,    // electrical circuits
        Acid,               // attack >|>|>| body
        Radioactive,        // attack ||||>| body
        Sonic,              // attack ~|~|~| body

        Draining,           // body   > ~energy~ > attack
        Healing,            // attack > ~energy~ > attack

        Explosive,          // attack ~/~\~/ body
        Elemental           // varies

    }
}
