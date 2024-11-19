using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class TransactionSummaryMessage
  {
    [JsonPropertyName("total_volume")]
    public decimal? TotalVolume { get; set; }

    [JsonPropertyName("total_fees")]
    public decimal? TotalFees { get; set; }

    [JsonPropertyName("fee_tier")]
    public FeeTierMessage FeeTier { get; set; }

    [JsonPropertyName("margin_rate")]
    public MarginRateMessage MarginRate { get; set; }

    [JsonPropertyName("goods_and_services_tax")]
    public GoodsAndServicesTax GoodsAndServicesTax { get; set; }

    [JsonPropertyName("advanced_trade_only_volume")]
    public decimal? AdvancedTradeOnlyVolume { get; set; }

    [JsonPropertyName("advanced_trade_only_fees")]
    public decimal? AdvancedTradeOnlyFees { get; set; }

    [JsonPropertyName("coinbase_pro_volume")]
    public decimal? CoinbaseProVolume { get; set; }

    [JsonPropertyName("coinbase_pro_fees")]
    public decimal? CoinbaseProFees { get; set; }
  }
}
