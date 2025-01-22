using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class OptionSymbolsCoreMessage
  {
    [JsonPropertyName("symbols")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SymbolMessage> Symbols { get; set; }
  }

  public class SymbolMessage
  {
    [JsonPropertyName("rootSymbol")]
    public string RootSymbol { get; set; }

    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Options { get; set; }
  }
}
