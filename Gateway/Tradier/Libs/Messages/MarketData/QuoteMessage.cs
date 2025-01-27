using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class QuotesCoreMessage
  {
    [JsonPropertyName("quotes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QuotesMessage Quotes { get; set; }
  }

  public class QuotesMessage
  {
    [JsonPropertyName("quote")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<QuoteMessage> Items { get; set; }
  }

  public class QuoteMessage
  {
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("exch")]
    public string Exch { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("last")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Last { get; set; }

    [JsonPropertyName("change")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Change { get; set; }

    [JsonPropertyName("volume")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Volume { get; set; }

    [JsonPropertyName("open")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Open { get; set; }

    [JsonPropertyName("high")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? High { get; set; }

    [JsonPropertyName("low")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Low { get; set; }

    [JsonPropertyName("close")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Close { get; set; }

    [JsonPropertyName("bid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Bid { get; set; }

    [JsonPropertyName("ask")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Ask { get; set; }

    [JsonPropertyName("change_percentage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ChangePercentage { get; set; }

    [JsonPropertyName("average_volume")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AverageVolume { get; set; }

    [JsonPropertyName("last_volume")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LastVolume { get; set; }

    [JsonPropertyName("trade_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TradeDate { get; set; }

    [JsonPropertyName("prevclose")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? PrevClose { get; set; }

    [JsonPropertyName("week_52_high")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Week52High { get; set; }

    [JsonPropertyName("week_52_low")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Week52Low { get; set; }


    [JsonPropertyName("bidsize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BidSize { get; set; }

    [JsonPropertyName("bidexch")]
    public string Bidexch { get; set; }

    [JsonPropertyName("bid_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? BidDate { get; set; }

    [JsonPropertyName("asksize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AskSize { get; set; }

    [JsonPropertyName("askexch")]
    public string AskExchange { get; set; }

    [JsonPropertyName("ask_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? AskDate { get; set; }

    [JsonPropertyName("root_symbols")]
    public string RootSymbols { get; set; }

    [JsonPropertyName("underlying")]
    public string Underlying { get; set; }

    [JsonPropertyName("strike")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Strike { get; set; }

    [JsonPropertyName("open_interest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? OpenInterest { get; set; }

    [JsonPropertyName("contract_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ContractSize { get; set; }

    [JsonPropertyName("expiration_date")]
    public DateTime? ExpirationDate { get; set; }

    [JsonPropertyName("expiration_type")]
    public string ExpirationType { get; set; }

    [JsonPropertyName("option_type")]
    public string OptionType { get; set; }

    [JsonPropertyName("root_symbol")]
    public string RootSymbol { get; set; }
  }
}
