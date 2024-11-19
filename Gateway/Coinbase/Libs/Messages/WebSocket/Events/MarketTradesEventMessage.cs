using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class MarketTradesEventMessage : SocketEventMessage
  {
    [JsonPropertyName("trades")]
    public List<TradeMessage> Trades { get; set; } = new List<TradeMessage>();
  }
}
