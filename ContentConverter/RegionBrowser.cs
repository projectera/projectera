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
    public partial class RegionBrowser : Form
    {
        /// <summary>
        /// Selected Name on Close
        /// </summary>
        internal String SelectedName { get; private set; }

        /// <summary>
        /// Selected Type on Close
        /// </summary>
        internal String SelectedType { get; private set; }

        /// <summary>
        /// Selected Type
        /// </summary>
        internal String RegionType
        {
            get
            {
                return ((this.ComboBoxRegionType.SelectedItem ?? "Region").ToString() ?? "Region");
            }
            set
            {
                for (int i = 0; i < this.ComboBoxRegionType.Items.Count; i++)
                {
                    if (this.ComboBoxRegionType.Items[i].ToString() == value)
                    {
                        this.ComboBoxRegionType.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal ERAServer.Data.Region SelectedData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RegionBrowser()
        {
            InitializeComponent();

            this.ComboBoxRegionType.Items.Clear();
            this.ComboBoxRegionType.Items.Add(typeof(ERAServer.Data.Region).Name);
            this.ComboBoxRegionType.Items.Add(typeof(ERAServer.Data.Planet).Name);
            this.ComboBoxRegionType.Items.Add(typeof(ERAServer.Data.Country).Name);
            this.ComboBoxRegionType.Items.Add(typeof(ERAServer.Data.District).Name);
            this.ComboBoxRegionType.Items.Add(typeof(ERAServer.Data.Area).Name);

            this.ComboBoxRegionType.SelectedIndex = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            this.ListResults.SelectedIndex = -1;
            this.ListResults.Items.Clear();
            this.ListResults.Items.AddRange(
                Data.Region.GetList(this.RegionType, this.TextBoxName.Text)
            );

            this.ButtonUse.Enabled = false;
            this.ButtonSave.Enabled = false;
            this.ListResults.Enabled = true;

            this.SelectedType = this.RegionType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            this.SelectedData.Put();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonUse_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ListResults.SelectedIndex == -1)
                return;

            this.ButtonUse.Enabled = true;
            this.ButtonSave.Enabled = true;
            this.SelectedName = this.ListResults.SelectedItem.ToString();
            this.SelectedData = ERAServer.Data.Region.GetBlocking(this.SelectedName);
        }

        private void ComboBoxRegionType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
