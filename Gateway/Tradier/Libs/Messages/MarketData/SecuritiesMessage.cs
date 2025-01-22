using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class SecuritiesCoreMessage
  {
    [JsonPropertyName("securities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SecuritiesMessage Securities { get; set; }
  }

  public class SecuritiesMessage
  {
    [JsonPropertyName("security")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SecurityMessage> Items { get; set; }
  }

  public class SecurityMessage
  {
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("exchange")]
    public string Exchange { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
  }
}
