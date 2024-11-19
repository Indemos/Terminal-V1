using Distribution.Services;
using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using Terminal.Core.Services;

namespace Terminal.Core.Indicators
{
  /// <summary>
  /// Calculation mode
  /// </summary>
  public enum AveragePriceEnum : byte
  {
    Bid = 1,
    Ask = 2,
    Close = 3
  }

  /// <summary>
  /// Implementation
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class MaIndicator : Indicator<PointModel, MaIndicator>
  {
    /// <summary>
    /// Number of bars to average
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// Calculation mode
    /// </summary>
    public AveragePriceEnum Mode { get; set; }

    /// <summary>
    /// Preserve last calculated value
    /// </summary>
    public IList<double> Values { get; protected set; } = [];

    /// <summary>
    /// Calculate single value
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public override MaIndicator Calculate(IList<PointModel> collection)
    {
      var currentPoint = collection.LastOrDefault();

      if (currentPoint is null)
      {
        return this;
      }

      var value = currentPoint.Last.Value;
      var comService = InstanceService<AverageService>.Instance;

      switch (Mode)
      {
        case AveragePriceEnum.Bid: value = currentPoint.Bid.Value; break;
        case AveragePriceEnum.Ask: value = currentPoint.Ask.Value; break;
      }

      Values.Add(value);

      var series = currentPoint.Series[Name] = currentPoint.Series.Get(Name) ?? new MaIndicator().Point;
      var average = comService.LinearWeightAverage(Values, Values.Count - 1, Interval);

      Point.Last = series.Last = average.Is(0) ? value : average;

      return this;
    }
  }
}
