namespace Oanda.Messages
{
  using System;
  using System.Text.Json.Serialization;

  public partial class JsonClientExtensions
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
  }
}
