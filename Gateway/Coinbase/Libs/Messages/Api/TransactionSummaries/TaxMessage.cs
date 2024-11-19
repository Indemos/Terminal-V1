using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class GoodsAndServicesTax
  {
    [JsonPropertyName("rate")]
    public decimal? Rate { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
  }
}
