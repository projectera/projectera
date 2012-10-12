using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MongoDB.Bson;
using ERAUtils.Enum;
using MongoDB.Driver.Builders;
using System.Drawing.Imaging;
using System.IO;
using ContentConverter.Data;

namespace ContentConverter
{
    public partial class TilesetEditor : Form
    {
        /// <summary>
        /// 
        /// </summary>
        internal ERAServer.Data.Tileset Tileset
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private AssetBrowser Browser
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private Object PasteBin
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean LinkingEnabled
        {
            get { return this.ButtonLink.Enabled; }
            set { this.ButtonLink.Enabled = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TilesetEditor()
        {
            InitializeComponent();

            this.FormClosing += new FormClosingEventHandler(TilesetEditor_FormClosing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TilesetEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            TilesetControl.UnloadTexture();
            TilesetControl.UnloadContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            ObjectId idTileset = ObjectId.Empty;
            if (!(ObjectId.TryParse(this.TextBoxTilesetId.Text, out idTileset)))
            {
                // error
                throw new FormatException("ObjectId not valid");
            }

            List<String> autoTileNames = new List<String>();

            if (!String.IsNullOrWhiteSpace(this.TextBoxAutoTile1.Text))
                autoTileNames.Add(this.TextBoxAutoTile1.Text);
            if (!String.IsNullOrWhiteSpace(this.TextBoxAutoTile2.Text))
                autoTileNames.Add(this.TextBoxAutoTile2.Text);
            if (!String.IsNullOrWhiteSpace(this.TextBoxAutoTile3.Text))
                autoTileNames.Add(this.TextBoxAutoTile3.Text);
            if (!String.IsNullOrWhiteSpace(this.TextBoxAutoTile4.Text))
                autoTileNames.Add(this.TextBoxAutoTile4.Text);
            if (!String.IsNullOrWhiteSpace(this.TextBoxAutoTile5.Text))
                autoTileNames.Add(this.TextBoxAutoTile5.Text);
            if (!String.IsNullOrWhiteSpace(this.TextBoxAutoTile6.Text))
                autoTileNames.Add(this.TextBoxAutoTile6.Text);
            if (!String.IsNullOrWhiteSpace(this.TextBoxAutoTile7.Text))
                autoTileNames.Add(this.TextBoxAutoTile7.Text);

            this.Tileset = ERAServer.Data.Tileset.Generate(idTileset, this.TextBoxName.Text, this.TextBoxGraphic.Text,
                autoTileNames, null, Tileset.Passages, Tileset.Priorities, Tileset.Flags, Tileset.Tags, Convert.ToInt32(this.TilesCount.Value), Tileset.Version + 1);

            this.Tileset.Put();

            if (!this.LinkingEnabled)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLink_Click(object sender, EventArgs e)
        {
            // First save
            ButtonSave_Click(sender, e);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            this.Close();
        }

        /// <summary>
        /// Generate 
        /// </summary>
        /// <param name="mapdata"></param>
        internal void Generate(Int32 id, String name, String assetName, List<String> autotileNames,
            Byte[] passages, Byte[] priorities, Byte[] flags, Byte[] tags, Int32 tiles)
        {
            this.Tileset = ContentConverter.Data.Tileset.Generate(id, name, assetName, autotileNames, passages, priorities, flags, tags, tiles);

            RefreshDisplay();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        internal void LoadFrom(ObjectId id)
        {
            this.Tileset = ERAServer.Data.Tileset.GetCollection().FindOneById(id);

            RefreshDisplay();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        internal void LoadFrom(String name)
        {
            this.Tileset = ERAServer.Data.Tileset.GetCollection().FindOne(Query.EQ("Name", name));

            RefreshDisplay();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileset"></param>
        internal void LoadFrom(ERAServer.Data.Tileset tileset)
        {
            this.Tileset = tileset;

            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes display, loads TilesetData data
        /// </summary>
        internal void RefreshDisplay()
        {
            this.TextBoxName.Text = Tileset.Name;
            this.TextBoxTilesetId.Text = Tileset.Id.ToString();
            this.TextBoxGraphic.Text = Tileset.AssetName;
            this.TilesCount.Value = Tileset.Tiles;
            
            TextBox[] fields = new TextBox[] { this.TextBoxAutoTile1, this.TextBoxAutoTile2, 
                this.TextBoxAutoTile3, this.TextBoxAutoTile4, this.TextBoxAutoTile5, 
                this.TextBoxAutoTile6, this.TextBoxAutoTile7 };
            TextBox[] boxes = new TextBox[] { TextBoxAutoTileGraphicId1, TextBoxAutoTileGraphicId2, TextBoxAutoTileGraphicId3, 
                TextBoxAutoTileGraphicId4, TextBoxAutoTileGraphicId5, TextBoxAutoTileGraphicId6, TextBoxAutoTileGraphicId7 };

            for (Int32 i = 0; i < 7; i++)
            {
                fields[i].Text = String.Empty;
            }
            TextBoxGraphicId_Click(this, EventArgs.Empty);
            TextBoxAutoTileGraphicId_Click(this, EventArgs.Empty);

            for (Int32 i = 0; i < Math.Min(Tileset.AutotileAssetNames.Count, 7); i++)
            {
                fields[i].Text = Tileset.AutotileAssetNames[i];

                Asset autotileGraphic = Asset.GetFile(AssetType.Autotile, fields[i].Text);
                if (autotileGraphic != null)
                {
                    autotileGraphic.Download("Temp/" + autotileGraphic.Id + " - " + fields[i].Text + ".png");
                    this.TilesetControl.LoadTexture("Temp/" + autotileGraphic.Id + " - " + fields[i].Text + ".png", i);
                }
                else
                {
                    this.TilesetControl.LoadTexture(null, i);
                }
            }

            Asset tilesetGraphic = Asset.GetFile(AssetType.Tileset, TextBoxGraphic.Text);
            if (tilesetGraphic != null)
            {
                TextBoxGraphicId.Text = tilesetGraphic.Id.ToString();
                tilesetGraphic.Download("Temp/" + TextBoxGraphicId.Text + " - " + TextBoxGraphic.Text + ".png");

                UpdateDisplayGraphic("Temp/" + TextBoxGraphicId.Text + " - " + TextBoxGraphic.Text + ".png");
            }
            else
            {
                this.TilesetControl.LoadTexture(null);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTilesetGraphicLookup_Click(object sender, EventArgs e)
        {
            if (TextBoxGraphicId.Visible)
            {
                TextBoxGraphicId_Click(sender, e);
                return;
            }

            Asset tilesetGraphic = Asset.GetFile(AssetType.Tileset, TextBoxGraphic.Text);
            if (tilesetGraphic != null)
            {
                TextBoxGraphicId.Text = tilesetGraphic.Id.ToString();
                tilesetGraphic.Download("Temp/" + TextBoxGraphicId.Text + " - " + TextBoxGraphic.Text + ".png");

                UpdateDisplayGraphic("Temp/" + TextBoxGraphicId.Text + " - " + TextBoxGraphic.Text + ".png");
            }

            TextBoxGraphicId.Visible = true;
            TextBoxGraphicId.Click += new EventHandler(TextBoxGraphicId_Click);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBoxGraphicId_Click(object sender, EventArgs e)
        {
            TextBoxGraphicId.Visible = false;
            TextBoxGraphicId.Click -= TextBoxGraphicId_Click;
            TextBoxGraphicId.Text = String.Empty;

            //this.PictureBox.Visible = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAutoTilesGraphicLookup_Click(object sender, EventArgs e)
        {
            TextBox[] boxes = new TextBox[] { TextBoxAutoTileGraphicId1, TextBoxAutoTileGraphicId2, TextBoxAutoTileGraphicId3, 
                TextBoxAutoTileGraphicId4, TextBoxAutoTileGraphicId5, TextBoxAutoTileGraphicId6, TextBoxAutoTileGraphicId7 };
            TextBox[] fields = new TextBox[] { TextBoxAutoTile1, TextBoxAutoTile2, TextBoxAutoTile3, TextBoxAutoTile4,
                TextBoxAutoTile5, TextBoxAutoTile6, TextBoxAutoTile7 };

            foreach (TextBox box in boxes)
            {
                if (box.Visible)
                {
                    TextBoxAutoTileGraphicId_Click(sender, e);
                    return;
                }
            }

            for (int i = 0; i < boxes.Length; i++)
            {
                Asset autotileGraphic = Asset.GetFile(AssetType.Autotile, fields[i].Text);
                if (autotileGraphic != null)
                {
                    boxes[i].Text = autotileGraphic.Id.ToString();
                    autotileGraphic.Download("Temp/" + boxes[i].Text + " - " + fields[i].Text + ".png");
                    TilesetControl.LoadTexture("Temp/" + boxes[i].Text + " - " + fields[i].Text + ".png", i);
                }
                else
                {
                    TilesetControl.LoadTexture(null, i);
                }

                boxes[i].Visible = true;
                boxes[i].Click += new EventHandler(TextBoxAutoTileGraphicId_Click);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxAutoTileGraphicId_Click(object sender, EventArgs e)
        {
            TextBox[] boxes = new TextBox[] { TextBoxAutoTileGraphicId1, TextBoxAutoTileGraphicId2, TextBoxAutoTileGraphicId3, 
                TextBoxAutoTileGraphicId4, TextBoxAutoTileGraphicId5, TextBoxAutoTileGraphicId6, TextBoxAutoTileGraphicId7  };

            foreach (TextBox box in boxes)
            {
                box.Visible = false;
                box.Click -= TextBoxAutoTileGraphicId_Click;
                box.Text = String.Empty;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxAutoTileGraphicId_Selected_Click(object sender, EventArgs e)
        {
            TextBox[] boxes = new TextBox[] { TextBoxAutoTileGraphicId1, TextBoxAutoTileGraphicId2, TextBoxAutoTileGraphicId3, 
                TextBoxAutoTileGraphicId4, TextBoxAutoTileGraphicId5, TextBoxAutoTileGraphicId6, TextBoxAutoTileGraphicId7  };
            TextBox[] fields = new TextBox[] { TextBoxAutoTile1, TextBoxAutoTile2, TextBoxAutoTile3, TextBoxAutoTile4,
                TextBoxAutoTile5, TextBoxAutoTile6, TextBoxAutoTile7 };

            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].Visible = false;
                boxes[i].Click -= TextBoxAutoTileGraphicId_Selected_Click;
                boxes[i].Text = String.Empty;

                if (boxes[i] == sender)
                {
                    fields[i].Text = (String)this.PasteBin;
                    this.PasteBin = null;
                    Asset autotileGraphic = Asset.GetFile(AssetType.Autotile, fields[i].Text);
                    if (autotileGraphic != null)
                    {
                        autotileGraphic.Download("Temp/" + autotileGraphic.Id + " - " + fields[i].Text + ".png");
                        TilesetControl.LoadTexture("Temp/" + autotileGraphic.Id + " - " + fields[i].Text + ".png", 1);
                    }
                    else
                    {
                        TilesetControl.LoadTexture(null, i);
                    }
                }
            }       
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButtonTS_Click(object sender, EventArgs e)
        {
            // Exit query fields
            if (TextBoxGraphicId.Visible)
            {
                TextBoxGraphicId_Click(sender, e);
            }

            TextBox[] boxes = new TextBox[] { TextBoxAutoTileGraphicId1, TextBoxAutoTileGraphicId2, TextBoxAutoTileGraphicId3, 
                TextBoxAutoTileGraphicId4, TextBoxAutoTileGraphicId5, TextBoxAutoTileGraphicId6, TextBoxAutoTileGraphicId7 };

            foreach (TextBox box in boxes)
            {
                if (box.Visible)
                {
                    if (this.PasteBin == null)
                    {
                        TextBoxAutoTileGraphicId_Click(sender, e);
                    }
                    else
                    {
                        TextBoxAutoTileGraphicId_Selected_Click(null, e);
                    }
                    break;
                }
            }

            // Open browser
            this.Browser = new AssetBrowser() { AssetType = AssetType.Tileset };
            this.Browser.FormClosed += new FormClosedEventHandler(AssetBrowser_FormClosed);
            this.Browser.ButtonSearch_Click(null, EventArgs.Empty);
            this.Browser.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            // Exit query fields
            if (TextBoxGraphicId.Visible)
            {
                TextBoxGraphicId_Click(sender, e);
            }

            TextBox[] boxes = new TextBox[] { TextBoxAutoTileGraphicId1, TextBoxAutoTileGraphicId2, TextBoxAutoTileGraphicId3, 
                TextBoxAutoTileGraphicId4, TextBoxAutoTileGraphicId5, TextBoxAutoTileGraphicId6, TextBoxAutoTileGraphicId7 };
            
            foreach (TextBox box in boxes)
            {
                if (box.Visible)
                {
                    if (this.PasteBin == null)
                    {
                        TextBoxAutoTileGraphicId_Click(sender, e);
                    }
                    else
                    {
                        TextBoxAutoTileGraphicId_Selected_Click(null, e);
                    }
                    break;
                }
            }
            
            // Open browser
            this.Browser = new AssetBrowser() { AssetType = AssetType.Autotile };
            this.Browser.FormClosed += new FormClosedEventHandler(AssetBrowser_FormClosed);
            this.Browser.ButtonSearch_Click(null, EventArgs.Empty);
            this.Browser.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssetBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            String name = this.Browser.SelectedName;
            AssetType type = this.Browser.SelectedType;
            this.Browser = null;

            if (String.IsNullOrEmpty(name))
                return;

            switch (type)
            {
                case AssetType.Tileset:
                    this.TextBoxGraphic.Text = name;
                    Asset tilesetGraphic = Asset.GetFile(AssetType.Tileset, TextBoxGraphic.Text);
                    if (tilesetGraphic != null)
                    {
                        tilesetGraphic.Download("Temp/" + TextBoxGraphicId.Text + " - " + TextBoxGraphic.Text + ".png");
                        UpdateDisplayGraphic("Temp/" + TextBoxGraphicId.Text + " - " + TextBoxGraphic.Text + ".png");
                    }
                    break;
                case AssetType.Autotile:

                    this.PasteBin = name;

                    TextBox[] boxes = new TextBox[] { TextBoxAutoTileGraphicId1, TextBoxAutoTileGraphicId2, TextBoxAutoTileGraphicId3, 
                        TextBoxAutoTileGraphicId4, TextBoxAutoTileGraphicId5, TextBoxAutoTileGraphicId6, TextBoxAutoTileGraphicId7  };
                    TextBox[] fields = new TextBox[] { TextBoxAutoTile1, TextBoxAutoTile2, TextBoxAutoTile3, TextBoxAutoTile4,
                        TextBoxAutoTile5, TextBoxAutoTile6, TextBoxAutoTile7 };

                    for (int i = 0; i < boxes.Length; i++)
                    {
                        boxes[i].Text = "Replace <" + fields[i].Text + ">";
                        boxes[i].Visible = true;
                        boxes[i].Click += new EventHandler(TextBoxAutoTileGraphicId_Selected_Click);
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxGraphic_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateDisplayGraphic(String location)
        {
            this.TilesetControl.Visible = false;
            this.TilesetControl.LoadTexture(location);
            this.TilesetControl.Visible = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonUpdateAutotiles_Click(object sender, EventArgs e)
        {
            TextBox[] fields = new TextBox[] { TextBoxAutoTile1, TextBoxAutoTile2, TextBoxAutoTile3, TextBoxAutoTile4,
                TextBoxAutoTile5, TextBoxAutoTile6, TextBoxAutoTile7 };

            for (int i = 0; i < fields.Length; i++)
            {
                Asset autotileGraphic = Asset.GetFile(AssetType.Autotile, fields[i].Text);
                if (autotileGraphic != null)
                {
                    fields[i].Text = autotileGraphic.RemoteFileName;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPassages_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            this.TilesetControl.DoAction(TilesetControl.Action.Passages);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLayers_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            this.TilesetControl.DoAction(TilesetControl.Action.Priorities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTerrain_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            this.TilesetControl.DoAction(TilesetControl.Action.Terrain);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEditId_Click(object sender, EventArgs e)
        {
            ObjectId value = ObjectId.Empty;
            if (!ObjectId.TryParse(this.TextBoxTilesetId.Text, out value))
            {
                value = ObjectId.Empty;
            }

            ObjectIdEditor oie = new ObjectIdEditor() { Value = value };
            oie.ShowDialog();

            if (oie.DialogResult == System.Windows.Forms.DialogResult.OK)
                this.TextBoxTilesetId.Text = oie.Value.ToString();
        }

        #region AutoTile Click
        private void TilesetControl_Click(object sender, EventArgs e)
        {
            TilesetControl.HandleInput();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBush_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            TilesetControl.DoAction(TilesetControl.Action.Bush);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBlocking_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            TilesetControl.DoAction(TilesetControl.Action.Blocking);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCounter_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            TilesetControl.DoAction(TilesetControl.Action.Counter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInheritPassages_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            TilesetControl.DoAction(TilesetControl.Action.Inherit);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPoison_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            TilesetControl.DoAction(TilesetControl.Action.Poison);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonHazard_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            TilesetControl.DoAction(TilesetControl.Action.Hazard);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonHealing_Click(object sender, EventArgs e)
        {
            this.TilesetControl.Data = this.Tileset;
            TilesetControl.DoAction(TilesetControl.Action.Healing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonUpdateNumTiles_Click(object sender, EventArgs e)
        {
            this.TilesCount.Value = TilesetControl.TilesetTileCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TilesCount_ValueChanged(object sender, EventArgs e)
        {
            this.Tileset.Resize(Convert.ToInt32(this.TilesCount.Value));
            TilesetControl.TilesetTileCount = Convert.ToInt32(this.TilesCount.Value);
        }

       


        
    }
}
