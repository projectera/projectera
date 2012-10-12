using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ContentConverter
{
    public partial class StartupForm : Form
    {
        MapBrowser Maps;
        TilesetBrowser Tilesets;
        AssetBrowser Assets;
        RegionBrowser Regions;

        public StartupForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Maps != null && Maps.IsDisposed)
                Maps = null;

            Maps = Maps ?? new MapBrowser();
            if (!Maps.Visible)
                Maps.Show(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Assets != null && Assets.IsDisposed)
                Assets = null;

            Assets = Assets ?? new AssetBrowser();

            if (!Assets.Visible)
                Assets.Show(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Tilesets != null && Tilesets.IsDisposed)
                Tilesets = null;

            Tilesets = Tilesets ?? new TilesetBrowser();

            if (!Tilesets.Visible)
                Tilesets.Show(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Regions != null && Regions.IsDisposed)
                Regions = null;

            Regions = Regions ?? new RegionBrowser();

            if (!Regions.Visible)
                Regions.Show();
        }
    }
}
