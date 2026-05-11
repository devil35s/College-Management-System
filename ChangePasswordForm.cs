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
    public partial class ChangePasswordForm : Form
    {
        public static SqlConnection con = new SqlConnection(
    "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"
);
        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {            
            // 🔹 Validation
            if (txtUsername.Text == "" || txtOldPassword.Text == "" ||
                txtNewPassword.Text == "" || txtConfirmPassword.Text == "")
            {
                MessageBox.Show("All fields are required ❗");
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("New Password & Confirm Password mismatch ❌");
                return;
            }

            try
            {
                SqlConnection con = DBConnection.con;
                con.Open();

                // 🔹 Check Old Password
                string checkQuery = "SELECT COUNT(*) FROM LoginTable WHERE Username=@u AND Password=@p";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);

                checkCmd.Parameters.AddWithValue("@u", txtUsername.Text);
                checkCmd.Parameters.AddWithValue("@p", txtOldPassword.Text);

                int count = (int)checkCmd.ExecuteScalar();

                if (count == 1)
                {
                    // 🔹 Update Password
                    string updateQuery = "UPDATE LoginTable SET Password=@new WHERE Username=@u";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, con);

                    updateCmd.Parameters.AddWithValue("@new", txtNewPassword.Text);
                    updateCmd.Parameters.AddWithValue("@u", txtUsername.Text);

                    updateCmd.ExecuteNonQuery();

                    MessageBox.Show("Password Updated Successfully ✅");

                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Old Password is incorrect ❌");
                }

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
        private void ClearFields()
        {
            txtUsername.Text = "";
            txtOldPassword.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";

            txtUsername.Focus();
        }

        private void ChangePasswordForm_Load(object sender, EventArgs e)
        {
            txtOldPassword.UseSystemPasswordChar = true;
            txtNewPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
        }
    }
}
