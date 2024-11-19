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
  public class ScaleIndicator : Indicator<PointModel, ScaleIndicator>
  {
    /// <summary>
    /// Number of bars to average
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// Bottom border of the normalized series
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Top border of the normalized series
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Preserve last calculated value
    /// </summary>
    public IList<double> Values { get; protected set; } = [];

    /// <summary>
    /// Preserve last calculated min value
    /// </summary>
    protected double? _min = null;

    /// <summary>
    /// Preserve last calculated max value
    /// </summary>
    protected double? _max = null;

    /// <summary>
    /// Calculate indicator value
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public override ScaleIndicator Calculate(IList<PointModel> collection)
    {
      var currentPoint = collection.LastOrDefault();

      if (currentPoint is null)
      {
        return this;
      }

      var value = currentPoint.Last ?? 0.0;
      var comService = InstanceService<AverageService>.Instance;

      _min = _min is null ? value : Math.Min(_min.Value, value);
      _max = _max is null ? value : Math.Max(_max.Value, value);

      if (_min.Value.Is(_max.Value) is false)
      {
        value = Min + (value - _min.Value) * (Max - Min) / (_max.Value - _min.Value);
      }

      Values.Add(value);

      var series = currentPoint.Series[Name] =
        currentPoint.Series.Get(Name) ??
        new ScaleIndicator().Point;

      Point.Last = series.Last = series.Bar.Close = comService.LinearWeightAverage(Values, Values.Count - 1, Interval);

      return this;
    }
  }
}
