using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MongoDB.Driver.Builders;
using System.IO;
using ContentConverter.Data;

namespace ContentConverter
{
    public partial class TilesetBrowser : Form
    {
        /// <summary>
        /// Tileset Editor Form
        /// </summary>
        private TilesetEditor Editor
        {
            get;
            set;
        }

        /// <summary>
        /// New Tileset Form
        /// </summary>
        private TilesetEditor EditorNew
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private Timer RefreshTimer
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean LinkingEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Selected Tileset
        /// </summary>
        private ERAServer.Data.Tileset SelectedTileset
        {
            get
            {
                return ERAServer.Data.Tileset.GetCollection().FindAll().Where(
                    s => (s.Name) == ListResults.SelectedItem.ToString() //s.Id + ": " + 
                ).FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal ERAServer.Data.Tileset LinkedTileset
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public TilesetBrowser()
        {
            InitializeComponent();

            RefreshList();

            this.RefreshTimer = new Timer();
            this.RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            this.RefreshTimer.Interval = 500;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            this.RefreshTimer.Stop();

            RefreshList();
        }

        /// <summary>
        /// Refreshes Tileset list
        /// </summary>
        public void RefreshList()
        {
            this.ListResults.Items.Clear();
            this.ListResults.Items.AddRange(ERAServer.Data.Tileset.GetCollection().FindAll().OrderBy(s => s.Name).Select(s => s.Name).ToArray()); //s.Id + ": " + 
            this.ListResults.SelectedIndex = -1;

            this.ButtonDelete.Enabled = false;
            this.ButtonRefresh.Enabled = true;
        }

        /// <summary>
        /// On Selected Index Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ListResults.SelectedIndex >= 0)
            {
                if (this.Editor == null)
                {
                    this.Editor = new TilesetEditor() { LinkingEnabled = this.LinkingEnabled };
                    this.Editor.FormClosed += new FormClosedEventHandler(Editor_FormClosed);
                }

                // Open selected Tileset
                this.Editor.Visible = false;
                this.Editor.LoadFrom(SelectedTileset);

                this.Editor.Show(this);
                this.Editor.Location = this.Location + new Size(this.Size.Width + 10, 5);
                if (EditorNew != null && EditorNew.Visible && Editor.Location == EditorNew.Location)
                    Editor.Location = Editor.Location + new Size(EditorNew.Size.Width + 15, 0);

                this.ButtonDelete.Enabled = true;
            }
            else
            {
                ButtonDelete.Enabled = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Editor.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (!this.LinkingEnabled)
                {
                    this.RefreshTimer.Stop();
                    this.RefreshTimer.Start();
                    this.ButtonRefresh.Enabled = false;
                }
                else
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.LinkedTileset = Editor.Tileset;
                    this.Close();
                }
            }

            Editor = null;
        }      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog1.ShowDialog(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            if (EditorNew == null)
            {
                this.EditorNew = new TilesetEditor();
                this.EditorNew.FormClosed += new FormClosedEventHandler(EditorNew_FormClosed);
            }

            this.EditorNew.Visible = false;
            this.EditorNew.Generate(0, String.Empty, String.Empty, new List<String>(), new Byte[0], new Byte[0], new Byte[0], new Byte[0], 0);
            this.EditorNew.Show(this);

            this.EditorNew.Location = this.Location + new Size(this.Size.Width + 10, 5);
            if (this.Editor != null && this.Editor.Visible && this.Editor.Location == this.EditorNew.Location)
                this.EditorNew.Location = this.EditorNew.Location + new Size(this.Editor.Size.Width + 15, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorNew_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.EditorNew.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                this.RefreshTimer.Stop();
                this.RefreshTimer.Start();
                this.ButtonRefresh.Enabled = false;
            }

            this.EditorNew = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete tileset <" + this.ListResults.SelectedItem.ToString() + ">?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ERAServer.Data.Tileset.GetCollection().Remove(
                    Query.EQ("_id", 
                        SelectedTileset.Id)
                    );

                if (this.Editor != null && Editor.Visible)
                {
                    this.Editor.Close();
                }

                RefreshList();
                this.ButtonRefresh.Enabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (this.EditorNew == null)
            {
                this.EditorNew = new TilesetEditor();
                this.EditorNew.FormClosed += new FormClosedEventHandler(EditorNew_FormClosed);
            }

            this.EditorNew.Visible = false;
            using (Stream stream = OpenFileDialog1.OpenFile())
            {
                this.EditorNew.LoadFrom(Tileset.FromFile(stream, Path.GetDirectoryName(OpenFileDialog1.FileName)));
            }
            this.EditorNew.Show(this);

            this.EditorNew.Location = this.Location + new Size(this.Size.Width + 10, 5);
            if (this.Editor != null && this.Editor.Visible && this.Editor.Location == this.EditorNew.Location)
                this.EditorNew.Location = this.EditorNew.Location + new Size(this.Editor.Size.Width + 15, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            this.ButtonRefresh.Enabled = false;
            this.RefreshTimer.Stop();
            this.RefreshTimer.Start();
        }
    }
}
