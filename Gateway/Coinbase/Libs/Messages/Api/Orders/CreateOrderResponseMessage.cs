using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CreateOrderResponse
  {
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("failure_reason")]
    public string FailureReason { get; set; }

    [JsonPropertyName("order_id")]
    public string OrderId { get; set; }

    [JsonPropertyName("success_response")]
    public CreateOrderSuccessMessage SuccessResponse { get; set; }

    [JsonPropertyName("error_response")]
    public CreateOrderErrorMessage ErrorResponse { get; set; }
  }
}
