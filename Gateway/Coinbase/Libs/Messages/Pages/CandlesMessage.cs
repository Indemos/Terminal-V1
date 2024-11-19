using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CandlesMessage
  {
    [JsonPropertyName("candles")]
    public List<CandleMessage> Candles { get; set; }
  }
}
