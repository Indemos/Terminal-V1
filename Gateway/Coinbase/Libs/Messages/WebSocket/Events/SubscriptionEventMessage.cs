using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class SubscriptionEventMessage : SocketEventMessage
  {
    [JsonPropertyName("subscriptions")]
    public SubscriptionMessage Subscriptions { get; set; }
  }
}
