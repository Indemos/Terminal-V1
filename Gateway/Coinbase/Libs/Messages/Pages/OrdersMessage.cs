using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class OrdersMessage : PageMessage
  {
    [JsonPropertyName("order")]
    public OrderMessage Order { get; set; }

    [JsonPropertyName("orders")]
    public List<OrderMessage> Orders { get; set; }

    [JsonPropertyName("sequence")]
    public long Sequence { get; set; }
  }
}
