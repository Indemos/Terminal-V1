using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CancelOrdersMessage
  {
    [JsonPropertyName("order_ids")]
    public List<string> OrderIds { get; set; }
  }
}
