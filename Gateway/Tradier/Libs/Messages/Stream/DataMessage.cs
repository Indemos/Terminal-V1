namespace Tradier.Messages.Stream
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class DataMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("symbols")]
    public List<string> Symbols { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("filter")]
    public List<string> Filter { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sessionid")]
    public string Session { get; set; }
  }
}
