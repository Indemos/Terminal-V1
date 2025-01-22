using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.Account
{
  public class ProfileCoreMessage
  {
    [JsonPropertyName("profile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProfileMessage Profile { get; set; }
  }

  public class ProfileMessage
  {
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("account")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AccountMessage> Account { get; set; }
  }

  public class AccountMessage
  {
    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; }

    [JsonPropertyName("classification")]
    public string Classification { get; set; }

    [JsonPropertyName("date_created")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DateCreated { get; set; }

    [JsonPropertyName("day_trader")]
    public bool DayTrader { get; set; }

    [JsonPropertyName("option_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? OptionLevel { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("last_update_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? LastUpdateDate { get; set; }
  }
}
