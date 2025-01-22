using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tradier.Messages.Watchlist;

namespace Tradier
{
  /// <summary>
  /// The <c>Watchlist</c> class
  /// </summary>
  public partial class Adapter
  {
    /// <summary>
    /// Retrieve all of a users watchlists
    /// </summary>
    public async Task<WatchlistsMessage> GetWatchlists()
    {
      var uri = $"{DataUri}/watchlists";
      var response = await Send<WatchlistsCoreMessage>(uri);
      return response.Data?.Watchlists;
    }

    /// <summary>
    /// Retrieve a specific watchlist by id
    /// </summary>
    public async Task<WatchlistMessage> GetWatchlist(string watchlistId)
    {
      var uri = $"{DataUri}/watchlists/{watchlistId}";
      var response = await Send<WatchlistCoreMessage>(uri);
      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Create a new watchlist
    /// </summary>
    public async Task<WatchlistMessage> CreateWatchlist(string name, List<string> symbols)
    {
      var strSymbols = string.Join(",", symbols);
      var data = new Dictionary<string, string>
      {
        { "name", name },
        { "symbols", strSymbols },
      };

      var uri = $"{DataUri}/watchlists";
      var response = await Send<WatchlistCoreMessage>(uri, HttpMethod.Post, data);
      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Update an existing watchlist
    /// </summary>
    public async Task<WatchlistMessage> UpdateWatchlist(string watchlistId, string name, List<string> symbols = null)
    {
      var strSymbols = string.Join(",", symbols);
      var data = new Dictionary<string, string>
      {
        { "name", name },
        { "symbols", strSymbols },
      };

      var uri = $"{DataUri}/watchlists/{watchlistId}";
      var response = await Send<WatchlistCoreMessage>(uri, HttpMethod.Put, data);
      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Delete a specific watchlist
    /// </summary>
    public async Task<WatchlistsMessage> DeleteWatchlist(string watchlistId)
    {
      var uri = $"watchlists/{watchlistId}";
      var response = await Send<WatchlistsCoreMessage>(uri, HttpMethod.Delete);
      return response.Data?.Watchlists;
    }

    /// <summary>
    /// Add symbols to an existing watchlist. If the symbol exists, it will be over-written
    /// </summary>
    public async Task<WatchlistMessage> AddSymbolsToWatchlist(string watchlistId, List<string> symbols)
    {
      var strSymbols = string.Join(",", symbols);
      var data = new Dictionary<string, string>
      {
        { "symbols", strSymbols },
      };

      var uri = $"{DataUri}/watchlists/{watchlistId}/symbols";
      var response = await Send<WatchlistCoreMessage>(uri, HttpMethod.Post, data);
      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Remove a symbol from a specific watchlist
    /// </summary>
    public async Task<WatchlistMessage> RemoveSymbolFromWatchlist(string watchlistId, string symbol)
    {
      var uri = $"{DataUri}/watchlists/{watchlistId}/symbols/{symbol}";
      var response = await Send<WatchlistCoreMessage>(uri, HttpMethod.Delete);
      return response.Data?.Watchlist;
    }
  }
}
