using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.Account
{
  public class GainLossCoreMessage
  {
    [JsonPropertyName("gainloss")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GainLossMessage GainLoss { get; set; }
  }

  public class GainLossMessage
  {
    [JsonPropertyName("closed_position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ClosedPositionMessage> ClosedPositions { get; set; }
  }

  public class ClosedPositionMessage
  {
    [JsonPropertyName("close_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CloseDate { get; set; }

    [JsonPropertyName("cost")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Cost { get; set; }

    [JsonPropertyName("gain_loss")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? GainLoss { get; set; }

    [JsonPropertyName("gain_loss_percent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? GainLossPercent { get; set; }

    [JsonPropertyName("open_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? OpenDate { get; set; }

    [JsonPropertyName("proceeds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Proceeds { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Quantity { get; set; }

    [JsonPropertyName("symbol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Symbol { get; set; }

    [JsonPropertyName("term")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Term { get; set; }
  }
}
