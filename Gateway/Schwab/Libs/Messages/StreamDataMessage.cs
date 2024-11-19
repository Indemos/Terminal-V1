namespace Schwab.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class StreamDataMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("service")]
    public string Service { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("content")]
    public List<Dictionary<string, object>> Content { get; set; }
  }
}
