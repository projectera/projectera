using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace ERAUtils
{
    public class Environment
    {
        private static UInt64 _machineId;
        private static String _machineName;

        /// <summary>
        /// Gets the machine Id as a 64 bit unsigned integer
        /// </summary>
        /// <remarks>Once got, any changes to the machine name are not reflected in the Id</remarks>
        public static UInt64 LongMachineId
        {
            get 
            { 
                if (_machineId == 0)
                {
                    List<Byte[]> processed = new List<Byte[]>();
                    foreach (NetworkInterface n in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        Byte[] mac = n.GetPhysicalAddress().GetAddressBytes();
                        if (mac.Length == 6 && !processed.Contains(mac))
                        {
                            // 5600 0000
                            UInt64 leftBits = ((UInt64)BitConverter.ToUInt32(mac, 2) << (32 + 16));
                            // 0000 1234
                            UInt64 rightBits = (UInt64)BitConverter.ToUInt32(mac, 0);
                            // 0056 0000 ^ 0012 0000 ^ 0034 0000
                            UInt64 middleBits = (leftBits >> 16) ^ (((UInt64)BitConverter.ToUInt16(mac, 0) << 32) ^ ((UInt64)BitConverter.ToUInt16(mac, 2) << 32));

                            // XX = 56 ^ 12 ^ 34
                            // byte align: 56XX1234
                            _machineId ^= (rightBits | leftBits | middleBits);
                            processed.Add(mac);
                        }
                    }

                    if (!String.IsNullOrEmpty(_machineName))
                    {
                        UInt64 stringBits = unchecked((UInt32)_machineName.GetHashCode());
                        _machineId ^= stringBits ^ (stringBits << 32);
                    }
                }

                return _machineId;
            }
        }

        /// <summary>
        /// Gets the machine Id as a 32 bit unsigned integer
        /// </summary>
        public static UInt32 MachineId
        {
            get
            {
                // 1234 56
                Byte[] bytes = BitConverter.GetBytes(LongMachineId << 32 | LongMachineId >> 32);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        /// <summary>
        /// Gets the machine Id as a 16 bit unsigned integer
        /// </summary>
        public static UInt16 ShortMachineId
        {
            get
            {
                // 56 ^ 12 ^ 34
                return BitConverter.ToUInt16(BitConverter.GetBytes(LongMachineId), 2);
            }
        }

        /// <summary>
        /// Gets/Sets the machine name
        /// </summary>
        public static String MachineName
        {
            get
            {
                return _machineName;
            }

            set
            {
                _machineName = value;
                // HACK: remove this later
                _machineId = 0;
                UInt64 dummy = LongMachineId;
                Logger.Logger.Debug(dummy.ToString());
            }
        }

        /// <summary>
        /// Returns distance between two points by latitude and longitude
        /// </summary>
        /// <param name="lat1">Latitude A</param>
        /// <param name="lon1">Longitude A</param>
        /// <param name="lat2">Latitude B</param>
        /// <param name="lon2">Longitude B</param>
        /// <returns>Distance in km between A and B</returns>
        public static Double EarthDistanceBetween(Double lat1, Double lon1, Double lat2, Double lon2)
        {
            // Earth radius in km
            Double rho = 6371.0;
            // 90 - latitude
            Double phi1, phi2;
            // longitude
            Double theta1, theta2;

            // Convert latitude and longitude to spherical coordinates in radians
            phi1 = (90.0 - lat1) * Math.PI / 180.0;
            phi2 = (90.0 - lat2) * Math.PI / 180.0;
            theta1 = lon1 * Math.PI / 180.0;
            theta2 = lon2 * Math.PI / 180.0;

            // Compute spherical distance from spherical coordinates
            // Arclen = arccos(sin(phi) sin(phi') cos(theta-theta') + cos(phi) * cos(phi'))
            // Distance = rho * arclen
            return rho * Math.Acos(Math.Sin(phi1) * Math.Sin(phi2) * Math.Cos(theta1 - theta2) + Math.Cos(phi1) * Math.Cos(phi2));
        }

        /// <summary>
        /// Returns distance between two points by latitude and longitude
        /// </summary>
        /// <param name="art1">A(Latitude, Longitude)</param>
        /// <param name="art2">B(Latitude, Longitude)</param>
        /// <returns>Dinstance in km between A and B</returns>
        public static Double EarthDistanceBetween(Tuple<Double, Double> a, Tuple<Double, Double> b)
        {
            return EarthDistanceBetween(a.Item1, a.Item2, b.Item1, b.Item2);
        }
    }
}