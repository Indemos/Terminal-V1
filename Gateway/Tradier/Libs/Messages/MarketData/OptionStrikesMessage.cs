using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class OptionStrikesCoreMessage
  {
    [JsonPropertyName("strikes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StrikesMessage Strikes { get; set; }
  }

  public class StrikesMessage
  {
    [JsonPropertyName("strike")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<double> Strikes { get; set; }
  }
}
