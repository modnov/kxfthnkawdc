namespace kxfthnkawdc.Models;

public sealed class Chat
{
    public required int ChatId { get; init; }
    public required User Interlocutor { get; init; }
}
