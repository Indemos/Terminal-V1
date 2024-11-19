using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class ErrorDetailMessage
  {
    [JsonPropertyName("type_url")]
    public string TypeUrl { get; set; }

    [JsonPropertyName("value")]
    public byte Value { get; set; }
  }
}
