using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.DataVisualization.Charting;
using DrawingFont = System.Drawing.Font;

namespace CollegeManagementSystem
{
    public partial class ReportModuleModernForm : Form
    {
        string cs = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True;TrustServerCertificate=True";
        private PrintDocument printDocument1 = new PrintDocument();
        private int currentRow = 0;
        private decimal collectedFee = 0;
        private decimal totalFee = 0;


        public ReportModuleModernForm()
        {
            InitializeComponent();
            printDocument1.DefaultPageSettings.Landscape = true;
            printDocument1.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);
            printDocument1.PrintPage += printDocument1_PrintPage;
            btnBottomExcel.Click += btnBottomExcel_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnGenerateReport.MouseEnter += btnGenerateReport_MouseEnter;
            btnGenerateReport.MouseLeave += btnGenerateReport_MouseLeave;

            btnExportExcel.MouseEnter += btnExportExcel_MouseEnter;
            btnExportExcel.MouseLeave += btnExportExcel_MouseLeave;

            btnExportPdf.MouseEnter += btnExportPdf_MouseEnter;
            btnExportPdf.MouseLeave += btnExportPdf_MouseLeave;

            btnPrint.MouseEnter += btnPrint_MouseEnter;
            btnPrint.MouseLeave += btnPrint_MouseLeave;

            btnRefresh.MouseEnter += btnRefresh_MouseEnter;
            btnRefresh.MouseLeave += btnRefresh_MouseLeave;

        }
        private void LoadFeeCollection()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(AmountPaid),0), ISNULL(SUM(TotalFee),0) FROM Fees", con);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    collectedFee = Convert.ToDecimal(dr[0]);
                    totalFee = Convert.ToDecimal(dr[1]);
                }

                con.Close();
            }

            // UI refresh
            pnlFeeCollection.Invalidate();
        }
        private void label17_Click(object sender, EventArgs e)
        {

        }        

        private void LoadDashboard()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                // 🔵 Total Students
                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Student", con);
                lblTotalStudents.Text = cmd1.ExecuteScalar().ToString();

                // 🟢 Total Faculty
                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM Faculty", con);
                lblTotalFaculty.Text = cmd2.ExecuteScalar().ToString();

                // 💰 Fee Collected
                SqlCommand cmd3 = new SqlCommand("SELECT ISNULL(SUM(AmountPaid),0) FROM Fees", con);
                decimal collected = Convert.ToDecimal(cmd3.ExecuteScalar());
                lblFeeCollected.Text = "₹ " + collected.ToString("N0");

                // 🔴 Pending Fees
                SqlCommand cmd4 = new SqlCommand("SELECT ISNULL(SUM(RemainingFee),0) FROM Fees", con);
                decimal pending = Convert.ToDecimal(cmd4.ExecuteScalar());
                lblPendingFees.Text = "₹ " + pending.ToString("N0");

                // 📊 Attendance %
                SqlCommand cmd5 = new SqlCommand(@"
                SELECT 
                CAST(
                    (SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END) * 100.0) 
                    / COUNT(*) 
                AS DECIMAL(5,2))
                FROM Attendance", con);

                object result = cmd5.ExecuteScalar();
                lblAttendance.Text = (result != DBNull.Value ? result.ToString() : "0") + " %";
            }
        }
        private void LoadAttendanceLabels()
        {
            SqlConnection con = new SqlConnection(cs);
            con.Open();

            // Total
            SqlCommand totalCmd = new SqlCommand("SELECT COUNT(*) FROM Attendance", con);
            int total = Convert.ToInt32(totalCmd.ExecuteScalar());

            // Present
            SqlCommand presentCmd = new SqlCommand("SELECT COUNT(*) FROM Attendance WHERE Status='Present'", con);
            int present = Convert.ToInt32(presentCmd.ExecuteScalar());

            // Absent
            SqlCommand absentCmd = new SqlCommand("SELECT COUNT(*) FROM Attendance WHERE Status='Absent'", con);
            int absent = Convert.ToInt32(absentCmd.ExecuteScalar());

            if (total > 0)
            {
                double presentPer = (present * 100.0) / total;
                double absentPer = (absent * 100.0) / total;

                lblPresent.Text = "Present " + presentPer.ToString("0") + "%";
                lblAbsent.Text = "Absent " + absentPer.ToString("0") + "%";
            }
            else
            {
                lblPresent.Text = "Present 0%";
                lblAbsent.Text = "Absent 0%";
            }

            con.Close();
        }

        private void ReportModuleModernForm_Load(object sender, EventArgs e)
        {

            LoadDashboard();
            LoadAttendanceLabels();
            LoadFeeCollection();
            
            cmbMonth.Items.Clear();
            cmbMonth.Items.Add("January");
            cmbMonth.Items.Add("February");
            cmbMonth.Items.Add("March");
            cmbMonth.Items.Add("April");
            cmbMonth.Items.Add("May");
            cmbMonth.Items.Add("June");
            cmbMonth.Items.Add("July");
            cmbMonth.Items.Add("August");
            cmbMonth.Items.Add("September");
            cmbMonth.Items.Add("October");
            cmbMonth.Items.Add("November");
            cmbMonth.Items.Add("December");

            cmbMonth.SelectedIndex = -1;

            dgvReport.EnableHeadersVisualStyles = false;
            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(10, 44, 92);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            dgvReport.ColumnHeadersHeight = 38;


            dgvReport.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10);
            dgvReport.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvReport.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dgvReport.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 255);
            dgvReport.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dgvReport.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            dgvReport.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvReport.RowHeadersVisible = false;
            dgvReport.RowTemplate.Height = 34;
            dgvReport.ReadOnly = true;
            dgvReport.AllowUserToAddRows = false;
            dgvReport.AllowUserToDeleteRows = false;
            dgvReport.AllowUserToResizeRows = false;
            dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvReport.BackgroundColor = System.Drawing.Color.White;
            dgvReport.BorderStyle = BorderStyle.None;
            dgvReport.GridColor = System.Drawing.Color.Gainsboro;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvReport.ScrollBars = ScrollBars.Both;

            cmbReportType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbClassDepartment.SelectedIndex = 0;
            cmbSemester.SelectedIndex = 0;

            LoadTotalStudentsCount();
            MakeCircular(pboxUser);

            MakeRoundedPanel(pnlAdmin, 40);
            MakeRounded(pnlDateTime, 10);
            MakeRounded(pnlActionButtons, 20);
            MakeRounded(pnlAttendanceOverview, 20);
            MakeRounded(pnlFeeCollection, 20);
            MakeRounded(cardStudents, 20);
            MakeRounded(cardFees, 20);
            MakeRounded(cardFaculty, 20);
            MakeRounded(cardPending, 20);
            MakeRounded(cardAttendance, 20);
            MakeRoundedButton(btnGenerateReport, 15);
            MakeRoundedButton(btnExportExcel, 15);
            MakeRoundedButton(btnExportPdf, 15);
            MakeRoundedButton(btnPrint, 15);

            MakeRoundedButton(btnBottomExcel, 15);
            MakeRoundedButton(btnBottomPdf, 15);
            MakeRoundedButton(btnBottomPrint, 15);
            MakeRoundedButton(btnRefresh, 15);

            AddShadow(cardStudents);
            AddShadow(cardFees);
            AddShadow(cardFaculty);
            AddShadow(cardPending);
            AddShadow(cardAttendance);
            LoadAttendanceChart();
            lblDateTime.Text = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
            timer1.Start();
            pnlFeeCollection.BackColor = System.Drawing.Color.White;
            pnlFeeCollection.Invalidate();

        }

        private int GetMonthNumber(string monthName)
        {
            if (string.IsNullOrWhiteSpace(monthName))
                return DateTime.Now.Month;

            DateTime dt;
            bool success = DateTime.TryParseExact(
                monthName,
                "MMMM",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out dt
            );

            if (success)
                return dt.Month;
            else
            {
                MessageBox.Show("Invalid month: " + monthName);
                return DateTime.Now.Month;
            }
        }

        private void LoadStudentReport()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"
            SELECT 
                StudentId AS [Student ID],
                Status AS [Status],
                EnrollmentNo AS [Enrollment No],
                FirstName + ' ' + LastName AS [Student Name],
                Gender AS [Gender],
                CONVERT(varchar, DateOfBirth, 105) AS [Date Of Birth],
                Course AS [Course],
                Semester AS [Semester],
                Phone AS [Contact],
                Email AS [Email],
                Address AS [Address]
            FROM Student
            WHERE 1=1";

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    // 🔍 Search
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        query += @" AND (
                    FirstName LIKE @search OR 
                    LastName LIKE @search OR 
                    EnrollmentNo LIKE @search
                )";
                        cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text.Trim() + "%");
                    }

                    // 🎯 Status filter
                    if (cmbStatus.Text != "All")
                    {
                        query += " AND Status = @status";
                        cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                    }

                    // 🎯 Course filter
                    if (cmbClassDepartment.Text != "All Courses")
                    {
                        query += " AND Course = @course";
                        cmd.Parameters.AddWithValue("@course", cmbClassDepartment.Text);
                    }

                    // 🎯 Semester filter
                    if (cmbSemester.Text != "All Semesters")
                    {
                        query += " AND Semester = @semester";
                        cmd.Parameters.AddWithValue("@semester", cmbSemester.Text);
                    }
                    
                    cmd.CommandText = query;

                    // 🔄 Load data
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    dgvReport.DataSource = dt;

                    // 🎨 Column Width
                    dgvReport.AutoSizeColumnsMode = (DataGridViewAutoSizeColumnsMode)DataGridViewAutoSizeColumnMode.None;
                    dgvReport.Columns["Student ID"].Width = 100;
                    dgvReport.Columns["Status"].Width = 100;
                    dgvReport.Columns["Enrollment No"].Width = 130;
                    dgvReport.Columns["Student Name"].Width = 180;
                    dgvReport.Columns["Gender"].Width = 90;
                    dgvReport.Columns["Date Of Birth"].Width = 120;
                    dgvReport.Columns["Course"].Width = 120;
                    dgvReport.Columns["Semester"].Width = 100;
                    dgvReport.Columns["Contact"].Width = 130;
                    dgvReport.Columns["Email"].Width = 200;
                    dgvReport.Columns["Address"].Width = 200;

                    // Hide Column Student ID
                    dgvReport.Columns["Student ID"].Visible = false;

                    // 🔥 Status color
                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        if (row.Cells["Status"].Value != null)
                        {
                            string status = row.Cells["Status"].Value.ToString().ToLower();

                            if (status == "active")
                            {
                                row.Cells["Status"].Style.ForeColor = Color.Green;
                                row.Cells["Status"].Style.Font =
                            new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                            }
                            else
                            {
                                row.Cells["Status"].Style.ForeColor = Color.Red;
                                row.Cells["Status"].Style.Font =
                                    new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading student report: " + ex.Message);
            }
        }

        private void LoadTotalStudentsCount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = "SELECT COUNT(*) FROM dbo.Student";
                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();
                    int totalStudents = Convert.ToInt32(cmd.ExecuteScalar());

                    lblTotalStudents.Text = totalStudents.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading total students count: " + ex.Message);
            }
        }

        private void LoadFeeReport()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"
                        SELECT 
                            f.FeeID AS [Fee ID],
                            s.EnrollmentNo AS [Enrollment No],
                            s.FirstName + ' ' + s.LastName AS [Student Name],
                            s.Course AS [Course],
                            s.Semester AS [Semester],
                            f.TotalFee AS [Total Fees],
                            f.AmountPaid AS [Amount Paid],
                            f.RemainingFee AS [Remaining Fees],
                            f.PaymentMode AS [Payment Mode],
                            f.PaymentStatus AS [Payment Status]
                        FROM Fees f
                        INNER JOIN Student s ON f.EnrollmentNo = s.EnrollmentNo
                        WHERE 1=1";

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    // 🔍 Search (ID / Name)
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        query += @" AND (
                    s.FirstName LIKE @search
                    OR s.LastName LIKE @search
                    OR s.EnrollmentNo LIKE @search
                )";
                        cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text.Trim() + "%");
                    }

                    // 🎯 Course filter
                    if (cmbClassDepartment.Text != "All Courses")
                    {
                        query += " AND s.Course = @course";
                        cmd.Parameters.AddWithValue("@course", cmbClassDepartment.Text);
                    }

                    // 🎯 Semester filter
                    if (cmbSemester.Text != "All Semesters")
                    {
                        query += " AND s.Semester = @semester";
                        cmd.Parameters.AddWithValue("@semester", cmbSemester.Text);
                    }

                    // 📅 Date filter
                    if (dtpFrom.Checked)
                    {
                        query += " AND CAST(f.PaymentDate AS DATE) = @date";
                        cmd.Parameters.AddWithValue("@date", dtpFrom.Value.Date);
                    }

                    // 🟢 Status filter
                    if (cmbStatus.Text != "All")
                    {
                        query += " AND f.PaymentStatus = @status";
                        cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                    }

                    cmd.CommandText = query;

                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    dgvReport.DataSource = dt;

                    dgvReport.Columns["Fee ID"].Visible = false;
                    // 💰 Currency format (₹)
                    var culture = new System.Globalization.CultureInfo("en-IN");

                    dgvReport.Columns["Total Fees"].DefaultCellStyle.Format = "C0";
                    dgvReport.Columns["Total Fees"].DefaultCellStyle.FormatProvider = culture;

                    dgvReport.Columns["Amount Paid"].DefaultCellStyle.Format = "C0";
                    dgvReport.Columns["Amount Paid"].DefaultCellStyle.FormatProvider = culture;

                    dgvReport.Columns["Remaining Fees"].DefaultCellStyle.Format = "C0";
                    dgvReport.Columns["Remaining Fees"].DefaultCellStyle.FormatProvider = culture;

                    dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // 🎨 Status color
                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        if (row.Cells["Payment Status"].Value != null)
                        {
                            string status = row.Cells["Payment Status"].Value.ToString().ToLower();

                            if (status == "paid")
                            {
                                row.Cells["Payment Status"].Style.ForeColor = Color.Green;
                                row.Cells["Payment Status"].Style.Font =
                                    new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                            }
                            else if (status == "pending")
                            {
                                row.Cells["Payment Status"].Style.ForeColor = Color.Red;
                                row.Cells["Payment Status"].Style.Font =
                                    new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading fee report: " + ex.Message);
            }
        }

        private void LoadFacultyReport()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"
            SELECT 
                FacultyID AS [Faculty ID],
                FirstName + ' ' + LastName AS [Faculty Name],
                Gender AS [Gender],
                Qualification AS [Qualification],
                Department AS [Department],
                CONVERT(varchar, JoinDate, 105) AS [Joining Date],
                Salary AS [Salary],
                Phone AS [Phone],
                Email AS [Email],
                Address AS [Address]
            FROM Faculty
            WHERE 1=1";

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    // 🔍 Search
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        query += @" AND (
                    FirstName LIKE @search OR 
                    LastName LIKE @search OR 
                    Phone LIKE @search
                )";
                        cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text.Trim() + "%");
                    }

                    // 🎯 Department filter
                    if (cmbClassDepartment.Text != "All Courses")
                    {
                        query += " AND Department = @dept";
                        cmd.Parameters.AddWithValue("@dept", cmbClassDepartment.Text);
                    }

                    cmd.CommandText = query;

                    // 🔄 Load data
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    dgvReport.DataSource = dt;

                    dgvReport.Columns["Salary"].DefaultCellStyle.Format = "C0";
                    dgvReport.Columns["Salary"].DefaultCellStyle.FormatProvider = new System.Globalization.CultureInfo("en-IN");

                    // 🎨 Column Width
                    dgvReport.AutoSizeColumnsMode = (DataGridViewAutoSizeColumnsMode)DataGridViewAutoSizeColumnMode.None;
                    dgvReport.Columns["Faculty ID"].Width = 90;
                    dgvReport.Columns["Faculty Name"].Width = 180;
                    dgvReport.Columns["Gender"].Width = 90;
                    dgvReport.Columns["Joining Date"].Width = 120;
                    dgvReport.Columns["Qualification"].Width = 130;
                    dgvReport.Columns["Department"].Width = 120;
                    dgvReport.Columns["Salary"].Width = 100;
                    dgvReport.Columns["Phone"].Width = 130;
                    dgvReport.Columns["Email"].Width = 200;
                    dgvReport.Columns["Address"].Width = 200;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading faculty report: " + ex.Message);
            }
        }

        private void LoadFacilityReport()
        {
            MessageBox.Show("Facility report table is not available yet.");
        }

        private void LoadAttendanceReport()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"
            SELECT 
                CONVERT(varchar, a.AttendanceDate, 105) AS [Date],
                s.EnrollmentNo AS [Enrollment No],
                s.FirstName + ' ' + s.LastName AS [Student Name],
                s.Course AS [Course],
                s.Semester AS [Semester],
                a.Status AS [Status]
            FROM attendance a
            INNER JOIN Student s ON a.student_id = s.StudentId
            WHERE 1=1";

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    // 🔍 Search
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        query += @" AND (
                    s.FirstName LIKE @search OR 
                    s.LastName LIKE @search OR 
                    s.EnrollmentNo LIKE @search
                )";
                        cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text.Trim() + "%");
                    }

                    // 🎯 Course filter
                    if (cmbClassDepartment.Text != "All Courses")
                    {
                        query += " AND s.Course = @course";
                        cmd.Parameters.AddWithValue("@course", cmbClassDepartment.Text);
                    }

                    // 🎯 Semester filter
                    if (cmbSemester.Text != "All Semesters")
                    {
                        query += " AND s.Semester = @semester";
                        cmd.Parameters.AddWithValue("@semester", cmbSemester.Text);
                    }

                    // 📅 Date filter
                    if (dtpFrom.Checked)
                    {
                        query += " AND CAST(a.AttendanceDate AS DATE) = @date";
                        cmd.Parameters.AddWithValue("@date", dtpFrom.Value.Date);
                    }

                    // 🟢 Status filter
                    if (cmbStatus.Text != "All")
                    {
                        query += " AND a.Status = @status";
                        cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                    }

                    cmd.CommandText = query;

                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    dgvReport.DataSource = dt;

                    dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    // 🎨 Column Width
                    dgvReport.Columns["Date"].Width = 180;
                    dgvReport.Columns["Enrollment No"].Width = 120;
                    dgvReport.Columns["Student Name"].Width = 240;
                    dgvReport.Columns["Course"].Width = 180;
                    dgvReport.Columns["Semester"].Width = 160;
                    dgvReport.Columns["Status"].Width = 160;

                    // 🔥 Status color
                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        if (row.Cells["Status"].Value != null)
                        {
                            string status = row.Cells["Status"].Value.ToString().ToLower();

                            if (status == "present")
                            {
                                row.Cells["Status"].Style.ForeColor = Color.Green;
                                row.Cells["Status"].Style.Font =
                                    new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                            }
                            else if (status == "absent")
                            {
                                row.Cells["Status"].Style.ForeColor = Color.Red;
                                row.Cells["Status"].Style.Font =
                                    new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading attendance report: " + ex.Message);
            }
        }

        private void label18_Click(object sender, EventArgs e)
        {

        }
        private void MakeCircular(PictureBox pic)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, pic.Width - 1, pic.Height - 1);
            pic.Region = new Region(gp);
        }

        private void MakeRoundedPanel(Panel pnl, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            path.AddArc(new System.Drawing.Rectangle(0, 0, radius, radius), 180, 90);
            path.AddArc(new System.Drawing.Rectangle(pnl.Width - radius, 0, radius, radius), 270, 90);
            path.AddArc(new System.Drawing.Rectangle(pnl.Width - radius, pnl.Height - radius, radius, radius), 0, 90);
            path.AddArc(new System.Drawing.Rectangle(0, pnl.Height - radius, radius, radius), 90, 90);

            path.CloseFigure();
            pnl.Region = new Region(path);
        }

        private void MakeRounded(Panel pnl, int radius)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            path.StartFigure();
            path.AddArc(new System.Drawing.Rectangle(0, 0, radius, radius), 180, 90);
            path.AddArc(new System.Drawing.Rectangle(pnl.Width - radius, 0, radius, radius), 270, 90);
            path.AddArc(new System.Drawing.Rectangle(pnl.Width - radius, pnl.Height - radius, radius, radius), 0, 90);
            path.AddArc(new System.Drawing.Rectangle(0, pnl.Height - radius, radius, radius), 90, 90);
            path.CloseFigure();

            pnl.Region = new Region(path);
        }

        private void LoadAttendanceChart()
        {
            SqlConnection con = new SqlConnection(cs);

            SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT Status, COUNT(*) AS Total
            FROM Attendance
            WHERE Status IN ('Present','Absent')
            GROUP BY Status", con);

            DataTable dt = new DataTable();
            da.Fill(dt);

            // 🔥 RESET CHART (IMPORTANT)
            chartAttendance.Series.Clear();
            chartAttendance.ChartAreas.Clear();
            chartAttendance.Legends.Clear();

            // Chart Area
            ChartArea ca = new ChartArea("AttendanceArea");
            ca.BackColor = Color.White;
            chartAttendance.ChartAreas.Add(ca);

            // Series
            Series s = new Series("Attendance");
            s.ChartType = SeriesChartType.Doughnut;
            s.ChartArea = "AttendanceArea";
            s.IsValueShownAsLabel = false;
            s["PieLabelStyle"] = "Disabled";
            s["DoughnutRadius"] = "60";

            // 🔥 DATA ADD FROM DB
            foreach (DataRow row in dt.Rows)
            {
                string status = row["Status"].ToString();
                int total = Convert.ToInt32(row["Total"]);

                s.Points.AddXY(status, total);
            }

            // 🎨 COLORS (safe mapping)
            foreach (DataPoint point in s.Points)
            {
                if (point.AxisLabel == "Present")
                    point.Color = Color.FromArgb(24, 160, 88);   // Green
                else if (point.AxisLabel == "Absent")
                    point.Color = Color.FromArgb(220, 53, 69);   // Red
            }

            chartAttendance.Series.Add(s);
        }


        private void DrawRoundedBorder(Panel pnl, PaintEventArgs e, int radius, System.Drawing.Color borderColor, int borderWidth)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                GraphicsPath path = new GraphicsPath();

                path.StartFigure();
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                e.Graphics.DrawPath(pen, path);
            }
        }

        private void MakeRoundedButton(Button btn, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);

            path.CloseFigure();
            btn.Region = new Region(path);
        }
        private void AddShadow(Panel pnl)
        {
            pnl.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle,
                    System.Drawing.Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid);
            };
        }

        private void PrintData()
        {
            try
            {
                if (dgvReport.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to print.");
                    return;
                }

                currentRow = 0;
                PrintPreviewDialog preview = new PrintPreviewDialog();
                preview.Document = printDocument1;
                preview.Width = 1200;
                preview.Height = 800;
                currentRow = 0;
                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while printing: " + ex.Message);
            }
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            int leftMargin = e.MarginBounds.Left;
            int topMargin = e.MarginBounds.Top;
            int y = topMargin;

            System.Drawing.Font titleFont = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold);
            System.Drawing.Font subFont = new System.Drawing.Font("Segoe UI", 9);
            System.Drawing.Font headerFont = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold);
            System.Drawing.Font cellFont = new System.Drawing.Font("Segoe UI", 9);

            // Title
            e.Graphics.DrawString("College Management System - Report Module", titleFont, Brushes.Black, leftMargin, y);
            y += 28;
            e.Graphics.DrawString("Generated on: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"), subFont, Brushes.Black, leftMargin, y);
            y += 25;

            // Print only selected columns
            string[] printColumns = { "Student ID", "First Name", "Last Name", "Gender", "Contact", "Course", "Semester", "Status" };
            int[] colWidths = { 80, 110, 110, 70, 110, 90, 80, 80 };

            int headerHeight = 28;
            int rowHeight = 26;

            int x = leftMargin;

            // Header
            for (int i = 0; i < printColumns.Length; i++)
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x, y, colWidths[i], headerHeight);
                e.Graphics.FillRectangle(Brushes.LightGray, rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;

                e.Graphics.DrawString(printColumns[i], headerFont, Brushes.Black, rect, sf);
                x += colWidths[i];
            }

            y += headerHeight;

            // Rows
            while (currentRow < dgvReport.Rows.Count)
            {
                x = leftMargin;

                for (int i = 0; i < printColumns.Length; i++)
                {
                    string colName = printColumns[i];
                    if (!dgvReport.Columns.Contains(colName))
                        continue;

                    string text = dgvReport.Rows[currentRow].Cells[colName].Value?.ToString() ?? "";

                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x, y, colWidths[i], rowHeight);
                    e.Graphics.DrawRectangle(Pens.Black, rect);

                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Trimming = StringTrimming.EllipsisCharacter;

                    System.Drawing.RectangleF textRect = new System.Drawing.RectangleF(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6);
                    e.Graphics.DrawString(text, cellFont, Brushes.Black, textRect, sf);

                    x += colWidths[i];
                }

                y += rowHeight;
                currentRow++;

                if (y + rowHeight > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            e.HasMorePages = false;
            currentRow = 0;
        }
        private void DrawFeeCollectionCard(Panel pnl, PaintEventArgs e, decimal collected, decimal total)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // White card background
            using (SolidBrush bgBrush = new SolidBrush(System.Drawing.Color.FromArgb(224,224,224)))
            {
                e.Graphics.FillRectangle(bgBrush, pnl.ClientRectangle);
            }

            // Rounded border
            using (Pen borderPen = new Pen(System.Drawing.Color.FromArgb(220, 220, 220), 1))
            {
                GraphicsPath borderPath = new GraphicsPath();
                int r = 20;

                borderPath.AddArc(0, 0, r, r, 180, 90);
                borderPath.AddArc(pnl.Width - r - 1, 0, r, r, 270, 90);
                borderPath.AddArc(pnl.Width - r - 1, pnl.Height - r - 1, r, r, 0, 90);
                borderPath.AddArc(0, pnl.Height - r - 1, r, r, 90, 90);
                borderPath.CloseFigure();

                e.Graphics.DrawPath(borderPen, borderPath);
            }

            // Title
            using (System.Drawing.Font titleFont = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(System.Drawing.Color.Black))
            {
                e.Graphics.DrawString("Fee Collection", titleFont, titleBrush, 18, 14);
            }

            // Progress bar values
            float percent = 0;
            if (total > 0)
                percent = (float)(collected / total);

            int x = 20;
            int y = 54;
            int barWidth = pnl.Width - 40;
            int barHeight = 16;

            // Light green background bar
            using (GraphicsPath bgPath = new GraphicsPath())
            {
                bgPath.AddArc(x, y, barHeight, barHeight, 90, 180);
                bgPath.AddArc(x + barWidth - barHeight, y, barHeight, barHeight, 270, 180);
                bgPath.CloseFigure();

                using (SolidBrush lightBrush = new SolidBrush(System.Drawing.Color.FromArgb(198, 234, 213)))
                {
                    e.Graphics.FillPath(lightBrush, bgPath);
                }
            }

            // Filled dark green bar
            int fillWidth = (int)(barWidth * percent);
            if (fillWidth > 0)
            {
                if (fillWidth < barHeight)
                    fillWidth = barHeight;

                using (GraphicsPath fillPath = new GraphicsPath())
                {
                    if (fillWidth >= barWidth)
                    {
                        fillPath.AddArc(x, y, barHeight, barHeight, 90, 180);
                        fillPath.AddArc(x + barWidth - barHeight, y, barHeight, barHeight, 270, 180);
                    }
                    else
                    {
                        fillPath.AddArc(x, y, barHeight, barHeight, 90, 180);
                        fillPath.AddArc(x + fillWidth - barHeight, y, barHeight, barHeight, 270, 180);
                    }

                    fillPath.CloseFigure();

                    using (SolidBrush darkBrush = new SolidBrush(System.Drawing.Color.FromArgb(24, 160, 88)))
                    {
                        e.Graphics.FillPath(darkBrush, fillPath);
                    }
                }
            }

            // Amount text
            System.Globalization.CultureInfo india = new System.Globalization.CultureInfo("en-IN");
            string feeText = "₹ " + collected.ToString("N0", india) + " / ₹ " + total.ToString("N0", india);

            using (System.Drawing.Font valueFont = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold))
            using (SolidBrush valueBrush = new SolidBrush(System.Drawing.Color.Black))
            {
                e.Graphics.DrawString(feeText, valueFont, valueBrush, 18, 84);
            }

            // Bottom orange highlight like image
            using (SolidBrush orangeBrush = new SolidBrush(System.Drawing.Color.FromArgb(245, 158, 11)))
            {
                e.Graphics.FillRectangle(orangeBrush, 0, pnl.Height - 4, 90, 4);
            }
        }
        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            lblRecordsTitle.Text = cmbReportType.Text.Replace("Report", "Records");

            if (cmbReportType.Text == "Student Report")
            {
                LoadStudentReport();
            }
            else if (cmbReportType.Text == "Fee Report")
            {
                LoadFeeReport();
            }
            else if (cmbReportType.Text == "Faculty Report")
            {
                LoadFacultyReport();
            }
            else if (cmbReportType.Text == "Facility Report")
            {
                LoadFacilityReport();
            }
            else if (cmbReportType.Text == "Attendance Report")
            {
                LoadAttendanceReport();
            }
            else if (cmbReportType.Text == "Monthly Attendance Report")
            {
                int month = GetMonthNumber(cmbMonth.Text);

                LoadMonthlyAttendanceReport(month);
            }
            else if (cmbReportType.Text == "Monthly Attendance Report")
            {
                if (cmbMonth.SelectedItem == null || string.IsNullOrWhiteSpace(cmbMonth.Text))
                {
                    MessageBox.Show("Please select a month first!");
                    return;
                }

                int month = GetMonthNumber(cmbMonth.Text);

                LoadMonthlyAttendanceReport(month);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                // Date time refresh
                lblDateTime.Text = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");

                // Top cards / counts refresh
                LoadTotalStudentsCount();

                // Chart refresh
                LoadAttendanceChart();

                // Fee panel redraw
                pnlFeeCollection.Invalidate();

                // Attendance panel redraw
                pnlAttendanceOverview.Invalidate();

                // Current selected report refresh
                if (cmbReportType.Text == "Student Report")
                {
                    LoadStudentReport();
                }
                else if (cmbReportType.Text == "Fee Report")
                {
                    LoadFeeReport();
                }
                else if (cmbReportType.Text == "Faculty Report")
                {
                    LoadFacultyReport();
                }
                else if (cmbReportType.Text == "Facility Report")
                {
                    LoadFacilityReport();
                }
                else if (cmbReportType.Text == "Attendance Report")
                {
                    LoadAttendanceReport();
                }
                else if (cmbReportType.Text == "Monthly Attendance Report")
                {
                    int month = GetMonthNumber(cmbMonth.Text);
                    LoadMonthlyAttendanceReport(month);
                }

                MessageBox.Show("Report refreshed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while refreshing: " + ex.Message);
            }
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (cmbReportType.Text == "Student Report")
            {
                LoadStudentReport();
            }
            else if (cmbReportType.Text == "Fee Report")
            {
                LoadFeeReport();
            }
            else if (cmbReportType.Text == "Faculty Report")
            {
                LoadFacultyReport();
            }
            else if (cmbReportType.Text == "Facility Report")
            {
                LoadFacilityReport();
            }
            else if (cmbReportType.Text == "Attendance Report")
            {
                LoadAttendanceReport();
            }
            else if (cmbReportType.Text == "Monthly Attendance Report")
            {
                if (cmbMonth.SelectedItem == null || string.IsNullOrWhiteSpace(cmbMonth.Text))
                {
                    MessageBox.Show("Please select a month first!");
                    return;
                }

                int month = GetMonthNumber(cmbMonth.Text);

                LoadMonthlyAttendanceReport(month);
            }
            else
            {
                MessageBox.Show("Please select a report type.");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            dtpFrom.Checked = false;

            cmbReportType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbClassDepartment.SelectedIndex = 0;
            cmbSemester.SelectedIndex = 0;
            cmbMonth.SelectedIndex = -1;


            if (cmbReportType.Text == "Student Report")
            {
                LoadStudentReport();
            }
            else
            {
                dgvReport.DataSource = null;
            }
        }

        private void lblTotalStudentsValue_Click(object sender, EventArgs e)
        {

        }

        private void cardStudents_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ExportToPdf()
        {
            try
            {
                if (dgvReport.Rows.Count == 0)
                {
                    MessageBox.Show("No data found to export.");
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF File|*.pdf";
                sfd.Title = "Save PDF File";
                sfd.FileName = "Report.pdf";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                    PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create));
                    doc.Open();

                    Paragraph title = new Paragraph("College Management System - Report Module");
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 10f;
                    doc.Add(title);

                    Paragraph subtitle = new Paragraph("Generated on: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"));
                    subtitle.Alignment = Element.ALIGN_CENTER;
                    subtitle.SpacingAfter = 10f;
                    doc.Add(subtitle);

                    PdfPTable table = new PdfPTable(dgvReport.Columns.Count);
                    table.WidthPercentage = 100;

                    // Header
                    for (int i = 0; i < dgvReport.Columns.Count; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(dgvReport.Columns[i].HeaderText));
                        cell.BackgroundColor = new iTextSharp.text.BaseColor(211, 211, 211); // or cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);
                    }

                    // Data
                    for (int i = 0; i < dgvReport.Rows.Count; i++)
                    {
                        for (int j = 0; j < dgvReport.Columns.Count; j++)
                        {
                            table.AddCell(dgvReport.Rows[i].Cells[j].Value?.ToString() ?? "");
                        }
                    }

                    doc.Add(table);
                    doc.Close();

                    MessageBox.Show("PDF exported successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message);
            }
        }
        private void ExportToExcel()
        {
            try
            {
                if (dgvReport.Rows.Count == 0)
                {
                    MessageBox.Show("No data found to export.");
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Excel Workbook|*.xlsx";
                sfd.Title = "Save Excel File";
                sfd.FileName = "Report.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (var wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("Report");

                        // Headers
                        for (int i = 0; i < dgvReport.Columns.Count; i++)
                        {
                            ws.Cell(1, i + 1).Value = dgvReport.Columns[i].HeaderText;
                            ws.Cell(1, i + 1).Style.Font.Bold = true;
                        }

                        // Rows
                        for (int i = 0; i < dgvReport.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgvReport.Columns.Count; j++)
                            {
                                ws.Cell(i + 2, j + 1).Value = dgvReport.Rows[i].Cells[j].Value?.ToString() ?? "";
                            }
                        }

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Excel exported successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting Excel: " + ex.Message);
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PDF clicked");
            ExportToPdf();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintData();
        }
        private void btnBottomExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnBottomPdf_Click(object sender, EventArgs e)
        {
            ExportToPdf();
        }

        private void btnBottomPrint_Click(object sender, EventArgs e)
        {
            PrintData();
        }
        private void btnBottomPdf_Click_1(object sender, EventArgs e)
        {
            ExportToPdf();
        }

        private void btnBottomPrint_Click_1(object sender, EventArgs e)
        {
            PrintData();
        }

        private void btnPrint_Click_1(object sender, EventArgs e)
        {
            PrintData();
        }

        private void btnExportPdf_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("PDF clicked");
            ExportToPdf();
        }

        private void btnBottomExcel_Click_1(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void picAdmin_Click(object sender, EventArgs e)
        {

        }
        private void pnlAdmin_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
        }
        private void pnlDateTime_Resize(object sender, EventArgs e)
        {
            MakeRounded(pnlDateTime, 30);
        }

        private void pnlFeeCollection_Paint(object sender, PaintEventArgs e)
        {
            DrawFeeCollectionCard(pnlFeeCollection, e, collectedFee, totalFee);

        }

        private void pnlAttendanceOverview_Paint(object sender, PaintEventArgs e)
        {
            DrawRoundedBorder(pnlAttendanceOverview, e, 20, System.Drawing.Color.LightGray, 2);
        }

        private void pnlActionButtons_Paint(object sender, PaintEventArgs e)
        {
            DrawRoundedBorder(pnlActionButtons, e, 20, System.Drawing.Color.LightGray, 2);
        }
        private void pnlFeeCollection_Resize(object sender, EventArgs e)
        {
            MakeRounded(pnlFeeCollection, 20);
        }

        private void pnlAttendanceOverview_Resize(object sender, EventArgs e)
        {
            MakeRounded(pnlAttendanceOverview, 20);
        }

        private void pnlActionButtons_Resize(object sender, EventArgs e)
        {
            MakeRounded(pnlActionButtons, 20);
        }


        private void cardFaculty_Paint(object sender, PaintEventArgs e)
        {
            DrawRoundedBorder(cardFaculty, e, 20, System.Drawing.Color.LightGray, 2);
        }
        private void cardPending_Paint(object sender, PaintEventArgs e)
        {
            DrawRoundedBorder(cardPending, e, 20, System.Drawing.Color.LightGray, 2);
        }
        private void cardAttendance_Paint(object sender, PaintEventArgs e)
        {
            DrawRoundedBorder(cardAttendance, e, 20, System.Drawing.Color.LightGray, 2);
        }
        private void cardFees_Resize(object sender, EventArgs e)
        {
            MakeRounded(cardFees, 20);
        }
        private void cardFaculty_Resize(object sender, EventArgs e)
        {
            MakeRounded(cardFaculty, 20);
        }
        private void cardPending_Resize(object sender, EventArgs e)
        {
            MakeRounded(cardPending, 20);
        }
        private void cardAttendance_Resize(object sender, EventArgs e)
        {
            MakeRounded(cardAttendance, 20);
        }
        private void btnGenerateReport_MouseEnter(object sender, EventArgs e)
        {
            btnGenerateReport.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
        }

        private void btnGenerateReport_MouseLeave(object sender, EventArgs e)
        {
            btnGenerateReport.BackColor = System.Drawing.Color.White;
        }
        private void btnExportExcel_MouseEnter(object sender, EventArgs e)
        {
            btnExportExcel.BackColor = System.Drawing.Color.FromArgb(240, 255, 240);
        }

        private void btnExportExcel_MouseLeave(object sender, EventArgs e)
        {
            btnExportExcel.BackColor = System.Drawing.Color.White;
        }
        private void btnExportPdf_MouseEnter(object sender, EventArgs e)
        {
            btnExportPdf.BackColor = System.Drawing.Color.FromArgb(255, 245, 245);
        }

        private void btnExportPdf_MouseLeave(object sender, EventArgs e)
        {
            btnExportPdf.BackColor = System.Drawing.Color.White;
        }
        private void btnPrint_MouseEnter(object sender, EventArgs e)
        {
            btnPrint.BackColor = System.Drawing.Color.FromArgb(245, 248, 250);
        }

        private void btnPrint_MouseLeave(object sender, EventArgs e)
        {
            btnPrint.BackColor = System.Drawing.Color.White;
        }

        private void btnRefresh_MouseEnter(object sender, EventArgs e)
        {
            btnRefresh.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
        }

        private void btnRefresh_MouseLeave(object sender, EventArgs e)
        {
            btnRefresh.BackColor = System.Drawing.Color.White;
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            LoadFeeCollection();
        }

        private void cmbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
                        
        }
        private void LoadMonthlyAttendanceReport(int month)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"
                        SELECT 
                            s.StudentId AS [Student ID],
                            s.EnrollmentNo AS [Enrollment No],
                            s.FirstName + ' ' + s.LastName AS [Student Name],
                            s.Course AS [Course],
                            s.Semester AS [Semester],
                            COUNT(a.attendance_id) AS [Total Days],
                            SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) AS [Present Days],
                            SUM(CASE WHEN a.Status = 'Absent' THEN 1 ELSE 0 END) AS [Absent Days],
                            CAST(
                                (SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) * 100.0) 
                                / COUNT(a.attendance_id)
                            AS DECIMAL(5,2)) AS [Attendance Percentage %]
                        FROM attendance a
                        INNER JOIN Student s ON a.student_id = s.StudentId
                        WHERE MONTH(a.AttendanceDate) = @month";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@month", month);

                    // 🔍 SEARCH
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        query += @" AND (
                                            s.FirstName LIKE @search OR 
                                            s.LastName LIKE @search OR 
                                            s.EnrollmentNo LIKE @search
                                        )";
                        cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text.Trim() + "%");
                    }

                    // 🎯 COURSE / DEPARTMENT
                    if (cmbClassDepartment.Text != "All Courses")
                    {
                        query += " AND s.Course = @course";
                        cmd.Parameters.AddWithValue("@course", cmbClassDepartment.Text);
                    }

                    // 🎯 SEMESTER
                    if (cmbSemester.Text != "All Semesters")
                    {
                        query += " AND s.Semester = @semester";
                        cmd.Parameters.AddWithValue("@semester", cmbSemester.Text);
                    }
                    // 🔥 GROUP BY (IMPORTANT)
                    query += @" GROUP BY s.StudentId, s.EnrollmentNo, s.FirstName, s.LastName, s.Course, s.Semester";

                    // ⭐ FINAL LINE (MOST IMPORTANT)
                    cmd.CommandText = query;

                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    dgvReport.DataSource = dt;

                    // 🎨 Column Width
                    dgvReport.Columns["Student ID"].Width = 100;
                    dgvReport.Columns["Enrollment No"].Width = 130;
                    dgvReport.Columns["Student Name"].Width = 180;
                    dgvReport.Columns["Course"].Width = 120;
                    dgvReport.Columns["Semester"].Width = 100;
                    dgvReport.Columns["Total Days"].Width = 120;
                    dgvReport.Columns["Present Days"].Width = 130;
                    dgvReport.Columns["Absent Days"].Width = 130;
                    dgvReport.Columns["Attendance Percentage %"].Width = 170;

                    // 🎯 Alignment
                    dgvReport.Columns["Student ID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Enrollment No"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Student Name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Course"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Semester"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Total Days"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Present Days"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Absent Days"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvReport.Columns["Attendance Percentage %"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


                    // Hide Student ID
                    dgvReport.Columns["Student ID"].Visible = false;

                    // 🎨 Header
                    dgvReport.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    // 🔥 Percentage formatting
                    dgvReport.Columns["Attendance Percentage %"].DefaultCellStyle.Format = "0.00\\%'";

                    // 🎯 Low attendance highlight
                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        if (row.Cells["Attendance Percentage %"].Value != null)
                        {
                            double percent = Convert.ToDouble(row.Cells["Attendance Percentage %"].Value);

                            if (percent < 75)
                            {
                                row.Cells["Attendance Percentage %"].Style.ForeColor = Color.Red;
                                row.Cells["Attendance Percentage %"].Style.Font =
                                    new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                            }
                            else
                            {
                                row.Cells["Attendance Percentage %"].Style.ForeColor = Color.Green;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading monthly attendance: " + ex.Message);
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {

        }

        private void timer1_Tick_2(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }
    }
}

