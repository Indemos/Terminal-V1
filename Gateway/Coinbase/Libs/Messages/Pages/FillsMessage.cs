using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class FillsMessage : PageMessage
  {
    [JsonPropertyName("fills")]
    public List<FillMessage> Fills { get; set; }
  }
}
