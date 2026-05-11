using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollegeManagementSystem
{
    public partial class Students : Form
    {
        private SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");
        public Students()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Flicker reduce ke liye
        }

        // Generate unique enrollment number
        private string GenerateEnrollmentNo()
        {
            string newEnrNo = "ENR001";

            using (SqlConnection con = new SqlConnection(
                @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                string query = "SELECT TOP 1 EnrollmentNo FROM Student ORDER BY StudentId DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string lastEnr = result.ToString(); // ENR005
                        int number = int.Parse(lastEnr.Substring(3)); // 5
                        number++;

                        newEnrNo = "ENR" + number.ToString("D3");
                    }
                }
            } // 👈 yaha auto close

            return newEnrNo;
        }

        private void ClearFields()
        {
            txtEnrollment.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            rbMale.Checked = false;
            rbFemale.Checked = false;
            cmbCourse.SelectedIndex = -1;
            cmbSemester.SelectedIndex = -1;
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            pictureBox1.Image = null;
            txtEnrollment.Focus();

        }

        private void btnUploadPhoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(fs); // ✅ FIX
                }

                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
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

                    // Agar Enrollment No. blank hai toh generate karo
                    if (string.IsNullOrWhiteSpace(txtEnrollment.Text))
                    {
                        txtEnrollment.Text = GenerateEnrollmentNo();
                    }

                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Student WHERE EnrollmentNo=@EnrollmentNo", con);

                    checkCmd.Parameters.AddWithValue("@EnrollmentNo", txtEnrollment.Text);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Enrollment already exists!");
                        return;
                    }
                    // 🔹 Photo Handling

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


                    // 🔹 Gender Check
                    string gender = "";
                    if (rbMale.Checked)
                        gender = "Male";
                    else if (rbFemale.Checked)
                        gender = "Female";

                    // 🔹 Insert Query
                    string query = @"INSERT INTO Student
                            (EnrollmentNo, FirstName, LastName, Gender,
                             DateOfBirth, Course, Semester,
                             Phone, Email, Address, Status, Photo)
                             VALUES
                            (@EnrollmentNo, @FirstName, @LastName, @Gender,
                             @DateOfBirth, @Course, @Semester,
                             @Phone, @Email, @Address, 'Active', @Photo)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    // 🔹 Parameters
                    cmd.Parameters.AddWithValue("@EnrollmentNo", txtEnrollment.Text);
                    cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                    cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@DateOfBirth", dtpDOB.Value.Date);
                    cmd.Parameters.AddWithValue("@Course", cmbCourse.Text);
                    cmd.Parameters.AddWithValue("@Semester", cmbSemester.Text);
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.Add("@Photo", SqlDbType.VarBinary).Value = (object)imageBytes ?? DBNull.Value;
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Student Saved Successfully!");

                    //Dashboard Auto Refresh
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm is Dashboard dashboard)
                        {
                            dashboard.LoadStatistics();
                        }
                    }
                    LoadStudents();   // Grid refresh
                    LoadCourses();
                    LoadSemesters();
                    LoadStatus();
                    ClearFields();    // Fields clear
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    // 🔹 Photo Handling
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

                    // 🔹 Gender Check
                    string gender = "";
                    if (rbMale.Checked)
                    {
                        gender = "Male";
                    }
                    else if (rbFemale.Checked)
                    {
                        gender = "Female";
                    }

                    // 🔹 Update Query
                    string query;

                    if (imageBytes != null)
                    {
                        query = @"UPDATE Student
                        SET FirstName=@FirstName,
                        LastName=@LastName,
                        Gender=@Gender,
                        DateOfBirth=@DateOfBirth,
                        Course=@Course,
                        Semester=@Semester,
                        Phone=@Phone,
                        Email=@Email,
                        Address=@Address,
                        Photo=@photo
                        WHERE EnrollmentNo=@EnrollmentNo";
                    }
                    else
                    {
                        query = @"UPDATE Student
                        SET FirstName=@FirstName,
                        LastName=@LastName,
                        Gender=@Gender,
                        DateOfBirth=@DateOfBirth,
                        Course=@Course,
                        Semester=@Semester,
                        Phone=@Phone,
                        Email=@Email,
                        Address=@Address
                        WHERE EnrollmentNo=@EnrollmentNo";
                    }

                    SqlCommand cmd = new SqlCommand(query, con);

                    // 🔹 Parameters
                    cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                    cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@DateOfBirth", dtpDOB.Value.Date);
                    cmd.Parameters.AddWithValue("@Course", cmbCourse.Text);
                    cmd.Parameters.AddWithValue("@Semester", cmbSemester.Text);
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@EnrollmentNo", txtEnrollment.Text);
                    if (imageBytes != null)
                    {
                        cmd.Parameters.Add("@photo", SqlDbType.VarBinary).Value = imageBytes;
                    }
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Student Updated Successfully!");

                    LoadStudents(); // Grid Refresh
                    LoadCourses();
                    LoadSemesters();
                    LoadStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                string query = @"SELECT 
                        StudentId,
                        EnrollmentNo,
                        FirstName,
                        LastName,
                        FirstName + ' ' + LastName AS FullName,
                        Gender,
                        DateOfBirth,
                        Course,
                        Semester,
                        Phone,
                        Email,
                        Address,
                        Status,
                        Photo
                        FROM Student
                        WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(txtSearch.Text) && txtSearch.Text != "Search Name / Enrollment No.")
                {
                    query += " AND (FirstName + ' ' + LastName LIKE @search)";
                }

                if (!string.IsNullOrWhiteSpace(cmbCourseFilter.Text) && cmbCourseFilter.Text != "All Courses")
                {
                    query += " AND Course = @course";
                }

                if (!string.IsNullOrWhiteSpace(cmbSemesterFilter.Text) && cmbSemesterFilter.Text != "All Semesters")
                {
                    query += " AND Semester = @semester";
                }

                if (!string.IsNullOrWhiteSpace(cmbStatusFilter.Text) && cmbStatusFilter.Text != "All Status")
                {
                    query += " AND Status = @status";
                }

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text + "%");

                if (!string.IsNullOrWhiteSpace(cmbCourseFilter.Text))
                    cmd.Parameters.AddWithValue("@course", cmbCourseFilter.Text);

                if (!string.IsNullOrWhiteSpace(cmbSemesterFilter.Text))
                    cmd.Parameters.AddWithValue("@semester", cmbSemesterFilter.Text);

                if (!string.IsNullOrWhiteSpace(cmbStatusFilter.Text))
                    cmd.Parameters.AddWithValue("@status", cmbStatusFilter.Text);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
                lblTotalStudents.Text = "Total Students: " + dt.Rows.Count;
            }
        }

        private void LoadStudents()
        {


            using (SqlConnection con = new SqlConnection(
                @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                

                string query = @"SELECT 
                        StudentId,
                        EnrollmentNo,
                        FirstName,
                        LastName,
                        FirstName + ' ' + LastName AS FullName,
                        Gender,
                        DateOfBirth,
                        Course,
                        Semester,
                        Phone,
                        Email,
                        Address,
                        Status,
                        Photo
                        FROM Student";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // IMPORTANT – Clear old columns
                dataGridView1.DataSource = null;
                dataGridView1.Columns.Clear();

                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = dt;

                if (dataGridView1.Columns.Contains("FirstName"))
                    dataGridView1.Columns["FirstName"].Visible = false;

                if (dataGridView1.Columns.Contains("LastName"))
                    dataGridView1.Columns["LastName"].Visible = false;

                // Hide Photo column
                if (dataGridView1.Columns.Contains("Photo"))
                    dataGridView1.Columns["Photo"].Visible = false;

                // Layout Settings
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dataGridView1.RowHeadersVisible = false;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                // Hide ID column
                if (dataGridView1.Columns.Contains("StudentId"))
                    dataGridView1.Columns["StudentId"].Visible = false;

                // Date Format
                if (dataGridView1.Columns.Contains("DateOfBirth"))
                    dataGridView1.Columns["DateOfBirth"].DefaultCellStyle.Format = "dd-MMM-yyyy";

                dataGridView1.Columns["EnrollmentNo"].HeaderText = "Enrollment No.";
                dataGridView1.Columns["FullName"].HeaderText = "Full Name";
                dataGridView1.Columns["DateOfBirth"].HeaderText = "Date of Birth";
                dataGridView1.ColumnHeadersHeight = 40;
                lblTotalStudents.Text = "Total Students: " + dt.Rows.Count;
            }

        }

        
        private void LoadCourses()
        {

            using (SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT DISTINCT Course FROM Student", con);
                SqlDataReader reader = cmd.ExecuteReader();

                cmbCourseFilter.Items.Clear();
                cmbCourseFilter.Items.Add("All Courses");

                while (reader.Read())
                {
                    cmbCourseFilter.Items.Add(reader["Course"].ToString());
                }

                cmbCourseFilter.SelectedIndex = 0; // Default = All
            }

        }
        private void LoadSemesters()
        {

            using (SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT DISTINCT Semester FROM Student", con);
                SqlDataReader reader = cmd.ExecuteReader();

                cmbSemesterFilter.Items.Clear();
                cmbSemesterFilter.Items.Add("All Semesters");

                while (reader.Read())
                {
                    cmbSemesterFilter.Items.Add(reader["Semester"].ToString());
                }

                cmbSemesterFilter.SelectedIndex = 0;
            }

        }
        private void LoadStatus()
        {

            using (SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                string query = "SELECT DISTINCT Status FROM Student";

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                cmbStatusFilter.Items.Clear();
                cmbStatusFilter.Items.Add("All Status");

                while (reader.Read())
                {
                    if (reader["Status"] != DBNull.Value)
                    {
                        cmbStatusFilter.Items.Add(reader["Status"].ToString());
                    }
                }

                cmbStatusFilter.SelectedIndex = 0;
            }

        }

        private void Students_Load(object sender, EventArgs e)
        {
            LoadStudents();
            LoadCourses();
            LoadSemesters();
            LoadStatus();
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Text = "Search Name / Enrollment No.";

            
            // Modern UI Styling
            StyleDataGridView();

            dataGridView1.Columns["Email"].FillWeight = 150;
            dataGridView1.Columns["Address"].FillWeight = 200;
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
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ScrollBars = ScrollBars.Both;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);

            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView1.RowTemplate.Height = 35;
            dataGridView1.DefaultCellStyle.Padding = new Padding(4);
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            // REMOVE THIS
            // dataGridView1.RowTemplate.Height = 40;
            
            

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    string query = "UPDATE Student SET Status='Inactive' WHERE EnrollmentNo=@EnrollmentNo";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@EnrollmentNo", txtEnrollment.Text);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Student Deactivated Successfully!");

                    LoadStudents();  // Grid Refresh
                    ClearFields();
                }
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


            if (dataGridView1.CurrentRow == null) return;

            txtEnrollment.Text = dataGridView1.CurrentRow.Cells["EnrollmentNo"].Value?.ToString();
            txtFirstName.Text = dataGridView1.CurrentRow.Cells["FirstName"].Value?.ToString();
            txtLastName.Text = dataGridView1.CurrentRow.Cells["LastName"].Value?.ToString();
            txtPhone.Text = dataGridView1.CurrentRow.Cells["Phone"].Value?.ToString();
            txtEmail.Text = dataGridView1.CurrentRow.Cells["Email"].Value?.ToString();
            txtAddress.Text = dataGridView1.CurrentRow.Cells["Address"].Value?.ToString();
            // Gender
            string gender = dataGridView1.CurrentRow.Cells["Gender"].Value?.ToString();
            if (gender == "Male") rbMale.Checked = true;
            else if (gender == "Female") rbFemale.Checked = true;

            // Course & Semester
            cmbCourse.Text = dataGridView1.CurrentRow.Cells["Course"].Value?.ToString();
            cmbSemester.Text = dataGridView1.CurrentRow.Cells["Semester"].Value?.ToString();

            // Date of Birth
            if (DateTime.TryParse(
                dataGridView1.CurrentRow.Cells["DateOfBirth"].Value?.ToString(),
                out DateTime dob))
            {
                dtpDOB.Value = dob;
            }
        }

        private void cmbCourseFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearch_TextChanged(null, null);
        }

        private void cmbSemesterFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearch_TextChanged(null, null);
        }

        private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearch_TextChanged(null, null);
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search Name / Enrollment No.")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search Name / Enrollment No.";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void dataGridView1_CellFormatting_1(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Status")
            {
                if (e.Value != null)
                {
                    if (e.Value.ToString() == "Inactive")
                    {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                    }
                    else if (e.Value.ToString() == "Active")
                    {
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                    }
                }
            }
        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}