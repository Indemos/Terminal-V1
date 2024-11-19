namespace Coinbase.Messages
{
  using System.Text.Json.Serialization;

  public partial class PortfolioBalanceMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_balance")]
    public FuturesGainMessage TotalBalance { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_futures_balance")]
    public FuturesGainMessage TotalFuturesBalance { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_cash_equivalent_balance")]
    public FuturesGainMessage TotalCashEquivalentBalance { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_crypto_balance")]
    public FuturesGainMessage TotalCryptoBalance { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("futures_unrealized_pnl")]
    public FuturesGainMessage FuturesUnrealizedPnl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("perp_unrealized_pnl")]
    public FuturesGainMessage PerpUnrealizedPnl { get; set; }
  }
}
