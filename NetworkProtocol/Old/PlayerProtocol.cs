using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Protocols
{
    public enum PlayerProtocol : byte
    {
        /// <summary>
        /// 
        /// Range 0-15
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 1: Login
        /// </summary>
        Login = 1,

        /// <summary>
        /// 2: Gets all Player information
        /// </summary>
        Get = 2,

        /// <summary>
        /// 5: Move player to position
        /// </summary>
        Move = 5,

        /// <summary>
        /// 6: Requests action key processing
        /// </summary>
        RequestActionKey = 6,

        /// <summary>
        /// 7: Requests accepting choice
        /// </summary>
        RequestAccept = 7,

        /// <summary>
        /// 8: Requests declining choice
        /// </summary>
        RequestCancel = 8,

        /// <summary>
        /// 9: Requests movement
        /// </summary>
        RequestMovement = 9,

        /// <summary>
        /// 10: Set player direction
        /// </summary>
        Turn = 10,

        /// <summary>
        /// 15: Logout
        /// </summary>
        Logout = 15,
    }
}
