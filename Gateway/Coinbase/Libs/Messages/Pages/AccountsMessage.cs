using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class AccountsMessage : PageMessage
  {
    [JsonPropertyName("account")]
    public AccountMessage Account { get; set; }

    [JsonPropertyName("accounts")]
    public List<AccountMessage> Accounts { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
  }
}
