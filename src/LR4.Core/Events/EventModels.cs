using System.Text.Json;
using LR4.Core.Streams;

namespace LR4.Core.Events;

public sealed record EventRecord(string Type, string? Payload, DateTime Timestamp);

public sealed record ReactionRecord(string Text, string? RuleName, DateTime Timestamp);

public sealed class EventJsonSerializer : ISerializer<EventRecord>, IDeserializer<EventRecord>
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public string Serialize(EventRecord item) => JsonSerializer.Serialize(item, Options);

    public EventRecord Deserialize(string line) =>
        JsonSerializer.Deserialize<EventRecord>(line, Options)
        ?? throw new FormatException("Invalid event JSON line.");
}
