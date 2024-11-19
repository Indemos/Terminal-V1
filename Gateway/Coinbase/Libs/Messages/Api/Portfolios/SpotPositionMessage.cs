namespace Coinbase.Messages
{
  using System;
  using System.Text.Json.Serialization;

  public partial class SpotPositionMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asset")]
    public string Asset { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("account_uuid")]
    public Guid? AccountUuid { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_balance_fiat")]
    public double? TotalBalanceFiat { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_balance_crypto")]
    public double? TotalBalanceCrypto { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("available_to_trade_fiat")]
    public double? AvailableToTradeFiat { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("allocation")]
    public double? Allocation { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("cost_basis")]
    public FuturesGainMessage CostBasis { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asset_img_url")]
    public string AssetImgUrl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("is_cash")]
    public bool? IsCash { get; set; }

    [JsonPropertyName("average_entry_price")]
    public object AverageEntryPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asset_uuid")]
    public string AssetUuid { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("available_to_trade_crypto")]
    public double? AvailableToTradeCrypto { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("unrealized_pnl")]
    public long? UnrealizedPnl { get; set; }
  }
}
