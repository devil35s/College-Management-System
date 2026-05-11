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
    public partial class BatchAttendanceForm : Form
    {
        public BatchAttendanceForm()
        {
            InitializeComponent();
        }

        
        private void BatchAttendanceForm_Load(object sender, EventArgs e)
        {
            LoadClasses();
            dgvAttendance.EnableHeadersVisualStyles = false;

            dgvAttendance.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(33, 150, 243);   // Dark Blue

            dgvAttendance.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            dgvAttendance.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);
            
            dgvAttendance.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            
            if (dtpDate.Value.Date > DateTime.Today)
{
    MessageBox.Show("Future date allowed nahi hai.");
    return;
}

            if (dtpDate.Value.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("Sunday is Holiday!");
                return;
            }

        }
        private void LoadClasses()
        {
            try
            {
                DBConnection.con.Open();

                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT DISTINCT Class FROM Student",
                    DBConnection.con
                );

                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbClass.DataSource = dt;
                cmbClass.DisplayMember = "Class";
                cmbClass.ValueMember = "Class";

                DBConnection.con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnLoadStudents_Click(object sender, EventArgs e)
        {

            try
            {
                DBConnection.con.Open();

                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT StudentId, Name FROM Student WHERE Class=@class",
                    DBConnection.con
                );

                da.SelectCommand.Parameters.AddWithValue("@class", cmbClass.Text);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvAttendance.DataSource = dt;

                DBConnection.con.Close();

                AddStatusColumn();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void AddStatusColumn()
        {
            if (!dgvAttendance.Columns.Contains("Status"))
            {
                DataGridViewComboBoxColumn statusColumn = new DataGridViewComboBoxColumn();
                statusColumn.HeaderText = "Status";
                statusColumn.Name = "Status";
                statusColumn.Items.Add("Present");
                statusColumn.Items.Add("Absent");

                dgvAttendance.Columns.Add(statusColumn);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            try
            {
                DateTime selectedDate = dtpDate.Value.Date;

                // ❌ Future Date Check
                if (selectedDate > DateTime.Today)
                {
                    MessageBox.Show("Future date attendance not allowed!");
                    return;
                }

                // ❌ Sunday Check
                if (selectedDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    MessageBox.Show("Sunday is a holiday. Attendance not allowed!");
                    return;
                }

                using (SqlConnection con = new SqlConnection(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    foreach (DataGridViewRow row in dgvAttendance.Rows)
                    {
                        if (row.Cells["StudentId"].Value != null)
                        {
                            int studentId = Convert.ToInt32(row.Cells["StudentId"].Value);
                            string status = row.Cells["Status"].Value?.ToString();

                            SqlCommand cmd = new SqlCommand(
                                @"IF EXISTS (SELECT 1 FROM Attendance 
                                     WHERE StudentId=@sid AND AttendanceDate=@date)
                          UPDATE Attendance 
                          SET Status=@status 
                          WHERE StudentId=@sid AND AttendanceDate=@date
                          ELSE
                          INSERT INTO Attendance(StudentId, AttendanceDate, Status)
                          VALUES(@sid,@date,@status)",
                                con);

                            cmd.Parameters.AddWithValue("@sid", studentId);
                            cmd.Parameters.AddWithValue("@date", selectedDate);
                            cmd.Parameters.AddWithValue("@status", status);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Attendance Saved Successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }


}