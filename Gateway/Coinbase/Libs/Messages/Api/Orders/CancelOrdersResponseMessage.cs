using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CancelOrdersResponseMessage
  {
    [JsonPropertyName("results")]
    public List<CancelOrdersStatusMessage> Results { get; set; }
  }
}
