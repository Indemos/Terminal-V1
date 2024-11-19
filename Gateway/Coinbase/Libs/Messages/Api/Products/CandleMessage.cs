using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CandleMessage
  {
    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("low")]
    public decimal? Low { get; set; }

    [JsonPropertyName("high")]
    public decimal? High { get; set; }

    [JsonPropertyName("open")]
    public decimal? Open { get; set; }

    [JsonPropertyName("close")]
    public decimal? Close { get; set; }

    [JsonPropertyName("volume")]
    public decimal? Volume { get; set; }
  }
}
