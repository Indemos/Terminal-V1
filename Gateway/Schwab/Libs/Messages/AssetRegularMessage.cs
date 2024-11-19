namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class AssetRegularMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("regularMarketLastPrice")]
    public double? RegularMarketLastPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("regularMarketLastSize")]
    public double? RegularMarketLastSize { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("regularMarketNetChange")]
    public double? RegularMarketNetChange { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("regularMarketPercentChange")]
    public double? RegularMarketPercentChange { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("regularMarketTradeTime")]
    public long? RegularMarketTradeTime { get; set; }
  }
}
