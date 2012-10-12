using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;

namespace ERAAuthentication.SRP6
{
    /// <summary>
    /// Summary description for Utils.
    /// </summary>
    public static class Utilities
    {
        public static string ToXmlString(object value)
        {
            string data = null;
            XmlSerializer ser = new XmlSerializer(value.GetType());
            using (StringWriter sw = new StringWriter())
            {
                ser.Serialize(sw, value);
                sw.Flush();
                data = sw.ToString();
                return data;
            }
        }

        public static object FromXmlString(string xmlString, Type type)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            object r = null;
            XmlSerializer ser = new XmlSerializer(type);
            using (StringReader sr = new StringReader(xmlString))
            {
                r = ser.Deserialize(sr);
            }
            return r;
        }

        /// <summary>
        /// Splits a string into chuck sizes of chunkSize.  If value is not evenly divided by chunkSize, the last chunk will contain what ever is left.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <param name="chunkSize">Number of character per chunk.</param>
        /// <returns>string array.</returns>
        public static string[] SplitChunks(string value, int chunkSize)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (chunkSize < 1)
                throw new ArgumentOutOfRangeException("chunkSize");
            StringBuilder sb = new StringBuilder();
            ArrayList al = new ArrayList();
            foreach (char c in value)
            {
                sb.Append(c);
                if (sb.Length == chunkSize)
                {
                    al.Add(sb.ToString());
                    sb = new StringBuilder();
                }
            }
            if (sb.Length > 0)
                al.Add(sb.ToString());
            return (string[])al.ToArray(typeof(String));
        }

        public static string ToUtcDateTimeString(DateTime value)
        {
            // Date expected to be UTC date already.
            // e.g 2003-10-26T14:33:41.1234567Z
            return value.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses the UTC formatted string, returning DateTime in UTC.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime FromUTCDateTimeString(string value)
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        }

        /// <summary>
        /// Return the first X bytes from the array.
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] GetBytes(byte[] ba, int len)
        {
            if (ba == null)
                throw new ArgumentNullException("ba");
            if (len < 0)
                throw new ArgumentOutOfRangeException("len must be > 0");
            if (ba.Length < len)
                throw new ArgumentOutOfRangeException("len must be <= ba length.");
            byte[] newArray = new byte[len];
            Buffer.BlockCopy(ba, 0, newArray, 0, len);
            return newArray;
        }

        public static byte[] JoinArrays(byte[] b1, byte[] b2)
        {
            byte[] ba = new byte[b1.Length + b2.Length];
            Buffer.BlockCopy(b1, 0, ba, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, ba, b1.Length, b2.Length);
            return ba;
        }

        public static byte[] JoinArrays(ArrayList byteArrays)
        {
            if (byteArrays == null)
                throw new ArgumentNullException("byteArrays");

            using (MemoryStream ms = new MemoryStream())
            {
                foreach (byte[] ba in byteArrays)
                {
                    if (ba == null)
                        continue;
                    ms.Write(ba, 0, ba.Length);
                }
                return ms.ToArray();
            }
        }

        public static bool ArraysEqual(byte[] ba1, byte[] ba2)
        {
            if (ba1 == null)
                throw new ArgumentNullException("ba1");
            if (ba2 == null)
                throw new ArgumentNullException("ba2");

            if (ba1.Length != ba2.Length)
                return false;

            for (int i = 0; i < ba1.Length; i++)
            {
                if (ba1[i] != ba2[i])
                    return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty(string s)
        {
            if (s == null)
                return true;
            if (s.Length == 0)
                return true;
            return false;
        }

        /// <summary>
        /// Converts hex string to byte array equivilent.  Spaces are ignored, however the resulting string length must be a multiple of 2
        /// as each hex pair (two hex digits) are converted to a byte.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            hex = RemoveWhiteSpaceCharacters(hex);
            //hex = hex.Replace(" ", ""); // Remove all spaces.
            if ((hex.Length % 2) > 0)
                throw new ArgumentOutOfRangeException("hex string must be a multiple of 2.");
            int arrayLen = hex.Length / 2;
            byte[] ba = new byte[arrayLen];
            int pos = 0;

            for (int i = 0; i < hex.Length; i = i + 2)
            {
                string hexDigits = hex.Substring(i, 2);
                ba[pos++] = Convert.ToByte(hexDigits, 16);
            }
            return ba;
        }

        /// <summary>
        /// Returns string with all whitespace characters removed.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveWhiteSpaceCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (Char.IsWhiteSpace(c))
                    continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static string ByteArrayToHexString(byte[] array, bool upper, string prefix, string delimiter, uint rowLength)
        {
            if (array == null)
                throw new ApplicationException("array is null.");
            if (array.Length == 0)
                return "";
            if (prefix == null)
                prefix = "";
            if (delimiter == null)
                delimiter = "";
            if (rowLength < 0)
                rowLength = 0;

            string newLineChar = "\r\n";
            string format;
            int colSpaces = 2;
            string colDelim = new String(' ', colSpaces);
            StringBuilder sb;
            StringBuilder line;
            StringBuilder ascString;
            StringBuilder hexSB;
            byte b;
            int arrayLength;
            int colWidth;
            int lastOffset;
            string offsetString;
            bool useOffsets = true;

            format = (upper) ? prefix + "{0:X2}" : prefix + "{0:x2}";
            int sbSize = (array.Length * 3 + colDelim.Length) +
                (prefix.Length * array.Length) +
                ((delimiter.Length * array.Length) - 1);
            sb = new StringBuilder(sbSize);
            hexSB = new StringBuilder();
            arrayLength = array.Length;
            colWidth = (int)((rowLength * 2) + prefix.Length + (delimiter.Length * rowLength) - 1);
            ascString = new StringBuilder();
            line = new StringBuilder();
            lastOffset = 0;

            for (int i = 1; i <= arrayLength; i++)
            {
                b = array[i - 1];
                hexSB.Append(String.Format(format, b));
                if (b < 31) //None displayable char.
                    ascString.Append(".");
                else
                    ascString.Append((char)b);

                //If we want to do column processing.
                if (rowLength > 0)
                {
                    if ((i % rowLength) == 0) //Column break?
                    {
                        //Note: Not appending field delim at end of column.
                        if (useOffsets)
                        {
                            offsetString = String.Format("0x{0:X5}  ", lastOffset);
                            line.Append(offsetString);
                            lastOffset = lastOffset + (i - lastOffset);
                        }
                        line.Append(hexSB);
                        line.Append(colDelim);
                        line.Append(ascString);
                        line.Append(newLineChar);
                        sb.Append(line);
                        line = new StringBuilder();  //Make line and ascString ready for next line.
                        ascString = new StringBuilder();
                        hexSB = new StringBuilder();
                    }
                    else //No column break, just append string and delim if needed. 
                    {
                        if (delimiter != null && !(i == arrayLength))
                            hexSB.Append(delimiter);
                        //Need to check if this is the last byte, if so, we build and output the line.
                        if (i == arrayLength) //Last byte?
                        {
                            if (useOffsets)
                            {
                                offsetString = String.Format("0x{0:X5}  ", lastOffset);
                                line.Append(offsetString);
                            }
                            if (hexSB.Length < colWidth)
                            {
                                hexSB.Append(new String(' ', (colWidth - hexSB.Length)));
                            }
                            line.Append(hexSB);
                            line.Append(colDelim);
                            line.Append(ascString);
                            sb.Append(line);
                        }
                    }
                }
                else //No columns used, so just append delim if needed.
                {
                    //Ignore offsets, even if supplied as this is one long line.
                    if (delimiter != null && !(i == arrayLength))
                        hexSB.Append(delimiter);
                    if (i == arrayLength) //Last byte?
                    {
                        hexSB.Append(newLineChar);
                        sb.Append(hexSB);
                    }
                    //Note: Using existing line and ascString stringbuilders.
                }
            }
            return sb.ToString();
        }

        public static byte[] RijndaelEncrypt(ICryptoTransform encryptor, byte[] data)
        {
            //ICryptoTransform encryptor = myRijndael.CreateEncryptor(key, IV);
            if (encryptor == null)
                throw new ArgumentNullException("encryptor");
            if (data == null)
                throw new ArgumentNullException("data");

            //Encrypt the data.
            using (MemoryStream msEncrypt = new MemoryStream())
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                //Convert the data to a byte array.
                //toEncrypt = textConverter.GetBytes(original);

                //Write all data to the crypto stream and flush it.
                csEncrypt.Write(data, 0, data.Length);
                csEncrypt.FlushFinalBlock();

                //Get encrypted array of bytes.
                byte[] encrypted = msEncrypt.ToArray();
                return encrypted;
            }
        }

        public static byte[] RijndaelDecrypt(ICryptoTransform decryptor, byte[] encrypted)
        {
            //Get a decryptor that uses the same key and IV as the encryptor.
            //ICryptoTransform decryptor = myRijndael.CreateDecryptor(key, IV);
            if (decryptor == null)
                throw new ArgumentNullException("decryptor");
            if (encrypted == null)
                throw new ArgumentNullException("encrypted");

            //Now decrypt the previously encrypted message using the decryptor
            // obtained in the above step.
            using (MemoryStream msDecrypt = new MemoryStream(encrypted))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                byte[] fromEncrypt = new byte[encrypted.Length];

                //Read the data out of the crypto stream.
                int read = csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
                if (read < fromEncrypt.Length)
                {
                    byte[] clearBytes = new byte[read];
                    Buffer.BlockCopy(fromEncrypt, 0, clearBytes, 0, read);
                    return clearBytes;
                }
                return fromEncrypt;
            }
        }
    }
}
