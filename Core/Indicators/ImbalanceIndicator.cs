using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Extensions;
using Terminal.Core.Models;

namespace Terminal.Core.Indicators
{
  /// <summary>
  /// Implementation
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ImbalanceIndicator : Indicator<PointModel, ImbalanceIndicator>
  {
    /// <summary>
    /// Calculate indicator value
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public ImbalanceIndicator Calculate(IList<PointModel> collection, int side = 0)
    {
      var currentPoint = collection.LastOrDefault();

      if (currentPoint is null)
      {
        return this;
      }

      var value = 0.0;
      var seriesItem = currentPoint.Series[Name] =
        currentPoint.Series.Get(Name) ??
        new ImbalanceIndicator().Point;

      switch (side)
      {
        case 0: value = currentPoint.AskSize.Value - currentPoint.BidSize.Value; break;
        case 1: value = currentPoint.AskSize.Value; break;
        case -1: value = currentPoint.BidSize.Value; break;
      }

      Point.Last = seriesItem.Last = value;

      return this;
    }
  }
}
