using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ContentConverter.Data;
using MongoDB.Bson;
using ERAUtils.Enum;
using ContentConverter.Properties;
using System.IO;

namespace ContentConverter
{
    public partial class MapEditor : Form
    {
        /// <summary>
        /// Toolbox for Mongo Object Ids
        /// </summary>
        private ObjectIdEditor MongoIdEditor
        {
            get;
            set;
        }

        /// <summary>
        /// Toolbox for MapData
        /// </summary>
        private MapDataEditor DataEditor
        {
            get;
            set;
        }

        /// <summary>
        /// Map object
        /// </summary>
        internal ERAServer.Data.Map Map
        {
            get;
            private set;
        }

        /// <summary>
        /// PasteBin helper
        /// </summary>
        private Object PasteBin
        {
            get;
            set;
        }

        /// <summary>
        /// Linking Enabled Flag enables the link button and reassigns the save button
        /// </summary>
        public Boolean LinkingEnabled
        {
            get { return this.ButtonLink.Enabled; }
            set { this.ButtonLink.Enabled = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MapEditor()
        {
            InitializeComponent();

            this.ComboBoxMapType.Items.Clear();
            this.ComboBoxMapType.Items.AddRange(System.Enum.GetNames(typeof(MapType)));
        }

        /// <summary>
        /// On save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            ObjectId idMap = ObjectId.Empty, idTileset = ObjectId.Empty, idRegion = ObjectId.Empty;
            if (! (ObjectId.TryParse(this.TextBoxMapId.Text, out idMap) &&
                ObjectId.TryParse(this.TextBoxTilesetId.Text, out idTileset) &&
                ObjectId.TryParse(this.TextBoxRegionId.Text, out idRegion)))
            {
                // error
                throw new FormatException();
            }

            ERAServer.Data.Map.MapSettings settings = new ERAServer.Data.Map.MapSettings();
            //settings.FogAssetName = this.TextBoxFog.Text;
            //settings.FogOpacity = Byte.Parse(this.TextBoxFogOpacity.Text);
            //settings.PanormaAssetName = this.TextBoxPanorama.Text;

            this.Map = ERAServer.Data.Map.Generate(idMap, idTileset, idRegion, this.TextBoxName.Text,
                (MapType)System.Enum.Parse(typeof(MapType), this.ComboBoxMapType.SelectedItem.ToString()),
                settings, Map.Width, Map.Height, Map.Data, Map.Version);

            this.Map.Put();

            if (!this.LinkingEnabled)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
           
        }

        /// <summary>
        /// Generate from map xml data
        /// </summary>
        /// <param name="mapdata"></param>
        internal void Generate(Int32 id, UInt16 width, UInt16 height, Int32 tilesetId, String name, String fog, Byte fogOpacity, String panorama, UInt16[][][] mapdata)
        {
            this.Map = ContentConverter.Data.Map.Generate(id, width, height, tilesetId, name, fog, fogOpacity, panorama, mapdata);

            RefreshDisplay();
        }

        /// <summary>
        /// Loads from map object
        /// </summary>
        /// <param name="map"></param>
        internal void LoadFrom(ERAServer.Data.Map map)
        {
            this.Map = map;

            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes display, loads Map data
        /// </summary>
        internal void RefreshDisplay()
        {
            this.Map = this.Map ?? new ERAServer.Data.Map();

            this.TextBoxName.Text = Map.Name;
            this.TextBoxMapId.Text = Map.Id.ToString();
            this.TextBoxTilesetId.Text = Map.TilesetId.ToString();
            this.TextBoxRegionId.Text = Map.RegionId.ToString();
            
            foreach(Object item in this.ComboBoxMapType.Items)
            {
                if (item is String)
                {
                    if ((String)item == Map.Type.ToString())
                    {
                        this.ComboBoxMapType.SelectedItem = item;
                        break;
                    }
                }
            }

            //this.LabelDimensions.Text = String.Format("{0}x{1}", Map.Width, Map.Height);

            ERAServer.Data.Map.MapSettings settings = (Map.Settings ?? new ERAServer.Data.Map.MapSettings());

            //this.TextBoxFog.Text = settings.FogAssetName ?? String.Empty;
            //this.TextBoxFogOpacity.Text = settings.FogOpacity.ToString();
            //this.TextBoxPanorama.Text = settings.PanormaAssetName ?? String.Empty;

            RefreshDataValidator();

            TextBoxTilesetName_Click(this, EventArgs.Empty);


        }

        /// <summary>
        /// Refreshes the mapdata validator graphic
        /// </summary>
        private void RefreshDataValidator()
        {
            if (Map.Data != null && Map.Width > 0 && Map.Height > 0)
            {
                this.PictureBox.Image = Resources.tick;
                ToolTip1.SetToolTip(this.PictureBox, "MapData is valid and loaded");
            }
            else
            {
                this.PictureBox.Image = Resources.cross;
                ToolTip1.SetToolTip(this.PictureBox, "MapData is invalid");
            }
        }

        /// <summary>
        /// On button tileset lookup click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTilesetLookup_Click(object sender, EventArgs e)
        {
            if (TextBoxTilesetName.Visible)
            {
                TextBoxTilesetName_Click(sender, e);
                return;
            }
            
            ObjectId idTileset;
            if (!ObjectId.TryParse(this.TextBoxTilesetId.Text, out idTileset))
                return;

            ERAServer.Data.Tileset tileset = ERAServer.Data.Tileset.GetCollection().FindOneById(idTileset);
            if (tileset != null)
                TextBoxTilesetName.Text = tileset.Name;

            TextBoxTilesetName.Visible = true;
            TextBoxTilesetName.Click += new EventHandler(TextBoxTilesetName_Click);
        }

        /// <summary>
        /// On tilesetname lick (de-activates lookup)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxTilesetName_Click(object sender, EventArgs e)
        {
            TextBoxTilesetName.Visible = false;
            TextBoxTilesetName.Click -= TextBoxTilesetName_Click;
            TextBoxTilesetName.Text = String.Empty;
        }

        /// <summary>
        /// Browse tileset button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButtonTileset_Click(object sender, EventArgs e)
        {
            TilesetBrowser tilesetBrowser = new TilesetBrowser() { LinkingEnabled = true };
            tilesetBrowser.ShowDialog(this);

            if (tilesetBrowser.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxTilesetId.Text = tilesetBrowser.LinkedTileset.Id.ToString();
            }
        }

        /// <summary>
        /// Browse region button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButonRegion_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Open Map Id Edit toolbox click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEditId_Click(object sender, EventArgs e)
        {
            ObjectId value = ObjectId.Empty;
            if (!ObjectId.TryParse(this.TextBoxMapId.Text, out value))
            {
                value = ObjectId.Empty;
            }

            RefreshMongoObjectIdEditor(value, this.TextBoxMapId);
        }

        /// <summary>
        /// Refresh Id Edit Toolbox
        /// </summary>
        /// <param name="value"></param>
        private void RefreshMongoObjectIdEditor(ObjectId value, Object tag)
        {
            if (this.MongoIdEditor == null)
            {
                this.MongoIdEditor = new ObjectIdEditor();
                this.MongoIdEditor.FormClosed += new FormClosedEventHandler(MongoIdEditor_FormClosed);
            }
            this.MongoIdEditor.Visible = false;
            this.MongoIdEditor.Value = value;
            this.MongoIdEditor.Tag = tag;
            
            this.MongoIdEditor.Show(this);
            this.MongoIdEditor.Location = this.Location + new Size(0, this.Size.Height + 15);
        }

        /// <summary>
        /// On Id Edit Tooblox closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MongoIdEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MongoIdEditor.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                (this.MongoIdEditor.Tag as TextBox).Text = this.MongoIdEditor.Value.ToString();
            }

            this.MongoIdEditor = null;
        }

        /// <summary>
        /// Open Tileset Id Edit toolbox click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEditTilesetId_Click(object sender, EventArgs e)
        {
            ObjectId value = ObjectId.Empty;
            if (!ObjectId.TryParse(this.TextBoxTilesetId.Text, out value))
            {
                value = ObjectId.Empty;
            }

            RefreshMongoObjectIdEditor(value, this.TextBoxTilesetId);
        }

        /// <summary>
        /// Open Region Id Edit toolbox click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEditRegionId_Click(object sender, EventArgs e)
        {
            ObjectId value = ObjectId.Empty;
            if (!ObjectId.TryParse(this.TextBoxRegionId.Text, out value))
            {
                value = ObjectId.Empty;
            }

            RefreshMongoObjectIdEditor(value, this.TextBoxRegionId);
        }

        /// <summary>
        /// Open Region Id Edit toolbox click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            if (DataEditor != null)
                return;

            this.DataEditor = new MapDataEditor() { MapWidth = this.Map.Width, MapHeight = this.Map.Height, Value = this.Map.Data };
            this.DataEditor.FormClosed += new FormClosedEventHandler(DataEditor_FormClosed);
            this.DataEditor.Visible = false;
            this.DataEditor.Show(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DataEditor.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                ContentConverter.Data.Map.SetData(this.Map, DataEditor.MapWidth, DataEditor.MapHeight, DataEditor.Value);
                RefreshDataValidator();
            }

            DataEditor = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLink_Click(object sender, EventArgs e)
        {
            ButtonSave_Click(sender, e);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }


    }
}
