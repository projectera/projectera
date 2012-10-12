using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAAuthentication.SRP6
{
    enum HandShakeState
    {
        NotInitialized = 0,
        Requesting = (1 << 0),
        Requested = (1 << 1),
        Responding = (1 << 2),
        Responded = (1 << 3),
        Verificating = (1 << 4),
        Verificated = (1 << 5),
        Expired = (1 << 6),
        Failed = (1 << 7),

        AllowRequest = Verificated | Expired | Failed,
        AllowResponse = Verificated | Expired | Failed,
        AllowVerificating = Responded | Requested,
        AllowVerification = Verificating


    }
}
