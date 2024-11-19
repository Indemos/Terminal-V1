using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class ChannelMessage
  {

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("product_ids")]
    public List<string> ProductIds { get; set; }
  }
}
