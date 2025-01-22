using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class HistoricalQuotesCoreMessage
  {
    [JsonPropertyName("history")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HistoricalQuotesMessage History { get; set; }
  }

  public class HistoricalQuotesMessage
  {
    [JsonPropertyName("day")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<DayMessage> Day { get; set; }
  }

  public class DayMessage
  {
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("open")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Open { get; set; }

    [JsonPropertyName("high")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? High { get; set; }

    [JsonPropertyName("low")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Low { get; set; }

    [JsonPropertyName("close")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Close { get; set; }

    [JsonPropertyName("volume")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Volume { get; set; }
  }
}
