using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class OrderConfigurationMessage
  {
    [JsonPropertyName("market_market_ioc")]
    public MarketIocMessage MarketIoc { get; set; }

    [JsonPropertyName("limit_limit_gtc")]
    public LimitGtcMessage LimitGtc { get; set; }

    [JsonPropertyName("limit_limit_gtd")]
    public LimitGtdMessage LimitGtd { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtc")]
    public StopLimitGtcMessage StopLimitGtc { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtd")]
    public StopLimitGtdMessage StopLimitGtd { get; set; }
  }
}
