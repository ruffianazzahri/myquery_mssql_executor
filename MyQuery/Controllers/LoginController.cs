using Microsoft.AspNetCore.Mvc;
using Synergy_Test.Function;
using Synergy_Test.Models;
using System.Data.SqlClient;
using System.Text;
using System.Security.Cryptography;

namespace Synergy_Test.Controllers
{
    public class LoginController : Controller
    {
        private readonly DatabaseAccessLayer dbAccess = new DatabaseAccessLayer();

        // View of login
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Index()
        {
            return View();
        }

        //Authenticate before redirect to dashboard
        [HttpPost]
        public IActionResult Index(LoginModel model)
        {
            ViewBag.Result = null;

            if (ModelState.IsValid)
            {
                string connectionString = dbAccess.DbConnection();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM [test].[dbo].[Staff] WHERE Username = @Username AND Password = @Password";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Password", model.Password);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var loginUser = new LoginModel();
                            loginUser.StaffID = Convert.ToInt32(reader["StaffID"]);
                            loginUser.FirstName = reader["FirstName"].ToString();
                            loginUser.LastName = reader["LastName"].ToString();
                            loginUser.Username = reader["UserName"].ToString();
                            loginUser.Role = reader["Role"].ToString();
                                    loginUser.ProfilePhoto = reader["ProfilePhoto"].ToString();
                                    loginUser.Last_Login = reader["Last_Login"] != DBNull.Value ? (DateTime?)reader["Last_Login"] : null;
                            Console.WriteLine(reader["Role"].ToString());
                            HttpContext.Session.SetString("StaffID", loginUser.StaffID.ToString());
                            HttpContext.Session.SetString("Username", loginUser.Username);
                            HttpContext.Session.SetString("FirstName", loginUser.FirstName);
                            HttpContext.Session.SetString("LastName", loginUser.LastName);
                            HttpContext.Session.SetString("Role", loginUser.Role);
                            HttpContext.Session.SetString("ProfilePhoto", loginUser.ProfilePhoto);
                                    DateTime currentDateTime = GetCurrentDateTime();
                                    UpdateLastLogin(loginUser.StaffID, currentDateTime);
                                }

                        // Close the SqlDataReader
                        reader.Close();



                        if (HttpContext.Session.GetString("Role") == "User") //if the role is user
                        {
                            return RedirectToAction("Index", "Dashboard");
                        }
                        else if (HttpContext.Session.GetString("Role") == "Admin") //If the role is admin
                        {
                            return RedirectToAction("IndexAdmin", "Dashboard");
                        }
                        else
                        {
                            ViewBag.Result = "You are not permitted to access the website!"; //Outside of user and admin
                        }
                    }
                    else
                    {
                        ViewBag.Result = "Invalid username or password."; //Wrong username or password
                    }
                }
            }
            else
            {
                ViewBag.Result = "Make sure the input is correct and try again."; //Unknown input
            }

            return View(model);
        }

private DateTime GetCurrentDateTime()
{
    // You can customize this function to get the current datetime based on your application's timezone requirements
    return DateTime.Now;
}

private void UpdateLastLogin(int staffId, DateTime lastLogin) //Track login time
{
    string connectionString = dbAccess.DbConnection();

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();
        using (SqlCommand updateCmd = new SqlCommand("UPDATE [test].[dbo].[Staff] SET Last_Login = @LastLogin WHERE StaffID = @StaffID", conn))
        {
            updateCmd.Parameters.AddWithValue("@LastLogin", lastLogin);
            updateCmd.Parameters.AddWithValue("@StaffID", staffId);
            updateCmd.ExecuteNonQuery();
        }
    }
}


        public IActionResult SignOut() //Sign out 
        {
            string staffid = HttpContext.Session.GetString("StaffID");
            string username = HttpContext.Session.GetString("Username");
            string role = HttpContext.Session.GetString("Role");


            HttpContext.Session.Clear();

            if (HttpContext.Session != null)
            {
                HttpContext.Session.Clear();
            }

            return View("Index");
        }

    }
}
