using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils
{
    [Serializable]
    public struct MongoObjectId
    {
        private static Random RandomInstance = new Random();
        private Byte[] data;
        private String sdata;

        /// <summary>
        /// Empty MongoObjectId
        /// </summary>
        public static MongoObjectId Empty = new MongoObjectId(new Byte[12]);

        /// <summary>
        /// Index to individual bytes
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Byte this[Int32 index]
        {
            get { return this.data[index]; }
            set
            {
                this.data[index] = value;
                CreateStringData();
            }
        }

        /// <summary>
        /// Get The MongoObjectId
        /// </summary>
        public Byte[] Id
        {
            get { return data; }
            set
            {
                if (value == null || value.Length != 12)
                    throw new ArgumentException("Data should be a 12 byte array", "data");
                this.data = value;

                CreateStringData();
            }
        }

        /// <summary>
        /// Creation TimeStamp (UNIX)
        /// </summary>
        public Int32 TimeStamp
        {
            get
            {
                return BitConverter.ToInt32(data, 0);
            }
        }

        /// <summary>
        /// Machine ID
        /// </summary>
        public Int32 Machine
        {
            get
            {
                return BitConverter.ToInt32(data, 4) & (1 << 1 | 1 << 2 | 1 << 3);
            }
        }

        /// <summary>
        /// Process ID
        /// </summary>
        public Int16 Pid
        {
            get
            {
                return BitConverter.ToInt16(data, 7);
            }
        }

        /// <summary>
        /// Increment Counter
        /// </summary>
        public Int32 Increment
        {
            get
            {
                return BitConverter.ToInt32(data, 8) & ~(1 << 3);
            }
        }

        /// <summary>
        /// Creation time
        /// </summary>
        public DateTime CreationTime
        {
            get
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(BitConverter.ToInt32(data, 0));

            }
        }

        /// <summary>
        /// Internal Helper function to cache string Data
        /// </summary>
        /// <returns></returns>
        private String CreateStringData()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Byte b in data)
                sb.Append(b);
            sdata = sb.ToString();
            return sdata;
        }

        /// <summary>
        /// Creates a new MongoObjectId from a data array
        /// </summary>
        /// <param name="data"></param>
        public MongoObjectId(Byte[] data)
        {
            if (data == null || data.Length != 12)
                    throw new ArgumentException("Data should be a 12 byte array", "data");

            this.data = new Byte[data.Length];
            Array.Copy(data, this.data, this.data.Length);

            this.sdata = String.Empty;

            CreateStringData();
        }

        /// <summary>
        /// Creates a new MongoObjectId from an Integer
        /// </summary>
        /// <param name="value"></param>
        public MongoObjectId(Int32 value)
        {
            this.data = new Byte[12];
            this.sdata = String.Empty;
            Array.Copy(BitConverter.GetBytes(value), 0, this.data, 8, 4);

            CreateStringData();
        }

        /// <summary>
        /// Creates a new MongoObjectId from an Unsigned Integer
        /// </summary>
        /// <param name="value"></param>
        public MongoObjectId(UInt32 value)
        {
            this.data = new Byte[12];
            this.sdata = String.Empty;
            Array.Copy(BitConverter.GetBytes(value), 0, this.data, 8, 4);

            CreateStringData();
        }

        /// <summary>
        /// Creates a new MongoObjectId from a Long
        /// </summary>
        /// <param name="value"></param>
        public MongoObjectId(Int64 value)
        {
            this.data = new Byte[12];
            this.sdata = String.Empty;
            Array.Copy(BitConverter.GetBytes(value), 0, this.data, 4, 8);

            CreateStringData();
        }

        /// <summary>
        /// Creates a new MongoObjectId from an Unsigned Long
        /// </summary>
        /// <param name="value"></param>
        public MongoObjectId(UInt64 value)
        {
            this.data = new Byte[12];
            this.sdata = String.Empty;
            Array.Copy(BitConverter.GetBytes(value), 0, this.data, 4, 8);

            CreateStringData();
        }

        /// <summary>
        /// Conversion from Byte Array to MongoObjectId
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static explicit operator MongoObjectId(Byte[] b)  // explicit byte to digit conversion operator
        {
            MongoObjectId d = new MongoObjectId(b);  // explicit conversion
            return d;
        }

        /// <summary>
        /// Conversion from Integer to MongoObjectId
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static implicit operator MongoObjectId(Int32 b)  // explicit byte to digit conversion operator
        {
            MongoObjectId d = new MongoObjectId(b);  // explicit conversion
            return d;
        }

        /// <summary>
        /// Conversion from Long to MongoObject Id
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static implicit operator MongoObjectId(Int64 b)  // explicit byte to digit conversion operator
        {
            MongoObjectId d = new MongoObjectId(b);  // explicit conversion
            return d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override Boolean Equals(Object obj)
        {
            if (obj is Byte[])
            {
                return ByteArrayComparer.InnerCheck((Byte[])obj, this.Id);
            }
            else if (obj is MongoObjectId)
            {
                return ByteArrayComparer.InnerCheck(((MongoObjectId)obj).Id, this.Id);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean operator ==(MongoObjectId a, MongoObjectId b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean operator !=(MongoObjectId a, MongoObjectId b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean operator ==(MongoObjectId a, Byte[] b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean operator !=(MongoObjectId a, Byte[] b)
        {
            return !a.Equals(b);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean operator ==(Byte[] a, MongoObjectId b)
        {
            return b.Equals(a);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Boolean operator !=(Byte[] a, MongoObjectId b)
        {
            return !b.Equals(a);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GetHashCode(this.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int GetHashCode(Byte[] x)
        {
            if (x == null)
                throw new ArgumentNullException();
            int iHash = 0;
            for (int i = 0; i < x.Length; ++i)
                iHash ^= (x[i] << ((0x03 & i) << 3));
            return iHash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (data == null)
                return String.Empty;

            return sdata;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static MongoObjectId GenerateRandom()
        {
            MongoObjectId result = new MongoObjectId(MongoObjectId.Empty.Id);
            RandomInstance.NextBytes(result.data);
            result.CreateStringData();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Byte[] ToCompressedStream()
        {
            return ByteArrayCompressor.CompressTiny(this.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Byte[] ToCompressedStream(MongoObjectId id)
        {
            return id.ToCompressedStream();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static MongoObjectId FromCompressedStream(Byte[] buffer)
        {
            return new MongoObjectId(ByteArrayCompressor.DecompressTiny(buffer));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Byte[] ToByteArray()
        {
            return this.Id;
        }
    }
}
