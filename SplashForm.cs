using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollegeManagementSystem
{
    public partial class SplashForm : Form
    {
        int progress = 0;
        public SplashForm()
        {
            InitializeComponent();
        }

        private async void SplashForm_Load(object sender, EventArgs e)
        {
            // Center alignment (same rakho)
            picLogoTop.Left = (this.ClientSize.Width - picLogoTop.Width) / 2;
            picLogoTop.Top = 80;

            lblLoading.Left = (this.ClientSize.Width - lblLoading.Width) / 2;
            lblLoading.Top = lblTitle.Bottom + 10;

            lblStatus.Left = (this.ClientSize.Width - lblStatus.Width) / 2;
            lblStatus.Top = lblLoading.Bottom + 5;

            progressBar1.Left = (this.ClientSize.Width - progressBar1.Width) / 2;
            progressBar1.Top = lblStatus.Bottom + 10;

            // Progress setup
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            // 🔥 REAL LOADING CALL
            await LoadApplicationData();

            Dashboard db = new Dashboard();
            db.Show();

            this.Hide();
        }

        private async Task LoadApplicationData()
        {
            lblStatus.Text = "Connecting to Database...";
            await Task.Delay(500);
            progressBar1.Value = 20;

            lblStatus.Text = "Loading Students...";
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            progressBar1.Value = 50;

            lblStatus.Text = "Loading Modules...";
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            progressBar1.Value = 80;

            lblStatus.Text = "Preparing Dashboard...";
            await Task.Delay(500);
            progressBar1.Value = 100;
        }

        private void SplashForm_Resize(object sender, EventArgs e)
        {
            
            picLogoTop.Left = (this.ClientSize.Width - picLogoTop.Width) / 2;
            picLogoTop.Top = (this.ClientSize.Height - picLogoTop.Height) / 2 - 80;

            lblTitle.Left = (this.ClientSize.Width - lblTitle.Width) / 2;
            lblLoading.Left = (this.ClientSize.Width - lblLoading.Width) / 2;
            lblStatus.Left = (this.ClientSize.Width - lblStatus.Width) / 2;
            progressBar1.Left = (this.ClientSize.Width - progressBar1.Width) / 2;
        }                
    }   
}