namespace Coinbase.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class PortfolioBreakdownMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("portfolio")]
    public PortfolioMessage Portfolio { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("portfolio_balances")]
    public PortfolioBalanceMessage PortfolioBalances { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("spot_positions")]
    public List<SpotPositionMessage> SpotPositions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("perp_positions")]
    public List<object> PerpPositions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("futures_positions")]
    public List<object> FuturesPositions { get; set; }
  }
}
