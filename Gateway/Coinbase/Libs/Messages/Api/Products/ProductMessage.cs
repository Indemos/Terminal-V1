using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class ProductMessage
  {
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("price")]
    public double? Price { get; set; }

    [JsonPropertyName("price_percentage_change_24h")]
    public double? PricePercentageChange24H { get; set; }

    [JsonPropertyName("volume_24h")]
    public double? Volume24H { get; set; }

    [JsonPropertyName("volume_percentage_change_24h")]
    public double? VolumePercentageChange24H { get; set; }

    [JsonPropertyName("base_increment")]
    public double? BaseIncrement { get; set; }

    [JsonPropertyName("quote_increment")]
    public double? QuoteIncrement { get; set; }

    [JsonPropertyName("quote_min_size")]
    public double? QuoteMinSize { get; set; }

    [JsonPropertyName("quote_max_size")]
    public double? QuoteMaxSize { get; set; }

    [JsonPropertyName("base_min_size")]
    public double? BaseMinSize { get; set; }

    [JsonPropertyName("base_max_size")]
    public double? BaseMaxSize { get; set; }

    [JsonPropertyName("base_name")]
    public string BaseName { get; set; }

    [JsonPropertyName("quote_name")]
    public string QuoteName { get; set; }

    [JsonPropertyName("watched")]
    public bool Watched { get; set; }

    [JsonPropertyName("is_disabled")]
    public bool IsDisabled { get; set; }

    [JsonPropertyName("new")]
    public bool IsNew { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("status_message")]
    public string StatusMessage { get; set; }

    [JsonPropertyName("cancel_only")]
    public bool CancelOnly { get; set; }

    [JsonPropertyName("min_market_funds")]
    public double? MinMarketFunds { get; set; }

    [JsonPropertyName("limit_only")]
    public bool LimitOnly { get; set; }

    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }

    [JsonPropertyName("trading_disabled")]
    public bool TradingDisabled { get; set; }

    [JsonPropertyName("auction_mode")]
    public bool AuctionMode { get; set; }

    [JsonPropertyName("product_type")]
    public string ProductType { get; set; }

    [JsonPropertyName("quote_currency")]
    public string QuoteCurrency { get; set; }

    [JsonPropertyName("quote_currency_id")]
    public string QuoteCurrencyId { get; set; }

    [JsonPropertyName("base_currency")]
    public string BaseCurrency { get; set; }

    [JsonPropertyName("base_currency_id")]
    public string BaseCurrencyId { get; set; }

    [JsonPropertyName("mid_market_price")]
    public double? MidMarketPrice { get; set; }

    [JsonPropertyName("base_display_symbol")]
    public string BaseDisplaySymbol { get; set; }

    [JsonPropertyName("quote_display_symbol")]
    public string QuoteDisplaySymbol { get; set; }
  }
}
