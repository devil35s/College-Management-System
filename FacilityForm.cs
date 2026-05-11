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
    public partial class FacilityForm : Form
    {
        string conStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";
        public FacilityForm()
        {
            InitializeComponent();
        }
        private void LoadDashboard()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();

                // Total Books (active)
                SqlCommand cmdbook = new SqlCommand("SELECT ISNULL(SUM(TotalCopies),0) FROM dbo.LibraryBooks WHERE IsActive = 1", con);
                lblTotalBooks.Text = cmdbook.ExecuteScalar().ToString();

                // Books Issued Today
                SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM dbo.LibraryIssuedBooks WHERE CAST(IssueDate AS DATE)=CAST(GETDATE() AS DATE)", con);
                lblBookIssue.Text = cmd3.ExecuteScalar().ToString();

                // Total Facilities
                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Facilities", con);
                lblTotalFacilities.Text = cmd1.ExecuteScalar().ToString();

                // Players Assigned (unique students)
                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(DISTINCT StudentID) FROM FacilityBookings", con);
                lblPlayersAssigned.Text = cmd2.ExecuteScalar().ToString();

                //Transport Usage
                SqlCommand cmdRoutes = new SqlCommand(
            "SELECT COUNT(DISTINCT Route) FROM TransportVehicle", con);
                lblVehicleRoutes.Text = cmdRoutes.ExecuteScalar().ToString();

                SqlCommand cmdStudents = new SqlCommand(
                    "SELECT COUNT(*) FROM TransportAssign", con);
                lblTransportStudents.Text = cmdStudents.ExecuteScalar().ToString();
                //Lab Usage

                // 🔵 TOTAL SYSTEMS
                SqlCommand totalCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Systems", con);
                lblTotalSystems.Text = totalCmd.ExecuteScalar().ToString();

                // 🟢 AVAILABLE SYSTEMS
                SqlCommand availCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Systems WHERE Status='Available'", con);
                lblAvailableSystems.Text = availCmd.ExecuteScalar().ToString();
            }
        }
        private async void btnLibrary_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;

            await Task.Delay(100); // UI update

            LibraryModule library = new LibraryModule();

            library.Opacity = 0;
            library.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            library.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;

        }

        private async void btnSports_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;

            await Task.Delay(100); // UI update

            SportsForm s = new SportsForm();

            s.Opacity = 0;
            s.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            s.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;
        }

        private void FacilityForm_Load(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        private async void btnLab_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;

            await Task.Delay(100); // UI update

            ComputerLab c = new ComputerLab();

            c.Opacity = 0;
            c.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            c.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;
        }

        private async void btnTransport_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;

            await Task.Delay(100); // UI update

            Transport t = new Transport();

            t.Opacity = 0;
            t.Show(); // hidden open

            // simulate heavy load (replace with real load later)
            await Task.Delay(1500);

            t.Opacity = 1; // ab dikhega
            this.Cursor = Cursors.Default;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }
    }
}
