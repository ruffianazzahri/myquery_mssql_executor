using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Synergy_Test.Function;
using Synergy_Test.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography.Xml;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Newtonsoft.Json;

using System.Text;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Synergy_Test.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DatabaseAccessLayer dbAccess = new DatabaseAccessLayer();

        private string DbConnection()
        {
            var dbAccess = new DatabaseAccessLayer();

            string dbString = dbAccess.ConnectionString;

            return dbString;
        }


        private readonly ILogger<HomeController> _logger;

        public DashboardController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        //View
        public IActionResult Index()
        {

           return View();

        }

        public IActionResult IndexAdmin()
        {

            return View();

        }

        //End of View

        //Execute query while user has inputted
        [HttpPost]
        public ActionResult ExecuteQuery(string query)
        {
            string connectionString = dbAccess.DbConnection();

            if (string.IsNullOrWhiteSpace(query))
            {
                return Content("Invalid query. Query cannot be empty or whitespace.");
            }

            try
            {
                DataTable dataTable = new DataTable();

                if (!IsSelectQuery(query))
                {

                    if (!IsValidSqlQuery(query))
                    {
                        return Content("There something wrong with server. Please contact administrator, if error persist");
                    }

                    return Content("You can run SELECT query, please submit to Administrator if you want to run others query");
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }

                if (dataTable.Rows.Count > 0)
                {
                    string htmlTable = DataTableToHtml(dataTable);
                    return Content(htmlTable);
                }
                else
                {
                    return Content("No records found.");
                }
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }


        //Validating SELECT Query
        private bool IsSelectQuery(string query)
        {
 
            string[] keywords = query.Trim().Split(' ', '\t');
            string firstWord = keywords[0].ToUpper();


            return firstWord == "SELECT";
        }

        //Validating Query
        private bool IsValidSqlQuery(string query)
        {
            string[] sqlKeywords = { "SELECT", "UPDATE", "DELETE", "INSERT", "ALTER", "DROP", "CREATE" };
            foreach (var keyword in sqlKeywords)
            {
                if (query.ToUpper().Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        //Convert query to table
        private string DataTableToHtml(DataTable table)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table border='1' class='table table-striped table-bordered'>");
            html.Append("<thead class='thead-secondary'>");
            html.Append("<tr>");

            foreach (DataColumn column in table.Columns)
            {
                html.Append("<th>" + column.ColumnName + "</th>");
            }
            html.Append("</tr>");
            html.Append("</thead>");
            html.Append("<tbody>");

            foreach (DataRow row in table.Rows)
            {
                html.Append("<tr>");
                foreach (DataColumn column in table.Columns)
                {
                    string cellValue = row[column].ToString();

                    if (column.ColumnName.Equals("Password", StringComparison.OrdinalIgnoreCase))
                    {
                        html.Append("<td>Confidential</td>");
                    }
                    else if (IsImage(cellValue))
                    {
                        html.Append("<td><img src='/images/profile/" + cellValue + "' alt='Image' class='img-thumbnail' /></td>");
                    }
                    else
                    {
                        html.Append("<td>" + cellValue + "</td>");
                    }
                }
                html.Append("</tr>");
            }

            html.Append("</tbody>");
            html.Append("</table>");
            return html.ToString();
        }

        //Convert value to img
        private bool IsImage(string value)
        {
            return value.EndsWith(".jpg") || value.EndsWith(".png") || value.EndsWith(".gif");
        }

        //Get current time
        private DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }


        //Insert Requested Query (Non select must request to admin)
        [HttpPost]
        public ActionResult RequestedQuery(RequestedQueryModel request, DateTime submittedDate)
        {
            string connectionString = dbAccess.DbConnection();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO [dbo].[RequestedQuery] (Requester, Status, SubmittedDate, ExecutedDate, SQLQuery) " +
                                   "VALUES (@Requester, @Status, GETDATE(), @ExecutedDate, @SQLQuery)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Requester", request.Requester);
                        command.Parameters.AddWithValue("@Status", request.Status);
                        command.Parameters.AddWithValue("@SQLQuery", request.SQLQuery);

                        command.Parameters.AddWithValue("@ExecutedDate", DBNull.Value);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return Content("Request success.");
            }
            catch (Exception ex)
            {
                return Content("Error query request: " + ex.Message);
            }
        }


        //Filter requested query by username
        [HttpGet]
        public ActionResult GetRequestedQueriesByUsername(string requester_username)
        {
            List<RequestedQueryModel> requests = new List<RequestedQueryModel>();
            string query = "SELECT * FROM [test].[dbo].[RequestedQuery] WHERE Requester = @requester_username";

            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@requester_username", requester_username);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    RequestedQueryModel data = new RequestedQueryModel();
                    data.No = (int)reader["No"];
                    data.Requester = reader.IsDBNull(reader.GetOrdinal("Requester")) ? null : reader.GetString(reader.GetOrdinal("Requester"));
                    data.Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status"));
                    data.SubmittedDate = reader.IsDBNull(reader.GetOrdinal("SubmittedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("SubmittedDate"));
                    data.ExecutedDate = reader.IsDBNull(reader.GetOrdinal("ExecutedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExecutedDate"));
                    data.SQLQuery = reader.IsDBNull(reader.GetOrdinal("SQLQuery")) ? null : reader.GetString(reader.GetOrdinal("SQLQuery"));

                    requests.Add(data);
                }

                conn.Close();
            }

            return Json(requests);
        }


        //Get all requested queries (for admin side)
        [HttpGet]
        public ActionResult GetRequestedQueries()
        {
            List<RequestedQueryModel> requests = new List<RequestedQueryModel>();
            string query = "SELECT * FROM [test].[dbo].[RequestedQuery]";

            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    RequestedQueryModel data = new RequestedQueryModel();
                    data.No = (int)reader["No"];
                    data.Requester = reader.IsDBNull(reader.GetOrdinal("Requester")) ? null : reader.GetString(reader.GetOrdinal("Requester"));
                    data.Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status"));
                    data.SubmittedDate = reader.IsDBNull(reader.GetOrdinal("SubmittedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("SubmittedDate"));
                    data.ExecutedDate = reader.IsDBNull(reader.GetOrdinal("ExecutedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExecutedDate"));
                    data.SQLQuery = reader.IsDBNull(reader.GetOrdinal("SQLQuery")) ? null : reader.GetString(reader.GetOrdinal("SQLQuery"));

                    requests.Add(data);
                }

                conn.Close();
            }

            return Json(requests);
        }

        //Approve requested query
        [HttpPost]
        public IActionResult ApproveRequestedQuery(int queryNo)
        {
            string updateQuery = @"
                UPDATE [test].[dbo].[RequestedQuery] 
                SET Status = 'Approved', ExecutedDate = @ExecutedDate 
                WHERE No = @QueryNo";

            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                cmd.Parameters.AddWithValue("@ExecutedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@QueryNo", queryNo);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                conn.Close();

                if (rowsAffected > 0)
                {
                    return Ok("Query approved successfully.");
                }
                else
                {
                    return BadRequest("Failed to approve query.");
                }
            }
        }

        //Reject requested query
        [HttpPost]
        public IActionResult RejectRequestedQuery(int queryNo)
        {
            string updateQuery = @"
                UPDATE [test].[dbo].[RequestedQuery] 
                SET Status = 'Rejected', ExecutedDate = @ExecutedDate 
                WHERE No = @QueryNo";

            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                cmd.Parameters.AddWithValue("@ExecutedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@QueryNo", queryNo);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                conn.Close();

                if (rowsAffected > 0)
                {
                    return Ok("Query rejected successfully.");
                }
                else
                {
                    return BadRequest("Failed to reject query.");
                }
            }
        }

        //GEt more information of requested queries specifically
        [HttpGet]
        public ActionResult GetRequestedQueriesByUsernameDetail(string requester_username, int no)
        {
            List<RequestedQueryModel> requests = new List<RequestedQueryModel>();
            string query = "SELECT * FROM [test].[dbo].[RequestedQuery] WHERE Requester = @requester_username and No = @No";

            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@requester_username", requester_username);
                cmd.Parameters.AddWithValue("@No", no);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    RequestedQueryModel data = new RequestedQueryModel();
                    data.No = (int)reader["No"];
                    data.Requester = reader.IsDBNull(reader.GetOrdinal("Requester")) ? null : reader.GetString(reader.GetOrdinal("Requester"));
                    data.Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status"));
                    data.SubmittedDate = reader.IsDBNull(reader.GetOrdinal("SubmittedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("SubmittedDate"));
                    data.ExecutedDate = reader.IsDBNull(reader.GetOrdinal("ExecutedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExecutedDate"));
                    data.SQLQuery = reader.IsDBNull(reader.GetOrdinal("SQLQuery")) ? null : reader.GetString(reader.GetOrdinal("SQLQuery"));

                    requests.Add(data);
                }

                conn.Close();
            }

            return Json(requests);
        }

        //Save query for user
        [HttpPost]
        public ActionResult SaveQuery(SaveQueryModel save, DateTime Date)
        {
            string connectionString = dbAccess.DbConnection();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO [dbo].[SavedQuery] (SavedQuery, SavedName, Username, Date, Status) " +
                                   "VALUES (@SavedQuery, @SavedName, @UserName, GETDATE(), @Status)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SavedQuery", save.SavedQuery);
                        command.Parameters.AddWithValue("@SavedName", save.SavedName);
                        command.Parameters.AddWithValue("@Username", save.Username);
                        command.Parameters.AddWithValue("@Status", save.Status);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return Content("Query have been saved");
            }
            catch (Exception ex)
            {
                return Content("Error query save: " + ex.Message);
            }
        }

        public IActionResult SharedQuery()
        {
            return View();
        }

        //Get query that saved by user
        [HttpGet]
        public ActionResult GetSavedQueryByUser(string username)
        {
            List<SaveQueryModel> requests = new List<SaveQueryModel>();
            string query = "SELECT * FROM [test].[dbo].[SavedQuery] WHERE Username = @username";

            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    SaveQueryModel data = new SaveQueryModel();
                    data.No = (int)reader["No"];
                    data.SavedQuery = reader.IsDBNull(reader.GetOrdinal("SavedQuery")) ? null : reader.GetString(reader.GetOrdinal("SavedQuery"));
                    data.SavedName = reader.IsDBNull(reader.GetOrdinal("SavedName")) ? null : reader.GetString(reader.GetOrdinal("SavedName"));
                    data.Username = reader.IsDBNull(reader.GetOrdinal("Username")) ? null : reader.GetString(reader.GetOrdinal("Username"));
                    data.Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("Date"));
                    data.Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status"));

                    requests.Add(data);
                }

                conn.Close();
            }

            return Json(requests);
        }

    }




}

