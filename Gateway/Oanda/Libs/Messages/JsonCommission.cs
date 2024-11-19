
namespace Oanda.Messages
{
  using System;
  using System.Text.Json.Serialization;

  public partial class JsonCommission
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("commission")]
    public double? Commission { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("minimumCommission")]
    public double? MinimumCommission { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("units")]
    public long? Units { get; set; }
  }
}
