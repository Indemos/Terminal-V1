using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class StatusEventMessage : SocketEventMessage
  {
    [JsonPropertyName("products")]
    public List<ProductMessage> Products { get; set; } = new List<ProductMessage>();
  }
}
