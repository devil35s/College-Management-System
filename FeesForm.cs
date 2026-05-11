using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollegeManagementSystem
{
    public partial class FeesForm : Form
    {
        int feeId = 0; // This variable will hold the ID of the selected fee record for update and delete operations
        decimal lastPaidAmount = 0; // This variable will hold the last paid amount to calculate the new paid amount during update

        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");
        public FeesForm()
        {
            InitializeComponent();
        }

        private void FeesForm_Load(object sender, EventArgs e)
        {

            LoadData();

            if (dgvFeesReport.Columns.Contains("FeeID"))
            {
                dgvFeesReport.Columns["FeeID"].Visible = false;
            }

            DesignFeesGrid();
            FormatFeesColumns();

            CheckRemaining();
            LoadStatistics();

            // Calling the method to round the corners of the buttons in the form
            RoundButton(btnSave);
            RoundButton(btnUpdate);
            RoundButton(btnDelete);
            RoundButton(btnPrint);
            RoundButton(btnSearch);
            RoundButton(btnClear);

            // Adding items to the payment status combo box
            cmbPaymentStatus.Items.Add("Paid");
            cmbPaymentStatus.Items.Add("Pending");

            cmbPaymentStatus.Enabled = false; // user change nahi karega


        }

        private void DesignFeesGrid()
        {
            // Basic UI
            dgvFeesReport.BorderStyle = BorderStyle.None;
            dgvFeesReport.BackgroundColor = Color.White;
            dgvFeesReport.EnableHeadersVisualStyles = false;

            // Header Style
            dgvFeesReport.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvFeesReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(10, 44, 92);
            dgvFeesReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvFeesReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvFeesReport.ColumnHeadersHeight = 40;

            // Rows Style
            dgvFeesReport.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            dgvFeesReport.RowTemplate.Height = 40;
            dgvFeesReport.GridColor = Color.LightGray;

            // Alternate row color
            dgvFeesReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            // Selection Style
            dgvFeesReport.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dgvFeesReport.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            // Behavior
            dgvFeesReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFeesReport.RowHeadersVisible = false;
            dgvFeesReport.AllowUserToAddRows = false;
            dgvFeesReport.AllowUserToDeleteRows = false;
            dgvFeesReport.ReadOnly = true;
            dgvFeesReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void FormatFeesColumns()
        {
            if (dgvFeesReport.Columns.Count == 0)
                return;

            if (dgvFeesReport.Columns.Contains("EnrollmentNo"))
                dgvFeesReport.Columns["EnrollmentNo"].HeaderText = "Enrollment No";

            if (dgvFeesReport.Columns.Contains("StudentName"))
                dgvFeesReport.Columns["StudentName"].HeaderText = "Student Name";

            if (dgvFeesReport.Columns.Contains("AmountPaid"))
                dgvFeesReport.Columns["AmountPaid"].HeaderText = "Paid";

            if (dgvFeesReport.Columns.Contains("Remaining"))
                dgvFeesReport.Columns["Remaining"].HeaderText = "Pending";

            // Currency format
            if (dgvFeesReport.Columns.Contains("TotalFee"))
                dgvFeesReport.Columns["TotalFee"].DefaultCellStyle.Format = "N0";

            if (dgvFeesReport.Columns.Contains("AmountPaid"))
                dgvFeesReport.Columns["AmountPaid"].DefaultCellStyle.Format = "N0";

            if (dgvFeesReport.Columns.Contains("Remaining"))
                dgvFeesReport.Columns["Remaining"].DefaultCellStyle.Format = "N0";

        }

        // This method is used to round the corners of the buttons in the form
        void RoundButton(Button btn)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, 20, 20, 180, 90);
            path.AddArc(btn.Width - 20, 0, 20, 20, 270, 90);
            path.AddArc(btn.Width - 20, btn.Height - 20, 20, 20, 0, 90);
            path.AddArc(0, btn.Height - 20, 20, 20, 90, 90);
            path.CloseAllFigures();
            btn.Region = new Region(path);
        }

        private void txtPaidAmount_TextChanged(object sender, EventArgs e)
        {

            if (txtTotalFees.Text == "" || txtPaidAmount.Text == "")
                return;

            decimal total = Convert.ToDecimal(txtTotalFees.Text);
            decimal paid = Convert.ToDecimal(txtPaidAmount.Text);

            decimal remaining = total - paid;

            txtRemainingFees.Text = remaining.ToString();

            if (remaining == 0)
                cmbPaymentStatus.Text = "Paid";
            else
                cmbPaymentStatus.Text = "Pending";
        }
        // This method is used to clear the form after saving, updating or deleting a record
        void ClearForm()
        {
            txtEnrollmentNo.Clear();
            txtStudentName.Clear();
            txtTotalFees.Clear();
            txtPaidAmount.Clear();
            txtRemainingFees.Clear();
            txtAddAmount.Clear();

            cmbPaymentMode.SelectedIndex = -1;
            cmbPaymentStatus.SelectedIndex = -1;
        }



        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                //Validation
                if (txtEnrollmentNo.Text == "" || txtTotalFees.Text == "" || txtPaidAmount.Text == "" || cmbPaymentMode.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill all required fields ❗");
                    return;
                }

                con.Open();

                // Duplicate Check
                string checkQuery = "SELECT COUNT(*) FROM Fees WHERE EnrollmentNo=@enr";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                                checkCmd.Parameters.AddWithValue("@enr", txtEnrollmentNo.Text);
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    MessageBox.Show("Fees record for this enrollment number already exists ❗");
                    con.Close();
                    return;
                }

                // Set lastPaidAmount for receipt printing
                lastPaidAmount = Convert.ToDecimal(txtPaidAmount.Text);

                // Insert Record
                SqlCommand cmd = new SqlCommand(
                "INSERT INTO Fees (EnrollmentNo, TotalFee, AmountPaid, PaymentMode, PaymentDate, PaymentStatus) " +
                "VALUES (@enr, @total, @paid, @mode, @date, @status)", con);

                cmd.Parameters.AddWithValue("@enr", txtEnrollmentNo.Text);
                cmd.Parameters.AddWithValue("@total", txtTotalFees.Text);
                cmd.Parameters.AddWithValue("@paid", txtPaidAmount.Text);
                cmd.Parameters.AddWithValue("@mode", cmbPaymentMode.Text);
                cmd.Parameters.AddWithValue("@date", dtpPaymentDate.Value);
                cmd.Parameters.AddWithValue("@status", cmbPaymentStatus.Text);
               
                cmd.ExecuteNonQuery();
                con.Close();

                MessageBox.Show("Saved Successfully ✅");

                foreach (Form frm in Application.OpenForms)
                {
                    if (frm is Dashboard dashboard)
                    {
                        dashboard.LoadStatistics();
                    }
                }

                LoadData();   // 🔥 grid refresh
                ClearForm();  // 🔥 form clear
                LoadStatistics(); // 🔥 update statistics
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);

                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void txtEnrollmentNo_TextChanged(object sender, EventArgs e)
        {

            if (txtEnrollmentNo.Text == "")
                return;

            SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");

            SqlCommand cmd = new SqlCommand("SELECT FirstName, LastName, Course, Semester FROM Student WHERE EnrollmentNo=@enr", con);
            cmd.Parameters.AddWithValue("@enr", txtEnrollmentNo.Text);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                txtStudentName.Text = dr["FirstName"].ToString() + " " + dr["LastName"].ToString();
                cmbCourse.Text = dr["Course"].ToString();
                cmbSemester.Text = dr["Semester"].ToString();
            }
            else
            {
                txtStudentName.Text = "";
                cmbCourse.Text = "";
                cmbSemester.Text = "";
            }

            txtStudentName.ReadOnly = true;
            cmbCourse.Enabled = false;
            cmbSemester.Enabled = false;

            con.Close();
        }
        void CheckRemaining()
        {
            if (txtRemainingFees.Text == "0.00" || txtRemainingFees.Text == "0")
            {
                txtAddAmount.Enabled = false;
            }
            else
            {
                txtAddAmount.Enabled = true;
            }
        }
        void LoadData()
        {
            SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True");

            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT f.FeeID, s.EnrollmentNo, s.FirstName + ' ' + s.LastName AS StudentName, s.Course, s.Semester, f.TotalFee, f.AmountPaid, f.RemainingFee, f.PaymentMode, f.PaymentDate, f.PaymentStatus FROM Fees f JOIN Student s ON f.EnrollmentNo = s.EnrollmentNo",
                con);

            DataTable dt = new DataTable();
            da.Fill(dt);

            dgvFeesReport.DataSource = dt;
            dgvFeesReport.Columns["TotalFee"].HeaderText = "Total Fee";
            dgvFeesReport.Columns["AmountPaid"].HeaderText = "Amount Paid";
            dgvFeesReport.Columns["RemainingFee"].HeaderText = "Remaining Fee";
            dgvFeesReport.Columns["PaymentMode"].HeaderText = "Payment Mode";
            dgvFeesReport.Columns["PaymentDate"].HeaderText = "Payment Date";
            dgvFeesReport.Columns["PaymentStatus"].HeaderText = "Payment Status";

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (feeId == 0)
                {
                    MessageBox.Show("Select record first!");
                    return;
                }

                decimal total = Convert.ToDecimal(txtTotalFees.Text);
                decimal paid = Convert.ToDecimal(txtPaidAmount.Text);

                decimal add = 0;

                if (txtAddAmount.Text != "")
                {
                    add = Convert.ToDecimal(txtAddAmount.Text);
                }

                // ✅ ALWAYS SET THIS
                if (add > 0)
                    lastPaidAmount = add;      // installment
                else
                    lastPaidAmount = Convert.ToDecimal(txtPaidAmount.Text); // first payment

                // ✅ FIXED CALCULATION
                decimal newPaid = paid + add;
                decimal newRemaining = total - newPaid;

                decimal remaining = total - paid;

                // ✅ VALIDATION
                if (add > remaining)
                {
                    MessageBox.Show("Amount exceeds remaining fees ❌");
                    return;
                }


                string status = newRemaining == 0 ? "Paid" : "Pending";

                SqlCommand cmd = new SqlCommand(
                "UPDATE Fees SET AmountPaid=@paid, PaymentMode=@mode, PaymentDate=@date, PaymentStatus=@status WHERE FeeID=@id",
                con);

                cmd.Parameters.AddWithValue("@id", feeId);
                cmd.Parameters.AddWithValue("@paid", newPaid);
                cmd.Parameters.AddWithValue("@mode", cmbPaymentMode.Text);
                cmd.Parameters.AddWithValue("@date", dtpPaymentDate.Value);
                cmd.Parameters.AddWithValue("@status", status);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                // ✅ UI UPDATE (VERY IMPORTANT)
                txtPaidAmount.Text = newPaid.ToString();
                txtRemainingFees.Text = newRemaining.ToString();
                cmbPaymentStatus.Text = status;
                txtAddAmount.Clear();

                MessageBox.Show("Payment Updated ✅");

                foreach (Form frm in Application.OpenForms)
                {
                    if (frm is Dashboard dashboard)
                    {
                        dashboard.LoadStatistics();
                    }
                }

                LoadData();
                CheckRemaining();
                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

            SqlCommand cmd = new SqlCommand("DELETE FROM Fees WHERE EnrollmentNo=@enr", con);

            cmd.Parameters.AddWithValue("@enr", txtEnrollmentNo.Text);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            MessageBox.Show("Deleted Successfully ❌");

            foreach (Form frm in Application.OpenForms)
            {
                if (frm is Dashboard dashboard)
                {
                    dashboard.LoadStatistics();
                }
            }

            LoadData();
            LoadStatistics();
        }

        private void dgvFeesReport_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvFeesReport.Rows[e.RowIndex];
                feeId = Convert.ToInt32(row.Cells["FeeID"].Value);
                txtEnrollmentNo.Text = row.Cells["EnrollmentNo"].Value.ToString();
                txtStudentName.Text = row.Cells["StudentName"].Value.ToString();
                cmbCourse.Text = row.Cells["Course"].Value.ToString();
                cmbSemester.Text = row.Cells["Semester"].Value.ToString();
                txtTotalFees.Text = row.Cells["TotalFee"].Value.ToString();
                txtPaidAmount.Text = row.Cells["AmountPaid"].Value.ToString();
                txtRemainingFees.Text = row.Cells["RemainingFee"].Value.ToString();
                cmbPaymentMode.Text = row.Cells["PaymentMode"].Value.ToString();
                dtpPaymentDate.Value = Convert.ToDateTime(row.Cells["PaymentDate"].Value);
                cmbPaymentStatus.Text = row.Cells["PaymentStatus"].Value.ToString();
                CheckRemaining();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();

            if (lastPaidAmount == 0)
            {
                lastPaidAmount = Convert.ToDecimal(txtPaidAmount.Text);
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            Font heading = new Font("Arial", 18, FontStyle.Bold);
            Font subHeading = new Font("Arial", 14, FontStyle.Bold);
            Font normal = new Font("Arial", 11);

            int y = 40;

            // ================= HEADER =================
            g.DrawString("COLLEGE MANAGEMENT SYSTEM", heading, Brushes.Black, 80, y);
            y += 40;

            g.DrawString("FEES RECEIPT", subHeading, Brushes.Black, 150, y);
            y += 30;

            g.DrawLine(Pens.Black, 40, y, 500, y);
            y += 20;

            // ================= RECEIPT INFO =================
            g.DrawString("Receipt No : " + feeId, normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Date       : " + dtpPaymentDate.Value.ToString("dd-MM-yyyy"), normal, Brushes.Black, 50, y);
            y += 30;

            g.DrawLine(Pens.Black, 40, y, 500, y);
            y += 20;

            // ================= STUDENT DETAILS =================
            g.DrawString("Enrollment No : " + txtEnrollmentNo.Text, normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Student Name  : " + txtStudentName.Text, normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Course        : " + cmbCourse.Text, normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Semester      : " + cmbSemester.Text, normal, Brushes.Black, 50, y);
            y += 30;

            g.DrawLine(Pens.Black, 40, y, 500, y);
            y += 20;

            // ================= FEES DETAILS =================
            g.DrawString("Total Fees     : ₹ " + txtTotalFees.Text, normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Last Paid      : ₹ " + lastPaidAmount.ToString(), normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Total Paid     : ₹ " + txtPaidAmount.Text, normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Remaining Fees : ₹ " + txtRemainingFees.Text, normal, Brushes.Black, 50, y);
            y += 30;

            g.DrawLine(Pens.Black, 40, y, 500, y);
            y += 20;

            // ================= PAYMENT DETAILS =================
            g.DrawString("Payment Mode : " + cmbPaymentMode.Text, normal, Brushes.Black, 50, y);
            y += 25;

            g.DrawString("Status       : " + cmbPaymentStatus.Text, normal, Brushes.Black, 50, y);
            y += 40;

            g.DrawLine(Pens.Black, 40, y, 500, y);
            y += 20;

            // ===================== FOOTER =======================
            g.DrawString("Thank you for your payment!", subHeading, Brushes.Black, 180, y);
        }
        void LoadStatistics()
        {
            try
            {
                con.Open();

                // Total Students
                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Student", con);
                lblTotalStudents.Text = cmd1.ExecuteScalar().ToString();

                // Total Fees
                SqlCommand cmd2 = new SqlCommand("SELECT ISNULL(SUM(TotalFee),0) FROM Fees", con);
                lblTotalFees.Text = "₹ " + Convert.ToDecimal(cmd2.ExecuteScalar()).ToString("N0");

                // Collected Fees
                SqlCommand cmd3 = new SqlCommand("SELECT ISNULL(SUM(AmountPaid),0) FROM Fees", con);
                lblCollectedFees.Text = "₹ " + Convert.ToDecimal(cmd3.ExecuteScalar()).ToString("N0");

                // Pending Fees
                SqlCommand cmd4 = new SqlCommand("SELECT ISNULL(SUM(TotalFee - AmountPaid),0) FROM Fees", con);
                lblPendingFees.Text = "₹ " + Convert.ToDecimal(cmd4.ExecuteScalar()).ToString("N0");

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                con.Close();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            
            try
            {
                using (SqlConnection con = new SqlConnection(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"))
                {
                    con.Open();

                    string query = @"SELECT 
                        f.FeeID,
                        f.EnrollmentNo,
                        s.FirstName + ' ' + s.LastName AS StudentName,
                        s.Course,
                        s.Semester,
                        f.TotalFee,
                        f.AmountPaid,
                        (f.TotalFee - f.AmountPaid) AS RemainingFee,
                        f.PaymentMode,
                        f.PaymentDate,
                        f.PaymentStatus
                    FROM Fees f
                    INNER JOIN Student s ON f.EnrollmentNo = s.EnrollmentNo
                    WHERE f.EnrollmentNo LIKE @EnrollmentNo";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@EnrollmentNo", "%" + txtEnrollmentNo.Text + "%");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvFeesReport.DataSource = dt;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No Record Found ❗");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgvFeesReport_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvFeesReport.Columns[e.ColumnIndex].Name == "PaymentStatus")
            {
                if (e.Value != null)
                {
                    string status = e.Value.ToString();

                    if (status == "Paid")
                    {
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font(dgvFeesReport.Font, FontStyle.Bold);
                    }
                    else if (status == "Pending")
                    {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(dgvFeesReport.Font, FontStyle.Bold);
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            
            // TextBoxes clear
            txtEnrollmentNo.Clear();
            txtStudentName.Clear();
            txtTotalFees.Clear();
            txtPaidAmount.Clear();
            txtRemainingFees.Clear();
            txtAddAmount.Clear();

            // ComboBoxes reset
            cmbCourse.SelectedIndex = -1;
            cmbSemester.SelectedIndex = -1;
            cmbPaymentMode.SelectedIndex = -1;
            cmbPaymentStatus.SelectedIndex = -1;

            // Date reset (aaj ki date)
            dtpPaymentDate.Value = DateTime.Now;

            // Focus back to first field
            txtEnrollmentNo.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }

        private void txtTotalFees_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) ) 
            {
                e.Handled = true; // non-numeric input block
            }
        }

        private void txtPaidAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // non-numeric input block
            }
        }

        private void txtAddAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // non-numeric input block
            }
        }
    }       
}