using System.Runtime.InteropServices.JavaScript;
using System.Web;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace kxfthnkawdc.Controllers;

[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ChatController(ILogger<ChatController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    [Route("messages")]
    public IEnumerable<ChatMessage> GetMessages()
    {
        using var command = _dbContext.DataSource.CreateCommand("select * from messages order by id");
        using var messageReader = command.ExecuteReader();
        var messages = new List<ChatMessage>();
        while (messageReader.Read())
        {
            messages.Add(new ChatMessage
            {
                Id = (int)messageReader["id"],
                Content = (string)messageReader["content"],
                Date = (DateTime)messageReader["date"],
                User = new User(_dbContext)
                {
                    Id = (int)messageReader["sender_id"]
                }
            });
        }

        int clientId = Request.HttpContext.Session.GetInt32("id")!.Value;
        Response.Headers.Add("client_id", clientId.ToString());
        return messages;
    }

    [HttpPost]
    [Route("send")]
    public async Task PostMessage([FromHeader] string content)
    {
        try
        {
            using var command = _dbContext.DataSource.CreateCommand(
                $"insert into messages (sender_id, content, date) values (@sender_id, @content, @date)");

            command.Parameters.Add(new NpgsqlParameter
            {
                ParameterName = "sender_id",
                Value = Request.HttpContext.Session.GetInt32("id")!.Value
            });
            command.Parameters.Add(new NpgsqlParameter
            {
                ParameterName = "content",
                Value = HttpUtility.UrlDecode(content)
            });
            command.Parameters.Add(new NpgsqlParameter
            {
                ParameterName = "date",
                Value = DateTime.Now
            });

            await command.ExecuteNonQueryAsync();
            
            Response.Redirect("/messages");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
