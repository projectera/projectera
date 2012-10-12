using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ERAUtils.Enum;
using System.Drawing.Imaging;
using System.IO;
using ContentConverter.Data;

namespace ContentConverter
{
    public partial class AssetEditor : Form
    {
        private ListEditor AliasEditor
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal String LocalImage
        {
            get { return this.PictureBox.ImageLocation; }
            set { this.PictureBox.ImageLocation = value; this.PictureBox.LoadAsync(); }
        }

        /// <summary>
        /// 
        /// </summary>
        internal String RemoteFileName
        {
            get { return AssetPath.Get(this.AssetType) + "." + this.RemoteName; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal String[] Aliases
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal AssetType AssetType
        {
            get 
            { 
                return (AssetType)Enum.Parse(typeof(AssetType), (this.ComboBoxAssetType.SelectedItem ?? "Invalid").ToString() ?? "Invalid"); 
            }
            set 
            { 
                for (int i = 0; i < this.ComboBoxAssetType.Items.Count; i++) 
                { 
                    if (this.ComboBoxAssetType.Items[i].ToString() == value.ToString()) 
                    { 
                        this.ComboBoxAssetType.SelectedIndex = i; 
                        break; 
                    } 
                } 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal String RemoteName 
        {
            get { return this.TextBoxName.Text; }
            set
            {
                this.TextBoxName.Text = value;
                this.PreviousName = this.PreviousName ?? value;
            }
        }

        internal String PreviousName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public AssetEditor()
        {
            InitializeComponent();

            this.ComboBoxAssetType.Items.Clear();
            this.ComboBoxAssetType.Items.AddRange(System.Enum.GetNames(typeof(AssetType)));

            this.ButtonSave.Enabled = false;
            this.PictureBox.LoadCompleted += new AsyncCompletedEventHandler(PictureBox_LoadCompleted);

            this.Shown += new EventHandler(AssetEditor_Shown);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AssetEditor_Shown(object sender, EventArgs e)
        {
            this.Aliases = ObtainOrCreateAsset().QueryableByArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal Asset ObtainOrCreateAsset()
        {
            return (Asset.GetFile(this.AssetType, this.RemoteName) ?? new Asset() { RemoteFileName = this.RemoteName, Type = this.AssetType, Aliases = (this.Aliases ?? new String[] { }) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.ButtonSave.Enabled = true;

            if (this.LocalImage == "ConvertedTileset.png")
            {
 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxAssetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LabelRemoteName.Text = RemoteFileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxName_TextChanged(object sender, EventArgs e)
        {
            LabelRemoteName.Text = RemoteFileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            Asset asset = new Asset() { RemoteFileName = this.RemoteName, Type = this.AssetType, Aliases = this.Aliases };

            if (asset.Type == ERAUtils.Enum.AssetType.Tileset)
            {
                if (FixGraphicAsTileset())
                {
                    this.PictureBox.LoadCompleted += new AsyncCompletedEventHandler(PictureBox_LoadCompleted);
                    this.LocalImage = "ConvertedTileset.png";
                }
            }

            asset.SaveFile(LocalImage, this.PreviousName);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.RemoteName = asset.RemoteFileName;

            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        private Boolean FixGraphicAsTileset()
        {
            using (Bitmap image = (Bitmap)Image.FromFile(this.LocalImage))
            {
                Int32 expansionwidth = 256;
                Int32 heightLimit = 2048;

                if (image.Height <= 2048)
                {
                    return false;
                }

                Int32 newHeight = heightLimit;
                Int32 newWidth = (image.Height / heightLimit + ((image.Height % heightLimit) == 0 ? 0 : 1)) * expansionwidth;

                using (Bitmap converted = new Bitmap(newWidth, newHeight, image.PixelFormat))
                {

                   
                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            Color pixel = image.GetPixel(x, y);
                            converted.SetPixel(x + (y / heightLimit * expansionwidth), (y % heightLimit), pixel);
                        }
                    }

                    converted.Save("TempConversion.png", ImageFormat.Png);
                }
            }

            using (Image image = Image.FromFile("TempConversion.png"))
            {
                image.Save("ConvertedTileset.png", ImageFormat.Png);
            }

            File.Delete("TempConversion.png");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAliases_Click(object sender, EventArgs e)
        {
            AliasEditor = new ListEditor() { Values = this.Aliases, SelectedItem = this.RemoteName, Root = ObtainOrCreateAsset() };
            AliasEditor.FormClosed += new FormClosedEventHandler(listEditor_FormClosed);
            AliasEditor.ShowDialog();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (AliasEditor.DialogResult == DialogResult.OK)
            {
                this.RemoteName = AliasEditor.SelectedItem.ToString();
                this.Aliases = AliasEditor.Values as String[];
            }

            AliasEditor = null;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void ReloadImage()
        {
            this.PictureBox.LoadAsync(this.LocalImage);
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
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.LocalImage = OpenFileDialog1.FileName;
            
            List<String> aliases = this.Aliases.ToList();
            aliases.AddRange(new List<String> { OpenFileDialog1.SafeFileName, OpenFileDialog1.SafeFileName.Remove(OpenFileDialog1.SafeFileName.LastIndexOf(".")) });
            this.Aliases = aliases.Distinct().ToArray();
        }
    }
}
