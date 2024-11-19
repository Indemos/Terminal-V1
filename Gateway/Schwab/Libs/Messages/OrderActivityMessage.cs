namespace Schwab.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class OrderActivityMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("activityType")]
    public string ActivityType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("executionType")]
    public string ExecutionType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("quantity")]
    public double? Quantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orderRemainingQuantity")]
    public double? OrderRemainingQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("executionLegs")]
    public List<ExecutionLegMessage> ExecutionLegs { get; set; }
  }
}
