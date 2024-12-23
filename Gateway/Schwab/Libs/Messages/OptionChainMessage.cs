using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Schwab.Messages
{
  public class OptionChainMessage
  {
    [JsonPropertyName("assetMainType")]
    public string AssetType { get; set; }

    [JsonPropertyName("assetSubType")]
    public string AssetSubType { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("underlying")]
    public UnderlyingMessage Underlying { get; set; }
    
    [JsonPropertyName("strategy")]
    public string Strategy { get; set; }
    
    [JsonPropertyName("interval")]
    public double? Interval { get; set; }
    
    [JsonPropertyName("isDelayed")]
    public bool? IsDelayed { get; set; }
    
    [JsonPropertyName("isIndex")]
    public bool? IsIndex { get; set; }
    
    [JsonPropertyName("daysToExpiration")]
    public double? DaysToExpiration { get; set; }

    [JsonPropertyName("interestRate")]
    public double? InterestRate { get; set; }
    
    [JsonPropertyName("underlyingPrice")]
    public double? UnderlyingPrice { get; set; }
    
    [JsonPropertyName("volatility")]
    public double? Volatility { get; set; }
    
    [JsonPropertyName("callExpDateMap")]
    public Dictionary<string, Dictionary<string, List<OptionMessage>>> CallExpDateMap { get; set; }
    
    [JsonPropertyName("putExpDateMap")]
    public Dictionary<string, Dictionary<string, List<OptionMessage>>> PutExpDateMap { get; set; }
  }
}
