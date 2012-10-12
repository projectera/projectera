using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ERAUtils.Enum;
using ContentConverter.Data;

namespace ContentConverter
{
    public partial class AssetBrowser : Form
    {
        /// <summary>
        /// Image displayed
        /// </summary>
        internal String LocalImage
        {
            get { return this.PictureBox.ImageLocation; }
            set { this.PictureBox.LoadAsync(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        internal AssetEditor Editor
        {
            get;
            set;
        }

        /// <summary>
        /// Timer for retrieving list
        /// </summary>
        private Timer SearchTimer
        {
            get;
            set;
        }

        /// <summary>
        /// Selected Name on Close
        /// </summary>
        internal String SelectedName { get; private set; }

        /// <summary>
        /// Selected Type on Close
        /// </summary>
        internal AssetType SelectedType { get; private set; }

        /// <summary>
        /// Selected Type
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
        public AssetBrowser()
        {
            InitializeComponent();

            this.ComboBoxAssetType.Items.Clear();
            this.ComboBoxAssetType.Items.AddRange(System.Enum.GetNames(typeof(AssetType)));

            this.ButtonUse.Enabled = false;
            this.ButtonEdit.Enabled = false;
            this.PictureBox.LoadCompleted += new AsyncCompletedEventHandler(PictureBox_LoadCompleted);

            this.FormClosing += new FormClosingEventHandler(AssetBrowser_FormClosing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssetBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
 
        }

        /// <summary>
        /// Enable use on Load picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.ButtonUse.Enabled = true;
        }

        /// <summary>
        /// On Use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonUse_Click(object sender, EventArgs e)
        {
            Asset asset = Asset.GetFile(this.AssetType, this.ListResults.SelectedItem.ToString());
            this.SelectedName = asset.RemoteFileName;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            this.Close();
        }

        /// <summary>
        /// On Search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonSearch_Click(object sender, EventArgs e)
        {
            String[] result;
            AssetOperationResult operation = Asset.GetFileList(this.AssetType, this.TextBoxName.Text, out result);

            if (operation != AssetOperationResult.Ok)
            {
                MessageBox.Show(this, operation.ToString(), "File List error", MessageBoxButtons.OK);
                result = new String[] { };
            }

            this.ListResults.SelectedIndex = -1;
            this.ListResults.Items.Clear();
            this.ListResults.Items.AddRange(result);

            this.ButtonUse.Enabled = false;
            this.ButtonEdit.Enabled = false;
            this.ListResults.Enabled = true;

            this.SelectedType = this.AssetType;
        }

        /// <summary>
        /// On Select
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ListResults.SelectedIndex >= 0)
            {
                Asset asset = Asset.GetFile(this.AssetType, this.ListResults.SelectedItem.ToString());
                asset.Download("Temp/" + "AssetBrowser.png");

                this.LocalImage = "Temp/" + "AssetBrowser.png";
                this.PictureBox.Visible = true;
                this.ButtonUse.Enabled = true;
                this.ButtonEdit.Enabled = true;
            }
            else
            {
                this.PictureBox.Visible = false;
                this.ButtonUse.Enabled = false;
                this.ButtonEdit.Enabled = false;
            }
        }

        /// <summary>
        /// On Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog1.ShowDialog();
        }

        /// <summary>
        /// On File Chosen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (this.Editor == null)
            {
                Editor = new AssetEditor()
                {
                    AssetType = this.AssetType,
                    LocalImage = OpenFileDialog1.FileName,
                    RemoteName = OpenFileDialog1.SafeFileName
                };
                this.Editor.FormClosed += new FormClosedEventHandler(Editor_FormClosed);
            }
            else
            {
                this.Editor.AssetType = this.AssetType;
                this.Editor.LocalImage = OpenFileDialog1.FileName;
                this.Editor.RemoteName = OpenFileDialog1.SafeFileName;
                this.Editor.ReloadImage();


            }
            // Open selected
            this.Editor.Visible = false;
           
            this.Editor.Show(this);
            this.Editor.Location = this.Location + new Size(this.Size.Width + 15, 0);
        }

        /// <summary>
        /// On Edit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (this.Editor == null)
            {
                Editor = new AssetEditor()
                {
                    AssetType = this.AssetType,
                    LocalImage = "Temp/" + "AssetBrowser.png",
                    RemoteName = this.ListResults.SelectedItem.ToString()
                };
                this.Editor.FormClosed += new FormClosedEventHandler(Editor_FormClosed);
            }
            else
            {
                this.Editor.AssetType = this.AssetType;
                this.Editor.RemoteName = this.ListResults.SelectedItem.ToString();
                this.Editor.LocalImage = "Temp/" + "AssetBrowser.png";
                this.Editor.AssetEditor_Shown(this.ButtonEdit, e);

                this.Editor.ReloadImage();
            }
            // Open selected
            this.Editor.Visible = false;
            
            this.Editor.Show(this);
            this.Editor.Location = this.Location + new Size(this.Size.Width + 15, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_FormClosed(object sender, FormClosedEventArgs e)
        {
            ButtonSearch_Click(ButtonEdit, e);

            for (int i = 0; i < this.ListResults.Items.Count; i++)
            {
                if (this.ListResults.Items[i].ToString() == this.Editor.RemoteName.ToString())
                {
                    this.ListResults.SelectedIndex = i;
                    break;
                }
            } 

            Editor = null;
        }

        /// <summary>
        /// On Type Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxAssetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SearchTimer == null)
            {
                SearchTimer = new Timer();
                SearchTimer.Tick += new EventHandler(SearchTimer_Tick);
                SearchTimer.Interval = 500;
                SearchTimer.Start();
            }
            else
            {
                SearchTimer.Stop();
                SearchTimer.Start();
            }
        }

        /// <summary>
        /// On Search Query Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxName_TextChanged(object sender, EventArgs e)
        {
            if (SearchTimer == null)
            {
                SearchTimer = new Timer();
                SearchTimer.Tick += new EventHandler(SearchTimer_Tick);
                SearchTimer.Interval = 500;
                SearchTimer.Start();
            }
            else
            {
                SearchTimer.Stop();
                SearchTimer.Start();
            }
        }

        /// <summary>
        /// On Timer Expired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            SearchTimer.Stop();
            ButtonSearch_Click(SearchTimer, e);
        }
    }
}