namespace Coinbase.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class PortfoliosMessage : PageMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("portfolios")]
    public List<PortfolioMessage> Portfolios { get; set; }
  }
}
