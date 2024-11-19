using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class TradesMessage
  {
    [JsonPropertyName("trades")]
    public List<TradeMessage> Trades { get; set; }

    [JsonPropertyName("best_bid")]
    public string BestBid { get; set; }

    [JsonPropertyName("best_ask")]
    public string BestAsk { get; set; }
  }
}
