using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils
{
    public class ByteArrayComparer : IEqualityComparer<Byte[]>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Boolean Equals(MongoObjectId a, MongoObjectId b)
        {
            return Equals(a.Id, b.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Boolean Equals(MongoObjectId a, Byte b)
        {
            return Equals(a.Id, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Boolean Equals(Byte a, MongoObjectId b)
        {
            return Equals(a, b.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Boolean Equals(Byte[] a, Byte[] b)
        {
            if (a == null || b == null)
                return a == b;
            return InnerCheck(a, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int GetHashCode(MongoObjectId x)
        {
            if (x == null || x.Id == null)
                throw new ArgumentNullException();
            int iHash = 0;
            for (int i = 0; i < 12; ++i)
                iHash ^= (x[i] << ((0x03 & i) << 3));
            return iHash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int GetHashCode(Byte[] x)
        {
            if (x == null)
                throw new ArgumentNullException();
            int iHash = 0;
            for (int i = 0; i < x.Length; ++i)
                iHash ^= (x[i] << ((0x03 & i) << 3));
            return iHash;
        }

         /// <summary>
        /// Checks if Arrays are Equal
        /// </summary>
        /// <param name="ba1"></param>
        /// <param name="ba2"></param>
        /// <returns>True if equal</returns>
        public static Boolean InnerCheck(Byte[] ba1, Byte[] ba2)
        {
            if (ba1 == null)
                throw new ArgumentNullException("ba1");
            if (ba2 == null)
                throw new ArgumentNullException("ba2");

            if (ba1.Length != ba2.Length)
                return false;

            Boolean inequality = false;

            for (int i = 0; i < ba1.Length; i++)
            {
                inequality |= ba1[i] != ba2[i];
            }

            return !inequality;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ba1"></param>
        /// <param name="ba2"></param>
        /// <returns></returns>
        public static Boolean InnerCheck(MongoObjectId ba1, MongoObjectId ba2)
        {
            return InnerCheck(ba1.Id, ba2.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ba1"></param>
        /// <param name="ba2"></param>
        /// <returns></returns>
        public static Boolean InnerCheck(MongoObjectId ba1, Byte[] ba2)
        {
            return InnerCheck(ba1.Id, ba2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ba1"></param>
        /// <param name="ba2"></param>
        /// <returns></returns>
        public static Boolean InnerCheck(Byte[] ba1, MongoObjectId ba2)
        {
            return InnerCheck(ba1, ba2.Id);
        }
    }

}
