using System.Text.Json.Serialization;

namespace Schwab.Messages
{
  public class OptionDeliverableMessage
  {    
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
    
    [JsonPropertyName("assetType")]
    public string AssetType { get; set; }
    
    [JsonPropertyName("deliverableUnits")]
    public double DeliverableUnits { get; set; }
    
    [JsonPropertyName("currencyType")]
    public string CurrencyType { get; set; }
  }
}
