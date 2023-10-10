using kxfthnkawdc.Models;

namespace kxfthnkawdc.Models;

public class Message
{
    public int Id { get; init; }
    public required User User { get; init; }
    public required string Content { get; init; }
    public required DateTime Date { get; init; }
    public required int ChatId { get; init; }
}
