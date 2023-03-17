using System.Text.Json.Serialization;

namespace Library;
public class TodoItem
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("text")]
    public string? Text { get; set; }
   [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
}
