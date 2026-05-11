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
using static ClosedXML.Excel.XLPredefinedFormat;

namespace CollegeManagementSystem
{
    public partial class Transport : Form
    {
        public Transport()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete this vehicle?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                string q = "DELETE FROM TransportVehicle WHERE VehicleNo=@no";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@no", txtVehicleNo.Text);

                int rows = cmd.ExecuteNonQuery();

                MessageBox.Show(rows > 0 ? "Deleted 🗑️" : "Vehicle not found ❌");

                LoadVehicles();
                ClearVehicleFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }
        private void CreateTransportTables()
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                con.Open();

                string vehicleTable = @"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TransportVehicle')
        CREATE TABLE TransportVehicle(
            VehicleID INT IDENTITY(1,1) PRIMARY KEY,
            VehicleNo VARCHAR(50),
            VehicleType VARCHAR(20),
            DriverName VARCHAR(100),
            DriverPhone VARCHAR(15),
            Route VARCHAR(100),
            Capacity INT
        )";

                string assignTable = @"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TransportAssign')
        CREATE TABLE TransportAssign(
            AssignID INT IDENTITY(1,1) PRIMARY KEY,
            EnrollmentNo VARCHAR(50),
            StudentName VARCHAR(100),
            VehicleNo VARCHAR(50),
            VehicleType VARCHAR(20),
            Route VARCHAR(100),
            PickupTime VARCHAR(20)
        )";

                new SqlCommand(vehicleTable, con).ExecuteNonQuery();
                new SqlCommand(assignTable, con).ExecuteNonQuery();
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
        private void txtEnroll_TextChanged(object sender, EventArgs e)
        {
            if (txtEnroll.Text.Trim() == "")
            {
                txtStudentName.Clear();
                return;
            }

            using (SqlConnection con = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT FirstName + ' ' + LastName FROM Student WHERE EnrollmentNo=@enr",
                    con);

                cmd.Parameters.AddWithValue("@enr", txtEnroll.Text.Trim());

                con.Open();
                var result = cmd.ExecuteScalar();
                con.Close();

                if (result != null)
                {
                    txtStudentName.Text = result.ToString();
                }
                else
                {
                    txtStudentName.Text = "Not Found ❌";
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                con.Open();

                string query = @"INSERT INTO TransportVehicle
        (VehicleNo, VehicleType, DriverName, DriverPhone, Route, Capacity)
        VALUES (@VehicleNo, @VehicleType, @DriverName, @DriverPhone, @Route, @Capacity)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@VehicleNo", txtVehicleNo.Text);
                cmd.Parameters.AddWithValue("@VehicleType", cmbVehicleType.Text);
                cmd.Parameters.AddWithValue("@DriverName", txtDriverName.Text);
                cmd.Parameters.AddWithValue("@DriverPhone", txtDriverPhone.Text);
                cmd.Parameters.AddWithValue("@Route", txtRoute.Text);
                cmd.Parameters.AddWithValue("@Capacity", txtCapacity.Text);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Vehicle Saved ✅");
                LoadVehicles();
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
        private void LoadVehicles()
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                con.Open();

                string query = "SELECT * FROM TransportVehicle";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvVehicles.DataSource = dt;

                cmbVehicleNo.DataSource = dt;
                cmbVehicleNo.DisplayMember = "VehicleNo";

                dgvVehicles.Columns["VehicleID"].Visible = false;
                dgvVehicles.Columns["DriverPhone"].HeaderText = "Driver Phone";
                dgvVehicles.Columns["VehicleType"].HeaderText = "Type";
                dgvVehicles.Columns["DriverName"].HeaderText = "Driver Name";
                dgvVehicles.Columns["VehicleNo"].HeaderText = "Vehicle No";
                dgvVehicles.Columns["Route"].HeaderText = "Route";
                dgvVehicles.Columns["Capacity"].HeaderText = "Capacity";
                StyleDataGridView();
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

        private void cmbVehicleNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                con.Open();

                string q = "SELECT VehicleType, Route FROM TransportVehicle WHERE VehicleNo=@no";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@no", cmbVehicleNo.Text);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    cmbVehicleType2.Text = dr["VehicleType"].ToString();
                    txtRoute2.Text = dr["Route"].ToString();
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

        private void btnAssign_Click(object sender, EventArgs e)
        {
            // 🔴 Validation
            if (txtEnroll.Text == "" || txtStudentName.Text == "")
            {
                MessageBox.Show("Enter valid student ❗");
                return;
            }

            string vehicleNo = cmbVehicleNo.Text;

            // 🔴 Duplicate check
            if (IsAlreadyAssigned(txtEnroll.Text, vehicleNo))
            {
                MessageBox.Show("Already Assigned ❌");
                return;
            }

            // 🔴 Capacity check
            if (!HasSeatAvailable(vehicleNo))
            {
                MessageBox.Show("No Seats Available 🚫");
                return;
            }

            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                string q = @"INSERT INTO TransportAssign
        (EnrollmentNo, StudentName, VehicleNo, VehicleType, Route, PickupTime)
        VALUES (@e,@n,@v,@t,@r,@p)";

                SqlCommand cmd = new SqlCommand(q, con);

                cmd.Parameters.AddWithValue("@e", txtEnroll.Text);
                cmd.Parameters.AddWithValue("@n", txtStudentName.Text);
                cmd.Parameters.AddWithValue("@v", vehicleNo);
                cmd.Parameters.AddWithValue("@t", cmbVehicleType2.Text);
                cmd.Parameters.AddWithValue("@r", txtRoute2.Text);
                cmd.Parameters.AddWithValue("@p", dtpTime.Value.ToString("hh:mm tt"));

                cmd.ExecuteNonQuery();

                MessageBox.Show("Assigned ✅");

                LoadAssignedStudents();
                LoadTransportStats();   // dashboard update
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }
        private void LoadAssignedStudents()
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                con.Open();

                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM TransportAssign", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvAssignedStudents.DataSource = dt;
                dgvAssignedStudents.Columns["AssignID"].HeaderText = "Assign ID";
                dgvAssignedStudents.Columns["EnrollmentNo"].HeaderText = "Enrollment No";
                dgvAssignedStudents.Columns["StudentName"].HeaderText = "Student Name";
                dgvAssignedStudents.Columns["VehicleNo"].HeaderText = "Vehicle No";
                dgvAssignedStudents.Columns["VehicleType"].HeaderText = "Vehicle Type";
                dgvAssignedStudents.Columns["Route"].HeaderText = "Route";
                dgvAssignedStudents.Columns["PickupTime"].HeaderText = "Pickup Time";

                StyleDataGridView2();

                dgvAssignedStudents.Columns["StudentName"].Width = 200;
                dgvAssignedStudents.Columns["Route"].Width = 200;
                dgvAssignedStudents.Columns["EnrollmentNo"].Width = 132;
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

        private void Transport_Load(object sender, EventArgs e)
        {
            CreateTransportTables();
            LoadVehicles();
            LoadAssignedStudents();
            LoadTransportStats();
        }
        private void LoadTransportStats()
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                con.Open();

                // ✅ 1. Total Vehicles
                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM TransportVehicle", con);
                lblTotalVehicles.Text = cmd1.ExecuteScalar().ToString();

                // ✅ 2. Assigned Students
                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM TransportAssign", con);
                lblAssignedStudents.Text = cmd2.ExecuteScalar().ToString();

                // ✅ 3. Available Seats
                SqlCommand cmd3 = new SqlCommand(
                    @"SELECT ISNULL(SUM(Capacity),0) - 
              (SELECT COUNT(*) FROM TransportAssign)
              FROM TransportVehicle", con);

                lblAvailableSeats.Text = cmd3.ExecuteScalar().ToString();

                // ✅ 4. Total Routes (unique routes)
                SqlCommand cmd4 = new SqlCommand(
                    "SELECT COUNT(DISTINCT Route) FROM TransportVehicle", con);

                lblTotalRoutes.Text = cmd4.ExecuteScalar().ToString();
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
        private void StyleDataGridView()
        {
            dgvVehicles.BorderStyle = BorderStyle.None;
            dgvVehicles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvVehicles.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvVehicles.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvVehicles.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvVehicles.BackgroundColor = Color.White;

            dgvVehicles.EnableHeadersVisualStyles = false;
            dgvVehicles.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dgvVehicles.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvVehicles.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            dgvVehicles.ColumnHeadersHeight = 40;

            dgvVehicles.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dgvVehicles.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvVehicles.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dgvVehicles.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dgvVehicles.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dgvVehicles.RowTemplate.Height = 35;
            dgvVehicles.ReadOnly = true;
            dgvVehicles.AllowUserToAddRows = false;
            dgvVehicles.AllowUserToDeleteRows = false;
            dgvVehicles.AllowUserToResizeRows = false;

            dgvVehicles.BackgroundColor = System.Drawing.Color.White;
            dgvVehicles.BorderStyle = BorderStyle.None;
            dgvVehicles.GridColor = System.Drawing.Color.Gainsboro;
            dgvVehicles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVehicles.ScrollBars = ScrollBars.Both;

            dgvVehicles.RowHeadersVisible = false;
            dgvVehicles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVehicles.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dgvVehicles.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            dgvVehicles.RowTemplate.Height = 40;
            dgvVehicles.ColumnHeadersHeight = 45;
        }
        private void StyleDataGridView2()
        {
            dgvAssignedStudents.BorderStyle = BorderStyle.None;
            dgvAssignedStudents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvAssignedStudents.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvAssignedStudents.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvAssignedStudents.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvAssignedStudents.BackgroundColor = Color.White;

            dgvAssignedStudents.EnableHeadersVisualStyles = false;
            dgvAssignedStudents.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dgvAssignedStudents.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvAssignedStudents.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            dgvAssignedStudents.ColumnHeadersHeight = 40;

            dgvAssignedStudents.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dgvAssignedStudents.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvAssignedStudents.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dgvAssignedStudents.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dgvAssignedStudents.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dgvAssignedStudents.RowTemplate.Height = 35;
            dgvAssignedStudents.ReadOnly = true;
            dgvAssignedStudents.AllowUserToAddRows = false;
            dgvAssignedStudents.AllowUserToDeleteRows = false;
            dgvAssignedStudents.AllowUserToResizeRows = false;

            dgvAssignedStudents.BackgroundColor = System.Drawing.Color.White;
            dgvAssignedStudents.BorderStyle = BorderStyle.None;
            dgvAssignedStudents.GridColor = System.Drawing.Color.Gainsboro;
            dgvAssignedStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAssignedStudents.ScrollBars = ScrollBars.Both;

            dgvAssignedStudents.RowHeadersVisible = false;
            dgvAssignedStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAssignedStudents.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dgvAssignedStudents.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            dgvAssignedStudents.RowTemplate.Height = 40;
            dgvAssignedStudents.ColumnHeadersHeight = 45;   
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                string q = @"UPDATE TransportVehicle SET
                     VehicleType=@type,
                     DriverName=@name,
                     DriverPhone=@phone,
                     Route=@route,
                     Capacity=@cap
                     WHERE VehicleNo=@no";

                SqlCommand cmd = new SqlCommand(q, con);

                cmd.Parameters.AddWithValue("@no", txtVehicleNo.Text);
                cmd.Parameters.AddWithValue("@type", cmbVehicleType.Text);
                cmd.Parameters.AddWithValue("@name", txtDriverName.Text);
                cmd.Parameters.AddWithValue("@phone", txtDriverPhone.Text);
                cmd.Parameters.AddWithValue("@route", txtRoute.Text);
                cmd.Parameters.AddWithValue("@cap", txtCapacity.Text);

                int rows = cmd.ExecuteNonQuery();

                MessageBox.Show(rows > 0 ? "Updated ✅" : "Vehicle not found ❌");

                LoadVehicles();   // grid refresh
                ClearVehicleFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearVehicleFields();
        }

        private void ClearVehicleFields()
        {
            txtVehicleNo.Clear();
            cmbVehicleType.SelectedIndex = -1;
            txtDriverName.Clear();
            txtDriverPhone.Clear();
            txtRoute.Clear();
            txtCapacity.Clear();

            txtVehicleNo.Focus();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dgvAssignedStudents.CurrentRow == null)
            {
                MessageBox.Show("Select a record first ❗");
                return;
            }

            if (MessageBox.Show("Remove this student?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                // AssignID primary key use कर रहे हैं
                int id = Convert.ToInt32(dgvAssignedStudents.CurrentRow.Cells["AssignID"].Value);

                string q = "DELETE FROM TransportAssign WHERE AssignID=@id";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Removed ❌");

                LoadAssignedStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        private void dgvVehicles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var r = dgvVehicles.Rows[e.RowIndex];

                txtVehicleNo.Text = r.Cells["VehicleNo"].Value.ToString();
                cmbVehicleType.Text = r.Cells["VehicleType"].Value.ToString();
                txtDriverName.Text = r.Cells["DriverName"].Value.ToString();
                txtDriverPhone.Text = r.Cells["DriverPhone"].Value.ToString();
                txtRoute.Text = r.Cells["Route"].Value.ToString();
                txtCapacity.Text = r.Cells["Capacity"].Value.ToString();
            }
        }
        private bool HasSeatAvailable(string vehicleNo)
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                // Capacity
                SqlCommand cmdCap = new SqlCommand(
                    "SELECT Capacity FROM TransportVehicle WHERE VehicleNo=@v", con);
                cmdCap.Parameters.AddWithValue("@v", vehicleNo);

                int capacity = Convert.ToInt32(cmdCap.ExecuteScalar());

                // Assigned Count
                SqlCommand cmdCount = new SqlCommand(
                    "SELECT COUNT(*) FROM TransportAssign WHERE VehicleNo=@v", con);
                cmdCount.Parameters.AddWithValue("@v", vehicleNo);

                int assigned = Convert.ToInt32(cmdCount.ExecuteScalar());

                return assigned < capacity;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }
        private bool IsAlreadyAssigned(string enroll, string vehicleNo)
        {
            SqlConnection con = DBConnection.con;

            try
            {
                if (con.State == ConnectionState.Open) con.Close();
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM TransportAssign WHERE EnrollmentNo=@e AND VehicleNo=@v", con);

                cmd.Parameters.AddWithValue("@e", enroll);
                cmd.Parameters.AddWithValue("@v", vehicleNo);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;
            }
            catch
            {
                return true;
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }

        private void txtCapacity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // non-numeric input block
            }
        }
    }
}