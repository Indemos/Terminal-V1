namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class AccountsMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("securitiesAccount")]
    public SecuritiesMessage SecuritiesAccount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("aggregatedBalance")]
    public AggregatedBalanceMessage AggregatedBalance { get; set; }
  }
}
