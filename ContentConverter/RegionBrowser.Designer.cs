namespace ContentConverter
{
    partial class RegionBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegionBrowser));
            this.Title = new System.Windows.Forms.Label();
            this.ListResults = new System.Windows.Forms.ListBox();
            this.LabelAssetType = new System.Windows.Forms.Label();
            this.ComboBoxRegionType = new System.Windows.Forms.ComboBox();
            this.TextBoxName = new System.Windows.Forms.TextBox();
            this.LabelFileName = new System.Windows.Forms.Label();
            this.ButtonSearch = new System.Windows.Forms.Button();
            this.ButtonUse = new System.Windows.Forms.Button();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Title
            // 
            this.Title.AutoSize = true;
            this.Title.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Title.Location = new System.Drawing.Point(12, 9);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(80, 24);
            this.Title.TabIndex = 26;
            this.Title.Text = "Regions";
            this.Title.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ListResults
            // 
            this.ListResults.Enabled = false;
            this.ListResults.FormattingEnabled = true;
            this.ListResults.Location = new System.Drawing.Point(15, 89);
            this.ListResults.Name = "ListResults";
            this.ListResults.Size = new System.Drawing.Size(257, 160);
            this.ListResults.TabIndex = 31;
            this.ListResults.SelectedIndexChanged += new System.EventHandler(this.ListResults_SelectedIndexChanged);
            // 
            // LabelAssetType
            // 
            this.LabelAssetType.AutoSize = true;
            this.LabelAssetType.Location = new System.Drawing.Point(12, 65);
            this.LabelAssetType.Name = "LabelAssetType";
            this.LabelAssetType.Size = new System.Drawing.Size(31, 13);
            this.LabelAssetType.TabIndex = 32;
            this.LabelAssetType.Text = "Type";
            // 
            // ComboBoxRegionType
            // 
            this.ComboBoxRegionType.FormattingEnabled = true;
            this.ComboBoxRegionType.Items.AddRange(new object[] {
            "NotSpecified"});
            this.ComboBoxRegionType.Location = new System.Drawing.Point(83, 62);
            this.ComboBoxRegionType.Name = "ComboBoxRegionType";
            this.ComboBoxRegionType.Size = new System.Drawing.Size(111, 21);
            this.ComboBoxRegionType.TabIndex = 29;
            this.ComboBoxRegionType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxRegionType_SelectedIndexChanged);
            // 
            // TextBoxName
            // 
            this.TextBoxName.Location = new System.Drawing.Point(83, 36);
            this.TextBoxName.Name = "TextBoxName";
            this.TextBoxName.Size = new System.Drawing.Size(189, 20);
            this.TextBoxName.TabIndex = 27;
            // 
            // LabelFileName
            // 
            this.LabelFileName.AutoSize = true;
            this.LabelFileName.Location = new System.Drawing.Point(12, 39);
            this.LabelFileName.Name = "LabelFileName";
            this.LabelFileName.Size = new System.Drawing.Size(35, 13);
            this.LabelFileName.TabIndex = 28;
            this.LabelFileName.Text = "Name";
            // 
            // ButtonSearch
            // 
            this.ButtonSearch.Image = global::ContentConverter.Properties.Resources.magnifier;
            this.ButtonSearch.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.ButtonSearch.Location = new System.Drawing.Point(200, 61);
            this.ButtonSearch.Name = "ButtonSearch";
            this.ButtonSearch.Size = new System.Drawing.Size(72, 23);
            this.ButtonSearch.TabIndex = 30;
            this.ButtonSearch.Text = "Search";
            this.ButtonSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonSearch.UseVisualStyleBackColor = true;
            this.ButtonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
            // 
            // ButtonUse
            // 
            this.ButtonUse.Enabled = false;
            this.ButtonUse.Image = global::ContentConverter.Properties.Resources.link;
            this.ButtonUse.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonUse.Location = new System.Drawing.Point(376, 225);
            this.ButtonUse.Name = "ButtonUse";
            this.ButtonUse.Size = new System.Drawing.Size(164, 24);
            this.ButtonUse.TabIndex = 33;
            this.ButtonUse.Text = "Link";
            this.ButtonUse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonUse.UseVisualStyleBackColor = true;
            this.ButtonUse.Click += new System.EventHandler(this.ButtonUse_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.Enabled = false;
            this.ButtonSave.Image = ((System.Drawing.Image)(resources.GetObject("ButtonSave.Image")));
            this.ButtonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonSave.Location = new System.Drawing.Point(278, 225);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(92, 24);
            this.ButtonSave.TabIndex = 34;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // RegionBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 262);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.ButtonUse);
            this.Controls.Add(this.ListResults);
            this.Controls.Add(this.ButtonSearch);
            this.Controls.Add(this.LabelAssetType);
            this.Controls.Add(this.ComboBoxRegionType);
            this.Controls.Add(this.TextBoxName);
            this.Controls.Add(this.LabelFileName);
            this.Controls.Add(this.Title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "RegionBrowser";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Region Browser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Title;
        private System.Windows.Forms.ListBox ListResults;
        private System.Windows.Forms.Button ButtonSearch;
        private System.Windows.Forms.Label LabelAssetType;
        private System.Windows.Forms.ComboBox ComboBoxRegionType;
        private System.Windows.Forms.TextBox TextBoxName;
        private System.Windows.Forms.Label LabelFileName;
        private System.Windows.Forms.Button ButtonUse;
        private System.Windows.Forms.Button ButtonSave;
    }
}