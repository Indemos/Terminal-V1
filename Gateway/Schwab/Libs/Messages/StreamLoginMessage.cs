namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class StreamLoginMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("Authorization")]
    public string Authorization { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("SchwabClientChannel")]
    public string Channel { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("SchwabClientFunctionId")]
    public string FunctionId { get; set; }
  }
}
