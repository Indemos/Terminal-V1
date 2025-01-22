using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.Watchlist
{
  public class WatchlistsCoreMessage
  {
    [JsonPropertyName("watchlists")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WatchlistsMessage Watchlists { get; set; }
  }

  public class WatchlistCoreMessage
  {
    [JsonPropertyName("watchlist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WatchlistMessage Watchlist { get; set; }
  }

  public class WatchlistsMessage
  {
    [JsonPropertyName("watchlist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<WatchlistMessage> Watchlist { get; set; }
  }

  public class WatchlistMessage
  {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("public_id")]
    public string PublicId { get; set; }

    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ItemsMessage Items { get; set; }
  }

  public class ItemsMessage
  {
    [JsonPropertyName("item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ItemMessage> Items { get; set; }
  }

  public class ItemMessage
  {
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
  }
}
