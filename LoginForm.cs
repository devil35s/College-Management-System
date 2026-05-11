using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollegeManagementSystem
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();

            pnlLogin.BackColor = Color.FromArgb(200, Color.White);
            lblPassword.BackColor = Color.FromArgb(0, Color.Transparent);
            lblTitle.BackColor = Color.FromArgb(0, Color.Transparent);
            lblUsername.BackColor = Color.FromArgb(0, Color.Transparent);
            picLogo.BackColor = Color.FromArgb(0, Color.Transparent);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    if (DBConnection.con.State == ConnectionState.Closed)
                    {
                        DBConnection.con.Open();
                    }

                    SqlCommand cmd = new SqlCommand(
                        "SELECT * FROM LoginTable WHERE Username=@user AND Password=@pass",
                        DBConnection.con);

                    cmd.Parameters.AddWithValue("@user", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@pass", txtPassword.Text);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable temp = new DataTable();
                    da.Fill(temp);

                    DBConnection.con.Close();

                    return temp;
                });

                // 🔥 UI thread (safe)
                if (dt.Rows.Count > 0)
                {
                    this.Cursor = Cursors.AppStarting;

                    await Task.Delay(100);

                    Dashboard d = new Dashboard();
                    
                    d.Opacity = 0;                    
                    d.Show();

                    await Task.Delay(1500);

                    d.Opacity = 1;
                    this.Cursor = Cursors.Default;

                    this.Hide();

                }
                else
                {
                    MessageBox.Show("Invalid Username or Password");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
           
            pnlLogin.Left = (this.ClientSize.Width - pnlLogin.Width) / 2;
            pnlLogin.Top = (this.ClientSize.Height - pnlLogin.Height) / 2;
        }

        private async void lblChangePassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Cursor = Cursors.AppStarting;

            await Task.Delay(100);

            ChangePasswordForm cpf = new ChangePasswordForm();

            cpf.Opacity = 0;
            cpf.Show();

            await Task.Delay(1500);

            cpf.Opacity = 1;
            this.Cursor = Cursors.Default;
            
            
        }
    }
}
