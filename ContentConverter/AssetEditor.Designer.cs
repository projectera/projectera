namespace ContentConverter
{
    partial class AssetEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetEditor));
            this.LabelFileName = new System.Windows.Forms.Label();
            this.LabelAssetType = new System.Windows.Forms.Label();
            this.ComboBoxAssetType = new System.Windows.Forms.ComboBox();
            this.TextBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LabelRemoteName = new System.Windows.Forms.Label();
            this.ButtonAliases = new System.Windows.Forms.Button();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelFileName
            // 
            this.LabelFileName.AutoSize = true;
            this.LabelFileName.Location = new System.Drawing.Point(12, 9);
            this.LabelFileName.Name = "LabelFileName";
            this.LabelFileName.Size = new System.Drawing.Size(35, 13);
            this.LabelFileName.TabIndex = 0;
            this.LabelFileName.Text = "Name";
            // 
            // LabelAssetType
            // 
            this.LabelAssetType.AutoSize = true;
            this.LabelAssetType.Location = new System.Drawing.Point(12, 35);
            this.LabelAssetType.Name = "LabelAssetType";
            this.LabelAssetType.Size = new System.Drawing.Size(31, 13);
            this.LabelAssetType.TabIndex = 1;
            this.LabelAssetType.Text = "Type";
            // 
            // ComboBoxAssetType
            // 
            this.ComboBoxAssetType.FormattingEnabled = true;
            this.ComboBoxAssetType.Items.AddRange(new object[] {
            "NotSpecified"});
            this.ComboBoxAssetType.Location = new System.Drawing.Point(83, 32);
            this.ComboBoxAssetType.Name = "ComboBoxAssetType";
            this.ComboBoxAssetType.Size = new System.Drawing.Size(156, 21);
            this.ComboBoxAssetType.TabIndex = 2;
            this.ToolTip1.SetToolTip(this.ComboBoxAssetType, "Asset type. This is used to determine the namespace of this asset");
            this.ComboBoxAssetType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxAssetType_SelectedIndexChanged);
            // 
            // TextBoxName
            // 
            this.TextBoxName.Location = new System.Drawing.Point(83, 6);
            this.TextBoxName.Name = "TextBoxName";
            this.TextBoxName.Size = new System.Drawing.Size(189, 20);
            this.TextBoxName.TabIndex = 1;
            this.ToolTip1.SetToolTip(this.TextBoxName, "Filename of the asset");
            this.TextBoxName.TextChanged += new System.EventHandler(this.TextBoxName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Remote";
            // 
            // LabelRemoteName
            // 
            this.LabelRemoteName.AutoSize = true;
            this.LabelRemoteName.Location = new System.Drawing.Point(80, 68);
            this.LabelRemoteName.Name = "LabelRemoteName";
            this.LabelRemoteName.Size = new System.Drawing.Size(61, 13);
            this.LabelRemoteName.TabIndex = 21;
            this.LabelRemoteName.Text = "Graphics....";
            this.ToolTip1.SetToolTip(this.LabelRemoteName, "Remote location constructed of rootspace.localspace.name");
            // 
            // ButtonAliases
            // 
            this.ButtonAliases.Image = global::ContentConverter.Properties.Resources.table_relationship;
            this.ButtonAliases.Location = new System.Drawing.Point(245, 32);
            this.ButtonAliases.Name = "ButtonAliases";
            this.ButtonAliases.Size = new System.Drawing.Size(27, 24);
            this.ButtonAliases.TabIndex = 3;
            this.ToolTip1.SetToolTip(this.ButtonAliases, "Click to edit the aliases under which this asset is available");
            this.ButtonAliases.UseVisualStyleBackColor = true;
            this.ButtonAliases.Click += new System.EventHandler(this.ButtonAliases_Click);
            // 
            // PictureBox
            // 
            this.PictureBox.ErrorImage = global::ContentConverter.Properties.Resources.picture_error;
            this.PictureBox.InitialImage = global::ContentConverter.Properties.Resources.picture_empty;
            this.PictureBox.Location = new System.Drawing.Point(15, 88);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(257, 132);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PictureBox.TabIndex = 19;
            this.PictureBox.TabStop = false;
            this.ToolTip1.SetToolTip(this.PictureBox, "Graphical representation of this asset");
            // 
            // ButtonSave
            // 
            this.ButtonSave.Image = ((System.Drawing.Image)(resources.GetObject("ButtonSave.Image")));
            this.ButtonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonSave.Location = new System.Drawing.Point(12, 226);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(179, 24);
            this.ButtonSave.TabIndex = 5;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonSave, "Save the asset to the Remote location");
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // ButtonLoad
            // 
            this.ButtonLoad.Image = global::ContentConverter.Properties.Resources.folder_explore;
            this.ButtonLoad.Location = new System.Drawing.Point(197, 226);
            this.ButtonLoad.Name = "ButtonLoad";
            this.ButtonLoad.Size = new System.Drawing.Size(75, 24);
            this.ButtonLoad.TabIndex = 4;
            this.ButtonLoad.Text = "Replace";
            this.ButtonLoad.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonLoad, "Replace image with another");
            this.ButtonLoad.UseVisualStyleBackColor = true;
            this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // OpenFileDialog1
            // 
            this.OpenFileDialog1.Filter = "Images|*.png";
            this.OpenFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // AssetEditor
            // 
            this.AcceptButton = this.ButtonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.ButtonLoad);
            this.Controls.Add(this.ButtonAliases);
            this.Controls.Add(this.LabelRemoteName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.TextBoxName);
            this.Controls.Add(this.ComboBoxAssetType);
            this.Controls.Add(this.LabelAssetType);
            this.Controls.Add(this.LabelFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "AssetEditor";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Asset Editor";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelFileName;
        private System.Windows.Forms.Label LabelAssetType;
        private System.Windows.Forms.ComboBox ComboBoxAssetType;
        private System.Windows.Forms.TextBox TextBoxName;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label LabelRemoteName;
        private System.Windows.Forms.Button ButtonAliases;
        private System.Windows.Forms.ToolTip ToolTip1;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog1;
    }
}