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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CollegeManagementSystem
{
    public partial class SportsForm : Form
    {
        string conStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";
        public SportsForm()
        {
            InitializeComponent();
        }
        private void LoadCoaches()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "SELECT FacultyID, FirstName + ' ' + LastName AS Name FROM Faculty";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbCoach.DataSource = dt;
                cmbCoach.DisplayMember = "Name";     // UI me name dikhega
                cmbCoach.ValueMember = "FacultyID";  // internally ID store hoga
            }
        }
        private void LoadFacilities()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "SELECT FacilityID, FacilityName FROM Facilities";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbFacility.DataSource = dt;
                cmbFacility.DisplayMember = "FacilityName";
                cmbFacility.ValueMember = "FacilityID";
            }
        }
        private void LoadBookings()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = @"
                    SELECT 
                        b.BookingID,
                        b.BookingCode,
                        s.EnrollmentNo,
                        s.FirstName + ' ' + s.LastName AS StudentName,
                        f.FacilityName,
                        b.BookingDate,
                        b.TimeSlot,
                        b.Status,
                        b.Purpose
                    FROM FacilityBookings b
                    JOIN Student s ON b.StudentID = s.StudentID
                    JOIN Facilities f ON b.FacilityID = f.FacilityID
                    ORDER BY b.BookingDate DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView2.DataSource = dt;
                dataGridView2.Columns["BookingID"].Visible = false;
                dataGridView2.Columns["BookingCode"].HeaderText = "Booking ID";
                dataGridView2.Columns["StudentName"].HeaderText = "Student Name";
                dataGridView2.Columns["FacilityName"].HeaderText = "Facility Name";
                dataGridView2.Columns["BookingDate"].HeaderText = "Booking Date";
                dataGridView2.Columns["TimeSlot"].HeaderText = "Time Slot";
                dataGridView2.Columns["EnrollmentNo"].HeaderText = "Enrollment No";
                dataGridView2.Columns["Purpose"].HeaderText = "Purpose";
            }
        }
        private void LoadFacilityGrid(string search = "", string type = "All", string status = "All")
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = @"
                    SELECT 
                        f.FacilityID,
                        f.FacilityCode,
                        f.FacilityName,
                        f.Type,
                        f.Location,
                        f.Capacity,
                        f.Status,                        
                        fa.FirstName + ' ' + fa.LastName AS CoachName,
                        f.Equipment,
                        f.Remarks
                    FROM Facilities f
                    JOIN Faculty fa ON f.CoachID = fa.FacultyID";

                // 🔍 Search
                if (!string.IsNullOrEmpty(search) && search != "Search by Facility Name")
                {
                    query += " AND f.FacilityName LIKE @search";
                }

                // 🎯 Type Filter
                if (type != "All Types")
                {
                    query += " AND f.Type = @type";
                }

                // 🎯 Status Filter
                if (status != "All Status")
                {
                    query += " AND f.Status = @status";
                }

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(search) && search != "Search by Facility Name")
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                if (type != "All Types")
                    cmd.Parameters.AddWithValue("@type", type);

                if (status != "All Status")
                    cmd.Parameters.AddWithValue("@status", status);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridViewFacility.DataSource = dt;
                dataGridViewFacility.Columns["FacilityID"].Visible = false;
                dataGridViewFacility.Columns["FacilityCode"].HeaderText = "Facility ID";
                dataGridViewFacility.Columns["FacilityName"].HeaderText = "Facility Name";
                dataGridViewFacility.Columns["CoachName"].HeaderText = "Coach / Incharge";
            }            
        }
        private void LoadDashboardData()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();

                // 🔥 Total Facilities
                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Facilities", con);
                lblTotalFacilities.Text = cmdTotal.ExecuteScalar().ToString();

                // ✅ Available
                SqlCommand cmdAvailable = new SqlCommand("SELECT COUNT(*) FROM Facilities WHERE Status = 'Available'", con);
                lblAvailableFacilities.Text = cmdAvailable.ExecuteScalar().ToString();

                // 📅 Booked
                SqlCommand cmdBooked = new SqlCommand("SELECT COUNT(*) FROM Facilities WHERE Status = 'Booked'", con);
                lblBookedFacilities.Text = cmdBooked.ExecuteScalar().ToString();

                // 🛠 Maintenance
                SqlCommand cmdMaintenance = new SqlCommand("SELECT COUNT(*) FROM Facilities WHERE Status = 'Maintenance'", con);
                lblMaintenanceFacilities.Text = cmdMaintenance.ExecuteScalar().ToString();
            }
        }
        private void SportsForm_Load(object sender, EventArgs e)
        {
            LoadCoaches();
            LoadFacilityGrid(txtSearch.Text, cmbFilterType.Text, cmbFilterStatus.Text);
            LoadDashboardData();
            txtFacilityID.Text = GenerateFacilityCode(); // Auto-generate Facility Code for new entries
            LoadFacilities();
            LoadBookings();            
            LoadFacilityNames();
            StyleDataGridView();
            StyleDataGridView2();

            // 🔥 Combo values
            cmbType.Items.Clear();
            cmbType.Items.Add("Indoor");
            cmbType.Items.Add("Outdoor");

            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Available");
            cmbStatus.Items.Add("Maintenance");
            cmbStatus.Items.Add("Booked");

            txtSearch.Text = "Search by Facility Name";
            cmbFilterStatus.SelectedIndex=0; // All Status
            cmbFilterType.SelectedIndex=0; // All Types
        }

        private void StyleDataGridView2()
        {
            dataGridViewFacility.BorderStyle = BorderStyle.None;
            dataGridViewFacility.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dataGridViewFacility.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewFacility.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dataGridViewFacility.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridViewFacility.BackgroundColor = Color.White;

            dataGridViewFacility.EnableHeadersVisualStyles = false;
            dataGridViewFacility.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dataGridViewFacility.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridViewFacility.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            dataGridViewFacility.ColumnHeadersHeight = 40;

            dataGridViewFacility.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dataGridViewFacility.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dataGridViewFacility.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dataGridViewFacility.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dataGridViewFacility.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dataGridViewFacility.RowTemplate.Height = 35;
            dataGridViewFacility.ReadOnly = true;
            dataGridViewFacility.AllowUserToAddRows = false;
            dataGridViewFacility.AllowUserToDeleteRows = false;
            dataGridViewFacility.AllowUserToResizeRows = false;

            dataGridViewFacility.BackgroundColor = System.Drawing.Color.White;
            dataGridViewFacility.BorderStyle = BorderStyle.None;
            dataGridViewFacility.GridColor = System.Drawing.Color.Gainsboro;
            dataGridViewFacility.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewFacility.ScrollBars = ScrollBars.Both;

            dataGridViewFacility.RowHeadersVisible = false;
            dataGridViewFacility.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridViewFacility.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dataGridViewFacility.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            dataGridViewFacility.RowTemplate.Height = 40;
            dataGridViewFacility.ColumnHeadersHeight = 45;

        }
        private void StyleDataGridView()
        {
            dataGridView2.BorderStyle = BorderStyle.None;
            dataGridView2.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView2.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dataGridView2.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView2.BackgroundColor = Color.White;

            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView2.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            dataGridView2.ColumnHeadersHeight = 40;

            dataGridView2.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dataGridView2.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dataGridView2.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dataGridView2.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dataGridView2.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dataGridView2.RowTemplate.Height = 35;
            dataGridView2.ReadOnly = true;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.AllowUserToResizeRows = false;

            dataGridView2.BackgroundColor = System.Drawing.Color.White;
            dataGridView2.BorderStyle = BorderStyle.None;
            dataGridView2.GridColor = System.Drawing.Color.Gainsboro;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ScrollBars = ScrollBars.Both;

            dataGridView2.RowHeadersVisible = false;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView2.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dataGridView2.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            dataGridView2.RowTemplate.Height = 40;
            dataGridView2.ColumnHeadersHeight = 45;

        }

        private void LoadFacilityNames()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "SELECT FacilityID, FacilityName FROM Facilities";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbFacility.DataSource = dt;
                cmbFacility.DisplayMember = "FacilityName"; // user ko kya dikhega
                cmbFacility.ValueMember = "FacilityID";     // hidden ID
            }
        }
        private string GenerateBookingID()
        {
            string newID = "BK-0001";

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "SELECT TOP 1 BookingCode FROM FacilityBookings ORDER BY BookingCode DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    string lastID = result.ToString(); // BK-0005

                    int num = int.Parse(lastID.Substring(3)); // 5
                    num++;

                    newID = "BK-" + num.ToString("D4"); // BK-0006
                }
            }

            return newID;
        }

        private void txtEnrollmentNo_TextChanged(object sender, EventArgs e)
        {            
            if (txtEnrollmentNo.Text.Trim() == "") return;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "SELECT StudentID, FirstName + ' ' + LastName AS Name FROM Student WHERE EnrollmentNo=@enr";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@enr", txtEnrollmentNo.Text);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    txtStudentName.Text = dr["Name"].ToString();
                    txtStudentName.Tag = dr["StudentID"]; // 🔥 ID yahan store
                }
                else
                {
                    txtStudentName.Text = "";
                    txtStudentName.Tag = null;
                }
            }
        }
        private string GenerateFacilityCode()
        {
            string newID = "FS-0001";

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "SELECT TOP 1 FacilityCode FROM Facilities ORDER BY FacilityID DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result.ToString() != "")
                {
                    string lastID = result.ToString();
                    int num = int.Parse(lastID.Substring(3));
                    num++;

                    newID = "FS-" + num.ToString("D4");
                }
            }

            return newID;
        }

        private void btnBook_Click(object sender, EventArgs e)
        {
            if (txtStudentName.Tag == null)
            {
                MessageBox.Show("Invalid Student ❌");
                return;
            }

            string bookingCode = GenerateBookingID(); // BK-0001

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();

                // 🔥 Conflict check
                string checkQuery = @"SELECT COUNT(*) FROM FacilityBookings 
                             WHERE FacilityID=@fid 
                             AND BookingDate=@date 
                             AND TimeSlot=@time";

                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@fid", cmbFacility.SelectedValue);
                checkCmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                checkCmd.Parameters.AddWithValue("@time", cmbTimeSlot.Text);

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("Already Booked ❌");
                    return;
                }

                // ✅ Insert
                string query = @"INSERT INTO FacilityBookings
        (BookingCode, StudentID, FacilityID, BookingDate, TimeSlot, Purpose, Status)
        VALUES (@bcode,@sid,@fid,@date,@time,@purpose,@status)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@bcode", bookingCode);
                cmd.Parameters.AddWithValue("@sid", txtStudentName.Tag);
                cmd.Parameters.AddWithValue("@fid", cmbFacility.SelectedValue);
                cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                cmd.Parameters.AddWithValue("@time", cmbTimeSlot.Text);
                cmd.Parameters.AddWithValue("@purpose", txtPurpose.Text);
                cmd.Parameters.AddWithValue("@status", "Confirmed");

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Booking Successful ✅");

            ClearBooking();
            LoadBookings();
        }
        private void ClearFacility()
        {
            txtFacilityID.Text = GenerateFacilityCode();
            txtFacilityName.Clear();
            txtLocation.Clear();
            txtCapacity.Clear();
            txtEquipment.Clear();
            txtRemarks.Clear();

            cmbType.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            cmbCoach.SelectedIndex = -1;

            txtFacilityName.Focus();
        }
        private void btnSaveFacility_Click(object sender, EventArgs e)
        {            
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = @"INSERT INTO Facilities
        (FacilityCode, FacilityName, Type, Location, Capacity, Equipment, Status, CoachID, Remarks)
        VALUES (@code,@name,@type,@loc,@cap,@eq,@status,@coach,@rem)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@code", txtFacilityID.Text);
                cmd.Parameters.AddWithValue("@name", txtFacilityName.Text);
                cmd.Parameters.AddWithValue("@type", cmbType.Text);
                cmd.Parameters.AddWithValue("@loc", txtLocation.Text);
                cmd.Parameters.AddWithValue("@cap", txtCapacity.Text);
                cmd.Parameters.AddWithValue("@eq", txtEquipment.Text);
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@coach", cmbCoach.SelectedValue);
                cmd.Parameters.AddWithValue("@rem", txtRemarks.Text);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Facility Saved ✅");
            LoadFacilityGrid(txtSearch.Text, cmbFilterType.Text, cmbFilterStatus.Text);
            ClearFacility();
            LoadFacilities();
            LoadDashboardData();
            txtFacilityID.Text = GenerateFacilityCode(); // Prepare for next entry

        }

        private void dataGridViewFacility_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex >= 0)
            {
                var row = dataGridViewFacility.Rows[e.RowIndex];

                txtFacilityID.Text = row.Cells["FacilityCode"].Value.ToString();
                txtFacilityName.Text = row.Cells["FacilityName"].Value.ToString();
                cmbType.Text = row.Cells["Type"].Value.ToString();
                txtLocation.Text = row.Cells["Location"].Value.ToString();
                txtCapacity.Text = row.Cells["Capacity"].Value.ToString();
                cmbStatus.Text = row.Cells["Status"].Value.ToString();
                txtRemarks.Text = row.Cells["Remarks"].Value.ToString();
                txtEquipment.Text = row.Cells["Equipment"].Value.ToString();
            }
        }

        private void btnUpdateFacility_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = @"UPDATE Facilities SET
                                        FacilityName=@name,
                                        Type=@type,
                                        Location=@loc,
                                        Capacity=@cap,
                                        Equipment=@eq,
                                        Status=@status,
                                        CoachID=@coach,
                                        Remarks=@rem
                                        WHERE FacilityCode=@code"; // 🔥 important

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@code", txtFacilityID.Text); // FS-0001
                cmd.Parameters.AddWithValue("@name", txtFacilityName.Text);
                cmd.Parameters.AddWithValue("@type", cmbType.Text);
                cmd.Parameters.AddWithValue("@loc", txtLocation.Text);
                cmd.Parameters.AddWithValue("@cap", txtCapacity.Text);
                cmd.Parameters.AddWithValue("@eq", txtEquipment.Text);
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                if (cmbCoach.SelectedValue != null)
                {
                    cmd.Parameters.AddWithValue("@coach", cmbCoach.SelectedValue);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@coach", DBNull.Value);
                }
                cmd.Parameters.AddWithValue("@rem", txtRemarks.Text);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            LoadFacilityGrid(txtSearch.Text, cmbFilterType.Text, cmbFilterStatus.Text);
            MessageBox.Show("Updated ✅");
            LoadFacilities();
            LoadDashboardData();
        }
        
        private void btnDeleteFacility_Click(object sender, EventArgs e)
        {
            if (txtFacilityID.Text == "")
            {
                MessageBox.Show("Select record ❌");
                return;
            }

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "DELETE FROM Facilities WHERE FacilityCode=@code";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@code", txtFacilityID.Text);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            LoadFacilityGrid(txtSearch.Text, cmbFilterType.Text, cmbFilterStatus.Text);
            LoadFacilities();
            LoadDashboardData();
            MessageBox.Show("Deleted ✅");
        }

        private void btnClearFacility_Click(object sender, EventArgs e)
        {
            ClearFacility();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadFacilityGrid(txtSearch.Text, cmbFilterType.Text, cmbFilterStatus.Text);
        }

        private void cmbFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFacilityGrid(txtSearch.Text, cmbFilterType.Text, cmbFilterStatus.Text);
        }

        private void cmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFacilityGrid(txtSearch.Text, cmbFilterType.Text, cmbFilterStatus.Text);
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if(txtSearch.Text == "Search by Facility Name")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if  (txtSearch.Text == "")
            {
                txtSearch.Text = "Search by Facility Name";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void btnClearBooking_Click(object sender, EventArgs e)
        {
            ClearBooking();
        }
        private void ClearBooking()
        {
            txtEnrollmentNo.Clear();
            txtStudentName.Clear();
            txtStudentName.Tag = null;

            cmbFacility.SelectedIndex = -1;
            cmbTimeSlot.SelectedIndex = -1;

            txtPurpose.Clear();
            dtpDate.Value = DateTime.Now;

            txtEnrollmentNo.Focus();
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {            
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView2.Rows[e.RowIndex];

                // 🔥 BookingCode (optional use)
                string bookingCode = row.Cells["BookingCode"].Value.ToString();

                // 🎯 Enrollment No
                txtEnrollmentNo.Text = row.Cells["EnrollmentNo"].Value.ToString();

                // 🎯 Student Name
                txtStudentName.Text = row.Cells["StudentName"].Value.ToString();

                // 🎯 Facility select
                cmbFacility.Text = row.Cells["FacilityName"].Value.ToString();

                // 🎯 Date
                dtpDate.Value = Convert.ToDateTime(row.Cells["BookingDate"].Value);

                // 🎯 Time Slot
                cmbTimeSlot.Text = row.Cells["TimeSlot"].Value.ToString();

                // 🎯 Purpose
                txtPurpose.Text = row.Cells["Purpose"].Value.ToString();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }

        private void txtCapacity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            // Check booking selected or not
            if (dataGridView2.CurrentRow == null)
            {
                MessageBox.Show("Select booking record ❌");
                return;
            }

            string bookingCode = dataGridView2.CurrentRow.Cells["BookingCode"].Value.ToString();

            DialogResult result = MessageBox.Show(
                "Return this facility?",
                "Confirm Return",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();

                    // Delete booking
                    string query = "DELETE FROM FacilityBookings WHERE BookingCode=@code";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@code", bookingCode);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Facility Returned Successfully ✅");

                // Reload grids
                LoadFacilities();
                LoadBookings();
                LoadFacilityGrid("", "All Types", "All Status");

                // Dashboard refresh
                LoadDashboardData();

                ClearFacility();
            }
        }
    }    
}