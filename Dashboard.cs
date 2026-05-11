using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollegeManagementSystem
{
    public partial class Dashboard : Form
    {
        string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";
        public Dashboard()
        {
            InitializeComponent();
        }
        
        private async void btnAttendance_Click(object sender, EventArgs e)
        {

            this.Cursor = Cursors.AppStarting;

            // Loader show
            lblLoading.Visible = true;
            lblLoading.Text = "Loading Attendace Module...";

            await Task.Delay(100); // UI update

            Attendance a = new Attendance();

            a.Opacity = 0;
            a.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            a.Opacity = 1; // ab dikhega

            this.Cursor = Cursors.Default;

            lblLoading.Visible = false;
        }
        private async void btnLogout_Click(object sender, EventArgs e)
        {

            this.Cursor = Cursors.AppStarting;
            
            await Task.Delay(100); // UI update

            LoginForm l = new LoginForm();

            l.Opacity = 0;
            l.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            l.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;            

            this.Close();
        }

        private async void btnStudent_Click_1(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;
            // Loader show
            lblLoading.Visible = true;
            lblLoading.Text = "Loading Student Module...";

            await Task.Delay(100); // UI update

            Students s = new Students();

            s.Opacity = 0;
            s.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            s.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;

            lblLoading.Visible = false;
        }


        public void LoadStatistics()
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            // Total Students
            SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Student", con);
            lblStudentsCount.Text = cmd1.ExecuteScalar().ToString();

            // Total Faculty
            SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM Faculty", con);
            lblFacultyCount.Text = cmd2.ExecuteScalar().ToString();

            // Today's Attendance
            SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM Attendance WHERE AttendanceDate = CAST(GETDATE() AS DATE)", con);
            lblAttendanceCount.Text = cmd3.ExecuteScalar().ToString();

            // Fees Collected
            SqlCommand cmd4 = new SqlCommand("SELECT ISNULL(SUM(AmountPaid),0) FROM Fees", con);
            lblFeesCount.Text = "₹ " + cmd4.ExecuteScalar().ToString();

            con.Close();
        }

        private void Dashboard_Load_1(object sender, EventArgs e)
        {
            LoadStatistics();
            tlpStatistics.BackColor = Color.FromArgb(120, Color.White);
        }

        private async void btnReport_Click(object sender, EventArgs e)
        {

            this.Cursor = Cursors.AppStarting;
            // Loader show
            lblLoading.Visible = true;
            lblLoading.Text = "Loading Report Module...";

            await Task.Delay(100); // UI update

            ReportModuleModernForm reportForm = new ReportModuleModernForm();

            reportForm.Opacity = 0;
            reportForm.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            reportForm.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;

            lblLoading.Visible = false;

        }

        private async void btnFee_Click(object sender, EventArgs e)
        {
                        
            this.Cursor = Cursors.AppStarting;

            // Loader show
            lblLoading.Visible = true;
            lblLoading.Text = "Loading Fees Module...";

            await Task.Delay(100); // UI update

            FeesForm feeForm = new FeesForm();

            feeForm.Opacity = 0;
            feeForm.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            feeForm.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;

            lblLoading.Visible = false;
        }

        private async void btnFaculty_Click(object sender, EventArgs e)
        {            
            this.Cursor = Cursors.AppStarting;

            // Loader show
            lblLoading.Visible = true;
            lblLoading.Text = "Loading Faculty Module...";

            await Task.Delay(100); // UI update

            Faculty faculty = new Faculty();

            faculty.Opacity = 0;
            faculty.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            faculty.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;

            lblLoading.Visible = false;
        }

        private async void btnFacility_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;

            // Loader show
            lblLoading.Visible = true;
            lblLoading.Text = "Loading Facility Module...";

            await Task.Delay(100); // UI update

            FacilityForm x = new FacilityForm();

            x.Opacity = 0;
            x.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            x.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;

            lblLoading.Visible = false;                       
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }
    }
}
