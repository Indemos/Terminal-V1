using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tradier.Messages.MarketData;

namespace Tradier
{
  public partial class Adapter
  {
    /// <summary>
    /// Get all quotes in an option chain
    /// </summary>
    public async Task<OptionsMessage> GetOptionChain(string symbol, DateTime? expiration, bool greeks = true)
    {
      var stringExpiration = $"{expiration:yyyy-MM-dd}";
      var uri = $"{DataUri}/markets/options/chains?symbol={symbol}&expiration={stringExpiration}&greeks={greeks}";
      var response = await Send<OptionChainCoreMessage>(uri);
      return response.Data?.Options;
    }

    /// <summary>
    /// Get expiration dates for a particular underlying
    /// </summary>
    public async Task<ExpirationsMessage> GetOptionExpirations(string symbol, bool? includeAllRoots = true, bool? strikes = true)
    {
      var uri = $"{DataUri}/markets/options/expirations?symbol={symbol}&includeAllRoots={includeAllRoots}&strikes={strikes}";
      var response = await Send<OptionExpirationsCoreMessage>(uri);
      return response.Data?.Expirations;
    }

    /// <summary>
    /// Get a list of symbols using a keyword lookup on the symbols description
    /// </summary>
    public async Task<QuotesMessage> GetQuotes(IList<string> symbols, bool greeks = true)
    {
      var uri = $"{DataUri}/markets/quotes?symbols={string.Join(",", symbols)}&greeks={greeks}";
      var response = await Send<QuotesCoreMessage>(uri);
      return response.Data?.Quotes;
    }

    /// <summary>
    /// Get historical pricing for a security
    /// </summary>
    public async Task<HistoricalQuotesMessage> GetHistoricalQuotes(string symbol, string interval, DateTime start, DateTime end)
    {
      var maxDate = $"{end:yyyy-MM-dd}";
      var minDate = $"{start:yyyy-MM-dd}";
      var uri = $"{DataUri}/markets/history?symbol={symbol}&interval={interval}&start={minDate}&end={maxDate}";
      var response = await Send<HistoricalQuotesCoreMessage>(uri);
      return response.Data?.History;
    }

    /// <summary>
    /// Get a list of symbols using a keyword lookup on the symbols description using POST request
    /// </summary>
    public async Task<QuotesMessage> SearchQuotes(IList<string> symbols, bool greeks = true)
    {
      var names = string.Join(",", symbols);
      var data = new Dictionary<string, string>
      {
        { "symbols", names },
        { "greeks", $"{greeks}" },
      };

      var uri = $"{DataUri}/markets/quotes";
      var response = await Send<QuotesCoreMessage>(uri, HttpMethod.Post, data);
      return response.Data?.Quotes;
    }

    /// <summary>
    /// Get an options strike prices for a specified expiration date
    /// </summary>
    public async Task<StrikesMessage> GetStrikes(string symbol, DateTime expiration)
    {
      var stringExpiration = $"{expiration:yyyy-MM-dd}";
      var uri = $"{DataUri}/markets/options/strikes?symbol={symbol}&expiration={stringExpiration}";
      var response = await Send<OptionStrikesCoreMessage>(uri);
      return response.Data?.Strikes;
    }

    /// <summary>
    /// Time and Sales (timesales) is typically used for charting purposes. It captures pricing across a time slice at predefined intervals.
    /// </summary>
    public async Task<SeriesMessage> GetTimeSales(string symbol, string interval, DateTime start, DateTime end, string filter = "all")
    {
      var stringStart = $"{start:yyyy-MM-dd HH:mm}";
      var stringEnd = $"{end:yyyy-MM-dd HH:mm}";
      var uri = $"{DataUri}/markets/timesales?symbol={symbol}&interval={interval}&start={stringStart}&end={stringEnd}&session_filter={filter}";
      var response = await Send<TimeSalesCoreMessage>(uri);
      return response.Data?.Series;
    }

    /// <summary>
    /// The ETB list contains securities that are able to be sold short with a Tradier Brokerage account.
    /// </summary>
    public async Task<SecuritiesMessage> GetEtbSecurities()
    {
      var uri = $"{DataUri}/markets/etb";
      var response = await Send<SecuritiesCoreMessage>(uri);
      return response.Data?.Securities;
    }

    /// <summary>
    /// The ETB list contains securities that are able to be sold short with a Tradier Brokerage account.
    /// </summary>
    public async Task<ClockMessage> GetClock()
    {
      var uri = $"{DataUri}/markets/clock";
      var response = await Send<ClockCoreMessage>(uri);
      return response.Data?.Clock;
    }

    /// <summary>
    /// Get the market calendar for the current or given month
    /// </summary>
    public async Task<CalendarMessage> GetCalendar(int? month = null, int? year = null)
    {
      var uri = $"{DataUri}/markets/calendar?month={month}&year={year}";
      var response = await Send<CalendarCoreMessage>(uri);
      return response.Data?.Calendar;
    }

    /// <summary>
    /// Get the market calendar for the current or given month
    /// </summary>
    public async Task<SecuritiesMessage> SearchCompanies(string query, bool indexes = false)
    {
      var uri = $"{DataUri}/markets/search?q={query}&indexes={indexes}";
      var response = await Send<SecuritiesCoreMessage>(uri);
      return response.Data?.Securities;
    }

    /// <summary>
    /// Search for a symbol using the ticker symbol or partial symbol
    /// </summary>
    public async Task<SecuritiesMessage> LookupSymbol(string query, string exchanges = null, string types = null)
    {
      var urlBuilder = $"{DataUri}/markets/lookup?q={query}";

      urlBuilder += string.IsNullOrEmpty(exchanges) ? string.Empty : $"&exchanges={exchanges}";
      urlBuilder += string.IsNullOrEmpty(types) ? string.Empty : $"&types={types}";

      var response = await Send<SecuritiesCoreMessage>(urlBuilder);
      return response.Data?.Securities;
    }

    /// <summary>
    /// Get all options symbols for the given underlying
    /// </summary>
    public async Task<List<SymbolMessage>> LookupOptionSymbols(string symbol)
    {
      var uri = $"{DataUri}/markets/options/lookup?underlying={symbol}";
      var response = await Send<OptionSymbolsCoreMessage>(uri);
      return response.Data?.Symbols;
    }

    /// Fundamentals
    public async Task<List<CompanyDataMessage>> GetCompany(string symbols)
    {
      var uri = $"{DataUri}/beta/markets/fundamentals/company?symbols={symbols}";
      var response = await Send<CompanyDataCoreMessage>(uri);
      return response.Data?.Results;
    }

    public async Task<List<CorporateCalendarDataMessage>> GetCorporateCalendars(string symbols)
    {
      var uri = $"{DataUri}/beta/markets/fundamentals/calendars?symbols={symbols}";
      var response = await Send<CorporateCalendarCoreMessage>(uri);
      return response.Data?.Results;
    }
  }
}
