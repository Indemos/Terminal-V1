using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CurrencyValueMessage
  {
    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("value")]
    public decimal? Value { get; set; }
  }
}
