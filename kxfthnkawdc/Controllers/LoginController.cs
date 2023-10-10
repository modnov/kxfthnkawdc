using System.Runtime.InteropServices.JavaScript;
using System.Text;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;

namespace kxfthnkawdc.Controllers;

[Route("login")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public LoginController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private bool IsAlreadyLogined => Request.HttpContext.Session.GetInt32("id") > 0;

    private void CreateUserSession(int id, string username)
    {
        HttpContext.Session.SetInt32("id", id);
        HttpContext.Session.SetString("username", username.ToLower());
        HttpContext.Session.CommitAsync().Wait();
    }

    [HttpPost]
    [Route("login")]
    public string Login([FromHeader] string username, [FromHeader] string password)
    {
        if (IsAlreadyLogined)
            return "Success";

        using var command =
            _dbContext.DataSource.CreateCommand(
                "SELECT * FROM users WHERE name=(@username) AND password=(@password) LIMIT 1");
        command.Parameters.AddWithValue("username", username.ToLower());
        command.Parameters.AddWithValue("password", password);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            CreateUserSession((int)reader["id"], username);
            return "Success";
        }

        return "Wrong username or password";
    }

    [HttpPost]
    [Route("register")]
    public string Register([FromHeader] string username, [FromHeader] string password)
    {
        if (IsAlreadyLogined)
            return "Success";

        if (UserAlreadyExist())
            return "Success";

        if (UsernameIsAvailable() == false)
            return "This username is being used by another user";

        CompleteRegistration();
        return "Success";

        bool UserAlreadyExist()
        {
            using var command =
                _dbContext.DataSource.CreateCommand(
                    "SELECT id, name FROM users WHERE name=(@username) AND password=(@password)");
            command.Parameters.AddWithValue("username", username.ToLower());
            command.Parameters.AddWithValue("password", password);
            var result = command.ExecuteScalar();
            if (result != null && (int)result > 0)
            {
                return true;
            }

            return false;
        }

        bool UsernameIsAvailable()
        {
            using var command =
                _dbContext.DataSource.CreateCommand(
                    "SELECT id FROM users WHERE name=(@username)");
            command.Parameters.AddWithValue("username", username.ToLower());
            var result = command.ExecuteScalar();
            if (result != null && (int)result > 0)
            {
                return false;
            }

            return true;
        }

        void CompleteRegistration()
        {
            using var command =
                _dbContext.DataSource.CreateCommand(
                    "INSERT INTO users (name, password) VALUES (@username, @password);" +
                    "SELECT id FROM users WHERE name=(@username) AND password=(@password)");
            command.Parameters.AddWithValue("username", username.ToLower());
            command.Parameters.AddWithValue("password", password);
            var newUserId = command.ExecuteScalar();

            CreateUserSession((int)newUserId, username);
        }
    }
}
