namespace Coinbase.Messages
{
  using System.Text.Json.Serialization;

  public partial class PortfolioBreakdownsMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("breakdown")]
    public PortfolioBreakdownMessage Breakdown { get; set; }
  }
}
