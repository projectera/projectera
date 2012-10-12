namespace ContentConverter
{
    partial class MapDataEditor
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
            this.ButtonAccept = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.NumericWidth = new System.Windows.Forms.NumericUpDown();
            this.NumericHeigth = new System.Windows.Forms.NumericUpDown();
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.NumericWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHeigth)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonAccept
            // 
            this.ButtonAccept.Image = global::ContentConverter.Properties.Resources.accept;
            this.ButtonAccept.Location = new System.Drawing.Point(12, 115);
            this.ButtonAccept.Name = "ButtonAccept";
            this.ButtonAccept.Size = new System.Drawing.Size(78, 24);
            this.ButtonAccept.TabIndex = 4;
            this.ButtonAccept.Text = "Accept";
            this.ButtonAccept.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonAccept.UseVisualStyleBackColor = true;
            this.ButtonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Image = global::ContentConverter.Properties.Resources.cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(99, 115);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(78, 24);
            this.ButtonCancel.TabIndex = 5;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // NumericWidth
            // 
            this.NumericWidth.Location = new System.Drawing.Point(99, 12);
            this.NumericWidth.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.NumericWidth.Name = "NumericWidth";
            this.NumericWidth.Size = new System.Drawing.Size(78, 20);
            this.NumericWidth.TabIndex = 1;
            // 
            // NumericHeigth
            // 
            this.NumericHeigth.Location = new System.Drawing.Point(99, 38);
            this.NumericHeigth.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.NumericHeigth.Name = "NumericHeigth";
            this.NumericHeigth.Size = new System.Drawing.Size(78, 20);
            this.NumericHeigth.TabIndex = 2;
            // 
            // ButtonLoad
            // 
            this.ButtonLoad.Image = global::ContentConverter.Properties.Resources.folder_explore;
            this.ButtonLoad.Location = new System.Drawing.Point(12, 64);
            this.ButtonLoad.Name = "ButtonLoad";
            this.ButtonLoad.Size = new System.Drawing.Size(165, 45);
            this.ButtonLoad.TabIndex = 3;
            this.ButtonLoad.Text = "Set Data Source";
            this.ButtonLoad.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonLoad.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonLoad.UseVisualStyleBackColor = true;
            this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Width (tiles)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Heigth (tiles)";
            // 
            // OpenFileDialog1
            // 
            this.OpenFileDialog1.Filter = "TXT Map|*MapData*.txt|TXT data|*.txt";
            this.OpenFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // MapDataEdit
            // 
            this.AcceptButton = this.ButtonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(189, 151);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonLoad);
            this.Controls.Add(this.NumericHeigth);
            this.Controls.Add(this.NumericWidth);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonAccept);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MapDataEdit";
            this.Text = "Map Data Editor";
            ((System.ComponentModel.ISupportInitialize)(this.NumericWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericHeigth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonAccept;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.NumericUpDown NumericWidth;
        private System.Windows.Forms.NumericUpDown NumericHeigth;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog1;
    }
}