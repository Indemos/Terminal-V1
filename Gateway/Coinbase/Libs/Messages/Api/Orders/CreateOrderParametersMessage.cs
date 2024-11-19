using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class CreateOrderParametersMessage
  {
    [JsonPropertyName("client_order_id")]
    public string ClientOrderId { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("order_configuration")]
    public OrderConfigurationMessage OrderConfiguration { get; set; }
  }
}
