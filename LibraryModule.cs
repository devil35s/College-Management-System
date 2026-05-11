using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CollegeManagementSystem
{
    public partial class LibraryModule : Form
    {
        string cs = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True;TrustServerCertificate=True";

        public LibraryModule()
        {
            InitializeComponent();

            this.Load += LibraryModule_Load;

            btnIssueBook.Click += btnIssueBook_Click;
            

            txtSearch.TextChanged += txtBookSearch_TextChanged;
            txtStudentId.TextChanged += txtStudentId_TextChanged;

            dgvIssuedBooks.CellContentClick += dgvIssuedBooks_CellContentClick;
        }

        private void LibraryModule_Load(object sender, EventArgs e)
        {
            dtpIssueDate.Value = DateTime.Now;
            dtpReturnDate.Value = DateTime.Now.AddDays(7);

            LoadBooksGrid();
            LoadBookDropdown();
            LoadIssuedBooksGrid();
            LoadDashboardCounts();
            StyleDataGrids();
            cmbAvailability.Items.Clear();
            cmbAvailability.Items.Add("Available");
           

            cmbAvailability.SelectedIndex = 0; // default

            txtSearch.Text = "Search by Title / Author / Category";
            txtSearch.ForeColor = Color.Gray;
        }

        private void LoadDashboardCounts()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                SqlCommand cmd1 = new SqlCommand("SELECT ISNULL(SUM(TotalCopies),0) FROM dbo.LibraryBooks WHERE IsActive = 1", con);
                lblTotalBooksValue.Text = cmd1.ExecuteScalar().ToString();

                SqlCommand cmd2 = new SqlCommand("SELECT ISNULL(SUM(AvailableCopies),0) FROM dbo.LibraryBooks WHERE IsActive = 1", con);
                lblAvailableBooksValue.Text = cmd2.ExecuteScalar().ToString();

                SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM dbo.LibraryIssuedBooks WHERE CAST(IssueDate AS DATE)=CAST(GETDATE() AS DATE)", con);
                lblIssuedTodayValue.Text = cmd3.ExecuteScalar().ToString();

                SqlCommand cmd4 = new SqlCommand("SELECT COUNT(*) FROM dbo.Student", con);
                lblTotalStudentsValue.Text = cmd4.ExecuteScalar().ToString();
            }
        }

        private void LoadBookDropdown()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"SELECT BookId, BookTitle 
                                 FROM dbo.LibraryBooks
                                 WHERE IsActive = 1 AND AvailableCopies > 0
                                 ORDER BY BookTitle";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbIssueBook.DataSource = dt;
                cmbIssueBook.DisplayMember = "BookTitle";
                cmbIssueBook.ValueMember = "BookId";
                cmbIssueBook.SelectedIndex = -1;
            }
        }

        
        private void LoadBooksGrid()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT 
                        b.BookId,
                        b.BookTitle AS [Book Title],
                        b.AuthorName AS [Author],
                        b.CategoryName AS [Category],
                        b.TotalCopies AS [Total Copies],
                        b.AvailableCopies AS [Available Copies],
                        b.ShelfNo AS [Shelf No],
                        b.ISBNNo AS [ISBN],
                        CASE 
                            WHEN b.AvailableCopies > 0 THEN 'Available'
                            ELSE 'Issued'
                        END AS [Availability]
                    FROM dbo.LibraryBooks b
                    WHERE b.IsActive = 1
                    ORDER BY b.BookTitle";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvBooks.DataSource = dt;

                if (dgvBooks.Columns["BookId"] != null)
                    dgvBooks.Columns["BookId"].Visible = false;
            }
        }        

        private void txtBookSearch_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txtStudentId_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                txtStudentName.Text = "";
                return;
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = "SELECT FirstName + ' ' + LastName FROM dbo.Student WHERE EnrollmentNo = @EnrollmentNo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@EnrollmentNo", txtStudentId.Text.Trim());

                con.Open();
                object result = cmd.ExecuteScalar();

                txtStudentName.Text = result != null ? result.ToString() : "";
            }
        }
        private void btnIssueBook_Click(object sender, EventArgs e)
        {
            try
            {
                string enroll = txtStudentId.Text.Trim();

                if (enroll == "" || cmbIssueBook.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill all fields");
                    return;
                }

                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    // ✅ 1. GET STUDENT ID
                    SqlCommand cmdGetStudent = new SqlCommand(
                        "SELECT StudentId FROM Student WHERE EnrollmentNo=@enroll", con);

                    cmdGetStudent.Parameters.AddWithValue("@enroll", enroll);

                    object studentObj = cmdGetStudent.ExecuteScalar();

                    if (studentObj == null)
                    {
                        MessageBox.Show("Student not found!");
                        return;
                    }

                    int studentId = Convert.ToInt32(studentObj);

                    // ✅ 2. GET BOOK ID (direct from dropdown)
                    int bookId = Convert.ToInt32(cmbIssueBook.SelectedValue);

                    // ✅ 3. CHECK AVAILABLE COPIES
                    SqlCommand cmdCheck = new SqlCommand(
                        "SELECT AvailableCopies FROM LibraryBooks WHERE BookId=@book", con);

                    cmdCheck.Parameters.AddWithValue("@book", bookId);

                    int available = Convert.ToInt32(cmdCheck.ExecuteScalar());

                    if (available <= 0)
                    {
                        MessageBox.Show("Book not available!");
                        return;
                    }

                    // ✅ 4. CHECK DUPLICATE ISSUE
                    SqlCommand cmdDuplicate = new SqlCommand(
                        "SELECT COUNT(*) FROM LibraryIssuedBooks WHERE StudentId=@studentId AND BookId=@book AND Status='Issued'", con);

                    cmdDuplicate.Parameters.AddWithValue("@studentId", studentId);
                    cmdDuplicate.Parameters.AddWithValue("@book", bookId);

                    int alreadyIssued = (int)cmdDuplicate.ExecuteScalar();

                    if (alreadyIssued > 0)
                    {
                        MessageBox.Show("This book is already issued to this student!");
                        return;
                    }

                    // ✅ 5. INSERT ISSUE RECORD
                    SqlCommand cmdInsert = new SqlCommand(
                        "INSERT INTO LibraryIssuedBooks (StudentId, BookId, IssueDate, ReturnDate, Status) VALUES (@studentId,@book,@issue,@return,'Issued')", con);

                    cmdInsert.Parameters.AddWithValue("@studentId", studentId);
                    cmdInsert.Parameters.AddWithValue("@book", bookId);
                    cmdInsert.Parameters.AddWithValue("@issue", dtpIssueDate.Value);
                    cmdInsert.Parameters.AddWithValue("@return", dtpReturnDate.Value);

                    cmdInsert.ExecuteNonQuery();

                    // ✅ 6. UPDATE AVAILABLE COPIES
                    SqlCommand cmdUpdate = new SqlCommand(
                        "UPDATE LibraryBooks SET AvailableCopies = AvailableCopies - 1 WHERE BookId=@book", con);

                    cmdUpdate.Parameters.AddWithValue("@book", bookId);
                    cmdUpdate.ExecuteNonQuery();

                    MessageBox.Show("Book Issued Successfully!");

                    // 🔄 refresh
                    LoadBooksGrid();
                    LoadIssuedBooksGrid();
                    LoadBookDropdown();
                    LoadDashboardCounts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadIssuedBooksGrid()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
            SELECT 
                i.IssueId,
                s.FirstName + ' ' + s.LastName AS [Student Name],
                b.BookTitle AS [Book Title],
                b.AuthorName AS [Author],
                i.IssueDate AS [Issue Date],
                i.ReturnDate AS [Return Date],
                i.Status
            FROM dbo.LibraryIssuedBooks i
            INNER JOIN dbo.Student s ON i.StudentId = s.StudentId
            INNER JOIN dbo.LibraryBooks b ON i.BookId = b.BookId
            WHERE i.Status = 'Issued'
            ORDER BY i.IssueId DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvIssuedBooks.DataSource = dt;

                if (dgvIssuedBooks.Columns["IssueId"] != null)
                    dgvIssuedBooks.Columns["IssueId"].Visible = false;

                if (!dgvIssuedBooks.Columns.Contains("Action"))
                {
                    DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                    btn.Name = "Action";
                    btn.HeaderText = "Action";
                    btn.Text = "Return";
                    btn.UseColumnTextForButtonValue = true;
                    dgvIssuedBooks.Columns.Add(btn);
                }
            }
        }

        private void dgvIssuedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvIssuedBooks.Columns[e.ColumnIndex].Name == "Action")
            {
                int issueId = Convert.ToInt32(dgvIssuedBooks.Rows[e.RowIndex].Cells["IssueId"].Value);

                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    SqlTransaction trans = con.BeginTransaction();

                    try
                    {
                        string getBookIdQuery = "SELECT BookId FROM dbo.LibraryIssuedBooks WHERE IssueId = @IssueId";
                        SqlCommand cmd0 = new SqlCommand(getBookIdQuery, con, trans);
                        cmd0.Parameters.AddWithValue("@IssueId", issueId);
                        int bookId = Convert.ToInt32(cmd0.ExecuteScalar());

                        string updateIssueQuery = @"
                            UPDATE dbo.LibraryIssuedBooks
                            SET Status = 'Returned',
                                ActualReturnDate = CAST(GETDATE() AS DATE)
                            WHERE IssueId = @IssueId";

                        SqlCommand cmd1 = new SqlCommand(updateIssueQuery, con, trans);
                        cmd1.Parameters.AddWithValue("@IssueId", issueId);
                        cmd1.ExecuteNonQuery();

                        string updateBookQuery = @"
                            UPDATE dbo.LibraryBooks
                            SET AvailableCopies = AvailableCopies + 1
                            WHERE BookId = @BookId";

                        SqlCommand cmd2 = new SqlCommand(updateBookQuery, con, trans);
                        cmd2.Parameters.AddWithValue("@BookId", bookId);
                        cmd2.ExecuteNonQuery();

                        trans.Commit();

                        MessageBox.Show("Book returned successfully.");

                        LoadBooksGrid();
                        LoadBookDropdown();
                        LoadIssuedBooksGrid();
                        LoadDashboardCounts();
                        
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void btnAddBook_Click(object sender, EventArgs e)
        {            
            LoadBooksGrid();
            LoadBookDropdown();
            LoadIssuedBooksGrid();
            LoadDashboardCounts();
        }

        private void StyleDataGrids()
        {
            DataGridView[] grids = { dgvBooks, dgvIssuedBooks };

            foreach (DataGridView dgv in grids)
            {
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(10, 44, 92);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgv.ColumnHeadersHeight = 36;

                dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
                dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
                dgv.RowTemplate.Height = 30;
                dgv.RowHeadersVisible = false;
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgv.MultiSelect = false;
                dgv.AllowUserToAddRows = false;
                dgv.AllowUserToDeleteRows = false;
                dgv.BackgroundColor = Color.White;
                dgv.BorderStyle = BorderStyle.None;
                dgvBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
        }

        private void cmbAuthor_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnAddBook_Click_1(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtBookTitle.Text) ||
                string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                MessageBox.Show("Book Title and Author are required!");
                return;
            }

            int totalCopies = (int)numTotalCopies.Value;

            if (totalCopies <= 0)
            {
                MessageBox.Show("Total Copies must be greater than 0!");
                return;
            }

            string availability = cmbAvailability.SelectedItem.ToString();

            // Logic sync
            int availableCopies = 0;

            if (availability == "Available")
                availableCopies = totalCopies;
            else
                availableCopies = 0;

            string conStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True";

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();

                string query = @"INSERT INTO LibraryBooks
        (BookTitle, AuthorName, CategoryName, TotalCopies, AvailableCopies, ShelfNo, ISBNNo)
        VALUES
        (@BookTitle, @AuthorName, @Category, @TotalCopies, @AvailableCopies, @ShelfNo, @ISBNNo)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@BookTitle", txtBookTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@AuthorName", txtAuthor.Text.Trim());
                    cmd.Parameters.AddWithValue("@Category", txtCategory.Text.Trim());
                    cmd.Parameters.AddWithValue("@TotalCopies", totalCopies);
                    cmd.Parameters.AddWithValue("@AvailableCopies", availableCopies);
                    cmd.Parameters.AddWithValue("@ShelfNo", txtShelfNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@ISBNNo", txtISBN.Text.Trim());
                    
                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Book Added Successfully!");

                        ClearFields();
                        LoadBooksGrid();
                        LoadBookDropdown();
                        LoadDashboardCounts();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                            MessageBox.Show("ISBN already exists!");
                        else
                            MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void ClearFields()
        {
            txtBookTitle.Clear();
            txtAuthor.Clear();
            txtCategory.Clear();
            txtShelfNo.Clear();
            txtISBN.Clear();
            numTotalCopies.Value = 0;
            cmbAvailability.SelectedIndex = 0;
        }
        
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search by Title / Author / Category")
                return;

            using (SqlConnection con = new SqlConnection(cs))
            {
                string searchText = txtSearch.Text.Trim();

                string query = @"
                    SELECT 
                        b.BookId,
                        b.BookTitle AS [Book Title],
                        b.AuthorName AS [Author],
                        b.CategoryName AS [Category],
                        b.TotalCopies AS [Total Copies],
                        b.AvailableCopies AS [Available Copies],
                        b.ShelfNo AS [Shelf No],
                        b.ISBNNo AS [ISBN],
                        CASE 
                            WHEN b.AvailableCopies > 0 THEN 'Available'
                            ELSE 'Issued'
                        END AS [Availability]
                    FROM dbo.LibraryBooks b
                    WHERE b.IsActive = 1
                    AND (b.BookTitle LIKE @Search OR b.AuthorName LIKE @Search OR b.CategoryName LIKE @Search)
                    ORDER BY b.BookTitle";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvBooks.DataSource = dt;

                if (dgvBooks.Columns["BookId"] != null)
                    dgvBooks.Columns["BookId"].Visible = false;
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search by Title / Author / Category")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search by Title / Author / Category";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = System.DateTime.Now.ToString("dd-MMM-yyyy hh:mm");
        }



        //   private void btnAddBook_Clickobject sender, EventArgs e)
        //    {
        //        try
        //        {
        //            string title = txtBookTitle.Text.Trim();
        //            string author = txtAuthor.Text.Trim();
        //            string category = cmbCategory.Text;
        //            int total = Convert.ToInt32(txtTotalCopies.Text);

        //            if (title == "" || author == "" || category == "")
        //            {
        //                MessageBox.Show("Please fill all fields");
        //                return;
        //            }

        //            SqlConnection con = new SqlConnection("your_connection_string_here");
        //            con.Open();

        //            // ❗ Check duplicate book
        //            SqlCommand checkCmd = new SqlCommand(
        //                "SELECT COUNT(*) FROM LibraryBooks WHERE BookTitle=@title", con);
        //            checkCmd.Parameters.AddWithValue("@title", title);

        //            int exists = (int)checkCmd.ExecuteScalar();

        //            if (exists > 0)
        //            {
        //                MessageBox.Show("Book already exists!");
        //                con.Close();
        //                return;
        //            }

        //            // ✅ Get CategoryId
        //            SqlCommand catCmd = new SqlCommand(
        //                "SELECT CategoryId FROM LibraryCategories WHERE CategoryName=@cat", con);
        //            catCmd.Parameters.AddWithValue("@cat", category);

        //            object catIdObj = catCmd.ExecuteScalar();

        //            if (catIdObj == null)
        //            {
        //                MessageBox.Show("Invalid Category!");
        //                con.Close();
        //                return;
        //            }

        //            int catId = Convert.ToInt32(catIdObj);

        //            // ✅ Insert Book
        //            SqlCommand cmd = new SqlCommand(
        //                @"INSERT INTO LibraryBooks 
        //        (BookTitle, AuthorName, CategoryId, TotalCopies, AvailableCopies) 
        //        VALUES (@title,@auth,@cat,@total,@total)", con);

        //            cmd.Parameters.AddWithValue("@title", title);
        //            cmd.Parameters.AddWithValue("@auth", author);
        //            cmd.Parameters.AddWithValue("@cat", catId);
        //            cmd.Parameters.AddWithValue("@total", total);

        //            cmd.ExecuteNonQuery();

        //            MessageBox.Show("Book Added Successfully!");

        //            con.Close();

        //            // 🔄 Refresh grid
        //            LoadBooksGrid();

        //            // 🧹 Clear fields
        //            txtBookTitle.Text = "";
        //            txtAuthor.Text = "";
        //            cmbCategory.SelectedIndex = -1;
        //            txtTotalCopies.Text = "";
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }

        //}
    }
}