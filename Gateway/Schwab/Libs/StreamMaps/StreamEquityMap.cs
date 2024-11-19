using System.Collections.Generic;

namespace Schwab.Mappers
{
  public class StreamEquityMap
  {
    public static IDictionary<string, string> Map { get; set; } = new Dictionary<string, string>
    {
      {"Symbol", "0"},
      {"Bid Price", "1"},
      {"Ask Price", "2"},
      {"Last Price", "3"},
      {"Bid Size", "4"},
      {"Ask Size", "5"},
      {"Ask ID", "6"},
      {"Bid ID", "7"},
      {"Total Volume", "8"},
      {"Last Size", "9"},
      {"High Price", "10"},
      {"Low Price", "11"},
      {"Close Price", "12"},
      {"Exchange ID", "13"},
      {"Marginable", "14"},
      {"Description", "15"},
      {"Last ID", "16"},
      {"Open Price", "17"},
      {"Net Change", "18"},
      {"52 Week High", "19"},
      {"52 Week Low", "20"},
      {"PE Ratio", "21"},
      {"Annual Dividend Amount", "22"},
      {"Dividend Yield", "23"},
      {"NAV", "24"},
      {"Exchange Name", "25"},
      {"Dividend Date", "26"},
      {"Regular Market Quote", "27"},
      {"Regular Market Trade", "28"},
      {"Regular Market Last Price", "29"},
      {"Regular Market Last Size", "30"},
      {"Regular Market Net Change", "31"},
      {"Security Status", "32"},
      {"Mark Price", "33"},
      {"Quote Time in Long", "34"},
      {"Trade Time in Long", "35"},
      {"Regular Market Trade Time in Long", "36"},
      {"Bid Time", "37"},
      {"Ask Time", "38"},
      {"Ask MIC ID", "39"},
      {"Bid MIC ID", "40"},
      {"Last MIC ID", "41"},
      {"Net Percent Change", "42"},
      {"Regular Market Percent Change", "43"},
      {"Mark Price Net Change", "44"},
      {"Mark Price Percent Change", "45"},
      {"Hard to Borrow Quantity", "46"},
      {"Hard To Borrow Rate", "47"},
      {"Hard to Borrow", "48"},
      {"Shortable", "49"},
      {"Post-Market Net Change", "50"},
      {"Post-Market Percent Change", "51"}
    };
  }
}
