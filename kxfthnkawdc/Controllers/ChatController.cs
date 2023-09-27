using System.Runtime.InteropServices.JavaScript;
using System.Web;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace kxfthnkawdc.Controllers;

[Route("chat")]
// TODO [Authorize]
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

        return messages;
    }

    [HttpGet]
    [Route("chats")]
    public IEnumerable<Chat> GetChats()
    {
        int clientId = Request.HttpContext.Session.GetInt32("id")!.Value;
        using var command =
            _dbContext.DataSource.CreateCommand(
                "select * from chats where first_user_id = @client_id or second_user_id = @client_id");
        command.Parameters.Add(new NpgsqlParameter()
        {
            ParameterName = "client_id",
            Value = clientId
        });
        using var messageReader = command.ExecuteReader();
        var chats = new List<Chat>();
        while (messageReader.Read())
        {
            chats.Add(new Chat()
            {
                ChatId = (int)messageReader["chat_id"],
                Interlocutor = new User(_dbContext)
                {
                    Id = (int)messageReader["first_user_id"] == clientId
                        ? (int)messageReader["second_user_id"]
                        : (int)messageReader["first_user_id"]
                }
            });
        }

        return chats;
    }

    [HttpPost]
    [Route("send")]
    public async Task PostMessage([FromHeader] string content, [FromHeader] int chatId)
    {
        using var command = _dbContext.DataSource.CreateCommand(
            $"insert into messages (sender_id, content, date, chat_id) values (@sender_id, @content, @date, @chat_id)");

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
            Value = DateTime.UtcNow
        });
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "chat_id",
            Value = chatId
        });

        await command.ExecuteNonQueryAsync();
    }
}
