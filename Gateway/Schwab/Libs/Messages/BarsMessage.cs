namespace Schwab.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class BarsMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("candles")]
    public List<BarMessage> Bars { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
  }
}
