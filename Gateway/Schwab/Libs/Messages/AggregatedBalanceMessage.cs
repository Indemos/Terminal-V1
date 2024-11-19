namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class AggregatedBalanceMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentLiquidationValue")]
    public double? CurrentLiquidationValue { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("liquidationValue")]
    public double? LiquidationValue { get; set; }
  }
}
