using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Services.Network
{
    internal static class Stats
    {
        /// <summary>
        /// 
        /// </summary>
        public static class Asset
        {
            /// <summary>
            /// Asset was not requested (key)
            /// </summary>
            public static Int32 NotRequested;

            /// <summary>
            /// Asset chunk was not requested (key)
            /// </summary>
            public static Int32 ChunkNotRequested;
            
            /// <summary>
            /// Asset download was not requested (key)
            /// </summary>
            public static Int32 DownloadNotRequested;

            /// <summary>
            /// Asset operation failed (server)
            /// </summary>
            public static Int32 Failed;

            /// <summary>
            /// Asset chunk operation failed (server)
            /// </summary>
            public static Int32 ChunkFailed;

            /// <summary>
            /// Asset download operation failed (server)
            /// </summary>
            public static Int32 DownloadFailed;

            /// <summary>
            /// Asset data request timed out
            /// </summary>
            public static Int32 DataRequestTimeout;

            /// <summary>
            /// Asset request timed out
            /// </summary>
            public static Int32 RequestTimeout;
        }

        public static class Interactable
        {
            public static Int32 JoinNull;
            public static Int32 MessageNull;
            public static Int32 AppearanceNull;
            public static Int32 MoveNull;
        }

    }
}
