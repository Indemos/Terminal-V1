namespace Tradier.Messages.Account
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;
  using Tradier.Converters;

  public class OrdersCoreMessage
  {
    [JsonPropertyName("orders")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OrdersMessage Orders { get; set; }
  }

  public class OrdersMessage
  {
    [JsonPropertyName("order")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SingularConverter<OrderMessage>))]
    public List<OrderMessage> Items { get; set; }
  }

  public class OrderMessage
  {
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Quantity { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Price { get; set; }

    [JsonPropertyName("stop_price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? StopPrice { get; set; }

    [JsonPropertyName("avg_fill_price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AvgFillPrice { get; set; }

    [JsonPropertyName("exec_quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ExecQuantity { get; set; }

    [JsonPropertyName("last_fill_price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? LastFillPrice { get; set; }

    [JsonPropertyName("last_fill_quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? LastFillQuantity { get; set; }

    [JsonPropertyName("remaining_quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? RemainingQuantity { get; set; }

    [JsonPropertyName("create_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CreateDate { get; set; }

    [JsonPropertyName("transaction_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? TransactionDate { get; set; }

    [JsonPropertyName("_class")]
    public string Class { get; set; }

    [JsonPropertyName("option_symbol")]
    public string OptionSymbol { get; set; }

    [JsonPropertyName("num_legs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumLegs { get; set; }

    [JsonPropertyName("strategy")]
    public string Strategy { get; set; }

    [JsonPropertyName("leg")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OrderMessage[] Orders { get; set; }
  }
}
