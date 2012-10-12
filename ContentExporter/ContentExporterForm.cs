using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace ContentExporter
{
    public partial class ContentExporterForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public ContentExporterForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog1.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            ButtonLoad.Enabled = false;
            ButtonSave.Enabled = false;
            ButtonSaveAs.Enabled = false;

            Convert(null);

            ButtonSaveAs.Enabled = true;
            ButtonLoad.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog1.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Convert(Stream saveStream)
        {
            Progress(0);
            
            //using (Stream openFileStream = OpenFileDialog1.OpenFile())
            //{
                using (Bitmap image = (Bitmap) Image.FromFile (OpenFileDialog1.FileName))
                {
                    ConversionProgress.Maximum = image.Height * image.Width;

                    // CONVERSION
                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            Color c = image.GetPixel(x, y);

                            c = Color.FromArgb(
                                (Byte)(c.R * c.A / 255),
                                (Byte)(c.G * c.A / 255),
                                (Byte)(c.B * c.A / 255)
                            );

                            image.SetPixel(x, y, c);
                        }

                        Increment(image.Width);
                    }

                    image.Save("temp.png", ImageFormat.Png);
                }

                using (Image image = Image.FromFile("temp.png"))
                {
                    if (saveStream != null)
                        image.Save(saveStream, ImageFormat.Png);
                    else
                        image.Save(OpenFileDialog1.FileName, ImageFormat.Png);
                }

                File.Delete("temp.png");
            }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void Progress(Int32 value)
        {
            LabelProgress.Text = String.Format("{0} %", ConversionProgress.Maximum > 0 ? Math.Round((value / (Single)ConversionProgress.Maximum) * 100) : value);
            ConversionProgress.Value = value;
            LabelProgress.Update();
            ConversionProgress.Update();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void Increment(Int32 value = 1)
        {
            ConversionProgress.Increment(value);
            LabelProgress.Text = String.Format("{0} %", ConversionProgress.Maximum > 0 ? Math.Round((ConversionProgress.Value / (Single)ConversionProgress.Maximum) * 100) : ConversionProgress.Value);
            LabelProgress.Update();
            ConversionProgress.Update();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            ButtonLoad.Enabled = false;
            ButtonSave.Enabled = false;
            ButtonSaveAs.Enabled = false;

            using (Stream saveStream = SaveFileDialog1.OpenFile())
                Convert(saveStream);

            ButtonSave.Enabled = true;
            ButtonSaveAs.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            LabelFileName.Text = OpenFileDialog1.FileName;
            ButtonSave.Enabled = true;
            ButtonSaveAs.Enabled = true;
        }
    }
}
