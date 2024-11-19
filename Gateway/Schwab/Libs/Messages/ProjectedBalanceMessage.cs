namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class ProjectedBalanceMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("availableFunds")]
    public double? AvailableFunds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("availableFundsNonMarginableTrade")]
    public double? AvailableFundsNonMarginableTrade { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("buyingPower")]
    public double? BuyingPower { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("dayTradingBuyingPower")]
    public double? DayTradingBuyingPower { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("dayTradingBuyingPowerCall")]
    public double? DayTradingBuyingPowerCall { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("maintenanceCall")]
    public double? MaintenanceCall { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("regTCall")]
    public double? RegTCall { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("isInCall")]
    public bool? IsInCall { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stockBuyingPower")]
    public double? StockBuyingPower { get; set; }
  }
}
