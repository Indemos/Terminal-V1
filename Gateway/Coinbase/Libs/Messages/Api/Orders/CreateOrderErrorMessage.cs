using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CreateOrderErrorMessage
  {
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("error_details")]
    public string ErrorDetails { get; set; }

    [JsonPropertyName("preview_failure_reason")]
    public string PreviewFailureReason { get; set; }

    [JsonPropertyName("new_order_failure_reason")]
    public string NewOrderFailureReason { get; set; }
  }
}
