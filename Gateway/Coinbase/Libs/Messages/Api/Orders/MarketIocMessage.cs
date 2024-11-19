using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class MarketIocMessage
  {
    [JsonPropertyName("quote_size")]
    public string QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; }
  }
}
