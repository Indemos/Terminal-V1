using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class AccountMessage
  {
    [JsonPropertyName("uuid")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("available_balance")]
    public double AvailableBalance { get; set; }

    [JsonPropertyName("default")]
    public bool Default { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("ready")]
    public bool Ready { get; set; }

    [JsonPropertyName("hold")]
    public CurrencyValueMessage Hold { get; set; }
  }
}
