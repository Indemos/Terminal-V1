using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class CalendarCoreMessage
  {
    [JsonPropertyName("calendar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CalendarMessage Calendar { get; set; }
  }

  public class CalendarMessage
  {
    [JsonPropertyName("month")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Month { get; set; }

    [JsonPropertyName("year")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Year { get; set; }

    [JsonPropertyName("days")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DaysMessage Days { get; set; }
  }

  public class DaysMessage
  {
    [JsonPropertyName("day")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CalendarDayMessage> Items { get; set; }
  }

  public class CalendarDayMessage
  {
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("premarket")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PremarketMessage Premarket { get; set; }

    [JsonPropertyName("open")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenMessage Open { get; set; }

    [JsonPropertyName("postmarket")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PostmarketMessage Postmarket { get; set; }
  }

  public class PremarketMessage
  {
    [JsonPropertyName("start")]
    public string Start { get; set; }

    [JsonPropertyName("end")]
    public string End { get; set; }
  }

  public class OpenMessage
  {
    [JsonPropertyName("start")]
    public string Start { get; set; }

    [JsonPropertyName("end")]
    public string End { get; set; }
  }

  public class PostmarketMessage
  {
    [JsonPropertyName("start")]
    public string Start { get; set; }

    [JsonPropertyName("end")]
    public string End { get; set; }
  }
}
