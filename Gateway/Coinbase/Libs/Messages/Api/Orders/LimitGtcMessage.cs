using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class LimitGtcMessage
  {
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; }

    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }
  }
}
