namespace ContentConverter
{
    partial class MapEditor
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
            this.TextBoxMapId = new System.Windows.Forms.TextBox();
            this.LabelMapId = new System.Windows.Forms.Label();
            this.LabelTilesetId = new System.Windows.Forms.Label();
            this.TextBoxTilesetId = new System.Windows.Forms.TextBox();
            this.TextBoxRegionId = new System.Windows.Forms.TextBox();
            this.LabelRegionId = new System.Windows.Forms.Label();
            this.LabelName = new System.Windows.Forms.Label();
            this.TextBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ComboBoxMapType = new System.Windows.Forms.ComboBox();
            this.TextBoxTilesetName = new System.Windows.Forms.TextBox();
            this.TextBoxRegionName = new System.Windows.Forms.TextBox();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ButtonSave = new System.Windows.Forms.Button();
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.ButtonLink = new System.Windows.Forms.Button();
            this.ButtonEditRegionId = new System.Windows.Forms.Button();
            this.ButtonEditTilesetId = new System.Windows.Forms.Button();
            this.ButtonWeather = new System.Windows.Forms.Button();
            this.ButtonRegionLookup = new System.Windows.Forms.Button();
            this.BrowseButonRegion = new System.Windows.Forms.Button();
            this.BrowseButtonTileset = new System.Windows.Forms.Button();
            this.ButtonTilesetLookup = new System.Windows.Forms.Button();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.ButtonEditId = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // TextBoxMapId
            // 
            this.TextBoxMapId.Location = new System.Drawing.Point(81, 6);
            this.TextBoxMapId.Name = "TextBoxMapId";
            this.TextBoxMapId.Size = new System.Drawing.Size(177, 20);
            this.TextBoxMapId.TabIndex = 0;
            this.ToolTip1.SetToolTip(this.TextBoxMapId, "Map id of the map");
            // 
            // LabelMapId
            // 
            this.LabelMapId.AutoSize = true;
            this.LabelMapId.Location = new System.Drawing.Point(12, 9);
            this.LabelMapId.Name = "LabelMapId";
            this.LabelMapId.Size = new System.Drawing.Size(42, 13);
            this.LabelMapId.TabIndex = 1;
            this.LabelMapId.Text = "Map ID";
            // 
            // LabelTilesetId
            // 
            this.LabelTilesetId.AutoSize = true;
            this.LabelTilesetId.Location = new System.Drawing.Point(12, 35);
            this.LabelTilesetId.Name = "LabelTilesetId";
            this.LabelTilesetId.Size = new System.Drawing.Size(38, 13);
            this.LabelTilesetId.TabIndex = 2;
            this.LabelTilesetId.Text = "Tileset";
            // 
            // TextBoxTilesetId
            // 
            this.TextBoxTilesetId.Location = new System.Drawing.Point(81, 32);
            this.TextBoxTilesetId.Name = "TextBoxTilesetId";
            this.TextBoxTilesetId.Size = new System.Drawing.Size(176, 20);
            this.TextBoxTilesetId.TabIndex = 3;
            this.ToolTip1.SetToolTip(this.TextBoxTilesetId, "Tileset id");
            // 
            // TextBoxRegionId
            // 
            this.TextBoxRegionId.Location = new System.Drawing.Point(81, 58);
            this.TextBoxRegionId.Name = "TextBoxRegionId";
            this.TextBoxRegionId.Size = new System.Drawing.Size(177, 20);
            this.TextBoxRegionId.TabIndex = 8;
            this.ToolTip1.SetToolTip(this.TextBoxRegionId, "Region id");
            // 
            // LabelRegionId
            // 
            this.LabelRegionId.AutoSize = true;
            this.LabelRegionId.Location = new System.Drawing.Point(12, 61);
            this.LabelRegionId.Name = "LabelRegionId";
            this.LabelRegionId.Size = new System.Drawing.Size(41, 13);
            this.LabelRegionId.TabIndex = 5;
            this.LabelRegionId.Text = "Region";
            // 
            // LabelName
            // 
            this.LabelName.AutoSize = true;
            this.LabelName.Location = new System.Drawing.Point(12, 106);
            this.LabelName.Name = "LabelName";
            this.LabelName.Size = new System.Drawing.Size(35, 13);
            this.LabelName.TabIndex = 6;
            this.LabelName.Text = "Name";
            // 
            // TextBoxName
            // 
            this.TextBoxName.Location = new System.Drawing.Point(81, 103);
            this.TextBoxName.Name = "TextBoxName";
            this.TextBoxName.Size = new System.Drawing.Size(177, 20);
            this.TextBoxName.TabIndex = 12;
            this.ToolTip1.SetToolTip(this.TextBoxName, "The name of the map");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Type";
            // 
            // ComboBoxMapType
            // 
            this.ComboBoxMapType.FormattingEnabled = true;
            this.ComboBoxMapType.Items.AddRange(new object[] {
            "NotSpecified"});
            this.ComboBoxMapType.Location = new System.Drawing.Point(81, 129);
            this.ComboBoxMapType.Name = "ComboBoxMapType";
            this.ComboBoxMapType.Size = new System.Drawing.Size(177, 21);
            this.ComboBoxMapType.TabIndex = 14;
            this.ToolTip1.SetToolTip(this.ComboBoxMapType, "The maptype of the map");
            // 
            // TextBoxTilesetName
            // 
            this.TextBoxTilesetName.Location = new System.Drawing.Point(81, 32);
            this.TextBoxTilesetName.Name = "TextBoxTilesetName";
            this.TextBoxTilesetName.ReadOnly = true;
            this.TextBoxTilesetName.Size = new System.Drawing.Size(177, 20);
            this.TextBoxTilesetName.TabIndex = 2;
            this.ToolTip1.SetToolTip(this.TextBoxTilesetName, "Name of the tileset");
            this.TextBoxTilesetName.Visible = false;
            // 
            // TextBoxRegionName
            // 
            this.TextBoxRegionName.Location = new System.Drawing.Point(81, 58);
            this.TextBoxRegionName.Name = "TextBoxRegionName";
            this.TextBoxRegionName.ReadOnly = true;
            this.TextBoxRegionName.Size = new System.Drawing.Size(176, 20);
            this.TextBoxRegionName.TabIndex = 7;
            this.ToolTip1.SetToolTip(this.TextBoxRegionName, "Name of the region");
            this.TextBoxRegionName.Visible = false;
            // 
            // ButtonSave
            // 
            this.ButtonSave.Image = global::ContentConverter.Properties.Resources.database_save;
            this.ButtonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonSave.Location = new System.Drawing.Point(81, 170);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(177, 24);
            this.ButtonSave.TabIndex = 16;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonSave, "Saves this map to the database");
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // ButtonLoad
            // 
            this.ButtonLoad.Image = global::ContentConverter.Properties.Resources.folder_wrench;
            this.ButtonLoad.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonLoad.Location = new System.Drawing.Point(264, 100);
            this.ButtonLoad.Name = "ButtonLoad";
            this.ButtonLoad.Size = new System.Drawing.Size(57, 24);
            this.ButtonLoad.TabIndex = 13;
            this.ButtonLoad.Text = "Data";
            this.ButtonLoad.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ToolTip1.SetToolTip(this.ButtonLoad, "Set the map data for this map");
            this.ButtonLoad.UseVisualStyleBackColor = true;
            this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // ButtonLink
            // 
            this.ButtonLink.Enabled = false;
            this.ButtonLink.Image = global::ContentConverter.Properties.Resources.link;
            this.ButtonLink.Location = new System.Drawing.Point(15, 170);
            this.ButtonLink.Name = "ButtonLink";
            this.ButtonLink.Size = new System.Drawing.Size(60, 24);
            this.ButtonLink.TabIndex = 17;
            this.ButtonLink.Text = "Link";
            this.ButtonLink.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonLink, "Uses this map in the parent window. Auto saves!");
            this.ButtonLink.UseVisualStyleBackColor = true;
            this.ButtonLink.Click += new System.EventHandler(this.ButtonLink_Click);
            // 
            // ButtonEditRegionId
            // 
            this.ButtonEditRegionId.Image = global::ContentConverter.Properties.Resources.pencil;
            this.ButtonEditRegionId.Location = new System.Drawing.Point(263, 55);
            this.ButtonEditRegionId.Name = "ButtonEditRegionId";
            this.ButtonEditRegionId.Size = new System.Drawing.Size(27, 24);
            this.ButtonEditRegionId.TabIndex = 9;
            this.ToolTip1.SetToolTip(this.ButtonEditRegionId, "Edit the region id");
            this.ButtonEditRegionId.UseVisualStyleBackColor = true;
            this.ButtonEditRegionId.Click += new System.EventHandler(this.ButtonEditRegionId_Click);
            // 
            // ButtonEditTilesetId
            // 
            this.ButtonEditTilesetId.Image = global::ContentConverter.Properties.Resources.pencil;
            this.ButtonEditTilesetId.Location = new System.Drawing.Point(263, 28);
            this.ButtonEditTilesetId.Name = "ButtonEditTilesetId";
            this.ButtonEditTilesetId.Size = new System.Drawing.Size(27, 24);
            this.ButtonEditTilesetId.TabIndex = 4;
            this.ToolTip1.SetToolTip(this.ButtonEditTilesetId, "Edit the tileset ID");
            this.ButtonEditTilesetId.UseVisualStyleBackColor = true;
            this.ButtonEditTilesetId.Click += new System.EventHandler(this.ButtonEditTilesetId_Click);
            // 
            // ButtonWeather
            // 
            this.ButtonWeather.Enabled = false;
            this.ButtonWeather.Image = global::ContentConverter.Properties.Resources.weather_cloudy;
            this.ButtonWeather.Location = new System.Drawing.Point(263, 126);
            this.ButtonWeather.Name = "ButtonWeather";
            this.ButtonWeather.Size = new System.Drawing.Size(87, 24);
            this.ButtonWeather.TabIndex = 15;
            this.ButtonWeather.Text = "Weather...";
            this.ButtonWeather.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonWeather, "Change the weather settings for this map");
            this.ButtonWeather.UseVisualStyleBackColor = true;
            // 
            // ButtonRegionLookup
            // 
            this.ButtonRegionLookup.Enabled = false;
            this.ButtonRegionLookup.Image = global::ContentConverter.Properties.Resources.information;
            this.ButtonRegionLookup.Location = new System.Drawing.Point(323, 54);
            this.ButtonRegionLookup.Name = "ButtonRegionLookup";
            this.ButtonRegionLookup.Size = new System.Drawing.Size(27, 24);
            this.ButtonRegionLookup.TabIndex = 11;
            this.ToolTip1.SetToolTip(this.ButtonRegionLookup, "Shows/Hides the name of the linked region");
            this.ButtonRegionLookup.UseVisualStyleBackColor = true;
            // 
            // BrowseButonRegion
            // 
            this.BrowseButonRegion.Enabled = false;
            this.BrowseButonRegion.Image = global::ContentConverter.Properties.Resources.database_link;
            this.BrowseButonRegion.Location = new System.Drawing.Point(293, 54);
            this.BrowseButonRegion.Name = "BrowseButonRegion";
            this.BrowseButonRegion.Size = new System.Drawing.Size(27, 24);
            this.BrowseButonRegion.TabIndex = 10;
            this.BrowseButonRegion.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.BrowseButonRegion, "Opens the region browser to link a region");
            this.BrowseButonRegion.UseVisualStyleBackColor = true;
            this.BrowseButonRegion.Click += new System.EventHandler(this.BrowseButonRegion_Click);
            // 
            // BrowseButtonTileset
            // 
            this.BrowseButtonTileset.Image = global::ContentConverter.Properties.Resources.database_link;
            this.BrowseButtonTileset.Location = new System.Drawing.Point(293, 29);
            this.BrowseButtonTileset.Name = "BrowseButtonTileset";
            this.BrowseButtonTileset.Size = new System.Drawing.Size(27, 24);
            this.BrowseButtonTileset.TabIndex = 5;
            this.BrowseButtonTileset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.BrowseButtonTileset, "Opens the tileset browser to link a tileset");
            this.BrowseButtonTileset.UseVisualStyleBackColor = true;
            this.BrowseButtonTileset.Click += new System.EventHandler(this.BrowseButtonTileset_Click);
            // 
            // ButtonTilesetLookup
            // 
            this.ButtonTilesetLookup.Image = global::ContentConverter.Properties.Resources.information;
            this.ButtonTilesetLookup.Location = new System.Drawing.Point(323, 29);
            this.ButtonTilesetLookup.Name = "ButtonTilesetLookup";
            this.ButtonTilesetLookup.Size = new System.Drawing.Size(27, 24);
            this.ButtonTilesetLookup.TabIndex = 6;
            this.ToolTip1.SetToolTip(this.ButtonTilesetLookup, "Shows/Hides the name of the linked tileset");
            this.ButtonTilesetLookup.UseVisualStyleBackColor = true;
            this.ButtonTilesetLookup.Click += new System.EventHandler(this.ButtonTilesetLookup_Click);
            // 
            // PictureBox
            // 
            this.PictureBox.ErrorImage = global::ContentConverter.Properties.Resources.cross;
            this.PictureBox.InitialImage = global::ContentConverter.Properties.Resources.hourglass;
            this.PictureBox.Location = new System.Drawing.Point(323, 100);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(27, 24);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PictureBox.TabIndex = 57;
            this.PictureBox.TabStop = false;
            // 
            // ButtonEditId
            // 
            this.ButtonEditId.Image = global::ContentConverter.Properties.Resources.pencil;
            this.ButtonEditId.Location = new System.Drawing.Point(263, 3);
            this.ButtonEditId.Name = "ButtonEditId";
            this.ButtonEditId.Size = new System.Drawing.Size(27, 24);
            this.ButtonEditId.TabIndex = 1;
            this.ButtonEditId.UseVisualStyleBackColor = true;
            this.ButtonEditId.Click += new System.EventHandler(this.ButtonEditId_Click);
            // 
            // MapEditor
            // 
            this.AcceptButton = this.ButtonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 206);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.ButtonLoad);
            this.Controls.Add(this.ButtonLink);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.ButtonEditRegionId);
            this.Controls.Add(this.ButtonEditTilesetId);
            this.Controls.Add(this.ButtonWeather);
            this.Controls.Add(this.TextBoxRegionName);
            this.Controls.Add(this.ButtonRegionLookup);
            this.Controls.Add(this.BrowseButonRegion);
            this.Controls.Add(this.BrowseButtonTileset);
            this.Controls.Add(this.ButtonTilesetLookup);
            this.Controls.Add(this.ButtonEditId);
            this.Controls.Add(this.TextBoxTilesetName);
            this.Controls.Add(this.ComboBoxMapType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TextBoxName);
            this.Controls.Add(this.LabelName);
            this.Controls.Add(this.LabelRegionId);
            this.Controls.Add(this.TextBoxRegionId);
            this.Controls.Add(this.TextBoxTilesetId);
            this.Controls.Add(this.LabelTilesetId);
            this.Controls.Add(this.LabelMapId);
            this.Controls.Add(this.TextBoxMapId);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MapEditor";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Map Editor";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxMapId;
        private System.Windows.Forms.Label LabelMapId;
        private System.Windows.Forms.Label LabelTilesetId;
        private System.Windows.Forms.TextBox TextBoxTilesetId;
        private System.Windows.Forms.TextBox TextBoxRegionId;
        private System.Windows.Forms.Label LabelRegionId;
        private System.Windows.Forms.Label LabelName;
        private System.Windows.Forms.TextBox TextBoxName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ComboBoxMapType;
        private System.Windows.Forms.TextBox TextBoxTilesetName;
        private System.Windows.Forms.Button ButtonEditId;
        private System.Windows.Forms.Button ButtonTilesetLookup;
        private System.Windows.Forms.Button BrowseButtonTileset;
        private System.Windows.Forms.Button BrowseButonRegion;
        private System.Windows.Forms.Button ButtonRegionLookup;
        private System.Windows.Forms.TextBox TextBoxRegionName;
        private System.Windows.Forms.Button ButtonWeather;
        private System.Windows.Forms.Button ButtonEditTilesetId;
        private System.Windows.Forms.Button ButtonEditRegionId;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Button ButtonLink;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.ToolTip ToolTip1;
    }
}