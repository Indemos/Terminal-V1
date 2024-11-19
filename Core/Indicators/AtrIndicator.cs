using System;
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
  public class AtrIndicator : Indicator<PointModel, AtrIndicator>
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
    public override AtrIndicator Calculate(IList<PointModel> collection)
    {
      var currentPoint = collection.ElementAtOrDefault(collection.Count - 1);
      var previousPoint = collection.ElementAtOrDefault(collection.Count - 2);

      if (currentPoint is null || previousPoint is null)
      {
        return this;
      }

      var value =
        Math.Max(currentPoint.Bar.High.Value, previousPoint.Bar.Close.Value) -
        Math.Min(currentPoint.Bar.Low.Value, previousPoint.Bar.Close.Value);

      if (Values.Count > Interval)
      {
        value = (Values.Last() * Math.Max(Interval - 1, 0) + value) / Interval;
      }

      Values.Add(value);

      var series = currentPoint.Series[Name] =
        currentPoint.Series.Get(Name) ??
        new AtrIndicator().Point;

      Point.Last = series.Last = value;

      return this;
    }
  }
}
