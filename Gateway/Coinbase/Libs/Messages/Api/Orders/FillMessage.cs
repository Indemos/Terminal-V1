using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class FillMessage
  {
    [JsonPropertyName("entry_id")]
    public string EntryId { get; set; }

    [JsonPropertyName("trade_id")]
    public string TradeId { get; set; }

    [JsonPropertyName("order_id")]
    public string OrderId { get; set; }

    [JsonPropertyName("trade_time")]
    public DateTime? TradeTime { get; set; }

    [JsonPropertyName("trade_type")]
    public string TradeType { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("size")]
    public decimal? Size { get; set; }

    [JsonPropertyName("commission")]
    public decimal? Commission { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("sequence_timestamp")]
    public DateTime? SequenceTimestamp { get; set; }

    [JsonPropertyName("liquidity_indicator")]
    public string LiquidityIndicator { get; set; }

    [JsonPropertyName("size_in_quote")]
    public bool SizeInQuote { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }
  }
}
