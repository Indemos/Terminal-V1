namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class PositionMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("shortQuantity")]
    public double? ShortQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("averagePrice")]
    public double? AveragePrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentDayProfitLoss")]
    public double? CurrentDayProfitLoss { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentDayProfitLossPercentage")]
    public double? CurrentDayProfitLossPercentage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("longQuantity")]
    public double? LongQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("settledLongQuantity")]
    public double? SettledLongQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("settledShortQuantity")]
    public double? SettledShortQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("instrument")]
    public InstrumentMessage Instrument { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marketValue")]
    public double? MarketValue { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("maintenanceRequirement")]
    public double? MaintenanceRequirement { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("averageLongPrice")]
    public double? AverageLongPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("taxLotAverageLongPrice")]
    public double? TaxLotAverageLongPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("longOpenProfitLoss")]
    public double? LongOpenProfitLoss { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("previousSessionLongQuantity")]
    public double? PreviousSessionLongQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentDayCost")]
    public double? CurrentDayCost { get; set; }
  }
}
