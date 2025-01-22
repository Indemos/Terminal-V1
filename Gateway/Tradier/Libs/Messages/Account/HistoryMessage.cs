using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.Account
{
  public class HistoryCoreMessage
  {
    [JsonPropertyName("history")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HistoryMessage History { get; set; }
  }

  public class HistoryMessage
  {
    [JsonPropertyName("event")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<EventMessage> Events { get; set; }
  }

  public class EventMessage
  {
    [JsonPropertyName("amount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Amount { get; set; }

    [JsonPropertyName("date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? Date { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; }

    [JsonPropertyName("trade")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TradeMessage Trade { get; set; }

    [JsonPropertyName("adjustment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AdjustmentMessage Adjustment { get; set; }

    [JsonPropertyName("option")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EventOptionMessage Option { get; set; }

    [JsonPropertyName("journal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JournalMessage Journal { get; set; }
  }

  public class TradeMessage
  {
    [JsonPropertyName("commission")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Commission { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Price { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Quantity { get; set; }

    [JsonPropertyName("symbol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Symbol { get; set; }

    [JsonPropertyName("trade_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string TradeType { get; set; }
  }

  public class AdjustmentMessage
  {
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Quantity { get; set; }
  }

  public class EventOptionMessage
  {
    [JsonPropertyName("option_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string OptionType { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double Quantity { get; set; }
  }

  public class JournalMessage
  {
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Quantity { get; set; }
  }
}
