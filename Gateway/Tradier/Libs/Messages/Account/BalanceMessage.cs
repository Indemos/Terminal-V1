using System.Text.Json.Serialization;

namespace Tradier.Messages.Account
{
  public class BalanceCoreMessage
  {
    [JsonPropertyName("balances")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BalanceMessage Balance { get; set; }
  }

  public class BalanceMessage
  {
    [JsonPropertyName("option_short_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OptionShortValue { get; set; }

    [JsonPropertyName("total_equity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TotalEquity { get; set; }

    [JsonPropertyName("account_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AccountNumber { get; set; }

    [JsonPropertyName("account_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AccountType { get; set; }

    [JsonPropertyName("close_pl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ClosePL { get; set; }

    [JsonPropertyName("current_requirement")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CurrentRequirement { get; set; }

    [JsonPropertyName("equity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Equity { get; set; }

    [JsonPropertyName("long_market_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? LongMarketValue { get; set; }

    [JsonPropertyName("market_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MarketValue { get; set; }

    [JsonPropertyName("open_pl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OpenPL { get; set; }

    [JsonPropertyName("option_long_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OptionLongValue { get; set; }

    [JsonPropertyName("option_requirement")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OptionRequirement { get; set; }

    [JsonPropertyName("pending_orders_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PendingOrdersCount { get; set; }

    [JsonPropertyName("short_market_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ShortMarketValue { get; set; }

    [JsonPropertyName("stock_long_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? StockLongValue { get; set; }

    [JsonPropertyName("total_cash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TotalCash { get; set; }

    [JsonPropertyName("uncleared_funds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? UnclearedFunds { get; set; }

    [JsonPropertyName("pending_cash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? PendingCash { get; set; }

    [JsonPropertyName("margin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MarginMessage Margin { get; set; }

    [JsonPropertyName("cash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CashMessage Cash { get; set; }

    [JsonPropertyName("pdt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PatternDayTraderMessage PatternDayTrader { get; set; }
  }

  public class MarginMessage
  {
    [JsonPropertyName("fed_call")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FedCall { get; set; }

    [JsonPropertyName("maintenance_call")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaintenanceCall { get; set; }

    [JsonPropertyName("option_buying_power")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OptionBuyingPower { get; set; }

    [JsonPropertyName("stock_buying_power")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? StockBuyingPower { get; set; }

    [JsonPropertyName("stock_short_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StockShortValue { get; set; }

    [JsonPropertyName("sweep")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Sweep { get; set; }
  }

  public class CashMessage
  {
    [JsonPropertyName("cash_available")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CashAvailable { get; set; }

    [JsonPropertyName("sweep")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Sweep { get; set; }

    [JsonPropertyName("unsettled_funds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? UnsettledFunds { get; set; }
  }

  public class PatternDayTraderMessage
  {
    [JsonPropertyName("fed_call")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FedCall { get; set; }

    [JsonPropertyName("maintenance_call")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaintenanceCall { get; set; }

    [JsonPropertyName("option_buying_power")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? OptionBuyingPower { get; set; }

    [JsonPropertyName("stock_buying_power")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? StockBuyingPower { get; set; }

    [JsonPropertyName("stock_short_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StockShortValue { get; set; }
  }
}
