using System.Data.SqlClient;

namespace CollegeManagementSystem
{
    public class DBConnection
    {
        public static SqlConnection con = new SqlConnection(
            "Data Source=.\\SQLEXPRESS;Initial Catalog=CollegeDB;Integrated Security=True"
        );
    }
}
