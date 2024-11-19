using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Schwab.Messages
{
  public class UnderlyingMessage
  {    
    [JsonPropertyName("ask")]
    public double? Ask { get; set; }
    
    [JsonPropertyName("askSize")]
    public double? AskSize { get; set; }
    
    [JsonPropertyName("bid")]
    public double? Bid { get; set; }
    
    [JsonPropertyName("bidSize")]
    public double? BidSize { get; set; }
    
    [JsonPropertyName("change")]
    public double? Change { get; set; }
    
    [JsonPropertyName("close")]
    public double? Close { get; set; }
    
    [JsonPropertyName("delayed")]
    public bool? Delayed { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("exchangeName")]
    public string ExchangeName { get; set; }
    
    [JsonPropertyName("fiftyTwoWeekHigh")]
    public double? FiftyTwoWeekHigh { get; set; }
    
    [JsonPropertyName("fiftyTwoWeekLow")]
    public double? FiftyTwoWeekLow { get; set; }
    
    [JsonPropertyName("highPrice")]
    public double? HighPrice { get; set; }
    
    [JsonPropertyName("last")]
    public double? Last { get; set; }
    
    [JsonPropertyName("lowPrice")]
    public double? LowPrice { get; set; }
    
    [JsonPropertyName("mark")]
    public double? Mark { get; set; }
    
    [JsonPropertyName("markChange")]
    public double? MarkChange { get; set; }
    
    [JsonPropertyName("markPercentChange")]
    public double? MarkPercentChange { get; set; }
    
    [JsonPropertyName("openPrice")]
    public double? OpenPrice { get; set; }
    
    [JsonPropertyName("percentChange")]
    public double? PercentChange { get; set; }
    
    [JsonPropertyName("quoteTime")]
    public double? QuoteTime { get; set; }
    
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
    
    [JsonPropertyName("totalVolume")]
    public double? TotalVolume { get; set; }
    
    [JsonPropertyName("tradeTime")]
    public double? TradeTime { get; set; }
  }
}
