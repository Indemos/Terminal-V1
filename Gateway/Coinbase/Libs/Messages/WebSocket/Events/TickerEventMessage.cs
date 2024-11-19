using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class TickerEventMessage : SocketEventMessage
  {
    [JsonPropertyName("tickers")]
    public List<TickerMessage> Tickers { get; set; }
  }
}
