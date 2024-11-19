namespace Coinbase.Messages
{
  using System.Text.Json.Serialization;

  public partial class FuturesGainMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
  }
}
