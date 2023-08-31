using kxfthnkawdc.Models;
using Microsoft.AspNetCore.Mvc;

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
    [Route("Messages")]
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
                User = new User()
                {
                    Id = (int)messageReader["sender_id"]
                }
            });
        }

        return messages;
    }

    [HttpGet]
    [Route("Users")]
    public IEnumerable<ChatMessage> GetUsers()
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
}
