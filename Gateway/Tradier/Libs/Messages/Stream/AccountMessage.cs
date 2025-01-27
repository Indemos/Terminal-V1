namespace Tradier.Messages.Stream
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class AccountMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("events")]
    public List<string> Events { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("excludeAccounts")]
    public List<string> ExcludeAccounts { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sessionid")]
    public string Session { get; set; }
  }
}
