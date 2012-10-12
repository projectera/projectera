namespace ContentConverter
{
    partial class ObjectIdEditor
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
            this.NumericMachine = new System.Windows.Forms.NumericUpDown();
            this.NumericProcess = new System.Windows.Forms.NumericUpDown();
            this.NumericInc = new System.Windows.Forms.NumericUpDown();
            this.LabelTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TimePicker = new System.Windows.Forms.DateTimePicker();
            this.DatePicker = new System.Windows.Forms.DateTimePicker();
            this.ButtonAccept = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.NumericMachine)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericProcess)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericInc)).BeginInit();
            this.SuspendLayout();
            // 
            // NumericMachine
            // 
            this.NumericMachine.Location = new System.Drawing.Point(66, 38);
            this.NumericMachine.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.NumericMachine.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.NumericMachine.Name = "NumericMachine";
            this.NumericMachine.Size = new System.Drawing.Size(110, 20);
            this.NumericMachine.TabIndex = 3;
            // 
            // NumericProcess
            // 
            this.NumericProcess.Location = new System.Drawing.Point(66, 64);
            this.NumericProcess.Maximum = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.NumericProcess.Minimum = new decimal(new int[] {
            32767,
            0,
            0,
            -2147483648});
            this.NumericProcess.Name = "NumericProcess";
            this.NumericProcess.Size = new System.Drawing.Size(110, 20);
            this.NumericProcess.TabIndex = 4;
            // 
            // NumericInc
            // 
            this.NumericInc.Location = new System.Drawing.Point(48, 90);
            this.NumericInc.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.NumericInc.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.NumericInc.Name = "NumericInc";
            this.NumericInc.Size = new System.Drawing.Size(128, 20);
            this.NumericInc.TabIndex = 5;
            // 
            // LabelTime
            // 
            this.LabelTime.AutoSize = true;
            this.LabelTime.Location = new System.Drawing.Point(12, 14);
            this.LabelTime.Name = "LabelTime";
            this.LabelTime.Size = new System.Drawing.Size(30, 13);
            this.LabelTime.TabIndex = 4;
            this.LabelTime.Text = "Time";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Machine";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Process";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Inc";
            // 
            // TimePicker
            // 
            this.TimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.TimePicker.Location = new System.Drawing.Point(109, 14);
            this.TimePicker.Name = "TimePicker";
            this.TimePicker.ShowUpDown = true;
            this.TimePicker.Size = new System.Drawing.Size(68, 20);
            this.TimePicker.TabIndex = 2;
            this.TimePicker.Value = new System.DateTime(2011, 12, 28, 21, 36, 42, 0);
            // 
            // DatePicker
            // 
            this.DatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.DatePicker.Location = new System.Drawing.Point(15, 14);
            this.DatePicker.Name = "DatePicker";
            this.DatePicker.Size = new System.Drawing.Size(91, 20);
            this.DatePicker.TabIndex = 1;
            this.DatePicker.Value = new System.DateTime(2011, 12, 28, 0, 0, 0, 0);
            // 
            // ButtonAccept
            // 
            this.ButtonAccept.Image = global::ContentConverter.Properties.Resources.accept;
            this.ButtonAccept.Location = new System.Drawing.Point(15, 116);
            this.ButtonAccept.Name = "ButtonAccept";
            this.ButtonAccept.Size = new System.Drawing.Size(78, 24);
            this.ButtonAccept.TabIndex = 6;
            this.ButtonAccept.Text = "Accept";
            this.ButtonAccept.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonAccept.UseVisualStyleBackColor = true;
            this.ButtonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Image = global::ContentConverter.Properties.Resources.cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(98, 116);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(78, 24);
            this.ButtonCancel.TabIndex = 7;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ObjectIdEditor
            // 
            this.AcceptButton = this.ButtonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(189, 151);
            this.Controls.Add(this.DatePicker);
            this.Controls.Add(this.TimePicker);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonAccept);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LabelTime);
            this.Controls.Add(this.NumericInc);
            this.Controls.Add(this.NumericProcess);
            this.Controls.Add(this.NumericMachine);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ObjectIdEditor";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ObjectId Editor";
            ((System.ComponentModel.ISupportInitialize)(this.NumericMachine)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericProcess)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericInc)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown NumericMachine;
        private System.Windows.Forms.NumericUpDown NumericProcess;
        private System.Windows.Forms.NumericUpDown NumericInc;
        private System.Windows.Forms.Label LabelTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ButtonAccept;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.DateTimePicker TimePicker;
        private System.Windows.Forms.DateTimePicker DatePicker;
    }
}