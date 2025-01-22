using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class OptionExpirationsCoreMessage
  {
    [JsonPropertyName("expirations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExpirationsMessage Expirations { get; set; }
  }

  public class ExpirationsMessage
  {
    [JsonPropertyName("date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<DateTime> Dates { get; set; }
  }
}
