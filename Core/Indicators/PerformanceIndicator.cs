using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Models;

namespace Terminal.Core.Indicators
{
  /// <summary>
  /// Implementation
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class PerformanceIndicator : Indicator<PointModel, PerformanceIndicator>
  {
    /// <summary>
    /// Calculate indicator value
    /// </summary>
    /// <param name="accounts"></param>
    /// <returns></returns>
    public PerformanceIndicator Calculate(IList<IAccount> accounts)
    {
      Point.Last = accounts.Sum(o => o.Balance + o.Positions.Sum(v => v.Value.GetGainEstimate()));

      return this;
    }
  }
}
