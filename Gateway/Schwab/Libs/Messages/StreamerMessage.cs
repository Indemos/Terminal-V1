namespace Schwab.Messages
{
  using System;
  using System.Text.Json.Serialization;

  public partial class StreamerMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("streamerSocketUrl")]
    public string SocketUrl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("schwabClientCustomerId")]
    public string CustomerId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("schwabClientCorrelId")]
    public string CorrelationId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("schwabClientChannel")]
    public string Channel { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("schwabClientFunctionId")]
    public string FunctionId { get; set; }
  }
}
