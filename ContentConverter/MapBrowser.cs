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
    public partial class MapBrowser : Form
    {
        /// <summary>
        /// Map Editor Form
        /// </summary>
        private MapEditor Editor
        {
            get;
            set;
        }

        /// <summary>
        /// New Map Form
        /// </summary>
        private MapEditor EditorNew
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

        private event EventHandler OnAnimationOpenCompleted = delegate { };
        private event EventHandler OnAnimationCloseCompleted = delegate { };

        /// <summary>
        /// 
        /// </summary>
        private Timer AnimationTimer
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private Size EditorClosedSize
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private Size EditorOpenedSize
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private TreeNode[] GetNodes
        {
            get
            {
                return ERAServer.Data.Map.GetCollection()
                    .FindAll()
                    .OrderBy(s => s.Name)
                    .Select(s => new TreeNode(s.Name ?? "Map " + s.Id.ToString()))
                    .ToArray();
            }
        }

        /// <summary>
        /// Selected Tileset
        /// </summary>
        private ERAServer.Data.Map SelectedMap
        {
            get
            {
                return ERAServer.Data.Map.GetCollection()
                    .FindAll()
                    .Where(
                        s => (s.Name ?? "Map " + s.Id.ToString()) == this.TreeResults.SelectedNode.Text //s.Id + ": " + 
                        )
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MapBrowser()
        {
            InitializeComponent();

            RefreshList();

            this.RefreshTimer = new Timer();
            this.RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            this.RefreshTimer.Interval = 500;

            this.AnimationTimer = new Timer();
            this.AnimationTimer.Tick += new EventHandler(AnimationTimer_Tick);
            this.AnimationTimer.Interval = 50;

            this.EditorClosedSize = this.Size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimationTimer_Tick(Object sender, EventArgs e)
        {
            Boolean complete = false;

            if (this.Editor != null)
            {
                this.Size = this.EditorOpenedSize; /*new Size((Int32)(this.Size.Width + Math.Min((EditorOpenedSize - this.Size).Width, Math.Max(1, (EditorOpenedSize - this.Size).Width * 0.3))),
                    (Int32)(this.Size.Height + (EditorOpenedSize - Size).Height * 0.3)); //EditorClosedSize + (EditorOpenedSize - EditorClosedSize) * amount*/

                complete = this.Size == this.EditorOpenedSize;

                if (complete)
                    this.OnAnimationOpenCompleted.Invoke(sender, e);
            }
            else
            {
                this.Size = this.EditorClosedSize; /*new Size((Int32)(this.Size.Width + Math.Max((EditorClosedSize - this.Size).Width, Math.Min(1, (EditorClosedSize - this.Size).Width * 0.3))),
                    (Int32)(this.Size.Height + (this.EditorClosedSize - Size).Height * 0.3));*/

                complete = this.Size == this.EditorClosedSize;

                if (complete)
                    this.OnAnimationCloseCompleted.Invoke(sender, e);
            }

            //this.Refresh();

            if (complete)
                this.AnimationTimer.Stop();
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
            if (this.Editor != null && this.Editor.Visible)
                this.Editor.Close();

            TreeNode[] newNodes = this.GetNodes;
            TreeNode[] oldNodes = new TreeNode[this.TreeResults.Nodes.Count];
            this.TreeResults.Nodes.CopyTo(oldNodes, 0);

            NodeComparer comparer = new NodeComparer();

            foreach (TreeNode removeNode in oldNodes)
            {
                if (!newNodes.Contains(removeNode, comparer))
                {
                    this.TreeResults.Nodes.Remove(removeNode);
                }
            }

            foreach (TreeNode addNode in newNodes)
            {
                if (!oldNodes.Contains(addNode, comparer))
                {
                    this.TreeResults.Nodes.Add(addNode);
                }
            }

            this.TreeResults.SelectedNode = null;

            this.ButtonDelete.Enabled = false;
            this.ButtonRefresh.Enabled = true;
        }

        private class NodeComparer : IEqualityComparer<TreeNode>
        {

            public Boolean Equals(TreeNode x, TreeNode y)
            {
                if (x == null && y == null)
                    return true;

                if (x != null && y == null || y != null && x == null)
                    return false;

                return x.Text.Equals(y.Text) &&
                    ((x.Parent == null && y.Parent == null) || Equals(x.Parent, y.Parent));

                // check children
            }

            public int GetHashCode(TreeNode obj)
            {
                if (obj == null)
                    return GetHashCode(new TreeNode());

                if (obj == new TreeNode())
                    return 0;

                return obj.Text.GetHashCode() ^ GetHashCode(obj.Parent);
            }
        }

        /// <summary>
        /// On Selected Index Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.TreeResults.SelectedNode != null)
            {
                if (this.Editor == null)
                {
                    this.Editor = new MapEditor();
                    this.Editor.FormClosed += new FormClosedEventHandler(Editor_FormClosed);
                }

                // Open selected Map
                this.Editor.Visible = false;
                this.Editor.LoadFrom(SelectedMap);
                this.Editor.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Editor.ShowInTaskbar = false;
                this.EditorOpenedSize = new Size(this.EditorClosedSize.Width + 10 + this.Editor.Size.Width, Math.Max(this.EditorClosedSize.Height, this.Editor.Size.Height));
                this.LocationChanged += new EventHandler(MapBrowser_LocationChanged);
                this.OnAnimationOpenCompleted -= MapBrowser_OnAnimationOpenCompleted;
                this.OnAnimationOpenCompleted += new EventHandler(MapBrowser_OnAnimationOpenCompleted);
                this.AnimationTimer.Start();

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
        private void MapBrowser_OnAnimationOpenCompleted(object sender, EventArgs e)
        {
            this.Editor.Show(this);
            this.Editor.Location = this.Location + new Size(this.EditorClosedSize.Width, 65);
            //if (EditorNew != null && EditorNew.Visible && Editor.Location == EditorNew.Location)
            //    Editor.Location = Editor.Location + new Size(EditorNew.Size.Width + 15, 0);

            

            this.Focus();
        }

        /// <summary>
        /// Stick editor to this thing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapBrowser_LocationChanged(object sender, EventArgs e)
        {
            this.Editor.Location = this.Location + new Size(this.EditorClosedSize.Width, 65);
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
                this.RefreshTimer.Stop();
                this.RefreshTimer.Start();
                this.ButtonRefresh.Enabled = false;
            }

            Editor = null;

            this.LocationChanged -= MapBrowser_LocationChanged;
            this.AnimationTimer.Start();
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
                this.EditorNew = new MapEditor();
                this.EditorNew.FormClosed += new FormClosedEventHandler(EditorNew_FormClosed);
            }

            this.EditorNew.Visible = false;
            this.EditorNew.Generate(0, 0, 0,0 , String.Empty, null, 0, null, null);
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
            if (MessageBox.Show("Are you sure you want to delete map <" + this.TreeResults.SelectedNode.Text + ">?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ERAServer.Data.Map.GetCollection().Remove(
                    Query.EQ("_id", 
                        SelectedMap.Id)
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
                this.EditorNew = new MapEditor();
                this.EditorNew.FormClosed += new FormClosedEventHandler(EditorNew_FormClosed);
            }

            this.EditorNew.Visible = false;
            using (Stream stream = OpenFileDialog1.OpenFile())
            {
                this.EditorNew.LoadFrom(Map.FromFile(stream, Path.GetDirectoryName(OpenFileDialog1.FileName)));
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
            if (this.Editor != null && this.Editor.Visible)
                if (MessageBox.Show(this, "This will refresh the map list. The open editor window will be closed and any unsaved changes " +
                        "will be lost. Are you sure you want to continue?", "Warning", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;

            this.ButtonRefresh.Enabled = false;
            this.RefreshTimer.Stop();
            this.RefreshTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeResults_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //(e.Node.Tag is Map)
            ListResults_SelectedIndexChanged(sender, e);
        }
    }
}
