using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MongoDB.Bson;

namespace ContentConverter
{
    public partial class ObjectIdEditor : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public ObjectId Value
        {
            get
            {
                return new ObjectId(
                    Convert.ToInt32(this.DatePicker.Value.Date.Add(this.TimePicker.Value.TimeOfDay).Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds),
                    Convert.ToInt32(this.NumericMachine.Value),
                    Convert.ToInt16(this.NumericProcess.Value), 
                    Convert.ToInt32(this.NumericInc.Value)
                    );
            }
            set
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                this.TimePicker.Value = epoch.AddSeconds(value.Timestamp);
                this.DatePicker.Value = epoch.AddSeconds(value.Timestamp);
                this.NumericMachine.Value = value.Machine;
                this.NumericProcess.Value = value.Pid;
                this.NumericInc.Value = value.Increment;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObjectIdEditor()
        {
            InitializeComponent();
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
        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
