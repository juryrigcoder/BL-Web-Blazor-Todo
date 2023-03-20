using System.Text.Json.Serialization;
using LiteDB;

namespace Library;
public class TodoItem
{
    [JsonPropertyName("_id")]
    [BsonId]
    public Guid? _id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("text")]
    public string? Text { get; set; }
   [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
}
