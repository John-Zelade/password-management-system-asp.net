using DBA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using System.Data.SqlClient;
using System.Data;

namespace DBA.Controllers
{
    public class HomeController : Controller
    {
        SqlConnection con = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;

   

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        void connectionString()
        {
            con.ConnectionString = "Data Source=DESKTOP-MPR7PCV\\SQLEXPRESS;Initial Catalog=DBAFinalProj;Integrated Security=True;";
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Users user)
        {
            connectionString();
            con.Open();
            cmd.Connection = con;
            cmd.CommandType = CommandType.StoredProcedure;

            /*** VALIDATE_SP_USER- is used to validate the credentials of user if it is authorized user. ****/
            cmd.CommandText = "VALIDATE_SP_USER";
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Password", user.Password);
            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int userId = Convert.ToInt32(reader["ID"]);

                string username = reader.GetString(reader.GetOrdinal("Username"));
                con.Close();

                return Json(new { Status = "Success", UserId = userId, Username = username });
            }
            else
            {
                con.Close();
                return View();
            }
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Users user)
        {
            try
            {
                connectionString();
                con.Open();
                cmd.Connection = con;

             
                cmd.CommandType = CommandType.StoredProcedure;


                // SELECT_COUNT_SP_ACCOUNT_INFO- Check if the username or email already exists.
                cmd.CommandText = "SELECT_COUNT_SP_ACCOUNT_INFO ";
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@Email", user.Email);

                int userCount = (int)cmd.ExecuteScalar();

                if (userCount > 0)
                {
                    con.Close();
                    ViewBag.ConnectionStatus = "This account already exists.";
                    return View();
                }

                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;

                // REGISTER_SP_USER_INFO- use to register the user.
                cmd.CommandText = "REGISTER_SP_USER_INFO";
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@Fullname", user.Fullname);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    con.Close();
                    ViewBag.ConnectionStatus = "Registered Successfully";
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    con.Close();
                    ViewBag.ConnectionStatus = "Registration Failed.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
            finally
            {
                con.Close();
            }
        }
        public IActionResult Dashboard()
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
