using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class UpdateMessage
  {
    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("event_time")]
    public DateTime? EventTime { get; set; }

    [JsonPropertyName("price_level")]
    public decimal? PriceLevel { get; set; }

    [JsonPropertyName("new_quantity")]
    public decimal? NewQuantity { get; set; }
  }
}
