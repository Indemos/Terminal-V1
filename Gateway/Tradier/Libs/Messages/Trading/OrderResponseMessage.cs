using System;
using System.Text.Json.Serialization;

namespace Tradier.Messages.Trading
{
  public class OrderResponseCoreMessage
  {
    [JsonPropertyName("order")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OrderResponseMessage OrderReponse { get; set; }
  }

  public class OrderResponseMessage
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("partner_id")]
    public string PartnerId { get; set; }

    [JsonPropertyName("commission")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Commission { get; set; }

    [JsonPropertyName("cost")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Cost { get; set; }

    [JsonPropertyName("fees")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Fees { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Quantity { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Result { get; set; }

    [JsonPropertyName("order_cost")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OrderCost { get; set; }

    [JsonPropertyName("margin_change")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MarginChange { get; set; }

    [JsonPropertyName("request_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RequestDate { get; set; }

    [JsonPropertyName("extended_hours")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ExtendedHours { get; set; }

    [JsonPropertyName("class")]
    public string ClassOrder { get; set; }

    [JsonPropertyName("strategy")]
    public string Strategy { get; set; }

    [JsonPropertyName("day_trades")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DayTrades { get; set; }
  }
}
