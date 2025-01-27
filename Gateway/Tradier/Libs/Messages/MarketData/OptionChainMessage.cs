using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class OptionChainCoreMessage
  {
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionsMessage Options { get; set; }
  }

  public class OptionsMessage
  {
    [JsonPropertyName("option")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OptionMessage> Options { get; set; }
  }

  public class OptionMessage
  {
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("exch")]
    public string Exchange { get; set; }

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

    [JsonPropertyName("underlying")]
    public string Underlying { get; set; }

    [JsonPropertyName("strike")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Strike { get; set; }

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
    public double? PreviousClose { get; set; }

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
    public string BidExchange { get; set; }

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

    [JsonPropertyName("greeks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GreeksMessage Greeks { get; set; }
  }

  public class GreeksMessage
  {
    [JsonPropertyName("delta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Delta { get; set; }

    [JsonPropertyName("gamma")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Gamma { get; set; }

    [JsonPropertyName("theta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Theta { get; set; }

    [JsonPropertyName("vega")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Vega { get; set; }

    [JsonPropertyName("rho")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Rho { get; set; }

    [JsonPropertyName("phi")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Phi { get; set; }

    [JsonPropertyName("bid_iv")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? BidIV { get; set; }

    [JsonPropertyName("mid_iv")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MidIV { get; set; }

    [JsonPropertyName("ask_iv")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AskIV { get; set; }

    [JsonPropertyName("smv_vol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SmvIV { get; set; }

    [JsonPropertyName("updated_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? UpdatedAt { get; set; }
  }
}
