using System.Runtime.InteropServices.JavaScript;
using System.Text;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;

namespace kxfthnkawdc.Controllers;

[Route("login")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public LoginController(ILogger<LoginController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost]
    [Route("verify")]
    public ActionResult Login([FromHeader] string username, [FromHeader] string password)
    {
        using var command =
            _dbContext.DataSource.CreateCommand(
                "select * from users where name=(@username) and password=(@password)");
        command.Parameters.AddWithValue("username", username.ToLower());
        command.Parameters.AddWithValue("password", password);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            HttpContext.Session.SetInt32("id", (int)reader["id"]);
            HttpContext.Session.SetString("username", username.ToLower());
            HttpContext.Session.CommitAsync();
            return RedirectPermanent("/chat.html");
        }

        return new UnauthorizedResult();
    }
}
