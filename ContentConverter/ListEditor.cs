using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ERAUtils.Enum;
using ContentConverter.Data;

namespace ContentConverter
{
    public partial class ListEditor : Form
    {
        internal Asset Root { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String[] Values
        {
            get
            {
                Int32 i = 0;
                String[] result = new String[this.List.Items.Count];
                foreach (Object item in this.List.Items)
                {
                    result[i++] = item.ToString();
                }

                return result; 
            }
            set
            {
                this.List.SelectedIndex = -1;
                this.List.Items.Clear();
                this.List.Items.AddRange(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Object SelectedItem
        {
            get { return this.List.SelectedItem; }
            set
            {
                for (int i = 0; i < this.List.Items.Count; i++)
                {
                    if (this.List.Items[i].Equals(value))
                    {
                        this.List.SelectedIndex = i;
                        this.ButtonSave.Enabled = true;
                        this.ButtonDelete.Enabled = true;
                        return;
                    }
                }

                this.List.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ListEditor()
        {
            InitializeComponent();

            this.ButtonAdd.Enabled = false;
            this.ButtonDelete.Enabled = false;
            this.ButtonSave.Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (this.SelectedItem == null)
                return;

            if (MessageBox.Show("Are you sure you want to delete item <" + this.SelectedItem.ToString() + ">?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.List.Items.Remove(this.SelectedItem);
                this.List.SelectedIndex = -1;
                this.ButtonDelete.Enabled = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(this.TextBox.Text))
                return;

            Asset duplicate = Asset.GetFile(this.Root.Type, this.TextBox.Text);

            if ((duplicate == null || duplicate.Id == this.Root.Id) && !this.List.Items.Contains(this.TextBox.Text))
            {
                this.List.Items.Add(this.TextBox.Text);
            }
            else
            {
                MessageBox.Show("There is already a file queryable by <" + this.TextBox.Text + "> in this namespace.", "Duplicate Alias!", MessageBoxButtons.OK);
            }

            this.TextBox.Text = String.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.List.SelectedIndex >= 0)
            {
                this.ButtonDelete.Enabled = true;
                this.ButtonSave.Enabled = true;
            }
            else
            {
                this.ButtonDelete.Enabled = false;
                this.ButtonSave.Enabled = false;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            this.ButtonAdd.Enabled = !String.IsNullOrWhiteSpace(this.TextBox.Text);
        }
    }
}
