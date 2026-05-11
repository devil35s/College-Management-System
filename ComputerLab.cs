using DocumentFormat.OpenXml.Wordprocessing;
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
    public partial class ComputerLab : Form
    {
        string selectedSystemID = "";
        bool isEditMode = false;
        public static SqlConnection con = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");
        public ComputerLab()
        {
            InitializeComponent();
        }

        void CreateDatabaseAndTables()
        {
            // 🔹 Step 1: Create Database if not exists
            SqlConnection conMaster = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True");

            conMaster.Open();

            SqlCommand cmdDB = new SqlCommand(@"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CollegeDB')
                CREATE DATABASE CollegeDB", conMaster);

            cmdDB.ExecuteNonQuery();
            conMaster.Close();

            // 🔹 Step 2: Connect to CollegeDB
            SqlConnection con = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");

            con.Open();

            // 🔹 Step 3: Systems Table
            SqlCommand systemsTable = new SqlCommand(@"
                                                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Systems' AND xtype='U')
                                                        CREATE TABLE Systems (
                                                            SystemID VARCHAR(20) PRIMARY KEY,
                                                            LabName VARCHAR(50),
                                                            InstalledSoftware VARCHAR(100),
                                                            Status VARCHAR(20)
                                                        )", con);

            systemsTable.ExecuteNonQuery();

            // 🔹 Step 4: SystemAssignment Table (WITH FOREIGN KEY 🔥)
            SqlCommand assignmentTable = new SqlCommand(@"
                                                            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SystemAssignment' AND xtype='U')
                                                            CREATE TABLE SystemAssignment (
                                                                AssignmentID INT IDENTITY(1,1) PRIMARY KEY,
                                                                EnrollmentNo NVARCHAR(100),
                                                                LabName VARCHAR(50),
                                                                SystemID VARCHAR(20),
                                                                AssignDate DATE,
                                                                TimeSlot VARCHAR(50),

                                                                FOREIGN KEY (EnrollmentNo) REFERENCES Student(EnrollmentNo),
                                                                FOREIGN KEY (SystemID) REFERENCES Systems(SystemID)
                                                            )", con);

            assignmentTable.ExecuteNonQuery();

            con.Close();
        }

        private void ComputerLab_Load(object sender, EventArgs e)
        {
            CreateDatabaseAndTables();
            LoadSystems();
            DesignSystemGrid();
            LoadAssignedToday();
            DesignAssignedGrid();
            LoadDashboard();

            if (!dgvAssignedToday.Columns.Contains("Return"))
            {
                DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                btn.Name = "Return";
                btn.HeaderText = "Action";
                btn.Text = "Return";
                btn.UseColumnTextForButtonValue = true;

                dgvAssignedToday.Columns.Add(btn);
            }
            LoadLabs();
        }
        private void DesignAssignedGrid()
        {
            // Basic UI
            dgvAssignedToday.BorderStyle = BorderStyle.None;
            dgvAssignedToday.BackgroundColor = System.Drawing.Color.White;
            dgvAssignedToday.EnableHeadersVisualStyles = false;

            // Header Style
            dgvAssignedToday.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvAssignedToday.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dgvAssignedToday.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvAssignedToday.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            dgvAssignedToday.ColumnHeadersHeight = 40;

            // Rows Style
            dgvAssignedToday.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dgvAssignedToday.RowTemplate.Height = 40;
            dgvAssignedToday.GridColor = System.Drawing.Color.LightGray;

            // Alternate row color
            dgvAssignedToday.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

            // Selection Style
            dgvAssignedToday.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dgvAssignedToday.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            // Behavior
            dgvAssignedToday.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAssignedToday.RowHeadersVisible = false;
            dgvAssignedToday.AllowUserToAddRows = false;
            dgvAssignedToday.AllowUserToDeleteRows = false;
            dgvAssignedToday.ReadOnly = true;
            dgvAssignedToday.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void DesignSystemGrid()
        {
            // Basic UI
            dvgSystems.BorderStyle = BorderStyle.None;
            dvgSystems.BackgroundColor = System.Drawing.Color.White;
            dvgSystems.EnableHeadersVisualStyles = false;

            // Header Style
            dvgSystems.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dvgSystems.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dvgSystems.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dvgSystems.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
            dvgSystems.ColumnHeadersHeight = 40;

            // Rows Style
            dvgSystems.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11);
            dvgSystems.RowTemplate.Height = 40;
            dvgSystems.GridColor = System.Drawing.Color.LightGray;

            // Alternate row color
            dvgSystems.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

            // Selection Style
            dvgSystems.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dvgSystems.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            // Behavior
            dvgSystems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dvgSystems.RowHeadersVisible = false;
            dvgSystems.AllowUserToAddRows = false;
            dvgSystems.AllowUserToDeleteRows = false;
            dvgSystems.ReadOnly = true;
            dvgSystems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void btnAddSystem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSystemID.Text) ||
                string.IsNullOrWhiteSpace(cmbLab.Text) ||
                string.IsNullOrWhiteSpace(cmbStatus.Text))
            {
                MessageBox.Show("Fill required fields ❌");
                return;
            }

            using (SqlConnection con = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                if (isEditMode)
                {
                    // 🔥 UPDATE
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Systems SET LabName=@lab, InstalledSoftware=@soft, Status=@status WHERE SystemID=@id",
                        con);

                    cmd.Parameters.AddWithValue("@id", selectedSystemID);
                    cmd.Parameters.AddWithValue("@lab", cmbLab.Text);
                    cmd.Parameters.AddWithValue("@soft", txtSoftware.Text);
                    cmd.Parameters.AddWithValue("@status", cmbStatus.Text);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("System Updated ✅");
                }
                else
                {
                    // 🔥 INSERT
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Systems VALUES (@id,@lab,@soft,@status)", con);

                    cmd.Parameters.AddWithValue("@id", txtSystemID.Text);
                    cmd.Parameters.AddWithValue("@lab", cmbLab.Text);
                    cmd.Parameters.AddWithValue("@soft", txtSoftware.Text);
                    cmd.Parameters.AddWithValue("@status", cmbStatus.Text);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("System Added ✅");
                }
            }

            // 🔄 Reset
            LoadSystems();
            ClearFields();
            ResetMode();
            LoadDashboard();
            LoadLabs(); // Lab list refresh (for assignment section)
        }

        void ResetMode()
        {
            isEditMode = false;
            selectedSystemID = "";
            txtSystemID.Enabled = true;
            btnAddSystem.Text = "Add System";
        }

        void ClearFields()
        {
            txtSystemID.Clear();
            txtSoftware.Clear();
            cmbLab.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ClearFields();
            ResetMode();
        }
        void LoadSystems()
        {
            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT SystemID, LabName, Status, InstalledSoftware FROM Systems",
                DBConnection.con);

            DataTable dt = new DataTable();
            da.Fill(dt);

            dvgSystems.DataSource = dt;

            dvgSystems.Columns["SystemID"].HeaderText = "System ID";
            dvgSystems.Columns["LabName"].HeaderText = "Lab Name";
            dvgSystems.Columns["InstalledSoftware"].HeaderText = "Installed Software";

            // 🔴 Check karo agar already button columns add ho chuke hain
            if (!dvgSystems.Columns.Contains("Edit"))
            {
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn();
                editBtn.Name = "Edit";
                editBtn.HeaderText = "Edit";
                editBtn.Text = "✏";
                editBtn.UseColumnTextForButtonValue = true;
                dvgSystems.Columns.Add(editBtn);
            }

            if (!dvgSystems.Columns.Contains("Delete"))
            {
                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn();
                deleteBtn.Name = "Delete";
                deleteBtn.HeaderText = "Delete";
                deleteBtn.Text = "🗑";
                deleteBtn.UseColumnTextForButtonValue = true;
                dvgSystems.Columns.Add(deleteBtn);
            }
        }
        private void dvgSystems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
           
            if (dvgSystems.Columns[e.ColumnIndex].Name == "Status")
            {
                string status = e.Value?.ToString();

                if (status == "Available")
                {
                    e.CellStyle.ForeColor = System.Drawing.Color.Green;
                }
                else if (status == "In Use")
                {
                    e.CellStyle.ForeColor = System.Drawing.Color.Orange;
                }
                else if (status == "Maintenance")
                {
                    e.CellStyle.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void dvgSystems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string systemID = dvgSystems.Rows[e.RowIndex].Cells["SystemID"].Value.ToString();

            // DELETE BUTTON
            if (dvgSystems.Columns[e.ColumnIndex].Name == "Delete")
            {
                DialogResult result = MessageBox.Show("Delete this system?", "Confirm", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Systems WHERE SystemID=@id", DBConnection.con);

                    cmd.Parameters.AddWithValue("@id", systemID);

                    DBConnection.con.Open();
                    cmd.ExecuteNonQuery();
                    DBConnection.con.Close();

                    LoadSystems();
                    LoadDashboard();
                }
            }

            // EDIT BUTTON
            if (dvgSystems.Columns[e.ColumnIndex].Name == "Edit")
            {
                DataGridViewRow row = dvgSystems.Rows[e.RowIndex];

                txtSystemID.Text = row.Cells["SystemID"].Value.ToString();
                cmbLab.Text = row.Cells["LabName"].Value.ToString();
                txtSoftware.Text = row.Cells["InstalledSoftware"].Value.ToString();
                cmbStatus.Text = row.Cells["Status"].Value.ToString();

                // 🔥 Edit mode ON
                selectedSystemID = txtSystemID.Text;
                txtSystemID.Enabled = false;   // ID change nahi hoga
                btnAddSystem.Text = "Update System";
                isEditMode = true;
            }
        }

        private void txtEnrollment_TextChanged(object sender, EventArgs e)
        {
            if (txtEnrollment.Text.Trim() == "")
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

                cmd.Parameters.AddWithValue("@enr", txtEnrollment.Text.Trim());

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
        void LoadLabs()
        {
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT DISTINCT LabName FROM Systems",
                "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbLabAssign.DataSource = dt;
                cmbLabAssign.DisplayMember = "LabName";
                cmbLabAssign.ValueMember = "LabName";
            }
        }
        private void cmbLabAssign_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (cmbLabAssign.Text == "") return;

            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT SystemID FROM Systems WHERE LabName=@lab AND Status='Available'",
                new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True")))
            {
                da.SelectCommand.Parameters.AddWithValue("@lab", cmbLabAssign.Text);

                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbSystemID.DataSource = dt;
                cmbSystemID.DisplayMember = "SystemID";
                cmbSystemID.ValueMember = "SystemID";
            }
        }    
        

        private void btnAssignSystem_Click(object sender, EventArgs e)
        {
            // 🔴 Basic validation
            if (string.IsNullOrWhiteSpace(txtEnrollment.Text) ||
                string.IsNullOrWhiteSpace(txtStudentName.Text) ||
                string.IsNullOrWhiteSpace(cmbLabAssign.Text) ||
                string.IsNullOrWhiteSpace(cmbSystemID.Text) ||
                string.IsNullOrWhiteSpace(cmbTimeSlot.Text))
            {
                MessageBox.Show("Fill all fields ❌");
                return;
            }

            // 🔴 Invalid enrollment check
            if (txtStudentName.Text.Contains("Not Found"))
            {
                MessageBox.Show("Invalid Enrollment No ❌");
                return;
            }

            using (SqlConnection con = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                // 🔒 Duplicate check (same system + date + slot)
                SqlCommand check = new SqlCommand(
                    @"SELECT COUNT(*) FROM SystemAssignment 
              WHERE SystemID=@sys AND AssignDate=@date AND TimeSlot=@slot", con);

                check.Parameters.AddWithValue("@sys", cmbSystemID.Text);
                check.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                check.Parameters.AddWithValue("@slot", cmbTimeSlot.Text);

                int exists = (int)check.ExecuteScalar();

                if (exists > 0)
                {
                    MessageBox.Show("System already assigned for this slot ❌");
                    return;
                }

                // 🔒 (Optional Strong Check) – same student same slot
                SqlCommand checkStudent = new SqlCommand(
                    @"SELECT COUNT(*) FROM SystemAssignment 
              WHERE EnrollmentNo=@enr AND AssignDate=@date AND TimeSlot=@slot", con);

                checkStudent.Parameters.AddWithValue("@enr", txtEnrollment.Text);
                checkStudent.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                checkStudent.Parameters.AddWithValue("@slot", cmbTimeSlot.Text);

                int studentBusy = (int)checkStudent.ExecuteScalar();

                if (studentBusy > 0)
                {
                    MessageBox.Show("Student already has a system in this slot ❌");
                    return;
                }

                // ✅ Insert assignment
        SqlCommand cmd = new SqlCommand(
            @"INSERT INTO SystemAssignment 
              (EnrollmentNo, LabName, SystemID, AssignDate, TimeSlot)
              VALUES (@enr,@lab,@sys,@date,@slot)", con);

                cmd.Parameters.AddWithValue("@enr", txtEnrollment.Text);
                cmd.Parameters.AddWithValue("@lab", cmbLabAssign.Text);
                cmd.Parameters.AddWithValue("@sys", cmbSystemID.Text);
                cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                cmd.Parameters.AddWithValue("@slot", cmbTimeSlot.Text);

                cmd.ExecuteNonQuery();

                // 🔄 Update system status → In Use
                SqlCommand update = new SqlCommand(
                    "UPDATE Systems SET Status='In Use' WHERE SystemID=@id", con);

                update.Parameters.AddWithValue("@id", cmbSystemID.Text);
                update.ExecuteNonQuery();

                con.Close();
            }

            MessageBox.Show("System Assigned Successfully ✅");

            // 🔄 Refresh UI
            LoadSystems();
            LoadAssignedToday();
            LoadDashboard();
            ClearAssignFields();
        }
        void ClearAssignFields()
        {
            txtEnrollment.Clear();
            txtStudentName.Clear();
            cmbLabAssign.SelectedIndex = -1;
            cmbSystemID.DataSource = null;
            cmbTimeSlot.SelectedIndex = -1;
        }
        void LoadAssignedToday()
        {
            using (SqlDataAdapter da = new SqlDataAdapter(
                @"SELECT 
            s.FirstName + ' ' + s.LastName AS StudentName,
            sa.EnrollmentNo,
            sa.LabName,
            sa.SystemID,
            sa.AssignDate,
            sa.TimeSlot,
            'Assigned' AS Status
          FROM SystemAssignment sa
          JOIN Student s ON sa.EnrollmentNo = s.EnrollmentNo",
                "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvAssignedToday.DataSource = dt;

                dgvAssignedToday.Columns["StudentName"].HeaderText = "Student Name";
                dgvAssignedToday.Columns["EnrollmentNo"].HeaderText = "Enrollment No";
                dgvAssignedToday.Columns["LabName"].HeaderText = "Lab Name";
                dgvAssignedToday.Columns["SystemID"].HeaderText = "System ID";
                dgvAssignedToday.Columns["AssignDate"].HeaderText = "Assign Date";
                dgvAssignedToday.Columns["TimeSlot"].HeaderText = "Time Slot";

                dgvAssignedToday.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvAssignedToday.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvAssignedToday.RowTemplate.Height = 35;
                dgvAssignedToday.ReadOnly = true;
                dgvAssignedToday.AllowUserToAddRows = false;
            }
        }
        void LoadDashboard()
        {
            using (SqlConnection con = new SqlConnection(
                "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
            {
                con.Open();

                // 🔵 TOTAL SYSTEMS
                SqlCommand totalCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Systems", con);
                lblTotalSystems.Text = totalCmd.ExecuteScalar().ToString();

                // 🟢 AVAILABLE SYSTEMS
                SqlCommand availCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Systems WHERE Status='Available'", con);
                lblAvailableSystems.Text = availCmd.ExecuteScalar().ToString();

                // 🟠 ASSIGNED TODAY
                SqlCommand assignCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM SystemAssignment WHERE AssignDate = CAST(GETDATE() AS DATE)", con);
                lblAssignedToday.Text = assignCmd.ExecuteScalar().ToString();

                // 🔴 UNDER MAINTENANCE
                SqlCommand mainCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Systems WHERE Status='Maintenance'", con);
                lblMaintenance.Text = mainCmd.ExecuteScalar().ToString();

                con.Close();
            }
        }
        private void dgvAssignedToday_CellClick(object sender, DataGridViewCellEventArgs e)
        {            
            if (e.RowIndex < 0) return;

            if (dgvAssignedToday.Columns[e.ColumnIndex].Name == "Return")
            {
                string systemID = dgvAssignedToday.Rows[e.RowIndex].Cells["SystemID"].Value.ToString();

                using (SqlConnection con = new SqlConnection(
                    "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    // 🔴 Delete assignment
                    SqlCommand delete = new SqlCommand(
                        "DELETE FROM SystemAssignment WHERE SystemID=@id", con);

                    delete.Parameters.AddWithValue("@id", systemID);
                    delete.ExecuteNonQuery();

                    // 🟢 Status back to Available
                    SqlCommand update = new SqlCommand(
                        "UPDATE Systems SET Status='Available' WHERE SystemID=@id", con);

                    update.Parameters.AddWithValue("@id", systemID);
                    update.ExecuteNonQuery();

                    con.Close();
                }

                MessageBox.Show("System Returned ✅");

                LoadAssignedToday();
                LoadSystems();
                LoadDashboard();
            }
        }

        private void dgvAssignedToday_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvAssignedToday.Columns[e.ColumnIndex].Name == "Status")
            {
                if (e.Value?.ToString() == "Assigned")
                {
                    e.CellStyle.ForeColor = System.Drawing.Color.Green;
                }
                if (e.Value?.ToString() == "Returned")
                {
                    e.CellStyle.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }
    }    
}
