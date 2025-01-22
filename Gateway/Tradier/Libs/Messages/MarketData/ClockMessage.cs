using System;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class ClockCoreMessage
  {
    [JsonPropertyName("clock")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClockMessage Clock { get; set; }
  }

  public class ClockMessage
  {
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("timestamp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("next_change")]
    public string NextChange { get; set; }

    [JsonPropertyName("next_state")]
    public string NextState { get; set; }
  }
}
