namespace ContentConverter
{
    partial class TilesetEditor
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
            this.TextBoxName = new System.Windows.Forms.TextBox();
            this.LabelName = new System.Windows.Forms.Label();
            this.TextBoxTilesetId = new System.Windows.Forms.TextBox();
            this.LabelTilesetId = new System.Windows.Forms.Label();
            this.TextBoxGraphic = new System.Windows.Forms.TextBox();
            this.LabelAssetName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBoxAutoTile1 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTile2 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTile5 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTile4 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTile6 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTile7 = new System.Windows.Forms.TextBox();
            this.TextBoxGraphicId = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTile3 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTileGraphicId1 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTileGraphicId2 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTileGraphicId3 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTileGraphicId4 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTileGraphicId5 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTileGraphicId6 = new System.Windows.Forms.TextBox();
            this.TextBoxAutoTileGraphicId7 = new System.Windows.Forms.TextBox();
            this.Panel = new System.Windows.Forms.Panel();
            this.TilesetControl = new ContentConverter.TilesetControl(this.components);
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ButtonSave = new System.Windows.Forms.Button();
            this.ButtonLink = new System.Windows.Forms.Button();
            this.ButtonEditId = new System.Windows.Forms.Button();
            this.ButtonHealing = new System.Windows.Forms.Button();
            this.ButtonHazard = new System.Windows.Forms.Button();
            this.ButtonPoison = new System.Windows.Forms.Button();
            this.ButtonInheritPassages = new System.Windows.Forms.Button();
            this.ButtonBlocking = new System.Windows.Forms.Button();
            this.ButtonCounter = new System.Windows.Forms.Button();
            this.ButtonBush = new System.Windows.Forms.Button();
            this.ButtonTerrain = new System.Windows.Forms.Button();
            this.ButtonLayers = new System.Windows.Forms.Button();
            this.ButtonPassages = new System.Windows.Forms.Button();
            this.ButtonUpdateAutotiles = new System.Windows.Forms.Button();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.BrowseButtonTS = new System.Windows.Forms.Button();
            this.ButtonAutoTilesGraphicLookup = new System.Windows.Forms.Button();
            this.ButtonTilesetGraphicLookup = new System.Windows.Forms.Button();
            this.TilesCount = new System.Windows.Forms.NumericUpDown();
            this.LabelTilesCount = new System.Windows.Forms.Label();
            this.ButtonUpdateNumTiles = new System.Windows.Forms.Button();
            this.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TilesCount)).BeginInit();
            this.SuspendLayout();
            // 
            // TextBoxName
            // 
            this.TextBoxName.Location = new System.Drawing.Point(83, 32);
            this.TextBoxName.Name = "TextBoxName";
            this.TextBoxName.Size = new System.Drawing.Size(189, 20);
            this.TextBoxName.TabIndex = 3;
            this.ToolTip1.SetToolTip(this.TextBoxName, "The name of the tileset");
            // 
            // LabelName
            // 
            this.LabelName.AutoSize = true;
            this.LabelName.Location = new System.Drawing.Point(12, 35);
            this.LabelName.Name = "LabelName";
            this.LabelName.Size = new System.Drawing.Size(35, 13);
            this.LabelName.TabIndex = 19;
            this.LabelName.Text = "Name";
            // 
            // TextBoxTilesetId
            // 
            this.TextBoxTilesetId.Location = new System.Drawing.Point(83, 6);
            this.TextBoxTilesetId.Name = "TextBoxTilesetId";
            this.TextBoxTilesetId.Size = new System.Drawing.Size(156, 20);
            this.TextBoxTilesetId.TabIndex = 1;
            this.ToolTip1.SetToolTip(this.TextBoxTilesetId, "The ObjectId used to refer to this tileset");
            // 
            // LabelTilesetId
            // 
            this.LabelTilesetId.AutoSize = true;
            this.LabelTilesetId.Location = new System.Drawing.Point(12, 9);
            this.LabelTilesetId.Name = "LabelTilesetId";
            this.LabelTilesetId.Size = new System.Drawing.Size(52, 13);
            this.LabelTilesetId.TabIndex = 17;
            this.LabelTilesetId.Text = "Tileset ID";
            // 
            // TextBoxGraphic
            // 
            this.TextBoxGraphic.Location = new System.Drawing.Point(83, 58);
            this.TextBoxGraphic.Name = "TextBoxGraphic";
            this.TextBoxGraphic.Size = new System.Drawing.Size(189, 20);
            this.TextBoxGraphic.TabIndex = 5;
            this.ToolTip1.SetToolTip(this.TextBoxGraphic, "The name of the tileset graphic asset");
            this.TextBoxGraphic.TextChanged += new System.EventHandler(this.TextBoxGraphic_TextChanged);
            // 
            // LabelAssetName
            // 
            this.LabelAssetName.AutoSize = true;
            this.LabelAssetName.Location = new System.Drawing.Point(12, 61);
            this.LabelAssetName.Name = "LabelAssetName";
            this.LabelAssetName.Size = new System.Drawing.Size(44, 13);
            this.LabelAssetName.TabIndex = 27;
            this.LabelAssetName.Text = "Graphic";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 139);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "AutoTiles";
            // 
            // TextBoxAutoTile1
            // 
            this.TextBoxAutoTile1.Location = new System.Drawing.Point(83, 136);
            this.TextBoxAutoTile1.Name = "TextBoxAutoTile1";
            this.TextBoxAutoTile1.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTile1.TabIndex = 9;
            // 
            // TextBoxAutoTile2
            // 
            this.TextBoxAutoTile2.Location = new System.Drawing.Point(83, 162);
            this.TextBoxAutoTile2.Name = "TextBoxAutoTile2";
            this.TextBoxAutoTile2.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTile2.TabIndex = 12;
            // 
            // TextBoxAutoTile5
            // 
            this.TextBoxAutoTile5.Location = new System.Drawing.Point(83, 240);
            this.TextBoxAutoTile5.Name = "TextBoxAutoTile5";
            this.TextBoxAutoTile5.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTile5.TabIndex = 18;
            // 
            // TextBoxAutoTile4
            // 
            this.TextBoxAutoTile4.Location = new System.Drawing.Point(83, 214);
            this.TextBoxAutoTile4.Name = "TextBoxAutoTile4";
            this.TextBoxAutoTile4.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTile4.TabIndex = 16;
            // 
            // TextBoxAutoTile6
            // 
            this.TextBoxAutoTile6.Location = new System.Drawing.Point(83, 266);
            this.TextBoxAutoTile6.Name = "TextBoxAutoTile6";
            this.TextBoxAutoTile6.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTile6.TabIndex = 20;
            // 
            // TextBoxAutoTile7
            // 
            this.TextBoxAutoTile7.Location = new System.Drawing.Point(83, 292);
            this.TextBoxAutoTile7.Name = "TextBoxAutoTile7";
            this.TextBoxAutoTile7.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTile7.TabIndex = 22;
            // 
            // TextBoxGraphicId
            // 
            this.TextBoxGraphicId.Location = new System.Drawing.Point(83, 58);
            this.TextBoxGraphicId.Name = "TextBoxGraphicId";
            this.TextBoxGraphicId.ReadOnly = true;
            this.TextBoxGraphicId.Size = new System.Drawing.Size(189, 20);
            this.TextBoxGraphicId.TabIndex = 4;
            this.ToolTip1.SetToolTip(this.TextBoxGraphicId, "The id of the tileset graphic asset");
            this.TextBoxGraphicId.Visible = false;
            this.TextBoxGraphicId.Click += new System.EventHandler(this.TextBoxGraphicId_Click);
            // 
            // TextBoxAutoTile3
            // 
            this.TextBoxAutoTile3.Location = new System.Drawing.Point(83, 188);
            this.TextBoxAutoTile3.Name = "TextBoxAutoTile3";
            this.TextBoxAutoTile3.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTile3.TabIndex = 14;
            // 
            // TextBoxAutoTileGraphicId1
            // 
            this.TextBoxAutoTileGraphicId1.Location = new System.Drawing.Point(83, 136);
            this.TextBoxAutoTileGraphicId1.Name = "TextBoxAutoTileGraphicId1";
            this.TextBoxAutoTileGraphicId1.ReadOnly = true;
            this.TextBoxAutoTileGraphicId1.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTileGraphicId1.TabIndex = 8;
            this.ToolTip1.SetToolTip(this.TextBoxAutoTileGraphicId1, "First autotile asset id");
            this.TextBoxAutoTileGraphicId1.Visible = false;
            this.TextBoxAutoTileGraphicId1.Click += new System.EventHandler(this.TextBoxAutoTileGraphicId_Click);
            // 
            // TextBoxAutoTileGraphicId2
            // 
            this.TextBoxAutoTileGraphicId2.Location = new System.Drawing.Point(83, 162);
            this.TextBoxAutoTileGraphicId2.Name = "TextBoxAutoTileGraphicId2";
            this.TextBoxAutoTileGraphicId2.ReadOnly = true;
            this.TextBoxAutoTileGraphicId2.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTileGraphicId2.TabIndex = 11;
            this.ToolTip1.SetToolTip(this.TextBoxAutoTileGraphicId2, "Second autotile asset id");
            this.TextBoxAutoTileGraphicId2.Visible = false;
            this.TextBoxAutoTileGraphicId2.Click += new System.EventHandler(this.TextBoxAutoTileGraphicId_Click);
            // 
            // TextBoxAutoTileGraphicId3
            // 
            this.TextBoxAutoTileGraphicId3.Location = new System.Drawing.Point(83, 188);
            this.TextBoxAutoTileGraphicId3.Name = "TextBoxAutoTileGraphicId3";
            this.TextBoxAutoTileGraphicId3.ReadOnly = true;
            this.TextBoxAutoTileGraphicId3.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTileGraphicId3.TabIndex = 13;
            this.ToolTip1.SetToolTip(this.TextBoxAutoTileGraphicId3, "Third autotile asset id");
            this.TextBoxAutoTileGraphicId3.Visible = false;
            this.TextBoxAutoTileGraphicId3.Click += new System.EventHandler(this.TextBoxAutoTileGraphicId_Click);
            // 
            // TextBoxAutoTileGraphicId4
            // 
            this.TextBoxAutoTileGraphicId4.Location = new System.Drawing.Point(83, 214);
            this.TextBoxAutoTileGraphicId4.Name = "TextBoxAutoTileGraphicId4";
            this.TextBoxAutoTileGraphicId4.ReadOnly = true;
            this.TextBoxAutoTileGraphicId4.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTileGraphicId4.TabIndex = 15;
            this.ToolTip1.SetToolTip(this.TextBoxAutoTileGraphicId4, "Fourth autotile asset id");
            this.TextBoxAutoTileGraphicId4.Visible = false;
            this.TextBoxAutoTileGraphicId4.Click += new System.EventHandler(this.TextBoxAutoTileGraphicId_Click);
            // 
            // TextBoxAutoTileGraphicId5
            // 
            this.TextBoxAutoTileGraphicId5.Location = new System.Drawing.Point(83, 240);
            this.TextBoxAutoTileGraphicId5.Name = "TextBoxAutoTileGraphicId5";
            this.TextBoxAutoTileGraphicId5.ReadOnly = true;
            this.TextBoxAutoTileGraphicId5.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTileGraphicId5.TabIndex = 17;
            this.ToolTip1.SetToolTip(this.TextBoxAutoTileGraphicId5, "Fifth autotile asset id");
            this.TextBoxAutoTileGraphicId5.Visible = false;
            this.TextBoxAutoTileGraphicId5.Click += new System.EventHandler(this.TextBoxAutoTileGraphicId_Click);
            // 
            // TextBoxAutoTileGraphicId6
            // 
            this.TextBoxAutoTileGraphicId6.Location = new System.Drawing.Point(83, 266);
            this.TextBoxAutoTileGraphicId6.Name = "TextBoxAutoTileGraphicId6";
            this.TextBoxAutoTileGraphicId6.ReadOnly = true;
            this.TextBoxAutoTileGraphicId6.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTileGraphicId6.TabIndex = 19;
            this.ToolTip1.SetToolTip(this.TextBoxAutoTileGraphicId6, "Sixt autotile asset id");
            this.TextBoxAutoTileGraphicId6.Visible = false;
            this.TextBoxAutoTileGraphicId6.Click += new System.EventHandler(this.TextBoxAutoTileGraphicId_Click);
            // 
            // TextBoxAutoTileGraphicId7
            // 
            this.TextBoxAutoTileGraphicId7.Location = new System.Drawing.Point(83, 292);
            this.TextBoxAutoTileGraphicId7.Name = "TextBoxAutoTileGraphicId7";
            this.TextBoxAutoTileGraphicId7.ReadOnly = true;
            this.TextBoxAutoTileGraphicId7.Size = new System.Drawing.Size(189, 20);
            this.TextBoxAutoTileGraphicId7.TabIndex = 21;
            this.ToolTip1.SetToolTip(this.TextBoxAutoTileGraphicId7, "Seventh autotile asset id");
            this.TextBoxAutoTileGraphicId7.Visible = false;
            this.TextBoxAutoTileGraphicId7.Click += new System.EventHandler(this.TextBoxAutoTileGraphicId_Click);
            // 
            // Panel
            // 
            this.Panel.AutoScroll = true;
            this.Panel.Controls.Add(this.TilesetControl);
            this.Panel.Location = new System.Drawing.Point(278, 6);
            this.Panel.Name = "Panel";
            this.Panel.Size = new System.Drawing.Size(273, 393);
            this.Panel.TabIndex = 53;
            // 
            // TilesetControl
            // 
            this.TilesetControl.AutotileTextureNames = null;
            this.TilesetControl.AutotileTextures = null;
            this.TilesetControl.Location = new System.Drawing.Point(0, 0);
            this.TilesetControl.MinimumSize = new System.Drawing.Size(256, 362);
            this.TilesetControl.Name = "TilesetControl";
            this.TilesetControl.Size = new System.Drawing.Size(273, 393);
            this.TilesetControl.TabIndex = 9;
            this.TilesetControl.Text = "TilesetControl";
            this.TilesetControl.TilesetTexture = null;
            this.TilesetControl.TilesetTextureName = null;
            this.TilesetControl.Click += new System.EventHandler(this.TilesetControl_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.Image = global::ContentConverter.Properties.Resources.database_save;
            this.ButtonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonSave.Location = new System.Drawing.Point(81, 375);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(191, 24);
            this.ButtonSave.TabIndex = 37;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonSave, "Save this tileset to the database");
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // ButtonLink
            // 
            this.ButtonLink.Enabled = false;
            this.ButtonLink.Image = global::ContentConverter.Properties.Resources.link;
            this.ButtonLink.Location = new System.Drawing.Point(15, 375);
            this.ButtonLink.Name = "ButtonLink";
            this.ButtonLink.Size = new System.Drawing.Size(60, 24);
            this.ButtonLink.TabIndex = 38;
            this.ButtonLink.Text = "Link";
            this.ButtonLink.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonLink, "Uses this tileset in the parent window. Auto saves!");
            this.ButtonLink.UseVisualStyleBackColor = true;
            this.ButtonLink.Click += new System.EventHandler(this.ButtonLink_Click);
            // 
            // ButtonEditId
            // 
            this.ButtonEditId.Image = global::ContentConverter.Properties.Resources.pencil;
            this.ButtonEditId.Location = new System.Drawing.Point(245, 3);
            this.ButtonEditId.Name = "ButtonEditId";
            this.ButtonEditId.Size = new System.Drawing.Size(27, 24);
            this.ButtonEditId.TabIndex = 2;
            this.ToolTip1.SetToolTip(this.ButtonEditId, "Open the ObjectId Editor for the Tileset ID");
            this.ButtonEditId.UseVisualStyleBackColor = true;
            this.ButtonEditId.Click += new System.EventHandler(this.ButtonEditId_Click);
            // 
            // ButtonHealing
            // 
            this.ButtonHealing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonHealing.Image = global::ContentConverter.Properties.Resources.shading;
            this.ButtonHealing.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonHealing.Location = new System.Drawing.Point(556, 315);
            this.ButtonHealing.Name = "ButtonHealing";
            this.ButtonHealing.Size = new System.Drawing.Size(79, 24);
            this.ButtonHealing.TabIndex = 35;
            this.ButtonHealing.Text = "Healing";
            this.ButtonHealing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonHealing.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonHealing, "Edit the healing flag for this tileset. When a tile is healing, battlers will reg" +
        "ain stats quicker");
            this.ButtonHealing.UseVisualStyleBackColor = true;
            this.ButtonHealing.Click += new System.EventHandler(this.ButtonHealing_Click);
            // 
            // ButtonHazard
            // 
            this.ButtonHazard.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonHazard.Image = global::ContentConverter.Properties.Resources.shading;
            this.ButtonHazard.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonHazard.Location = new System.Drawing.Point(557, 285);
            this.ButtonHazard.Name = "ButtonHazard";
            this.ButtonHazard.Size = new System.Drawing.Size(79, 24);
            this.ButtonHazard.TabIndex = 34;
            this.ButtonHazard.Text = "Hazard";
            this.ButtonHazard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonHazard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonHazard, "Edit the hazard flag for this tileset. Hazard tiles hurt Battlers when they are w" +
        "alked upon");
            this.ButtonHazard.UseVisualStyleBackColor = true;
            this.ButtonHazard.Click += new System.EventHandler(this.ButtonHazard_Click);
            // 
            // ButtonPoison
            // 
            this.ButtonPoison.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonPoison.Image = global::ContentConverter.Properties.Resources.shading;
            this.ButtonPoison.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonPoison.Location = new System.Drawing.Point(557, 255);
            this.ButtonPoison.Name = "ButtonPoison";
            this.ButtonPoison.Size = new System.Drawing.Size(79, 24);
            this.ButtonPoison.TabIndex = 32;
            this.ButtonPoison.Text = "Poison";
            this.ButtonPoison.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonPoison.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonPoison, "Edit the poison flag for this tileset. Poison tiles can inflict the Poisoned stat" +
        "e on Battlers");
            this.ButtonPoison.UseVisualStyleBackColor = true;
            this.ButtonPoison.Click += new System.EventHandler(this.ButtonPoison_Click);
            // 
            // ButtonInheritPassages
            // 
            this.ButtonInheritPassages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonInheritPassages.Image = global::ContentConverter.Properties.Resources.shading;
            this.ButtonInheritPassages.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonInheritPassages.Location = new System.Drawing.Point(557, 200);
            this.ButtonInheritPassages.Name = "ButtonInheritPassages";
            this.ButtonInheritPassages.Size = new System.Drawing.Size(79, 34);
            this.ButtonInheritPassages.TabIndex = 31;
            this.ButtonInheritPassages.Text = "Inherit Passages";
            this.ButtonInheritPassages.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonInheritPassages.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonInheritPassages, "Edit the transparant passability flag for this tileset. When a tile is transparan" +
        "t in passability, passages of the tile will be copied from the tile on which it " +
        "is stacked");
            this.ButtonInheritPassages.UseVisualStyleBackColor = true;
            this.ButtonInheritPassages.Click += new System.EventHandler(this.ButtonInheritPassages_Click);
            // 
            // ButtonBlocking
            // 
            this.ButtonBlocking.Image = global::ContentConverter.Properties.Resources.shading;
            this.ButtonBlocking.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBlocking.Location = new System.Drawing.Point(557, 156);
            this.ButtonBlocking.Name = "ButtonBlocking";
            this.ButtonBlocking.Size = new System.Drawing.Size(79, 24);
            this.ButtonBlocking.TabIndex = 30;
            this.ButtonBlocking.Text = "Blocking";
            this.ButtonBlocking.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBlocking.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonBlocking, "Edit the blocking flag for this tileset. A blocking tile blocks an interactable e" +
        "ven if a non-blocking tile is stacked on it");
            this.ButtonBlocking.UseVisualStyleBackColor = true;
            this.ButtonBlocking.Click += new System.EventHandler(this.ButtonBlocking_Click);
            // 
            // ButtonCounter
            // 
            this.ButtonCounter.Image = global::ContentConverter.Properties.Resources.shading;
            this.ButtonCounter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonCounter.Location = new System.Drawing.Point(557, 126);
            this.ButtonCounter.Name = "ButtonCounter";
            this.ButtonCounter.Size = new System.Drawing.Size(79, 24);
            this.ButtonCounter.TabIndex = 29;
            this.ButtonCounter.Text = "Counter";
            this.ButtonCounter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonCounter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonCounter, "Edit counter flag for this tileset. A counter tile will increase the action radiu" +
        "s by that tile");
            this.ButtonCounter.UseVisualStyleBackColor = true;
            this.ButtonCounter.Click += new System.EventHandler(this.ButtonCounter_Click);
            // 
            // ButtonBush
            // 
            this.ButtonBush.Image = global::ContentConverter.Properties.Resources.shading;
            this.ButtonBush.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBush.Location = new System.Drawing.Point(557, 96);
            this.ButtonBush.Name = "ButtonBush";
            this.ButtonBush.Size = new System.Drawing.Size(79, 24);
            this.ButtonBush.TabIndex = 28;
            this.ButtonBush.Text = "Bush";
            this.ButtonBush.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonBush.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonBush, "Edit bush flag for this tileset. A bush tile will partially obscure Interactables" +
        "");
            this.ButtonBush.UseVisualStyleBackColor = true;
            this.ButtonBush.Click += new System.EventHandler(this.ButtonBush_Click);
            // 
            // ButtonTerrain
            // 
            this.ButtonTerrain.Enabled = false;
            this.ButtonTerrain.Image = global::ContentConverter.Properties.Resources.map;
            this.ButtonTerrain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonTerrain.Location = new System.Drawing.Point(557, 375);
            this.ButtonTerrain.Name = "ButtonTerrain";
            this.ButtonTerrain.Size = new System.Drawing.Size(79, 24);
            this.ButtonTerrain.TabIndex = 36;
            this.ButtonTerrain.Text = "Terrain";
            this.ButtonTerrain.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonTerrain.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonTerrain, "Edit the terrain tags for this tileset. Terrain tags denote different type of ter" +
        "rain");
            this.ButtonTerrain.UseVisualStyleBackColor = true;
            this.ButtonTerrain.Click += new System.EventHandler(this.ButtonTerrain_Click);
            // 
            // ButtonLayers
            // 
            this.ButtonLayers.Image = global::ContentConverter.Properties.Resources.layers;
            this.ButtonLayers.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonLayers.Location = new System.Drawing.Point(557, 36);
            this.ButtonLayers.Name = "ButtonLayers";
            this.ButtonLayers.Size = new System.Drawing.Size(79, 24);
            this.ButtonLayers.TabIndex = 27;
            this.ButtonLayers.Text = "Layers";
            this.ButtonLayers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonLayers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonLayers, "Edit layers for this tileset");
            this.ButtonLayers.UseVisualStyleBackColor = true;
            this.ButtonLayers.Click += new System.EventHandler(this.ButtonLayers_Click);
            // 
            // ButtonPassages
            // 
            this.ButtonPassages.Image = global::ContentConverter.Properties.Resources.arrow_inout;
            this.ButtonPassages.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonPassages.Location = new System.Drawing.Point(557, 6);
            this.ButtonPassages.Name = "ButtonPassages";
            this.ButtonPassages.Size = new System.Drawing.Size(79, 24);
            this.ButtonPassages.TabIndex = 26;
            this.ButtonPassages.Text = "Passages";
            this.ButtonPassages.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonPassages.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.ButtonPassages, "Edit passability for this tileset");
            this.ButtonPassages.UseVisualStyleBackColor = true;
            this.ButtonPassages.Click += new System.EventHandler(this.ButtonPassages_Click);
            // 
            // ButtonUpdateAutotiles
            // 
            this.ButtonUpdateAutotiles.Image = global::ContentConverter.Properties.Resources.table_lightning;
            this.ButtonUpdateAutotiles.Location = new System.Drawing.Point(212, 318);
            this.ButtonUpdateAutotiles.Name = "ButtonUpdateAutotiles";
            this.ButtonUpdateAutotiles.Size = new System.Drawing.Size(27, 24);
            this.ButtonUpdateAutotiles.TabIndex = 24;
            this.ToolTip1.SetToolTip(this.ButtonUpdateAutotiles, "Update autotile asset names. All aliases will be replaced by their real name coun" +
        "terparts");
            this.ButtonUpdateAutotiles.UseVisualStyleBackColor = true;
            this.ButtonUpdateAutotiles.Click += new System.EventHandler(this.ButtonUpdateAutotiles_Click);
            // 
            // BrowseButton
            // 
            this.BrowseButton.Image = global::ContentConverter.Properties.Resources.database_link;
            this.BrowseButton.Location = new System.Drawing.Point(127, 318);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(79, 24);
            this.BrowseButton.TabIndex = 23;
            this.BrowseButton.Text = "Browse...";
            this.BrowseButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.BrowseButton, "Browse for an autotile asset");
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // BrowseButtonTS
            // 
            this.BrowseButtonTS.Image = global::ContentConverter.Properties.Resources.database_link;
            this.BrowseButtonTS.Location = new System.Drawing.Point(160, 84);
            this.BrowseButtonTS.Name = "BrowseButtonTS";
            this.BrowseButtonTS.Size = new System.Drawing.Size(79, 24);
            this.BrowseButtonTS.TabIndex = 6;
            this.BrowseButtonTS.Text = "Browse...";
            this.BrowseButtonTS.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ToolTip1.SetToolTip(this.BrowseButtonTS, "Browse for a tileset asset");
            this.BrowseButtonTS.UseVisualStyleBackColor = true;
            this.BrowseButtonTS.Click += new System.EventHandler(this.BrowseButtonTS_Click);
            // 
            // ButtonAutoTilesGraphicLookup
            // 
            this.ButtonAutoTilesGraphicLookup.Image = global::ContentConverter.Properties.Resources.information;
            this.ButtonAutoTilesGraphicLookup.Location = new System.Drawing.Point(245, 318);
            this.ButtonAutoTilesGraphicLookup.Name = "ButtonAutoTilesGraphicLookup";
            this.ButtonAutoTilesGraphicLookup.Size = new System.Drawing.Size(27, 24);
            this.ButtonAutoTilesGraphicLookup.TabIndex = 25;
            this.ToolTip1.SetToolTip(this.ButtonAutoTilesGraphicLookup, "Show/Hide the autotile asset id\'s");
            this.ButtonAutoTilesGraphicLookup.UseVisualStyleBackColor = true;
            this.ButtonAutoTilesGraphicLookup.Click += new System.EventHandler(this.ButtonAutoTilesGraphicLookup_Click);
            // 
            // ButtonTilesetGraphicLookup
            // 
            this.ButtonTilesetGraphicLookup.Image = global::ContentConverter.Properties.Resources.information;
            this.ButtonTilesetGraphicLookup.Location = new System.Drawing.Point(245, 84);
            this.ButtonTilesetGraphicLookup.Name = "ButtonTilesetGraphicLookup";
            this.ButtonTilesetGraphicLookup.Size = new System.Drawing.Size(27, 24);
            this.ButtonTilesetGraphicLookup.TabIndex = 7;
            this.ToolTip1.SetToolTip(this.ButtonTilesetGraphicLookup, "Show/Hide the tileset graphic asset id");
            this.ButtonTilesetGraphicLookup.UseVisualStyleBackColor = true;
            this.ButtonTilesetGraphicLookup.Click += new System.EventHandler(this.ButtonTilesetGraphicLookup_Click);
            // 
            // TilesCount
            // 
            this.TilesCount.Location = new System.Drawing.Point(162, 349);
            this.TilesCount.Maximum = new decimal(new int[] {
            262144,
            0,
            0,
            0});
            this.TilesCount.Name = "TilesCount";
            this.TilesCount.Size = new System.Drawing.Size(77, 20);
            this.TilesCount.TabIndex = 54;
            this.TilesCount.ThousandsSeparator = true;
            this.TilesCount.ValueChanged += new System.EventHandler(this.TilesCount_ValueChanged);
            // 
            // LabelTilesCount
            // 
            this.LabelTilesCount.AutoSize = true;
            this.LabelTilesCount.Location = new System.Drawing.Point(117, 351);
            this.LabelTilesCount.Name = "LabelTilesCount";
            this.LabelTilesCount.Size = new System.Drawing.Size(39, 13);
            this.LabelTilesCount.TabIndex = 55;
            this.LabelTilesCount.Text = "# Tiles";
            // 
            // ButtonUpdateNumTiles
            // 
            this.ButtonUpdateNumTiles.Image = global::ContentConverter.Properties.Resources.table_lightning;
            this.ButtonUpdateNumTiles.Location = new System.Drawing.Point(245, 345);
            this.ButtonUpdateNumTiles.Name = "ButtonUpdateNumTiles";
            this.ButtonUpdateNumTiles.Size = new System.Drawing.Size(27, 24);
            this.ButtonUpdateNumTiles.TabIndex = 56;
            this.ToolTip1.SetToolTip(this.ButtonUpdateNumTiles, "Update autotile asset names. All aliases will be replaced by their real name coun" +
        "terparts");
            this.ButtonUpdateNumTiles.UseVisualStyleBackColor = true;
            this.ButtonUpdateNumTiles.Click += new System.EventHandler(this.ButtonUpdateNumTiles_Click);
            // 
            // TilesetEditor
            // 
            this.AcceptButton = this.ButtonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 411);
            this.Controls.Add(this.ButtonUpdateNumTiles);
            this.Controls.Add(this.LabelTilesCount);
            this.Controls.Add(this.TilesCount);
            this.Controls.Add(this.ButtonLink);
            this.Controls.Add(this.ButtonEditId);
            this.Controls.Add(this.ButtonHealing);
            this.Controls.Add(this.ButtonHazard);
            this.Controls.Add(this.ButtonPoison);
            this.Controls.Add(this.ButtonInheritPassages);
            this.Controls.Add(this.ButtonBlocking);
            this.Controls.Add(this.ButtonCounter);
            this.Controls.Add(this.ButtonBush);
            this.Controls.Add(this.ButtonTerrain);
            this.Controls.Add(this.ButtonLayers);
            this.Controls.Add(this.ButtonPassages);
            this.Controls.Add(this.ButtonUpdateAutotiles);
            this.Controls.Add(this.Panel);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.BrowseButtonTS);
            this.Controls.Add(this.TextBoxAutoTileGraphicId7);
            this.Controls.Add(this.TextBoxAutoTileGraphicId6);
            this.Controls.Add(this.TextBoxAutoTileGraphicId5);
            this.Controls.Add(this.TextBoxAutoTileGraphicId4);
            this.Controls.Add(this.TextBoxAutoTileGraphicId3);
            this.Controls.Add(this.TextBoxAutoTileGraphicId2);
            this.Controls.Add(this.TextBoxAutoTileGraphicId1);
            this.Controls.Add(this.ButtonAutoTilesGraphicLookup);
            this.Controls.Add(this.TextBoxGraphicId);
            this.Controls.Add(this.ButtonTilesetGraphicLookup);
            this.Controls.Add(this.TextBoxAutoTile7);
            this.Controls.Add(this.TextBoxAutoTile6);
            this.Controls.Add(this.TextBoxAutoTile3);
            this.Controls.Add(this.TextBoxAutoTile4);
            this.Controls.Add(this.TextBoxAutoTile5);
            this.Controls.Add(this.TextBoxAutoTile2);
            this.Controls.Add(this.TextBoxAutoTile1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.LabelAssetName);
            this.Controls.Add(this.TextBoxGraphic);
            this.Controls.Add(this.TextBoxName);
            this.Controls.Add(this.LabelName);
            this.Controls.Add(this.TextBoxTilesetId);
            this.Controls.Add(this.LabelTilesetId);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "TilesetEditor";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Tileset Editor";
            this.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TilesCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxName;
        private System.Windows.Forms.Label LabelName;
        private System.Windows.Forms.TextBox TextBoxTilesetId;
        private System.Windows.Forms.Label LabelTilesetId;
        private System.Windows.Forms.TextBox TextBoxGraphic;
        private System.Windows.Forms.Label LabelAssetName;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TextBoxAutoTile1;
        private System.Windows.Forms.TextBox TextBoxAutoTile2;
        private System.Windows.Forms.TextBox TextBoxAutoTile5;
        private System.Windows.Forms.TextBox TextBoxAutoTile4;
        private System.Windows.Forms.TextBox TextBoxAutoTile6;
        private System.Windows.Forms.TextBox TextBoxAutoTile7;
        private System.Windows.Forms.Button ButtonTilesetGraphicLookup;
        private System.Windows.Forms.TextBox TextBoxGraphicId;
        private System.Windows.Forms.Button ButtonAutoTilesGraphicLookup;
        private System.Windows.Forms.TextBox TextBoxAutoTile3;
        private System.Windows.Forms.TextBox TextBoxAutoTileGraphicId1;
        private System.Windows.Forms.TextBox TextBoxAutoTileGraphicId2;
        private System.Windows.Forms.TextBox TextBoxAutoTileGraphicId3;
        private System.Windows.Forms.TextBox TextBoxAutoTileGraphicId4;
        private System.Windows.Forms.TextBox TextBoxAutoTileGraphicId5;
        private System.Windows.Forms.TextBox TextBoxAutoTileGraphicId6;
        private System.Windows.Forms.TextBox TextBoxAutoTileGraphicId7;
        private System.Windows.Forms.Button BrowseButtonTS;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Panel Panel;
        private System.Windows.Forms.Button ButtonUpdateAutotiles;
        private System.Windows.Forms.Button ButtonPassages;
        private System.Windows.Forms.Button ButtonLayers;
        private System.Windows.Forms.Button ButtonTerrain;
        private System.Windows.Forms.Button ButtonBush;
        private System.Windows.Forms.Button ButtonCounter;
        private System.Windows.Forms.Button ButtonBlocking;
        private System.Windows.Forms.Button ButtonInheritPassages;
        private System.Windows.Forms.Button ButtonPoison;
        private System.Windows.Forms.Button ButtonHazard;
        private System.Windows.Forms.Button ButtonHealing;
        private System.Windows.Forms.Button ButtonEditId;
        private System.Windows.Forms.ToolTip ToolTip1;
        private System.Windows.Forms.Button ButtonLink;
        private TilesetControl TilesetControl;
        private System.Windows.Forms.NumericUpDown TilesCount;
        private System.Windows.Forms.Label LabelTilesCount;
        private System.Windows.Forms.Button ButtonUpdateNumTiles;
    }
}