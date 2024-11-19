using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class SubscriptionMessage
  {
    [JsonPropertyName("level2")]
    public List<string> Level2 { get; set; }

    [JsonPropertyName("market_trades")]
    public List<string> MarketTrades { get; set; }

    [JsonPropertyName("status")]
    public List<string> Status { get; set; }

    [JsonPropertyName("ticker")]
    public List<string> Ticker { get; set; }

    [JsonPropertyName("ticker_batch")]
    public List<string> TickerBatch { get; set; }

    [JsonPropertyName("user")]
    public List<string> User { get; set; }
  }
}
