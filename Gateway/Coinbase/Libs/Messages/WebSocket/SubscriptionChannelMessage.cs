using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class SubscriptionChannelMessage
  {
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; }

    [JsonPropertyName("product_ids")]
    public List<string> ProductIds { get; set; }

    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("jwt")]
    public string Token { get; set; }
  }
}
