using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class SocketEventMessage
  {
    [JsonPropertyName("type")]
    public string Type { get; set; }
  }
}
