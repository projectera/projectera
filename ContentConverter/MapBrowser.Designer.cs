namespace ContentConverter
{
    partial class MapBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonRefresh = new System.Windows.Forms.Button();
            this.ButtonDelete = new System.Windows.Forms.Button();
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.ButtonAdd = new System.Windows.Forms.Button();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.TreeResults = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // OpenFileDialog1
            // 
            this.OpenFileDialog1.Filter = "XML Map|*MapXml*.xml|XML data|*.xml";
            this.OpenFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 24);
            this.label1.TabIndex = 5;
            this.label1.Text = "Maps";
            // 
            // ButtonRefresh
            // 
            this.ButtonRefresh.Image = global::ContentConverter.Properties.Resources.arrow_refresh;
            this.ButtonRefresh.Location = new System.Drawing.Point(179, 10);
            this.ButtonRefresh.Name = "ButtonRefresh";
            this.ButtonRefresh.Size = new System.Drawing.Size(27, 24);
            this.ButtonRefresh.TabIndex = 5;
            this.ToolTip1.SetToolTip(this.ButtonRefresh, "Refresh the the map list");
            this.ButtonRefresh.UseVisualStyleBackColor = true;
            this.ButtonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // ButtonDelete
            // 
            this.ButtonDelete.Image = global::ContentConverter.Properties.Resources.delete;
            this.ButtonDelete.Location = new System.Drawing.Point(45, 375);
            this.ButtonDelete.Name = "ButtonDelete";
            this.ButtonDelete.Size = new System.Drawing.Size(27, 24);
            this.ButtonDelete.TabIndex = 3;
            this.ToolTip1.SetToolTip(this.ButtonDelete, "Remove the selected map");
            this.ButtonDelete.UseVisualStyleBackColor = true;
            this.ButtonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // ButtonLoad
            // 
            this.ButtonLoad.Image = global::ContentConverter.Properties.Resources.folder_explore;
            this.ButtonLoad.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonLoad.Location = new System.Drawing.Point(147, 375);
            this.ButtonLoad.Name = "ButtonLoad";
            this.ButtonLoad.Size = new System.Drawing.Size(59, 24);
            this.ButtonLoad.TabIndex = 4;
            this.ButtonLoad.Text = "Load";
            this.ButtonLoad.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ToolTip1.SetToolTip(this.ButtonLoad, "Load map data from an XML Map data file");
            this.ButtonLoad.UseVisualStyleBackColor = true;
            this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // ButtonAdd
            // 
            this.ButtonAdd.Image = global::ContentConverter.Properties.Resources.add;
            this.ButtonAdd.Location = new System.Drawing.Point(12, 375);
            this.ButtonAdd.Name = "ButtonAdd";
            this.ButtonAdd.Size = new System.Drawing.Size(27, 24);
            this.ButtonAdd.TabIndex = 2;
            this.ToolTip1.SetToolTip(this.ButtonAdd, "Add a new map");
            this.ButtonAdd.UseVisualStyleBackColor = true;
            this.ButtonAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
            // 
            // TreeResults
            // 
            this.TreeResults.Location = new System.Drawing.Point(12, 40);
            this.TreeResults.Name = "TreeResults";
            this.TreeResults.Size = new System.Drawing.Size(194, 329);
            this.TreeResults.TabIndex = 1;
            this.TreeResults.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeResults_AfterSelect);
            // 
            // MapBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(218, 411);
            this.Controls.Add(this.TreeResults);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonRefresh);
            this.Controls.Add(this.ButtonDelete);
            this.Controls.Add(this.ButtonLoad);
            this.Controls.Add(this.ButtonAdd);
            this.MaximizeBox = false;
            this.Name = "MapBrowser";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Map Browser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonAdd;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.Button ButtonDelete;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog1;
        private System.Windows.Forms.Button ButtonRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip ToolTip1;
        private System.Windows.Forms.TreeView TreeResults;
    }
}