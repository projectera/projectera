namespace ContentExporter
{
    partial class ContentExporterForm
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
            this.SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.LabelConverting = new System.Windows.Forms.Label();
            this.ButtonSaveAs = new System.Windows.Forms.Button();
            this.ConversionProgress = new System.Windows.Forms.ProgressBar();
            this.LabelFileName = new System.Windows.Forms.Label();
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.LabelProgress = new System.Windows.Forms.Label();
            this.LabelAnalyse = new System.Windows.Forms.Label();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // SaveFileDialog1
            // 
            this.SaveFileDialog1.RestoreDirectory = true;
            this.SaveFileDialog1.ShowHelp = true;
            this.SaveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveFileDialog1_FileOk);
            // 
            // ButtonSave
            // 
            this.ButtonSave.Enabled = false;
            this.ButtonSave.Location = new System.Drawing.Point(93, 133);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(75, 23);
            this.ButtonSave.TabIndex = 1;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // LabelConverting
            // 
            this.LabelConverting.AutoSize = true;
            this.LabelConverting.Location = new System.Drawing.Point(9, 72);
            this.LabelConverting.Name = "LabelConverting";
            this.LabelConverting.Size = new System.Drawing.Size(69, 13);
            this.LabelConverting.TabIndex = 4;
            this.LabelConverting.Text = "Conversion...";
            // 
            // ButtonSaveAs
            // 
            this.ButtonSaveAs.Enabled = false;
            this.ButtonSaveAs.Location = new System.Drawing.Point(175, 133);
            this.ButtonSaveAs.Name = "ButtonSaveAs";
            this.ButtonSaveAs.Size = new System.Drawing.Size(75, 23);
            this.ButtonSaveAs.TabIndex = 2;
            this.ButtonSaveAs.Text = "Save As...";
            this.ButtonSaveAs.UseVisualStyleBackColor = true;
            this.ButtonSaveAs.Click += new System.EventHandler(this.ButtonSaveAs_Click);
            // 
            // ConversionProgress
            // 
            this.ConversionProgress.Location = new System.Drawing.Point(12, 88);
            this.ConversionProgress.Name = "ConversionProgress";
            this.ConversionProgress.Size = new System.Drawing.Size(238, 10);
            this.ConversionProgress.TabIndex = 5;
            // 
            // LabelFileName
            // 
            this.LabelFileName.AutoSize = true;
            this.LabelFileName.Location = new System.Drawing.Point(9, 9);
            this.LabelFileName.Name = "LabelFileName";
            this.LabelFileName.Size = new System.Drawing.Size(79, 13);
            this.LabelFileName.TabIndex = 3;
            this.LabelFileName.Text = "No File Loaded";
            // 
            // ButtonLoad
            // 
            this.ButtonLoad.Location = new System.Drawing.Point(12, 133);
            this.ButtonLoad.Name = "ButtonLoad";
            this.ButtonLoad.Size = new System.Drawing.Size(75, 23);
            this.ButtonLoad.TabIndex = 0;
            this.ButtonLoad.Text = "Load";
            this.ButtonLoad.UseVisualStyleBackColor = true;
            this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // LabelProgress
            // 
            this.LabelProgress.AutoSize = true;
            this.LabelProgress.Location = new System.Drawing.Point(226, 72);
            this.LabelProgress.Name = "LabelProgress";
            this.LabelProgress.Size = new System.Drawing.Size(24, 13);
            this.LabelProgress.TabIndex = 6;
            this.LabelProgress.Text = "0 %";
            this.LabelProgress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LabelAnalyse
            // 
            this.LabelAnalyse.AutoSize = true;
            this.LabelAnalyse.Location = new System.Drawing.Point(9, 36);
            this.LabelAnalyse.Name = "LabelAnalyse";
            this.LabelAnalyse.Size = new System.Drawing.Size(0, 13);
            this.LabelAnalyse.TabIndex = 7;
            // 
            // OpenFileDialog1
            // 
            //this.OpenFileDialog1.OverwritePrompt = false;
            this.OpenFileDialog1.Title = "Open File";
            this.OpenFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // ContentExporterForm
            // 
            this.AcceptButton = this.ButtonSave;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 168);
            this.Controls.Add(this.LabelAnalyse);
            this.Controls.Add(this.LabelProgress);
            this.Controls.Add(this.ButtonLoad);
            this.Controls.Add(this.LabelFileName);
            this.Controls.Add(this.ConversionProgress);
            this.Controls.Add(this.ButtonSaveAs);
            this.Controls.Add(this.LabelConverting);
            this.Controls.Add(this.ButtonSave);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ContentExporterForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ContentExporter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog SaveFileDialog1;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Label LabelConverting;
        private System.Windows.Forms.Button ButtonSaveAs;
        private System.Windows.Forms.ProgressBar ConversionProgress;
        private System.Windows.Forms.Label LabelFileName;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.Label LabelProgress;
        private System.Windows.Forms.Label LabelAnalyse;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog1;
    }
}

