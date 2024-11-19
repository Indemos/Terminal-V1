using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class StopLimitGtdMessage : StopLimitGtcMessage
  {
    [JsonPropertyName("end_time")]
    public DateTime? EndTime { get; set; }
  }
}
