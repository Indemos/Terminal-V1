using Distribution.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using Terminal.Core.Services;

namespace Terminal.Core.Indicators
{
  /// <summary>
  /// Implementation
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class RsiIndicator : Indicator<PointModel, RsiIndicator>
  {
    /// <summary>
    /// Number of bars to average
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// Preserve last calculated value
    /// </summary>
    public IList<double> Values { get; protected set; } = [];

    /// <summary>
    /// Calculate single value
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public override RsiIndicator Calculate(IList<PointModel> collection)
    {
      var currentPoint = collection.LastOrDefault();

      if (currentPoint is null)
      {
        return this;
      }

      var ups = new List<double>(Interval);
      var downs = new List<double>(Interval);
      var comService = InstanceService<AverageService>.Instance;

      for (var i = 1; i <= Interval; i++)
      {
        var nextPrice = collection.ElementAtOrDefault(collection.Count - i);
        var previousPrice = collection.ElementAtOrDefault(collection.Count - i - 1);

        if (nextPrice is not null && previousPrice is not null)
        {
          ups.Add(Math.Max(nextPrice.Last.Value - previousPrice.Last.Value, 0.0));
          downs.Add(Math.Max(previousPrice.Last.Value - nextPrice.Last.Value, 0.0));
        }
      }

      var averageUp = comService.SimpleAverage(ups, ups.Count - 1, Interval);
      var averageDown = comService.SimpleAverage(downs, downs.Count - 1, Interval);
      var average = averageDown.Is(0) ? 1.0 : averageUp / averageDown;
      var value = 100.0 - 100.0 / (1.0 + average);

      Values.Add(value);

      var series = currentPoint.Series[Name] =
        currentPoint.Series.Get(Name) ??
        new RsiIndicator().Point;

      Point.Last = series.Last = value;

      return this;
    }
  }
}
