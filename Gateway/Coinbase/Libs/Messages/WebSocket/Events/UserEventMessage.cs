using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class UserEventMessage : SocketEventMessage
  {
    [JsonPropertyName("orders")]
    public List<OrderMessage> Orders { get; set; }
  }
}
