using System.Collections.Generic;

namespace Schwab.Mappers
{
  public class StreamFutureOptionMap
  {
    public static IDictionary<string, string> Map { get; set; } = new Dictionary<string, string>
    {
      {"Symbol", "0"},
      {"Bid Price", "1"},
      {"Ask Price", "2"},
      {"Last Price", "3"},
      {"Bid Size", "4"},
      {"Ask Size", "5"},
      {"Bid ID", "6"},
      {"Ask ID", "7"},
      {"Total Volume", "8"},
      {"Last Size", "9"},
      {"Quote Time", "10"},
      {"Trade Time", "11"},
      {"High Price", "12"},
      {"Low Price", "13"},
      {"Close Price", "14"},
      {"Last ID", "15"},
      {"Description", "16"},
      {"Open Price", "17"},
      {"Open Interest", "18"},
      {"Mark", "19"},
      {"Tick", "20"},
      {"Tick Amount", "21"},
      {"Future Multiplier", "22"},
      {"Future Settlement Price", "23"},
      {"Underlying Symbol", "24"},
      {"Strike Price", "25"},
      {"Future Expiration Date", "26"},
      {"Expiration Style", "27"},
      {"Contract Type", "28"},
      {"Security Status", "29"},
      {"Exchange", "30"},
      {"Exchange Name", "31"}
    };
  }
}
