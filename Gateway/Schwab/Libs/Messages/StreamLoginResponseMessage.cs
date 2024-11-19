namespace Schwab.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class StreamLoginResponseMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("response")]
    public List<StreamLoginResponseItemMessage> Response { get; set; }
  }

  public partial class StreamLoginResponseItemMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("service")]
    public string Service { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("requestid")]
    public string RequestId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("SchwabClientCorrelId")]
    public string CorrellationId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("content")]
    public StreamLoginCodeMessage Content { get; set; }
  }

  public partial class StreamLoginCodeMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public long? Code { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("msg")]
    public string Message { get; set; }
  }
}
