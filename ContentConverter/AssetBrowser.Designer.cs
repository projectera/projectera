namespace ContentConverter
{
    partial class AssetBrowser
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
            this.LabelFileName = new System.Windows.Forms.Label();
            this.TextBoxName = new System.Windows.Forms.TextBox();
            this.ComboBoxAssetType = new System.Windows.Forms.ComboBox();
            this.LabelAssetType = new System.Windows.Forms.Label();
            this.ButtonSearch = new System.Windows.Forms.Button();
            this.ListResults = new System.Windows.Forms.ListBox();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.ButtonEdit = new System.Windows.Forms.Button();
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.ButtonUse = new System.Windows.Forms.Button();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelFileName
            // 
            this.LabelFileName.AutoSize = true;
            this.LabelFileName.Location = new System.Drawing.Point(12, 40);
            this.LabelFileName.Name = "LabelFileName";
            this.LabelFileName.Size = new System.Drawing.Size(35, 13);
            this.LabelFileName.TabIndex = 1;
            this.LabelFileName.Text = "Name";
            // 
            // TextBoxName
            // 
            this.TextBoxName.Location = new System.Drawing.Point(83, 37);
            this.TextBoxName.Name = "TextBoxName";
            this.TextBoxName.Size = new System.Drawing.Size(189, 20);
            this.TextBoxName.TabIndex = 1;
            this.ToolTip1.SetToolTip(this.TextBoxName, "Search query as LIKE %Name%");
            this.TextBoxName.TextChanged += new System.EventHandler(this.TextBoxName_TextChanged);
            // 
            // ComboBoxAssetType
            // 
            this.ComboBoxAssetType.FormattingEnabled = true;
            this.ComboBoxAssetType.Items.AddRange(new object[] {
            "NotSpecified"});
            this.ComboBoxAssetType.Location = new System.Drawing.Point(83, 63);
            this.ComboBoxAssetType.Name = "ComboBoxAssetType";
            this.ComboBoxAssetType.Size = new System.Drawing.Size(111, 21);
            this.ComboBoxAssetType.TabIndex = 2;
            this.ToolTip1.SetToolTip(this.ComboBoxAssetType, "Namespace filter");
            this.ComboBoxAssetType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxAssetType_SelectedIndexChanged);
            // 
            // LabelAssetType
            // 
            this.LabelAssetType.AutoSize = true;
            this.LabelAssetType.Location = new System.Drawing.Point(12, 66);
            this.LabelAssetType.Name = "LabelAssetType";
            this.LabelAssetType.Size = new System.Drawing.Size(31, 13);
            this.LabelAssetType.TabIndex = 16;
            this.LabelAssetType.Text = "Type";
            // 
            // ButtonSearch
            // 
            this.ButtonSearch.Image = global::ContentConverter.Properties.Resources.magnifier;
            this.ButtonSearch.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.ButtonSearch.Location = new System.Drawing.Point(200, 62);
            this.ButtonSearch.Name = "ButtonSearch";
            this.ButtonSearch.Size = new System.Drawing.Size(72, 23);
            this.ButtonSearch.TabIndex = 3;
            this.ButtonSearch.Text = "Search";
            this.ButtonSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonSearch, "Press to execute the search (again)");
            this.ButtonSearch.UseVisualStyleBackColor = true;
            this.ButtonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
            // 
            // ListResults
            // 
            this.ListResults.Enabled = false;
            this.ListResults.FormattingEnabled = true;
            this.ListResults.Location = new System.Drawing.Point(15, 90);
            this.ListResults.Name = "ListResults";
            this.ListResults.Size = new System.Drawing.Size(257, 160);
            this.ListResults.TabIndex = 4;
            this.ListResults.SelectedIndexChanged += new System.EventHandler(this.ListResults_SelectedIndexChanged);
            // 
            // OpenFileDialog1
            // 
            this.OpenFileDialog1.Filter = "Images|*.png";
            this.OpenFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // ButtonEdit
            // 
            this.ButtonEdit.Image = global::ContentConverter.Properties.Resources.pencil;
            this.ButtonEdit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonEdit.Location = new System.Drawing.Point(278, 226);
            this.ButtonEdit.Name = "ButtonEdit";
            this.ButtonEdit.Size = new System.Drawing.Size(27, 24);
            this.ButtonEdit.TabIndex = 5;
            this.ButtonEdit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ToolTip1.SetToolTip(this.ButtonEdit, "Edit the selected asset");
            this.ButtonEdit.UseVisualStyleBackColor = true;
            this.ButtonEdit.Click += new System.EventHandler(this.ButtonEdit_Click);
            // 
            // ButtonLoad
            // 
            this.ButtonLoad.Image = global::ContentConverter.Properties.Resources.folder_explore;
            this.ButtonLoad.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonLoad.Location = new System.Drawing.Point(311, 226);
            this.ButtonLoad.Name = "ButtonLoad";
            this.ButtonLoad.Size = new System.Drawing.Size(59, 24);
            this.ButtonLoad.TabIndex = 6;
            this.ButtonLoad.Text = "Load";
            this.ButtonLoad.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ToolTip1.SetToolTip(this.ButtonLoad, "Load a new asset from a file");
            this.ButtonLoad.UseVisualStyleBackColor = true;
            this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // ButtonUse
            // 
            this.ButtonUse.Enabled = false;
            this.ButtonUse.Image = global::ContentConverter.Properties.Resources.link;
            this.ButtonUse.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonUse.Location = new System.Drawing.Point(376, 226);
            this.ButtonUse.Name = "ButtonUse";
            this.ButtonUse.Size = new System.Drawing.Size(164, 24);
            this.ButtonUse.TabIndex = 7;
            this.ButtonUse.Text = "Link";
            this.ButtonUse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonUse, "Link the selected asset to the parent window");
            this.ButtonUse.UseVisualStyleBackColor = true;
            this.ButtonUse.Click += new System.EventHandler(this.ButtonUse_Click);
            // 
            // PictureBox
            // 
            this.PictureBox.ErrorImage = global::ContentConverter.Properties.Resources.picture_error;
            this.PictureBox.InitialImage = global::ContentConverter.Properties.Resources.picture_empty;
            this.PictureBox.Location = new System.Drawing.Point(278, 6);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(262, 214);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PictureBox.TabIndex = 21;
            this.PictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 24);
            this.label1.TabIndex = 25;
            this.label1.Text = "Assets";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AssetBrowser
            // 
            this.AcceptButton = this.ButtonUse;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 262);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonEdit);
            this.Controls.Add(this.ButtonLoad);
            this.Controls.Add(this.ButtonUse);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.ListResults);
            this.Controls.Add(this.ButtonSearch);
            this.Controls.Add(this.LabelAssetType);
            this.Controls.Add(this.ComboBoxAssetType);
            this.Controls.Add(this.TextBoxName);
            this.Controls.Add(this.LabelFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "AssetBrowser";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Asset Browser";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelFileName;
        private System.Windows.Forms.TextBox TextBoxName;
        private System.Windows.Forms.ComboBox ComboBoxAssetType;
        private System.Windows.Forms.Label LabelAssetType;
        private System.Windows.Forms.Button ButtonSearch;
        private System.Windows.Forms.ListBox ListResults;
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Button ButtonUse;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog1;
        private System.Windows.Forms.Button ButtonEdit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip ToolTip1;
    }
}