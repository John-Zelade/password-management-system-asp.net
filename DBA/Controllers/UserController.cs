using DBA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using System.Data.SqlClient;
using System.Data;
using System.Security.Principal;
using Microsoft.Extensions.Logging; // Import the namespace

namespace DBA.Controllers
{
    public class UserController : Controller
    {
        SqlConnection con = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;

        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        void connectionString()
        {
            con.ConnectionString = "Data Source=DESKTOP-MPR7PCV\\SQLEXPRESS;Initial Catalog=DBAFinalProj;Integrated Security=True;";
        }

        public IActionResult Dashboard()
        {
            return View ();
        }
        public IActionResult AddNewAccount()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddAccount(account acc)
        {
            try
            {
                connectionString();
                con.Open();
                cmd.Connection = con;

                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;

                //INSERT_SP_ACCOUNTS_INFO - is used to insert new accounts details to be stored in password management system.
                cmd.CommandText = "INSERT_SP_ACCOUNTS_INFO";
                cmd.Parameters.AddWithValue("@url", acc.url);
                cmd.Parameters.AddWithValue("@account_email", acc.account_email);
                cmd.Parameters.AddWithValue("@password", acc.password);
                //Retrieve usersID from the form data
                int usersID = Convert.ToInt32(Request.Form["usersID"]);
                cmd.Parameters.AddWithValue("@usersID", usersID);


                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    con.Close();
                    ViewBag.ConnectionStatus = "Registered Successfully";
                    List<account> accountList = FetchAccountData(usersID);
                    return RedirectToAction("Accounts", "User", new { userId = usersID });
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
        public IActionResult UpdateUser(int accountId)
        {
            var account = FetchAccountById(accountId);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }



        [HttpPost]
        public IActionResult UpdateUserAccount(account acc)
        {
            try
            {
                if (acc == null)
                {
                    return NotFound();
                }

                connectionString();
                con.Open();
                cmd.Connection = con;

                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;

                //UPDATE_SP_ACCOUNTS_INFO- is used to update the existing account records in password management system.
                cmd.CommandText = "UPDATE_SP_ACCOUNTS_INFO";
                cmd.Parameters.AddWithValue("@url", acc.url);
                cmd.Parameters.AddWithValue("@account_email", acc.account_email);
                cmd.Parameters.AddWithValue("@password", acc.password);
                cmd.Parameters.AddWithValue("@accountID", acc.accountID);

                int userId= Convert.ToInt32(Request.Form["usersID"]);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    con.Close();
                    ViewBag.ConnectionStatus = "Updated Successfully";
                    List<account> accountList = FetchAccountData(userId);
                    return RedirectToAction("Accounts", "User", new { userId = userId });
                }
                else
                {
                    con.Close();
                    ViewBag.ConnectionStatus = "Update Failed.";
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

        private account FetchAccountById(int accountId)
        {
            account account = null;
            connectionString();

            using (con)
            {
                con.Open();
                cmd.Connection = con;

                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.Text;
                cmd.CommandType = CommandType.StoredProcedure;

                //SELECT_SP_USER_ACCOUNT_INFO- is used to select specific account based on its id.
                cmd.CommandText = "SELECT_SP_USER_ACCOUNT_INFO";
                cmd.Parameters.AddWithValue("@accountID", accountId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        account = new account
                        {
                            accountID = Convert.ToInt32(reader["accountID"]),
                            url = reader["url"].ToString(),
                            account_email = reader["account_email"].ToString(),
                            password = reader["password"].ToString(),
                            usersID = Convert.ToInt32(reader["usersID"])
                        };
                    }
                }
            }

            return account;
        }

        public IActionResult deleteAccount(int accountId, int userId)
        {
            _logger.LogInformation("Delete User Account: {userId}", userId);
            _logger.LogInformation("Delete Account: {accountId}", accountId);
            try
            {
                connectionString();
                con.Open();

                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;

                //DELETE_SP_ACCOUNTS_INFO - is used to delete account.
                cmd.CommandText ="DELETE_SP_ACCOUNTS_INFO";
                cmd.Parameters.AddWithValue("@accountID", accountId);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    List<account> accountList = FetchAccountData(userId);
                    return RedirectToAction("Accounts", "User", new { userId = userId });
                }
                else
                {
                    return NotFound();
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



        public IActionResult UserList()
        {
            List<Users> userList = FetchData();

            return View(userList);
        }
        public List<Users> FetchData()
        {
            List<Users> userList = new List<Users>();
            try
            {
                connectionString();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = "SELECT ID, Username, Email, Password, Fullname FROM Users";
                reader= cmd.ExecuteReader();
                while (reader.Read())
                {
                    Users user = new Users
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        Fullname = reader["Fullname"].ToString(),
                    };

                    userList.Add(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during data retrieval: {ex.Message}");
            }
            return userList;
        }

        [HttpGet]
        public IActionResult Accounts(string userId)
        {
            int id=int.Parse(userId);
            _logger.LogInformation("User ID: {userId}", userId);

            List<account> accountList = FetchAccountData(id);

            return View(accountList);

        }


        public List<account> FetchAccountData( int userId)
        {
            List<account> accountList = new List<account>();
            try
            {
                _logger.LogInformation("fetch ID Received: {userId}", userId);
                connectionString();
                con.Open();
                cmd.Connection = con;

                // SELECT_SP_ACCOUNTS_INFO- is used to display or view the list of account stored in password management system by specific user.
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SELECT_SP_ACCOUNTS_INFO";

                cmd.Parameters.AddWithValue("@userID", userId);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    account account= new account
                    {
                        accountID = Convert.ToInt32(reader["accountID"]),
                        url = reader["url"].ToString(),
                        account_email = reader["account_email"].ToString(),
                        password = reader["password"].ToString(),
                    };

                    accountList.Add(account);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during data retrieval: {ex.Message}");
            }
            return accountList;
        }
        public IActionResult Logout()
        {
            return RedirectToAction("Login", "Home");
        }

    }
}
