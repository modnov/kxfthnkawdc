using System.Runtime.InteropServices.JavaScript;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace kxfthnkawdc.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : Controller
{
    private readonly ILogger<ChatController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ChatController(ILogger<ChatController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public IEnumerable<ChatMessage> Get()
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
                User = new User()
                {
                    Id = (int)messageReader["sender_id"]
                }
            });
        }

        return messages;
    }

    [HttpPost]
    public async Task Post(ChatMessage message)
    {
        try
        {
            using var command =
                new NpgsqlCommand($"insert into messages (sender_id, content, date) values ($1, $2, $3)", _dbContext.Connection)
                {
                    Parameters =
                    {
                        new()
                        {
                            Value = message.User.Id
                        },
                        new()
                        {
                            Value = message.Content
                        },
                        new()
                        {
                            Value = message.Date
                        }
                    }
                };
            //await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
