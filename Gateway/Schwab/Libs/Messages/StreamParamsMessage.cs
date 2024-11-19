namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class SrteamParamsMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("keys")]
    public string Keys { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("fields")]
    public string Fields { get; set; }
  }
}
