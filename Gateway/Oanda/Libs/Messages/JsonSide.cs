namespace Oanda.Messages
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class JsonSide
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("guaranteedExecutionFees")]
    public double? GuaranteedExecutionFees { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("dividendAdjustment")]
    public double? DividendAdjustment { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("financing")]
    public double? Financing { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("pl")]
    public double? Pl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("averagePrice")]
    public double? AveragePrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("resettablePL")]
    public double? ResettablePl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("units")]
    public double? Units { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("unrealizedPL")]
    public double? UnrealizedPl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tradeIDs")]
    public List<string> TradeIds { get; set; }
  }
}
