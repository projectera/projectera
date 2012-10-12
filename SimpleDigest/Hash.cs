using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SimpleDigest
{
    /// <summary>
    /// 
    /// </summary>
    public static class MD5
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static String Hash(String e_toBeHashed)
        {
            // Turn String into Bytes
            Byte[] original_bytes = System.Text.Encoding.ASCII.GetBytes(e_toBeHashed);
            // Return Hash
            return Hash(original_bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static String Hash(Byte[] e_toBeHashed)
        {
            Byte[] encoded_bytes = InternalHash(e_toBeHashed);

            // Create A StringBuilder
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            // Encode Each Byte
            for (int i = 0; i < encoded_bytes.Length; i++)
                result.Append(encoded_bytes[i].ToString("x2"));

            // Return the MD5 Hash
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static Byte[] InternalHash(Byte[] e_toBeHashed)
        {
            // Create Encoded Bytes
            return new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(e_toBeHashed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static Byte[] InternalHash(String e_toBeHashed)
        {
            // Turn String into Bytes
            Byte[] original_bytes = System.Text.Encoding.ASCII.GetBytes(e_toBeHashed);
            // Return Hash
            return InternalHash(original_bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static Byte[] InternalHash(Stream e_toBeHashed)
        {
            // Create Encoded Bytes
            return new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(e_toBeHashed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static String Hash(FileStream e_toBeHashed)
        {
            return Hash(InternalHash(e_toBeHashed));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class HMACMD5
    {
        public static Byte[] InternalHash(Byte[] e_toBeHashed)
        {
            return new System.Security.Cryptography.HMACMD5().ComputeHash(e_toBeHashed);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SHA1
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static String Hash(String e_toBeHashed)
        {
            // Turn String into Bytes
            Byte[] original_bytes = System.Text.Encoding.ASCII.GetBytes(e_toBeHashed);
            // Return Hash
            return Hash(original_bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static String Hash(Byte[] e_toBeHashed)
        {
            Byte[] encoded_bytes = InternalHash(e_toBeHashed);

            // Create A StringBuilder
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            // Encode Each Byte
            for (int i = 0; i < encoded_bytes.Length; i++)
                result.Append(encoded_bytes[i].ToString("x2"));

            // Return the MD5 Hash
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static Byte[] InternalHash(Byte[] e_toBeHashed)
        {
            return new System.Security.Cryptography.SHA1CryptoServiceProvider().ComputeHash(e_toBeHashed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static Byte[] InternalHash(String e_toBeHashed)
        {
            // Turn String into Bytes
            Byte[] original_bytes = System.Text.Encoding.ASCII.GetBytes(e_toBeHashed);
            // Return Hash
            return InternalHash(original_bytes);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SHA256
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static Byte[] InternalHash(Byte[] e_toBeHashed)
        {
            return new System.Security.Cryptography.SHA256CryptoServiceProvider().ComputeHash(e_toBeHashed);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SHA384
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e_toBeHashed"></param>
        /// <returns></returns>
        public static Byte[] InternalHash(Byte[] e_toBeHashed)
        {
            return new System.Security.Cryptography.SHA384CryptoServiceProvider().ComputeHash(e_toBeHashed);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SHA512
    {
        public static Byte[] InternalHash(Byte[] e_toBeHashed)
        {
            return new System.Security.Cryptography.SHA512CryptoServiceProvider().ComputeHash(e_toBeHashed);
        }
    }

    
}
