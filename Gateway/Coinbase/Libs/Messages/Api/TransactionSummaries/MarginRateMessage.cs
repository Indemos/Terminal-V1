using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class MarginRateMessage
  {
    [JsonPropertyName("value")]
    public decimal? Value { get; set; }
  }
}
