using kxfthnkawdc.Models;
using Microsoft.AspNetCore.SignalR;

namespace kxfthnkawdc.Hub;

public sealed class ChatHub : Microsoft.AspNetCore.SignalR.Hub
{
    private ApplicationDbContext _dbContext;

    public ChatHub(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task Send(string message)
    {
        await Clients.All.SendAsync("Receive", new ChatMessage()
        {
            User = new User(_dbContext)
            {
                Id = Context.GetHttpContext().Session.GetInt32("id").Value,
                Name = Context.GetHttpContext().Session.GetString("username")
            },
            Content = message,
            Date = DateTime.Now
        });
    }
}
