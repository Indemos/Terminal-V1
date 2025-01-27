using System.Collections;
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
      var source = $"{DataUri}/watchlists";
      var response = await Send<WatchlistsCoreMessage>(source);
      return response.Data?.Watchlists;
    }

    /// <summary>
    /// Retrieve a specific watchlist by id
    /// </summary>
    public async Task<WatchlistMessage> GetWatchlist(string watchlistId)
    {
      var source = $"{DataUri}/watchlists/{watchlistId}";
      var response = await Send<WatchlistCoreMessage>(source);
      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Create a new watchlist
    /// </summary>
    public async Task<WatchlistMessage> CreateWatchlist(string name, List<string> symbols)
    {
      var data = new Hashtable
      {
        { "name", name },
        { "symbols", string.Join(",", symbols) },
      };

      var source = $"{DataUri}/watchlists";
      var response = await Send<WatchlistCoreMessage>(source, HttpMethod.Post, data);
      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Update an existing watchlist
    /// </summary>
    public async Task<WatchlistMessage> UpdateWatchlist(string watchlistId, string name, List<string> symbols = null)
    {
      var data = new Hashtable
      {
        { "name", name },
        { "symbols", string.Join(",", symbols) },
      };

      var source = $"{DataUri}/watchlists/{watchlistId}";
      var response = await Send<WatchlistCoreMessage>(source, HttpMethod.Put, data);

      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Delete a specific watchlist
    /// </summary>
    public async Task<WatchlistsMessage> DeleteWatchlist(string watchlistId)
    {
      var source = $"watchlists/{watchlistId}";
      var response = await Send<WatchlistsCoreMessage>(source, HttpMethod.Delete);

      return response.Data?.Watchlists;
    }

    /// <summary>
    /// Add symbols to an existing watchlist. If the symbol exists, it will be over-written
    /// </summary>
    public async Task<WatchlistMessage> AddSymbolsToWatchlist(string watchlistId, List<string> symbols)
    {
      var data = new Hashtable
      {
        { "symbols", string.Join(",", symbols) },
      };

      var source = $"{DataUri}/watchlists/{watchlistId}/symbols";
      var response = await Send<WatchlistCoreMessage>(source, HttpMethod.Post, data);

      return response.Data?.Watchlist;
    }

    /// <summary>
    /// Remove a symbol from a specific watchlist
    /// </summary>
    public async Task<WatchlistMessage> RemoveSymbolFromWatchlist(string watchlistId, string symbol)
    {
      var source = $"{DataUri}/watchlists/{watchlistId}/symbols/{symbol}";
      var response = await Send<WatchlistCoreMessage>(source, HttpMethod.Delete);
      return response.Data?.Watchlist;
    }
  }
}
