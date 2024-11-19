namespace Oanda.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class JsonState
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NAV")]
    public double? Nav { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marginAvailable")]
    public double? MarginAvailable { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marginCloseoutMarginUsed")]
    public double? MarginCloseoutMarginUsed { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marginCloseoutNAV")]
    public double? MarginCloseoutNav { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marginCloseoutPercent")]
    public double? MarginCloseoutPercent { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marginCloseoutUnrealizedPL")]
    public double? MarginCloseoutUnrealizedPl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("marginUsed")]
    public double? MarginUsed { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orders")]
    public List<JsonOrder> Orders { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("positionValue")]
    public double? PositionValue { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("positions")]
    public List<JsonPosition> Positions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("trades")]
    public List<JsonTrade> Trades { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("unrealizedPL")]
    public double? UnrealizedPl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("withdrawalLimit")]
    public double? WithdrawalLimit { get; set; }
  }
}
