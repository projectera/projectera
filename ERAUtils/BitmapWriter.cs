using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace ERAUtils
{
    /// <summary>
    /// Provides the functions to write a texture to any of the available imageformats,
    /// without causing a memory leak, as happens with the Texture#SaveAs_ functions.
    /// </summary>
    public class BmpWriter
    {
        private Byte[] textureData;
        private Bitmap bmp;
        private BitmapData bitmapData;
        private IntPtr safePtr;
        private Rectangle rect;

        /// <summary>
        /// Gets or sets imageformat
        /// </summary>
        public ImageFormat ImageFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Sets output to png
        /// </summary>
        public void OutputPNG()
        {
            ImageFormat = ImageFormat.Png;
        }

        /// <summary>
        /// Sets output to bmp
        /// </summary>
        public void OutputBMP()
        {
            ImageFormat = ImageFormat.Bmp;
        }

        /// <summary>
        /// Sets output to jpg
        /// </summary>
        public void OutputJPG()
        {
            ImageFormat = ImageFormat.Jpeg;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">Bitmap Width</param>
        /// <param name="height">Bitmap Height</param>
        public BmpWriter(Int32 width, Int32 height)
        {
            textureData = new Byte[4 * width * height];

            bmp = new System.Drawing.Bitmap(
                           width, height,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            rect = new System.Drawing.Rectangle(0, 0, width, height);

            ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
        }

        /// <summary>
        /// Save Texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="filename"></param>
        public void TextureToBmp(Texture2D texture, String filename)
        {
            texture.GetData<Byte>(textureData);
            Byte blue;
            for (Int32 i = 0; i < textureData.Length; i += 4)
            {
                blue = textureData[i];
                textureData[i] = textureData[i + 2];
                textureData[i + 2] = blue;
            }

            bitmapData = bmp.LockBits(
                           rect,
                           System.Drawing.Imaging.ImageLockMode.WriteOnly,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            safePtr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bmp.UnlockBits(bitmapData);

            bmp.Save(filename, ImageFormat);
        }
        
        /// <summary>
        /// Save Texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="stream"></param>
        public void TextureToBmp(Texture2D texture, Stream stream)
        {
            texture.GetData<Byte>(textureData);
            Byte blue;
            for (Int32 i = 0; i < textureData.Length; i += 4)
            {
                blue = textureData[i];
                textureData[i] = textureData[i + 2];
                textureData[i + 2] = blue;
            }

            bitmapData = bmp.LockBits(
                           rect,
                           System.Drawing.Imaging.ImageLockMode.WriteOnly,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            safePtr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bmp.UnlockBits(bitmapData);

            bmp.Save(stream, ImageFormat);
        }

    }
}
