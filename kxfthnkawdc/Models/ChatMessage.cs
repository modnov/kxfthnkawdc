using kxfthnkawdc.Models;

namespace kxfthnkawdc.Models;

public class ChatMessage
{
    public required int Id { get; init; }
    public required User User { get; init; }
    public required string Content { get; init; }
    public required DateTime Date { get; init; }
}
