namespace Oanda.Messages
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class JsonAccountChange
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("changes")]
    public JsonChangeSummary Summary { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("lastTransactionID")]
    public long? LastTransactionId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("state")]
    public JsonState State { get; set; }
  }

  public partial class JsonChangeSummary
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("ordersCancelled")]
    public List<JsonOrder> OrdersCancelled { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("ordersCreated")]
    public List<JsonOrder> OrdersCreated { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("ordersFilled")]
    public List<JsonOrder> OrdersFilled { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("ordersTriggered")]
    public List<JsonOrder> OrdersTriggered { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("positions")]
    public List<JsonPosition> Positions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tradesClosed")]
    public List<JsonTradeSummary> TradesClosed { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tradesOpened")]
    public List<JsonTradeSummary> TradesOpened { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tradesReduced")]
    public List<JsonTradeSummary> TradesReduced { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("transactions")]
    public List<JsonTransaction> Transactions { get; set; }
  }
}
