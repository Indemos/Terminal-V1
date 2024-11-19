namespace Schwab.Messages
{
  using System.Text.Json.Serialization;

  public partial class AccountNumberMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("hashValue")]
    public string HashValue { get; set; }
  }
}
