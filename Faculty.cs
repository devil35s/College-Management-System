using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using Color = System.Drawing.Color;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollegeManagementSystem
{
    public partial class Faculty : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");
        public Faculty()
        {
            InitializeComponent();
        }
        
        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFaculty();
            StyleDataGridView();
            txtSearch.Text = "Search By Faculty Name";
            txtSearch.ForeColor = Color.Gray;

        }
        private void StyleDataGridView()
        {
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.BackgroundColor = Color.White;

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            dataGridView1.ColumnHeadersHeight = 40;

            dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dataGridView1.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dataGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dataGridView1.RowTemplate.Height = 35;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;

            dataGridView1.BackgroundColor = System.Drawing.Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.GridColor = System.Drawing.Color.Gainsboro;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.ScrollBars = ScrollBars.Both;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.ColumnHeadersHeight = 45;

        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "Image Files | *.jpg;*.png*.jpeg";

              if (op.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(op.FileName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    // 🔹 Photo (SAFE VERSION)
                    byte[] imageBytes = null;

                    if (pictureBox1.Image != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Bitmap bmp = new Bitmap(pictureBox1.Image); // 🔥 FIX
                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            imageBytes = ms.ToArray();
                            bmp.Dispose();
                        }
                    }

                    // 🔹 Gender
                    string gender = radioMale.Checked ? "Male" : "Female";

                    // 🔹 Query
                    string query = @"INSERT INTO Faculty 
            (FirstName, LastName, Gender, DOB, Salary, Phone, Email, Address, Department, JoinDate, Qualification, Photo) 
            VALUES 
            (@FirstName, @LastName, @Gender, @DOB, @Salary, @Phone, @Email, @Address, @Department, @JoinDate, @Qualification, @Photo)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                    cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                    cmd.Parameters.AddWithValue("@Salary", Convert.ToDecimal(txtSalary.Text)); // 🔥 FIX
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Department", cmbModule.Text);
                    cmd.Parameters.AddWithValue("@JoinDate", dtpJoinDate.Value);
                    cmd.Parameters.AddWithValue("@Qualification", txtQualification.Text);
                    cmd.Parameters.Add("@Photo", SqlDbType.VarBinary).Value =
                        (object)imageBytes ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Faculty Saved Successfully ✅");

                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm is Dashboard dashboard)
                        {
                            dashboard.LoadStatistics();
                        }
                    }

                    LoadFaculty();
                    LoadFacultyCount(); // 🔥 count update
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadFacultyCount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    string query = "SELECT COUNT(*) FROM Faculty";

                    SqlCommand cmd = new SqlCommand(query, con);

                    int count = (int)cmd.ExecuteScalar();

                    lblTotalFaculty.Text = "Total Faculty: " + count.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        


        private void LoadFaculty()
        {
            using (SqlConnection con = new SqlConnection(
                @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Faculty", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                
                dataGridView1.DataSource = dt;

                dataGridView1.Columns["FacultyID"].HeaderText = "Faculty ID";
                dataGridView1.Columns["FirstName"].HeaderText = "First Name";
                dataGridView1.Columns["LastName"].HeaderText = "Last Name";
                dataGridView1.Columns["JoinDate"].HeaderText = "Join Date";

                // Hide Photo column
                if (dataGridView1.Columns.Contains("Photo"))
                    dataGridView1.Columns["Photo"].Visible = false;

                // Layout Settings
                
                // Date Format
                if (dataGridView1.Columns.Contains("DateOfBirth"))
                    dataGridView1.Columns["DateOfBirth"].DefaultCellStyle.Format = "dd-MMM-yyyy";
                

                if (dataGridView1.Columns.Contains("Photo"))
                    dataGridView1.Columns["Photo"].Visible = false;
                lblTotalFaculty.Text = "Total Faculties: " + dt.Rows.Count;
            }
        }
        private void ClearFields()
        {
            txtFacultyID.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            radioMale.Checked = false;
            radioFemale.Checked = false;
            txtSalary.Clear();
            cmbModule.SelectedIndex = -1;
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            pictureBox1.Image = null;
            txtQualification.Clear();
            

        }

        

        private void btnUpdate_Click(object sender, EventArgs e)
        {
       
            try
            {
                // 🔹 Validation
                if (txtFacultyID.Text == "")
                {
                    MessageBox.Show("Please select a record to update");
                    return;
                }

                using (SqlConnection con = new SqlConnection(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    // 🔹 Image Convert (SAFE - GDI+ fix)
                    byte[] imageBytes = null;

                    if (pictureBox1.Image != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Bitmap bmp = new Bitmap(pictureBox1.Image);
                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            imageBytes = ms.ToArray();
                            bmp.Dispose();
                        }
                    }

                    // 🔹 Gender
                    string gender = radioMale.Checked ? "Male" : "Female";

                    // 🔹 Update Query
                    string query = @"UPDATE Faculty SET 
                FirstName=@FirstName,
                LastName=@LastName,
                Gender=@Gender,
                DOB=@DOB,
                Salary=@Salary,
                Phone=@Phone,
                Email=@Email,
                Address=@Address,
                Department=@Department,
                JoinDate=@JoinDate,
                Qualification=@Qualification,
                Photo=@Photo
                WHERE FacultyID=@FacultyID";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@FacultyID", txtFacultyID.Text);
                    cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                    cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                    cmd.Parameters.AddWithValue("@Salary", Convert.ToDecimal(txtSalary.Text));
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Department", cmbModule.Text);
                    cmd.Parameters.AddWithValue("@JoinDate", dtpJoinDate.Value);
                    cmd.Parameters.AddWithValue("@Qualification", txtQualification.Text);

                    cmd.Parameters.Add("@Photo", SqlDbType.VarBinary).Value =
                        (object)imageBytes ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Faculty Updated Successfully ✅");

                    LoadFaculty();       // Grid refresh
                    LoadFacultyCount();  // Count update
                    ClearFields();       // Clear form
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
            
        private void btnDelete_Click(object sender, EventArgs e)
        {
         
            try
            {
                // 🔹 Validation
                if (txtFacultyID.Text == "")
                {
                    MessageBox.Show("Please select a record to delete");
                    return;
                }

                // 🔹 Confirmation
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this faculty?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    using (SqlConnection con = new SqlConnection(
                        @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                    {
                        con.Open();

                        string query = "DELETE FROM Faculty WHERE FacultyID=@FacultyID";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@FacultyID", txtFacultyID.Text);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Faculty Deleted Successfully ✅");

                            foreach (Form frm in Application.OpenForms)
                            {
                                if (frm is Dashboard dashboard)
                                {
                                    dashboard.LoadStatistics();
                                }
                            }
                            LoadFaculty();       // 🔄 refresh grid
                            LoadFacultyCount(); // 🔢 update count
                            ClearFields();      // 🧹 clear form
                        }
                        else
                        {
                            MessageBox.Show("Record not found ❌");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
      
            try
            {
                // 🔹 TextBoxes clear
                txtFacultyID.Clear();
                txtFirstName.Clear();
                txtLastName.Clear();
                txtSalary.Clear();
                txtPhone.Clear();
                txtEmail.Clear();
                txtAddress.Clear();
                txtQualification.Clear();

                // 🔹 ComboBox reset
                cmbModule.SelectedIndex = -1;

                // 🔹 RadioButton reset
                radioMale.Checked = false;
                radioFemale.Checked = false;

                // 🔹 Date reset
                dtpDOB.Value = DateTime.Now;
                dtpJoinDate.Value = DateTime.Now;

                // 🔹 Image clear
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }

                // 🔹 Cursor focus (user-friendly)
                txtFirstName.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
         
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            // ID
            txtFacultyID.Text = row.Cells["FacultyID"].Value?.ToString();

            // Basic Info
            txtFirstName.Text = row.Cells["FirstName"].Value?.ToString();
            txtLastName.Text = row.Cells["LastName"].Value?.ToString();
            txtSalary.Text = row.Cells["Salary"].Value?.ToString();
            txtPhone.Text = row.Cells["Phone"].Value?.ToString();
            txtEmail.Text = row.Cells["Email"].Value?.ToString();
            txtAddress.Text = row.Cells["Address"].Value?.ToString();

            // Gender
            string gender = row.Cells["Gender"].Value?.ToString();
            if (gender == "Male")
                radioMale.Checked = true;
            else if (gender == "Female")
                radioFemale.Checked = true;

            // Module
            cmbModule.Text = row.Cells["Department"].Value?.ToString();

            // Date
            if (DateTime.TryParse(row.Cells["DOB"].Value?.ToString(), out DateTime dob))
            {
                dtpDOB.Value = dob;
            }

            // Photo
            if (row.Cells["Photo"].Value != DBNull.Value)
            {
                byte[] img = (byte[])row.Cells["Photo"].Value;

                using (MemoryStream ms = new MemoryStream(img))
                {
                    pictureBox1.Image = Image.FromStream(ms);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            else
            {
                pictureBox1.Image = null;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search By Faculty Name")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
            {
                txtSearch.Text = "Search By Faculty Name";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Agar placeholder ho ya empty ho → pura data load karo
                if (txtSearch.Text == "" || txtSearch.Text == "Search By Faculty Name")
                {
                    LoadFaculty();
                    return;
                }

                if (txtSearch.Text == "")
                {
                    LoadFaculty(); // jo maine pehle diya tha
                }
                string query = "SELECT * FROM Faculty WHERE " +
                               "(FirstName + ' ' + LastName) LIKE @name";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@name", "%" + txtSearch.Text + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dataGridView1.Columns["Salary"].DefaultCellStyle.Format = "₹#,##0";
            dataGridView1.Columns["Email"].Width = 200;
            dataGridView1.Columns["Address"].Width = 150;

        }

        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtFacultyID.Text = row.Cells["FacultyID"].Value?.ToString();
                txtFirstName.Text = row.Cells["FirstName"].Value?.ToString();
                txtLastName.Text = row.Cells["LastName"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                txtPhone.Text = row.Cells["Phone"].Value?.ToString();
                txtSalary.Text = row.Cells["Salary"].Value?.ToString();
                txtAddress.Text = row.Cells["Address"].Value?.ToString();
                txtQualification.Text = row.Cells["Qualification"].Value?.ToString();
                cmbModule.Text = row.Cells["Department"].Value?.ToString();

                // 👇 Gender
                if (row.Cells["Gender"].Value.ToString() == "Male")
                    radioMale.Checked = true;
                else
                    radioFemale.Checked = true;

                // 👇 Date
                dtpDOB.Value = Convert.ToDateTime(row.Cells["DOB"].Value);

                // 👇 PHOTO LOAD (IMPORTANT 🔥)
                if (row.Cells["Photo"].Value != DBNull.Value)
                {
                    byte[] img = (byte[])row.Cells["Photo"].Value;
                    MemoryStream ms = new MemoryStream(img);
                    pictureBox1.Image = Image.FromStream(ms);
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
        }

        private void txtSearch_TextChanged_1(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search By Faculty Name")
                return;

            string search = txtSearch.Text.Trim();

            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM Faculty WHERE FirstName LIKE @name OR LastName LIKE @name", con);

            da.SelectCommand.Parameters.AddWithValue("@name", "%" + search + "%");

            DataTable dt = new DataTable();
            da.Fill(dt);

            dataGridView1.DataSource = dt;
        }

        private void txtSearch_Enter_1(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search By Faculty Name")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search By Faculty Name";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))            
                e.Handled = true; // non-digit input block            
        }

        private void txtSalary_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true; // non-digit and non-dot input block
        }

        private void txtSalary_TextChanged(object sender, EventArgs e)
        {
            txtSalary.Text = System.Text.RegularExpressions.Regex.Replace(txtSalary.Text, @"[^0-9.]", ""); // non-digit and non-dot remove
        }

        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
            txtPhone.Text = System.Text.RegularExpressions.Regex.Replace(txtPhone.Text, @"[^0-9]", ""); // non-digit remove
        }
    }          
}