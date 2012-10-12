namespace ContentConverter
{
    partial class ContentConverterForm
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
            this.ButtonExportMap = new System.Windows.Forms.Button();
            this.LabelConverting = new System.Windows.Forms.Label();
            this.ButtonExportTileset = new System.Windows.Forms.Button();
            this.ConversionProgress = new System.Windows.Forms.ProgressBar();
            this.LabelFileName = new System.Windows.Forms.Label();
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.LabelProgress = new System.Windows.Forms.Label();
            this.LabelAnalyse = new System.Windows.Forms.Label();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.ButtonConvertTileset = new System.Windows.Forms.Button();
            this.OpenFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.ButtonLoadPng = new System.Windows.Forms.Button();
            this.ButtonStore = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SaveFileDialog1
            // 
            this.SaveFileDialog1.RestoreDirectory = true;
            this.SaveFileDialog1.ShowHelp = true;
            // 
            // ButtonExportMap
            // 
            this.ButtonExportMap.Enabled = false;
            this.ButtonExportMap.Location = new System.Drawing.Point(93, 133);
            this.ButtonExportMap.Name = "ButtonExportMap";
            this.ButtonExportMap.Size = new System.Drawing.Size(75, 23);
            this.ButtonExportMap.TabIndex = 1;
            this.ButtonExportMap.Text = "Export Map";
            this.ButtonExportMap.UseVisualStyleBackColor = true;
            this.ButtonExportMap.Click += new System.EventHandler(this.ButtonExportMap_Click);
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
            // ButtonExportTileset
            // 
            this.ButtonExportTileset.Enabled = false;
            this.ButtonExportTileset.Location = new System.Drawing.Point(175, 133);
            this.ButtonExportTileset.Name = "ButtonExportTileset";
            this.ButtonExportTileset.Size = new System.Drawing.Size(75, 23);
            this.ButtonExportTileset.TabIndex = 2;
            this.ButtonExportTileset.Text = "Exp. Tileset";
            this.ButtonExportTileset.UseVisualStyleBackColor = true;
            this.ButtonExportTileset.Click += new System.EventHandler(this.ButtonExportTileset_Click);
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
            this.ButtonLoad.Text = "Load XML";
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
            this.OpenFileDialog1.FileName = "OpenFileDialog1";
            this.OpenFileDialog1.Filter = "XML data|*.xml|XML Map|*MapXml*.xml|XML Tileset|*TilesetXml*.xml";
            this.OpenFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk_1);
            // 
            // ButtonConvertTileset
            // 
            this.ButtonConvertTileset.Enabled = false;
            this.ButtonConvertTileset.Location = new System.Drawing.Point(93, 104);
            this.ButtonConvertTileset.Name = "ButtonConvertTileset";
            this.ButtonConvertTileset.Size = new System.Drawing.Size(75, 23);
            this.ButtonConvertTileset.TabIndex = 8;
            this.ButtonConvertTileset.Text = "Fix Graphic";
            this.ButtonConvertTileset.UseVisualStyleBackColor = true;
            this.ButtonConvertTileset.Click += new System.EventHandler(this.ButtonConvertTileset_Click);
            // 
            // OpenFileDialog2
            // 
            this.OpenFileDialog2.FileName = "OpenFileDialog2";
            this.OpenFileDialog2.Filter = "Images|*.png|All files|*.*";
            this.OpenFileDialog2.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog2_FileOk);
            // 
            // ButtonLoadPng
            // 
            this.ButtonLoadPng.Location = new System.Drawing.Point(13, 104);
            this.ButtonLoadPng.Name = "ButtonLoadPng";
            this.ButtonLoadPng.Size = new System.Drawing.Size(75, 23);
            this.ButtonLoadPng.TabIndex = 9;
            this.ButtonLoadPng.Text = "Load PNG";
            this.ButtonLoadPng.UseVisualStyleBackColor = true;
            this.ButtonLoadPng.Click += new System.EventHandler(this.ButtonLoadPng_Click);
            // 
            // ButtonStore
            // 
            this.ButtonStore.Enabled = false;
            this.ButtonStore.Location = new System.Drawing.Point(175, 104);
            this.ButtonStore.Name = "ButtonStore";
            this.ButtonStore.Size = new System.Drawing.Size(75, 23);
            this.ButtonStore.TabIndex = 10;
            this.ButtonStore.Text = "Store PNG";
            this.ButtonStore.UseVisualStyleBackColor = true;
            this.ButtonStore.Click += new System.EventHandler(this.ButtonStore_Click);
            // 
            // ContentConverterForm
            // 
            this.AcceptButton = this.ButtonExportMap;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 168);
            this.Controls.Add(this.ButtonStore);
            this.Controls.Add(this.ButtonLoadPng);
            this.Controls.Add(this.ButtonConvertTileset);
            this.Controls.Add(this.LabelAnalyse);
            this.Controls.Add(this.LabelProgress);
            this.Controls.Add(this.ButtonLoad);
            this.Controls.Add(this.LabelFileName);
            this.Controls.Add(this.ConversionProgress);
            this.Controls.Add(this.ButtonExportTileset);
            this.Controls.Add(this.LabelConverting);
            this.Controls.Add(this.ButtonExportMap);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ContentConverterForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ContentConverter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog SaveFileDialog1;
        private System.Windows.Forms.Button ButtonExportMap;
        private System.Windows.Forms.Label LabelConverting;
        private System.Windows.Forms.Button ButtonExportTileset;
        private System.Windows.Forms.ProgressBar ConversionProgress;
        private System.Windows.Forms.Label LabelFileName;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.Label LabelProgress;
        private System.Windows.Forms.Label LabelAnalyse;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog1;
        private System.Windows.Forms.Button ButtonConvertTileset;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog2;
        private System.Windows.Forms.Button ButtonLoadPng;
        private System.Windows.Forms.Button ButtonStore;
    }
}

