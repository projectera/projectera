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
using ContentConverter;
using System.Xml;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ContentConverter.Data;

namespace ContentConverter
{
    public partial class ContentConverterForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public ContentConverterForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog1.ShowDialog();
        }

        /// <summary>
        /// Convert Map Data
        /// </summary>
        private UInt16[][][] ConvertMapData(String filename, UInt16 width, UInt16 height)
        {
            Progress(0);

            ConversionProgress.Maximum = width;

            UInt16[][][] result = new UInt16[width][][];
            using (Stream openFileStream = File.OpenRead(filename))
            {
                using (StreamReader reader = new StreamReader(openFileStream))
                {
                    for (UInt16 x = 0; x < width; x++)
                    {
                        result[x] = new UInt16[height][];
                        for (UInt16 y = 0; y < height; y++)
                        {
                            result[x][y] = new UInt16[3];
                            
                            String[] tiles = reader.ReadLine().Split(',');
                           
                            result[x][y][0] = UInt16.Parse(tiles[0]);
                            result[x][y][1] = UInt16.Parse(tiles[1]);
                            result[x][y][2] = UInt16.Parse(tiles[2]);                 
                        }

                        Increment(1);
                    }
                }
            }

            Progress(width);

            return result;
        }

        /// <summary>
        /// Convert Tileset Data
        /// </summary>
        private void ConvertTilesetData(String filename, UInt16 tiles, out Byte[] passages, out Byte[] priorities, out Byte[] flags, out Byte[] tags)
        {
            Progress(0);

            ConversionProgress.Maximum = 4;

            passages = new Byte[tiles];
            priorities = new Byte[tiles];
            flags = new Byte[tiles];
            tags = new Byte[tiles];

            using (Stream openFileStream = File.OpenRead(filename))
            {
                using (StreamReader reader = new StreamReader(openFileStream))
                {
                    // Passages
                    String[] line = reader.ReadLine().Split(' ');
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        passages[x] = Byte.Parse(line[x]);
                    }
                    Increment(1);

                    // Priorities
                    line = reader.ReadLine().Split(' ');
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        priorities[x] = Byte.Parse(line[x]);
                    }
                    Increment(1);

                    // Flags
                    line = reader.ReadLine().Split(' ');
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        flags[x] = Byte.Parse(line[x]);
                    }
                    Increment(1);

                    // Tags
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        tags[x] = 0;
                    }
                    Increment(1);
                }
            }

            Progress(4);
        }

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
        private void OpenFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {
            LabelFileName.Text = OpenFileDialog1.SafeFileName;
            ButtonExportMap.Enabled = true;
            ButtonExportTileset.Enabled = true;
            ButtonConvertTileset.Enabled = false;
            ButtonStore.Enabled = false;
            Progress(0);
        }

        /// <summary>
        /// Export as map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExportMap_Click(object sender, EventArgs e)
        {
            MapEditor mc = new MapEditor();

            Int32 id = 0, tilesetId = 0;
            UInt16 width = 0, height = 0;
            String name = String.Empty, panorama = String.Empty, fog = String.Empty, mapdata = String.Empty;
            Byte fogOpacity = 0;

            using (Stream stream = OpenFileDialog1.OpenFile())
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    try
                    {
                        Boolean settingsSub = false;
                        Boolean fogSub = false;
                        Boolean panoramaSub = false;

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                switch (reader.Name)
                                {
                                    case "settings":
                                        settingsSub = false;
                                        break;
                                    case "fog":
                                        fogSub = false;
                                        break;
                                    case "panorama":
                                        fogSub = false;
                                        break;
                                }

                                continue;
                            }

                            if (settingsSub)
                            {
                                if (fogSub)
                                {
                                    switch (reader.Name)
                                    {
                                        case "graphic":
                                            if (reader.Read())
                                                fog = reader.Value;
                                            break;
                                        case "opacity":
                                            if (reader.Read())
                                                fogOpacity = Byte.Parse(reader.Value);
                                            break;
                                    }
                                }
                                else if (panoramaSub)
                                {
                                    switch (reader.Name)
                                    {
                                        case "graphic":
                                            if (reader.Read())
                                                panorama = reader.Value;
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (reader.Name)
                                    {
                                        case "fog":
                                            fogSub = true;
                                            break;
                                        case "panorama":
                                            panoramaSub = true;
                                            break;
                                    }
                                }

                            } else {

                                switch (reader.Name)
                                {
                                    case "id":
                                        if (reader.Read())
                                            id = Int32.Parse(reader.Value);
                                        break;
                                    case "width":
                                        if (reader.Read())
                                            width = UInt16.Parse(reader.Value);
                                        break;
                                    case "height":
                                        if (reader.Read())
                                            height = UInt16.Parse(reader.Value);
                                        break;
                                    case "tilesetId":
                                        if (reader.Read())
                                            tilesetId = Int32.Parse(reader.Value);
                                        break;
                                    case "name":
                                        if (reader.Read())
                                            name = reader.Value;
                                        break;
                                    case "mapDataFile":
                                        if (reader.Read())
                                            mapdata = reader.Value;
                                        break;
                                    case "settings":
                                        settingsSub = true;
                                        break;
                                }
                            }
                        }
                    }
                    catch (XmlException)
                    {

                    }
                }
            }

            if (mapdata == String.Empty)
                return;

            mc.Generate(id, width, height, tilesetId, name, fog, fogOpacity, panorama,
                ConvertMapData(String.Format("{0}/{1}", Path.GetDirectoryName(OpenFileDialog1.FileName), mapdata), width, height));
            mc.ShowDialog();
        }

        /// <summary>
        /// Export as Tileset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonExportTileset_Click(object sender, EventArgs e)
        {
            TilesetEditor tilesetEditor = new TilesetEditor();

            using (Stream stream = OpenFileDialog1.OpenFile())
            {
                tilesetEditor.LoadFrom(Tileset.FromFile(stream, Path.GetDirectoryName(OpenFileDialog1.FileName)));
            }

            tilesetEditor.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonConvertTileset_Click(object sender, EventArgs e)
        {
            using (Bitmap image = (Bitmap)Image.FromFile(OpenFileDialog2.FileName))
            {
                Int32 expansionwidth = 256;
                Int32 heightLimit = 2048;

                if (image.Height <= 2048)
                {
                    Progress(ConversionProgress.Maximum);

                    ButtonConvertTileset.Enabled = false;
                    return;
                }

                Int32 newHeight = heightLimit;
                Int32 newWidth = (image.Height / heightLimit + ((image.Height % heightLimit) == 0 ? 0 : 1)) * expansionwidth;

                using (Bitmap converted = new Bitmap(newWidth, newHeight, image.PixelFormat))
                {

                    Progress(0);
                    ConversionProgress.Maximum = image.Width;

                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            Color pixel = image.GetPixel(x, y);
                            converted.SetPixel(x + (y / heightLimit * expansionwidth), (y % heightLimit), pixel);
                        }

                        Progress(x);

                    }

                    converted.Save("temp.png", ImageFormat.Png);
                }
            }

            using (Image image = Image.FromFile("temp.png"))
            {
                image.Save(OpenFileDialog2.FileName, ImageFormat.Png);
            }

            File.Delete("temp.png");           

            ButtonConvertTileset.Enabled = false;

            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoadPng_Click(object sender, EventArgs e)
        {
            OpenFileDialog2.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            LabelFileName.Text = OpenFileDialog2.SafeFileName;
            ButtonExportMap.Enabled = false;
            ButtonExportTileset.Enabled = false;
            ButtonConvertTileset.Enabled = true;
            ButtonStore.Enabled = true;
            Progress(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStore_Click(object sender, EventArgs e)
        {
            AssetEditor assetEditor = new AssetEditor();
            assetEditor.AssetType = ERAUtils.Enum.AssetType.Tileset;
            assetEditor.LocalImage = OpenFileDialog2.FileName;
            assetEditor.RemoteName = OpenFileDialog2.SafeFileName;
            assetEditor.ShowDialog();
        }
    }
}
