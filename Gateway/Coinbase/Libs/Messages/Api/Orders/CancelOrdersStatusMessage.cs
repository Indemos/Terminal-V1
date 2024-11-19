using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CancelOrdersStatusMessage
  {
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("failure_reason")]
    public string FailureReason { get; set; }

    [JsonPropertyName("order_id")]
    public string OrderId { get; set; }
  }
}
