using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class ErrorMessage
  {
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("details")]
    public List<ErrorDetailMessage> Details { get; set; }
  }
}
