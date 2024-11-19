using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class TradeMessage
  {
    [JsonPropertyName("trade_id")]
    public string TradeId { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("size")]
    public decimal? Size { get; set; }

    [JsonPropertyName("time")]
    public DateTime? Time { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("bid")]
    public decimal? Bid { get; set; }

    [JsonPropertyName("ask")]
    public decimal? Ask { get; set; }
  }
}
