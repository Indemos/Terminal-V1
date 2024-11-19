using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CreateOrderSuccessMessage
  {
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("client_order_id")]
    public string ClientOrderId { get; set; }
  }
}
