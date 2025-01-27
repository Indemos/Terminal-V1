namespace Tradier.Messages.Stream
{
  using System;
  using System.Text.Json.Serialization;

  public partial class SessionMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stream")]
    public Stream Stream { get; set; }
  }

  public partial class Stream
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sessionid")]
    public string Session { get; set; }
  }
}
