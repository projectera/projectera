using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ContentConverter.Data;
using System.IO;

namespace ContentConverter
{
    public partial class MapDataEditor : Form
    {

        /// <summary>
        /// 
        /// </summary>
        public UInt16 MapWidth
        {
            get { return (UInt16)this.NumericWidth.Value;  }
            set { this.NumericWidth.Value = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public UInt16 MapHeight
        {
            get { return (UInt16)this.NumericHeigth.Value; }
            set { this.NumericHeigth.Value = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public UInt16[][][] Value
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public MapDataEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog1.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            try
            {
                this.Value = Map.GetDataFromFile((Int32)this.NumericWidth.Value, (Int32)this.NumericHeigth.Value, OpenFileDialog1.FileName);
                this.Close();
            }
            catch (FormatException a)
            {
                MessageBox.Show(a.GetType().Name + " - " + a.Message);
            }
            catch (InvalidDataException a)
            {
                MessageBox.Show(a.GetType().Name + " - " + a.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }
    }
}
