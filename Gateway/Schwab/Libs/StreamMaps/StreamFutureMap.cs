using System.Collections.Generic;

namespace Schwab.Mappers
{
  public class StreamFutureMap
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
      {"Exchange ID", "15"},
      {"Description", "16"},
      {"Last ID", "17"},
      {"Open Price", "18"},
      {"Net Change", "19"},
      {"Future Percent Change", "20"},
      {"Exchange Name", "21"},
      {"Security Status", "22"},
      {"Open Interest", "23"},
      {"Mark", "24"},
      {"Tick", "25"},
      {"Tick Amount", "26"},
      {"Product", "27"},
      {"Future Price Format", "28"},
      {"Future Trading Hours", "29"},
      {"Future Is Tradable", "30"},
      {"Future Multiplier", "31"},
      {"Future Is Active", "32"},
      {"Future Settlement Price", "33"},
      {"Future Active Symbol", "34"},
      {"Future Expiration Date", "35"},
      {"Expiration Style", "36"},
      {"Ask Time", "37"},
      {"Bid Time", "38"},
      {"Quoted In Session", "39"},
      {"Settlement Date", "40"}
    };
  }
}
