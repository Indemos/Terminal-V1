using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class ProductsMessage
  {
    [JsonPropertyName("products")]
    public List<ProductMessage> Products { get; set; }

    [JsonPropertyName("num_products")]
    public int NumberOfProducts { get; set; }
  }
}
