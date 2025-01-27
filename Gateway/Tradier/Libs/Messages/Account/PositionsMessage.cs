using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tradier.Converters;

namespace Tradier.Messages.Account
{
  public class PositionsCoreMessage
  {
    [JsonPropertyName("positions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PositionsMessage Positions { get; set; }
  }

  public class PositionsMessage
  {
    [JsonPropertyName("position")]
    [JsonConverter(typeof(SingularConverter<PositionMessage>))]
    public List<PositionMessage> Items { get; set; }
  }

  public class PositionMessage
  {
    [JsonPropertyName("cost_basis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CostBasis { get; set; }

    [JsonPropertyName("date_acquired")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DateAcquired { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Quantity { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
  }
}
