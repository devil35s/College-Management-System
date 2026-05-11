namespace CollegeManagementSystem
{
    partial class SplashForm
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
            this.panelOverlay = new System.Windows.Forms.Panel();
            this.picLogoTop = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblLoading = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.panelOverlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogoTop)).BeginInit();
            this.SuspendLayout();
            // 
            // panelOverlay
            // 
            this.panelOverlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.panelOverlay.Controls.Add(this.progressBar1);
            this.panelOverlay.Controls.Add(this.lblStatus);
            this.panelOverlay.Controls.Add(this.lblLoading);
            this.panelOverlay.Controls.Add(this.lblTitle);
            this.panelOverlay.Controls.Add(this.picLogoTop);
            this.panelOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOverlay.Location = new System.Drawing.Point(0, 0);
            this.panelOverlay.Name = "panelOverlay";
            this.panelOverlay.Size = new System.Drawing.Size(800, 450);
            this.panelOverlay.TabIndex = 0;
            // 
            // picLogoTop
            // 
            this.picLogoTop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.picLogoTop.BackColor = System.Drawing.Color.Transparent;
            this.picLogoTop.Image = global::CollegeManagementSystem.Properties.Resources.MDDC_LOGO;
            this.picLogoTop.Location = new System.Drawing.Point(391, 60);
            this.picLogoTop.Name = "picLogoTop";
            this.picLogoTop.Size = new System.Drawing.Size(150, 120);
            this.picLogoTop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogoTop.TabIndex = 0;
            this.picLogoTop.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(165, 197);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(600, 40);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "MADHUSUDHAN DAS DEGREE COLLEGE, GORAKHPUR";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLoading
            // 
            this.lblLoading.AutoSize = true;
            this.lblLoading.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoading.Location = new System.Drawing.Point(413, 254);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(163, 28);
            this.lblLoading.TabIndex = 2;
            this.lblLoading.Text = "Loading. . . (0%)";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Location = new System.Drawing.Point(422, 292);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(241, 25);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Connecting to Database. . .";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(263, 352);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(400, 8);
            this.progressBar1.TabIndex = 4;
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::CollegeManagementSystem.Properties.Resources.Clg;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panelOverlay);
            this.DoubleBuffered = true;
            this.Name = "SplashForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.SplashForm_Load);
            this.Resize += new System.EventHandler(this.SplashForm_Resize);
            this.panelOverlay.ResumeLayout(false);
            this.panelOverlay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogoTop)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelOverlay;
        private System.Windows.Forms.PictureBox picLogoTop;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}