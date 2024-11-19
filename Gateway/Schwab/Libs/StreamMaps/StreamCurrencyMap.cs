using System.Collections.Generic;

namespace Schwab.Mappers
{
  public class StreamCurrencyMap
  {
    public static IDictionary<string, string> Map { get; set; } = new Dictionary<string, string>
    {
      {"Symbol", "0"},
      {"Bid Price", "1"},
      {"Ask Price", "2"},
      {"Last Price", "3"},
      {"Bid Size", "4"},
      {"Ask Size", "5"},
      {"Total Volume", "6"},
      {"Last Size", "7"},
      {"Quote Time", "8"},
      {"Trade Time", "9"},
      {"High Price", "10"},
      {"Low Price", "11"},
      {"Close Price", "12"},
      {"Exchange", "13"},
      {"Description", "14"},
      {"Open Price", "15"},
      {"Net Change", "16"},
      {"Percent Change", "17"},
      {"Exchange Name", "18"},
      {"Digits", "19"},
      {"Security Status", "20"},
      {"Tick", "21"},
      {"Tick Amount", "22"},
      {"Product", "23"},
      {"Trading Hours", "24"},
      {"Is Tradable", "25"},
      {"Market Maker", "26"},
      {"52 Week High", "27"},
      {"52 Week Low", "28"},
      {"Mark", "29"}
    };
  }
}
