namespace Oanda.Messages
{
  using System;
  using System.Text.Json.Serialization;
  public partial class JsonConfiguration
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("clientConfigureTransaction")]
    public JsonConfigurationTransaction ClientConfigureTransaction { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("lastTransactionID")]
    public long? LastTransactionId { get; set; }
  }

  public partial class JsonConfigurationTransaction
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("accountID")]
    public string AccountId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("alias")]
    public string Alias { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("batchID")]
    public long? BatchId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("userID")]
    public string UserId { get; set; }
  }
}
