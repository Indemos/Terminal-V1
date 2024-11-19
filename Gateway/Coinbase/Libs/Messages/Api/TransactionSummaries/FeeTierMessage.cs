using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class FeeTierMessage
  {
    [JsonPropertyName("pricing_tier")]
    public string PricingTier { get; set; }

    [JsonPropertyName("usd_from")]
    public decimal? UsdFrom { get; set; }

    [JsonPropertyName("usd_to")]
    public decimal? UsdTo { get; set; }

    [JsonPropertyName("taker_fee_rate")]
    public decimal? TakerFeeRate { get; set; }

    [JsonPropertyName("maker_fee_rate")]
    public decimal? MakerFeeRate { get; set; }
  }
}
