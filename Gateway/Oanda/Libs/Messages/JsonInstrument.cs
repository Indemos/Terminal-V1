namespace Oanda.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class JsonInstruments
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("instruments")]
    public List<JsonInstrument> Instruments { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("lastTransactionID")]
    public string LastTransactionId { get; set; }
  }

  public partial class JsonInstrument
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("displayPrecision")]
    public double? DisplayPrecision { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marginRate")]
    public double? MarginRate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("maximumOrderUnits")]
    public double? MaximumOrderUnits { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("maximumPositionSize")]
    public double? MaximumPositionSize { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("maximumTrailingStopDistance")]
    public double? MaximumTrailingStopDistance { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("minimumTradeSize")]
    public double? MinimumTradeSize { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("minimumTrailingStopDistance")]
    public double? MinimumTrailingStopDistance { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("commission")]
    public JsonCommission Commission { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("financing")]
    public double? Financing { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("pipLocation")]
    public double? PipLocation { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tradeUnitsPrecision")]
    public double? TradeUnitsPrecision { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string Type { get; set; }
  }
}
