using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace ERAUtils
{
    public class ByteArrayCompressor
    {
        /// <summary>
        /// Compresses a byte array using GZIP
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Byte[] Compress(Byte[] buffer)
        {
            if (buffer.Length > Int32.MaxValue)
                throw new InsufficientMemoryException("Memorystream for small compression supports up to " + Int32.MaxValue + " bytes");

            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(buffer, 0, buffer.Length);
            zip.Close();
            ms.Position = 0;

            MemoryStream outStream = new MemoryStream();

            Byte[] compressed = new Byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            Byte[] gzBuffer = new Byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return gzBuffer;
        }

        /// <summary>
        /// Compresses a Byte array using GZIP
        /// </summary>
        /// <param name="gzBuffer"></param>
        /// <returns></returns>
        public static Byte[] Decompress(Byte[] gzBuffer)
        {
            MemoryStream ms = new MemoryStream();
            Int32 msgLength = BitConverter.ToInt32(gzBuffer, 0);
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

            Byte[] buffer = new Byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Byte[] CompressTiny(Byte[] buffer)
        {
            if (buffer.Length > Byte.MaxValue)
                throw new InsufficientMemoryException("Memorystream for small compression supports up to " + Byte.MaxValue + " bytes");

            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(buffer, 0, (Byte)(buffer.Length));
            zip.Close();
            ms.Position = 0;

            MemoryStream outStream = new MemoryStream();

            Byte[] compressed = new Byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new Byte[compressed.Length + 1];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 1, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 1);
            return gzBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gzBuffer"></param>
        /// <returns></returns>
        public static Byte[] DecompressTiny(Byte[] gzBuffer)
        {
            MemoryStream ms = new MemoryStream();
            Byte msgLength = gzBuffer[0];
            ms.Write(gzBuffer, 1, gzBuffer.Length - 1);

            Byte[] buffer = new Byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}
