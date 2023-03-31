namespace FirmwareUpdater
{
    partial class FrmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.btnUpdate = new System.Windows.Forms.Button();
            this.edtFileName = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.ddlPort = new System.Windows.Forms.ComboBox();
            this.lblStatusValue = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblFwVersion = new System.Windows.Forms.Label();
            this.lblFwVerLbl = new System.Windows.Forms.Label();
            this.epdImg = new System.Windows.Forms.PictureBox();
            this.transmitProgress = new System.Windows.Forms.ProgressBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbPassphrase = new System.Windows.Forms.TextBox();
            this.cbAccessLevel = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.imageListEpds = new System.Windows.Forms.ImageList(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tmrCmdLineFallback = new System.Windows.Forms.Timer(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pnlStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epdImg)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnUpdate
            // 
            this.btnUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpdate.Location = new System.Drawing.Point(127, 184);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(88, 31);
            this.btnUpdate.TabIndex = 0;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // edtFileName
            // 
            this.edtFileName.Location = new System.Drawing.Point(87, 130);
            this.edtFileName.Name = "edtFileName";
            this.edtFileName.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.edtFileName.Size = new System.Drawing.Size(193, 20);
            this.edtFileName.TabIndex = 1;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "TruDose.enc";
            this.openFileDialog.Filter = "Encoded Firmware (*.bin)|*.bin|All Files(*.*)|*.*";
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(294, 128);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(25, 23);
            this.btnOpenFile.TabIndex = 2;
            this.btnOpenFile.Text = "...";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // ddlPort
            // 
            this.ddlPort.FormattingEnabled = true;
            this.ddlPort.Location = new System.Drawing.Point(87, 93);
            this.ddlPort.Name = "ddlPort";
            this.ddlPort.Size = new System.Drawing.Size(193, 21);
            this.ddlPort.TabIndex = 3;
            this.ddlPort.SelectedIndexChanged += new System.EventHandler(this.ddlPort_SelectedIndexChanged);
            // 
            // lblStatusValue
            // 
            this.lblStatusValue.Location = new System.Drawing.Point(1, 206);
            this.lblStatusValue.Name = "lblStatusValue";
            this.lblStatusValue.Size = new System.Drawing.Size(194, 23);
            this.lblStatusValue.TabIndex = 5;
            this.lblStatusValue.Text = "Transfer Status";
            this.lblStatusValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Reader Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Firmware File";
            // 
            // pnlStatus
            // 
            this.pnlStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlStatus.Controls.Add(this.lblFwVersion);
            this.pnlStatus.Controls.Add(this.lblFwVerLbl);
            this.pnlStatus.Controls.Add(this.epdImg);
            this.pnlStatus.Controls.Add(this.transmitProgress);
            this.pnlStatus.Controls.Add(this.lblStatusValue);
            this.pnlStatus.Location = new System.Drawing.Point(383, 19);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(203, 266);
            this.pnlStatus.TabIndex = 8;
            // 
            // lblFwVersion
            // 
            this.lblFwVersion.AutoSize = true;
            this.lblFwVersion.Location = new System.Drawing.Point(121, 173);
            this.lblFwVersion.Name = "lblFwVersion";
            this.lblFwVersion.Size = new System.Drawing.Size(40, 13);
            this.lblFwVersion.TabIndex = 8;
            this.lblFwVersion.Text = "0.0.0.0";
            // 
            // lblFwVerLbl
            // 
            this.lblFwVerLbl.AutoSize = true;
            this.lblFwVerLbl.Location = new System.Drawing.Point(18, 173);
            this.lblFwVerLbl.Name = "lblFwVerLbl";
            this.lblFwVerLbl.Size = new System.Drawing.Size(71, 13);
            this.lblFwVerLbl.TabIndex = 7;
            this.lblFwVerLbl.Text = "Firmware Ver.";
            // 
            // epdImg
            // 
            this.epdImg.Location = new System.Drawing.Point(45, 9);
            this.epdImg.Name = "epdImg";
            this.epdImg.Size = new System.Drawing.Size(116, 151);
            this.epdImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.epdImg.TabIndex = 6;
            this.epdImg.TabStop = false;
            // 
            // transmitProgress
            // 
            this.transmitProgress.Location = new System.Drawing.Point(4, 232);
            this.transmitProgress.Name = "transmitProgress";
            this.transmitProgress.Size = new System.Drawing.Size(194, 23);
            this.transmitProgress.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.tbPassphrase);
            this.panel1.Controls.Add(this.cbAccessLevel);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ddlPort);
            this.panel1.Controls.Add(this.btnOpenFile);
            this.panel1.Controls.Add(this.edtFileName);
            this.panel1.Controls.Add(this.btnUpdate);
            this.panel1.Location = new System.Drawing.Point(12, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(347, 266);
            this.panel1.TabIndex = 9;
            // 
            // tbPassphrase
            // 
            this.tbPassphrase.Location = new System.Drawing.Point(87, 57);
            this.tbPassphrase.Name = "tbPassphrase";
            this.tbPassphrase.Size = new System.Drawing.Size(159, 20);
            this.tbPassphrase.TabIndex = 11;
            this.tbPassphrase.UseSystemPasswordChar = true;
            // 
            // cbAccessLevel
            // 
            this.cbAccessLevel.FormattingEnabled = true;
            this.cbAccessLevel.Items.AddRange(new object[] {
            "Open",
            "Admin",
            "Regulator",
            "Manufacturer"});
            this.cbAccessLevel.Location = new System.Drawing.Point(87, 18);
            this.cbAccessLevel.Name = "cbAccessLevel";
            this.cbAccessLevel.Size = new System.Drawing.Size(121, 21);
            this.cbAccessLevel.TabIndex = 10;
            this.cbAccessLevel.SelectedIndexChanged += new System.EventHandler(this.cbAccessLevel_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Passphrase";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Access Level";
            // 
            // imageListEpds
            // 
            this.imageListEpds.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListEpds.ImageStream")));
            this.imageListEpds.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListEpds.Images.SetKeyName(0, "EPD_Mk3_BGiconGrayed.png");
            this.imageListEpds.Images.SetKeyName(1, "EPD_Mk3_BGicon.png");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(394, 291);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(181, 42);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // tmrCmdLineFallback
            // 
            this.tmrCmdLineFallback.Interval = 15000;
            this.tmrCmdLineFallback.Tick += new System.EventHandler(this.tmrCmdLineFallback_Tick);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 335);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FrmMain";
            this.Text = "TruDose Firmware Update Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epdImg)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.TextBox edtFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.ComboBox ddlPort;
        private System.Windows.Forms.Label lblStatusValue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.ProgressBar transmitProgress;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox epdImg;
        private System.Windows.Forms.ImageList imageListEpds;
        private System.Windows.Forms.Label lblFwVersion;
        private System.Windows.Forms.Label lblFwVerLbl;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer tmrCmdLineFallback;
        private System.Windows.Forms.TextBox tbPassphrase;
        private System.Windows.Forms.ComboBox cbAccessLevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer timer1;
    }
}

