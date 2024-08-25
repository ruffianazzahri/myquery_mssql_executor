using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Synergy_Test.Function;
using Synergy_Test.Models;

namespace Synergy_Test.Controllers
{
    public class HomeController : Controller
    {
        public string ConnectionString = "Data Source=LAPTOP-MU13V76B\\MSSQLSERVER01;Initial Catalog=test;User ID=LAPTOP-MU13V76B\\Zephyrus G14;Integrated Security=True;MultipleActiveResultSets=true;";

        private string DbConnection()
        {
            var dbAccess = new DatabaseAccessLayer();

            string dbString = dbAccess.ConnectionString;

            return dbString;
        }


        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
  
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
