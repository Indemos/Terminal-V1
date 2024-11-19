namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class OfferMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("level2Permissions")]
    public bool? Level2Permissions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("mktDataPermission")]
    public string MktDataPermission { get; set; }
  }
}
