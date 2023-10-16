using System.Runtime.InteropServices.JavaScript;
using System.Web;
using kxfthnkawdc.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace kxfthnkawdc.Controllers;

[Route("chat")]
[ApiController]
// TODO [Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    private int ClientId => Request.HttpContext.Session.GetInt32("id")!.Value;

    public ChatController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [Route("messages")]
    public IEnumerable<Message> GetMessages([FromBody] int chatId)
    {
        using var command = _dbContext.DataSource.CreateCommand(
            "SELECT * FROM messages AS m " +
            "INNER JOIN users AS u ON @client_id = u.id " +
            "WHERE m.chat_id = @chat_id " +
            "ORDER BY m.id");
        command.Parameters.AddWithValue("client_id", ClientId);
        command.Parameters.AddWithValue("chat_id", chatId);

        using var reader = command.ExecuteReader();

        var messages = new List<Message>();
        while (reader.Read())
        {
            messages.Add(new Message
            {
                Id = (int)reader["id"],
                Content = (string)reader["content"],
                Date = (DateTime)reader["date"],
                ChatId = chatId,
                User = new User()
                {
                    Id = (int)reader["sender_id"],
                    Name = (string)reader["name"]
                }
            });
        }

        return messages;
    }

    [HttpGet]
    [Route("chats")]
    public IEnumerable<Chat> GetChats()
    {
        using var command =
            _dbContext.DataSource.CreateCommand
            (
                "SELECT chat_id, name, date, content, first_user_id, second_user_id, sender_id " +
                "FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY m.chat_id ORDER BY m.date DESC) AS rn " +
                "FROM chats AS c " +
                "INNER JOIN messages AS m ON m.chat_id = c.id) AS mc " +
                "INNER JOIN users AS u ON u.id = mc.sender_id WHERE mc.rn = 1 AND " +
                "(first_user_id = @client_id OR second_user_id = @client_id) " +
                "ORDER BY date DESC");

        command.Parameters.AddWithValue("client_id", ClientId);

        using var reader = command.ExecuteReader();
        var chats = new List<Chat>();
        while (reader.Read())
        {
            chats.Add(new Chat()
            {
                ChatId = (int)reader["chat_id"],
                Interlocutor = new User()
                {
                    Id = (int)reader["first_user_id"] == ClientId
                        ? (int)reader["second_user_id"]
                        : (int)reader["first_user_id"],
                    Name = (string)reader["name"]
                },
                LastMessage = new Message()
                {
                    ChatId = (int)reader["chat_id"],
                    Content = (string)reader["content"],
                    Date = (DateTime)reader["date"],
                    User = new User()
                    {
                        Id = (int)reader["sender_id"],
                        Name = (string)reader["name"]
                    }
                }
            });
        }

        return chats;
    }

    [HttpPost]
    [Route("find")]
    public int FindUserAndCreateChat([FromBody] string userSearch)
    {
        NpgsqlCommand command;
        if (userSearch.All(e => char.IsDigit(e)))
        {
            command = _dbContext.DataSource.CreateCommand(
                "SELECT * FROM users WHERE @id = id");
            command.Parameters.AddWithValue("id", userSearch);
        }
        else
        {
            command = _dbContext.DataSource.CreateCommand(
                "SELECT * FROM users WHERE @name = name");
            command.Parameters.AddWithValue("name", userSearch);
        }

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            if (ClientId == (int)reader["id"])
            {
                return -1;
            }

            int chatId = ChatAlreadyExist((int)reader["id"]);

            return chatId != 0 ? chatId : CreateNewChat((int)reader["id"]);
        }

        return 0;

        int ChatAlreadyExist(int userId)
        {
            using var createChatCommand = _dbContext.DataSource.CreateCommand(
                "SELECT id FROM chats WHERE " +
                "((first_user_id = @user_id AND second_user_id = @client_id) OR " +
                "(first_user_id = @client_id AND second_user_id = @user_id))");
            createChatCommand.Parameters.AddWithValue("client_id", ClientId);
            createChatCommand.Parameters.AddWithValue("user_id", userId);
            var result = createChatCommand.ExecuteScalar();
            if (result == null)
                return 0;
            return (int)result;
        }

        int CreateNewChat(int userId)
        {
            using var createChatCommand = _dbContext.DataSource.CreateCommand(
                "INSERT INTO chats (first_user_id, second_user_id) VALUES (@client_id, @found_user);" +
                "SELECT id FROM chats WHERE first_user_id = @client_id AND second_user_id = @found_user");
            createChatCommand.Parameters.AddWithValue("client_id", ClientId);
            createChatCommand.Parameters.AddWithValue("found_user", userId);
            using var reader = createChatCommand.ExecuteReader();
            if (reader.Read())
            {
                return (int)reader["id"];
            }

            return 0;
        }
    }

    [HttpPost]
    [Route("send")]
    public void PostMessage([FromBody] NewMessageModel newMessage)
    {
        using var command = _dbContext.DataSource.CreateCommand(
            $"INSERT INTO messages (sender_id, content, date, chat_id) VALUES (@sender_id, @content, @date, @chat_id)");

        command.Parameters.AddRange(new NpgsqlParameter[]
        {
            new NpgsqlParameter()
            {
                ParameterName = "sender_id",
                Value = ClientId
            },
            new NpgsqlParameter()
            {
                ParameterName = "content",
                Value = HttpUtility.UrlDecode(newMessage.Content)
            },
            new NpgsqlParameter()
            {
                ParameterName = "date",
                Value = DateTime.UtcNow
            },
            new NpgsqlParameter()
            {
                ParameterName = "chat_id",
                Value = newMessage.ChatId
            }
        });

        command.ExecuteNonQuery();
    }
}
