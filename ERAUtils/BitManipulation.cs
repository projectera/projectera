using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ERAUtils
{
    public static class BitManipulation
    {
        /// <summary>
        /// Returns number of bits needed to hold value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int32 BitsToHold(dynamic value)
        {
            Int32 bits = 1;
            while ((value >>= 1) != 0)
                bits++;

            return bits;
        }

        /// <summary>
        /// Returns number of bytes needed to hold value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int32 BytesToHold(dynamic value)
        {
            Int32 bytes = 1;
            while ((value >>= 8) != 0)
                bytes++;

            return bytes;
        }

        /// <summary>
        /// Compress (lossy) a double in the range 0..1 using numberOfBits bits
        /// </summary>
        public static UInt32 CompressUnitDouble(Double value, Int32 numberOfBits)
        {
            Debug.Assert(((value >= 0.0) && (value <= 1.0)), " WriteUnitDouble() must be passed a double in the range 0 to 1; val is " + value);

            Int32 maxValue = (1 << numberOfBits) - 1;
            UInt32 writeVal = (UInt32)(value * (Double)maxValue);

            return writeVal;
        }

        /// <summary>
        /// Reads a 64>32 bit double value written using WriteUnitDouble()
        /// </summary>
        /// <param name="encodedVal">ReadUInt32</param>
        /// <param name="numberOfBits">The number of bits used when writing the value</param>
        /// <returns>A floating point value larger or equal to 0 and smaller or equal to 1</returns>
        public static Double DecompressUnitDouble(UInt32 encodedVal, Int32 numberOfBits)
        {
            Int32 maxVal = (1 << numberOfBits) - 1;
            return (Double)(encodedVal + 1) / (Double)(maxVal + 1);
        }
        
    }
}
