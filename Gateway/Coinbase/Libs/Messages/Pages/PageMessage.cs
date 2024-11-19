using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class PageMessage
  {
    [JsonPropertyName("has_next")]
    public bool HasNext { get; set; }

    [JsonPropertyName("cursor")]
    public string Cursor { get; set; }
  }
}
