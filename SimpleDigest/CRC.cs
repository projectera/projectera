using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SimpleDigest
{
    public static class CRC16
    {
        private const Int32 Mask = 0x8000;
        private const Int32 Shift = 0x1021;

        /// <summary>
        /// Adds a byte to the CRC
        /// </summary>
        /// <param name="crc">CRC</param>
        /// <param name="b">byte</param>
        /// <returns>Updated CRC</returns>
        private static UInt16 Update(UInt16 crc, Byte b)
        {
            crc ^= (UInt16)(b << 8);

            for (Int32 i = 0; i < 8; i++)
            {
                if ((crc & CRC16.Mask) > 0)
                    crc = (UInt16)((crc << 1) ^ CRC16.Shift);
                else
                    crc <<= 1;
            }

            return crc;
        }

        /// <summary>
        /// Cycles through data and returns CRC
        /// </summary>
        /// <param name="data"></param>
        /// <returns>CRC</returns>
        public static UInt16 Calc(Byte[] data)
        {
            UInt16 crc = 0xFFFF;

            for (Int32 i = 0; i < data.Length; i++)
                crc = CRC16.Update(crc, data[i]);

            return crc;
        }


    }

    public class Crc32 : HashAlgorithm {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;
        private static UInt32[] defaultTable;

        /// <summary>
        /// 
        /// </summary>
        public Crc32() {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="seed"></param>
        public Crc32(UInt32 polynomial, UInt32 seed) {
            table = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize() {
            hash = seed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        protected override void HashCore(Byte[] buffer, Int32 start, Int32 length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Byte[] HashFinal() 
        {
            Byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        public override Int32 HashSize {
            get { return 32; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static UInt32 Compute(Byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static UInt32 Compute(UInt32 seed, Byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="seed"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, Byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polynomial"></param>
        /// <returns></returns>
        private static UInt32[] InitializeTable(UInt32 polynomial) {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++) {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="seed"></param>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, Byte[] buffer, Int32 start, Int32 size)
        {
            UInt32 crc = seed;
            for (Int32 i = start; i < size; i++)
                unchecked {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private Byte[] UInt32ToBigEndianBytes(UInt32 x) {
            return new Byte[] {
			    (Byte)((x >> 24) & 0xff),
			    (Byte)((x >> 16) & 0xff),
			    (Byte)((x >> 8) & 0xff),
			    (Byte)(x & 0xff)
		    };
        }
    }
}
