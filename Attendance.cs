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
    public partial class Attendance : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");
        public Attendance()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime selectedDate = dtpDate.Value.Date;

                if (IsHoliday(selectedDate))
                {
                    MessageBox.Show("Today is Holiday 🎉\nAttendance not required");

                    dataGridView1.Rows.Clear(); // optional

                    return;
                }
                dataGridView1.Rows.Clear();

                string query = "SELECT StudentId, EnrollmentNo, FirstName + ' ' + LastName AS Name FROM Student WHERE 1=1";

                // ✅ Course filter
                if (cmbCourse.Text != "All")
                {
                    query += " AND Course = @course";
                }

                // ✅ Semester filter
                if (cmbSemester.Text != "All")
                {
                    query += " AND Semester = @sem";
                }

                SqlCommand cmd = new SqlCommand(query, con);

                if (cmbCourse.Text != "All")
                {
                    cmd.Parameters.AddWithValue("@course", cmbCourse.Text);
                }

                if (cmbSemester.Text != "All")
                {
                    cmd.Parameters.AddWithValue("@sem", cmbSemester.Text);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        dataGridView1.Rows.Add(
                            row["StudentId"],
                            row["EnrollmentNo"],
                            row["Name"],
                            false, // Present
                            false  // Absent
                        );
                    }
                }
                else
                {
                    MessageBox.Show("No students found");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Load courses into combo box
        private void LoadCourse()
        {
            cmbCourse.Items.Clear();
            cmbCourseReport.Items.Clear();

            cmbCourse.Items.Add("All");
            cmbCourseReport.Items.Add("All");

            SqlCommand cmd = new SqlCommand("SELECT DISTINCT Course FROM Student", con);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                string course = dr["Course"].ToString();

                cmbCourse.Items.Add(course);         // Left side
                cmbCourseReport.Items.Add(course);   // Right side 🔥
            }

            con.Close();

            if (cmbCourse.Items.Count > 0)
                cmbCourse.SelectedIndex = 0;

            if (cmbCourseReport.Items.Count > 0)
                cmbCourseReport.SelectedIndex = 0; // 🔥
        }

        // Load Semester values into cmbSemester
        private void LoadSemester()
        {
            cmbSemester.Items.Clear();
            cmbSemesterReport.Items.Clear();

            cmbSemester.Items.Add("All");
            cmbSemesterReport.Items.Add("All");

            SqlCommand cmd = new SqlCommand("SELECT DISTINCT Semester FROM Student", con);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                string sem = dr["Semester"].ToString();

                cmbSemester.Items.Add(sem);         // Left
                cmbSemesterReport.Items.Add(sem);   // Right 🔥
            }

            con.Close();

            if (cmbSemester.Items.Count > 0)
                cmbSemester.SelectedIndex = 0;

            if (cmbSemesterReport.Items.Count > 0)
                cmbSemesterReport.SelectedIndex = 0; // 🔥
        }

        // Setup DataGridView columns
        private void SetupGrid()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("StudentId", "ID");
            dataGridView1.Columns["StudentId"].Visible = false;

            dataGridView1.Columns.Add("EnrollmentNo", "Enrollment No");
            dataGridView1.Columns.Add("Name", "Student Name");

            // Present checkbox
            DataGridViewCheckBoxColumn present = new DataGridViewCheckBoxColumn();
            present.HeaderText = "Present";
            present.Name = "Present";
            dataGridView1.Columns.Add(present);

            // Absent checkbox
            DataGridViewCheckBoxColumn absent = new DataGridViewCheckBoxColumn();
            absent.HeaderText = "Absent";
            absent.Name = "Absent";
            dataGridView1.Columns.Add(absent);

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AllowUserToAddRows = false;
        }

        // Load dashboard stats
        private void LoadDashboard()
        {
            SqlCommand cmd = new SqlCommand(
            "SELECT " +
            "COUNT(*) AS TotalStudents, " +
            "SUM(CASE WHEN status='Present' THEN 1 ELSE 0 END) AS PresentCount, " +
            "SUM(CASE WHEN status='Absent' THEN 1 ELSE 0 END) AS AbsentCount " +
            "FROM Attendance WHERE CAST(AttendanceDate AS DATE)=@date", con);

            cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int total = dr["TotalStudents"] == DBNull.Value ? 0 : Convert.ToInt32(dr["TotalStudents"]);
                int present = dr["PresentCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["PresentCount"]);
                int absent = dr["AbsentCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["AbsentCount"]);

                // Labels update
                lblTotal.Text = total.ToString();
                lblPresent.Text = present.ToString();
                lblAbsent.Text = absent.ToString();

                // Percentage
                double percent = total == 0 ? 0 : (present * 100.0 / total);
                lblPercent.Text = percent.ToString("0.00") + " %";
            }

            con.Close();
        }
        private void Attendance_Load(object sender, EventArgs e)
        {
            LoadEvents();
            LoadCourse();
            LoadSemester();
            SetupGrid();
            SetupGid(dataGridView1);
            SetupGid(dataGridView2);
            SetupGid(dataGridView3);
            LoadDashboard();

            // Event type setup
            cmbType.Items.Add("Holiday");
            cmbType.Items.Add("Event");
            cmbType.Items.Add("Exam");
            cmbType.SelectedIndex = 0;

        }

        private void SetupGid(DataGridView dgv)
        {
            // 🔹 Basic
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Color.White;
            dgv.EnableHeadersVisualStyles = false;

            // 🔹 Header Design
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(10, 44, 92);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            // 🔹 Rows
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 167, 69);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            // 🔹 Alternate Rows
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // 🔹 Grid Style
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.LightGray;

            // 🔹 Size
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 30;

            // 🔹 Remove extras
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;

            // 🔹 Selection full row
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex < 0) return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Present")
            {
                dataGridView1.Rows[e.RowIndex].Cells["Absent"].Value = false;
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "Absent")
            {
                dataGridView1.Rows[e.RowIndex].Cells["Present"].Value = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsHoliday(dtpDate.Value.Date))
                {
                    MessageBox.Show("Today is Holiday");
                    return;
                }
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["StudentId"].Value == null) continue;

                    int studentId = Convert.ToInt32(row.Cells["StudentId"].Value);

                    bool isPresent = Convert.ToBoolean(row.Cells["Present"].Value);
                    string status = isPresent ? "Present" : "Absent";

                    SqlCommand cmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM Attendance WHERE student_id=@sid AND CAST(AttendanceDate AS DATE)=@date) " +
                    "UPDATE Attendance SET status=@status WHERE student_id=@sid AND CAST(AttendanceDate AS DATE)=@date " +
                    "ELSE " +
                    "INSERT INTO Attendance(student_id, AttendanceDate, status) VALUES(@sid,@date,@status)",
                    con);

                    cmd.Parameters.AddWithValue("@sid", studentId);
                    cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date); // 🔥 FIX
                    cmd.Parameters.AddWithValue("@status", status);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                MessageBox.Show("Attendance Saved / Updated ✅");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            LoadDashboard();
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is Dashboard dashboard)
                {
                    dashboard.LoadStatistics();
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {

            // Grid clear
            dataGridView1.Rows.Clear();

            // ComboBox reset
            cmbCourse.SelectedIndex = -1;
            cmbSemester.SelectedIndex = -1;

            // Date reset (today)
            dtpDate.Value = DateTime.Now;

            // Optional: focus wapas course pe
            cmbCourse.Focus();
        }

        private void dtpDate_ValueChanged(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();

                string query = @"
                SELECT 
                s.EnrollmentNo,
                s.FirstName + ' ' + s.LastName AS Name,
                s.Course,
                s.Semester,
                COUNT(a.attendance_id) AS TotalDays,
                SUM(CASE WHEN a.status='Present' THEN 1 ELSE 0 END) AS PresentDays,
                CAST(
                    CASE 
                        WHEN COUNT(a.attendance_id)=0 THEN 0 
                        ELSE (SUM(CASE WHEN a.status='Present' THEN 1 ELSE 0 END)*100.0/COUNT(a.attendance_id)) 
                    END 
                AS DECIMAL(5,2)) AS Percentage
                FROM Student s
                LEFT JOIN Attendance a 
                ON s.StudentId = a.student_id 
                AND MONTH(a.AttendanceDate) = @month
                WHERE 1=1";

                // ✅ Course filter
                if (cmbCourseReport.Text != "All")
                {
                    query += " AND s.Course = @course";
                }

                // ✅ Semester filter
                if (cmbSemesterReport.Text != "All")
                {
                    query += " AND s.Semester = @sem";
                }

                query += @"
                GROUP BY 
                s.EnrollmentNo, 
                s.FirstName, 
                s.LastName,
                s.Course,
                s.Semester";

                SqlCommand cmd = new SqlCommand(query, con);

                // ✅ Month parameter
                cmd.Parameters.AddWithValue("@month", cmbMonth.SelectedIndex + 1);

                // ✅ Course parameter
                if (cmbCourseReport.Text != "All")
                {
                    cmd.Parameters.AddWithValue("@course", cmbCourseReport.Text);
                }

                // ✅ Semester parameter
                if (cmbSemesterReport.Text != "All")
                {
                    cmd.Parameters.AddWithValue("@sem", cmbSemesterReport.Text);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView2.DataSource = dt;

                dataGridView2.Columns["EnrollmentNo"].HeaderText = "Enrollment No";
                dataGridView2.Columns["Name"].HeaderText = "Student Name";

                con.Close();

                // ✅ Percentage format (100%)
                if (dataGridView2.Columns.Contains("Percentage"))
                {
                    dataGridView2.Columns["Percentage"].DefaultCellStyle.Format = "0.##'%'";
                }

                // ❌ No data check
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No data found");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Events (Title, EventDate, Type, Description) VALUES (@title, @date, @type, @desc)",
                    con);

                cmd.Parameters.AddWithValue("@title", txtTitle.Text);
                cmd.Parameters.Add("@date", SqlDbType.Date).Value = dtpEventDate.Value.Date; // ✅ FIX
                cmd.Parameters.AddWithValue("@type", cmbType.Text);
                cmd.Parameters.AddWithValue("@desc", txtDescription.Text);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Event Added Successfully ✅");

                LoadEvents();

                txtTitle.Clear();
                txtDescription.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
        private void LoadEvents()
        {
            string cs = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT EventId, Title, EventDate, Type, Description FROM Events WHERE EventDate >= GETDATE() ORDER BY EventDate ASC",
                    con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView3.DataSource = dt;
                dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView3.Columns["EventId"].Visible = false;
                dataGridView3.Columns["EventDate"].HeaderText = "Event Date";

            }
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            
            try
            {
                string cs = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";

                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Events (Title, EventDate, Type, Description) VALUES (@title, @date, @type, @desc)",
                        con);

                    // Title
                    cmd.Parameters.AddWithValue("@title", txtTitle.Text);

                    // ✅ DATE FIX (IMPORTANT)
                    cmd.Parameters.Add("@date", SqlDbType.Date).Value = dtpEventDate.Value.Date;

                    // Type
                    cmd.Parameters.AddWithValue("@type", cmbType.Text);

                    // ✅ Description
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Event Added Successfully ✅");

                LoadEvents(); // refresh grid
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
           
            if (selectedEventId == 0)
            {
                MessageBox.Show("Please select event to delete");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                string cs = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";

                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM Events WHERE EventId=@id", con);

                    cmd.Parameters.AddWithValue("@id", selectedEventId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                MessageBox.Show("Deleted Successfully");

                LoadEvents();

                // Clear form
                txtTitle.Clear();
                txtDescription.Clear();
                selectedEventId = 0;
            }
        }

        private bool IsHoliday(DateTime date)
        {
            bool isHoliday = false;

            string cs = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Events WHERE EventDate = @date AND Type='Holiday'",
                    con);

                cmd.Parameters.AddWithValue("@date", date.Date);

                con.Open();
                int count = (int)cmd.ExecuteScalar();
                con.Close();

                if (count > 0)
                    isHoliday = true;
            }

            return isHoliday;
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            
            if (selectedEventId == 0)
            {
                MessageBox.Show("Please select event first");
                return;
            }

            string cs = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Events SET Title=@title, EventDate=@date, Type=@type, Description=@desc WHERE EventId=@id",
                    con);

                cmd.Parameters.AddWithValue("@title", txtTitle.Text);
                cmd.Parameters.AddWithValue("@date", dtpEventDate.Value.Date);
                cmd.Parameters.AddWithValue("@type", cmbType.Text);
                cmd.Parameters.AddWithValue("@desc", txtDescription.Text);
                cmd.Parameters.AddWithValue("@id", selectedEventId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            MessageBox.Show("Updated Successfully");

            LoadEvents();

            txtTitle.Clear();
            txtDescription.Clear();
            selectedEventId = 0;
        }
        

        private int selectedEventId = 0;
        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView3.Rows[e.RowIndex];

                selectedEventId = Convert.ToInt32(row.Cells["EventId"].Value);

                txtTitle.Text = row.Cells["Title"].Value.ToString();
                dtpEventDate.Value = Convert.ToDateTime(row.Cells["EventDate"].Value);
                cmbType.Text = row.Cells["Type"].Value.ToString();
                txtDescription.Text = row.Cells["Description"].Value.ToString();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }
    }
    
    
}