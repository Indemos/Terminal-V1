using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class TickerMessage
  {
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("volume_24_h")]
    public decimal? Volume24H { get; set; }

    [JsonPropertyName("low_24_h")]
    public decimal? Low24H { get; set; }

    [JsonPropertyName("high_24_h")]
    public decimal? High24H { get; set; }

    [JsonPropertyName("low_52_w")]
    public decimal? Low52W { get; set; }

    [JsonPropertyName("high_52_w")]
    public decimal? High52W { get; set; }

    [JsonPropertyName("price_percent_chg_24_h")]
    public decimal? PricePercentageChange24H { get; set; }
  }
}
