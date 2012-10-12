using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Protocols
{
    /// <summary>
    /// Range: 0-7
    /// </summary>
    public enum UpdateProtocol : byte
    {
        /// <summary>
        /// 0: Invalid
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 1: Update Request
        /// Server > Client: Version Request
        /// Client > Server: Update Available Request
        /// </summary>
        Request = 1,

        /// <summary>
        /// 2: Update Response
        /// Server > Client: Boolean: Availability
        /// Client > Server: Version Number
        /// </summary>
        Response = 2,

        /// <summary>
        /// 3: Data Header
        /// </summary>
        HeaderData = 3,

        /// <summary>
        /// 4: The actual Content
        /// </summary>
        ContentsData = 4,

        /// <summary>
        /// 5: On File Verification failure, Re-Request the file, to ensure
        /// that no data has been gone corrupt during earlier transmissions.
        /// </summary>
        IntegrityData = 5,

        /// <summary>
        /// 6: Last file was send - terminate update stream
        /// </summary>
        Termination = 6,
    }
}
