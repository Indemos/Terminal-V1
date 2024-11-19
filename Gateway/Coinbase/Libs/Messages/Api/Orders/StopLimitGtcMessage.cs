using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class StopLimitGtcMessage
  {
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; }

    [JsonPropertyName("stop_price")]
    public string StopPrice { get; set; }

    [JsonPropertyName("stop_direction")]
    public string StopDirection { get; set; }
  }
}
