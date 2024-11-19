using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class DomEventMessage : SocketEventMessage
  {
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("updates")]
    public List<UpdateMessage> Updates { get; set; }
  }
}
