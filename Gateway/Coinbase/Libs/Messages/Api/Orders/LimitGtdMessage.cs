using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class LimitGtdMessage : LimitGtcMessage
  {
    [JsonPropertyName("end_time")]
    public DateTime? EndTime { get; set; }
  }
}
