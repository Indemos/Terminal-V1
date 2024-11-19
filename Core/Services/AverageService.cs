using System.Collections.Generic;
using System.Linq;

namespace Terminal.Core.Services
{
  public class AverageService
  {
    /// <summary>
    /// Simple moving average
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public virtual double SimpleAverage(IList<double> collection, int index, int interval)
    {
      var response = 0.0;

      if (interval > 0 && index >= interval - 1)
      {
        for (var i = 0; i < interval; i++)
        {
          response += collection.ElementAt(index - i);
        }

        response /= interval;
      }

      return response;
    }

    /// <summary>
    /// Exponential moving average
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <param name="interval"></param>
    /// <param name="previous"></param>
    /// <returns></returns>
    public virtual double ExponentialAverage(IList<double> collection, int index, int interval, double previous)
    {
      var response = 0.0;

      if (interval > 0)
      {
        var pr = 2.0 / (interval + 1.0);
        response = collection.ElementAt(index) * pr + previous * (1 - pr);
      }

      return response;
    }

    /// <summary>
    /// Smooth moving average
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <param name="interval"></param>
    /// <param name="previous"></param>
    /// <returns></returns>
    public virtual double SmoothAverage(IList<double> collection, int index, int interval, double previous)
    {
      var response = 0.0;

      if (interval > 0)
      {
        if (index == interval - 1)
        {
          for (var i = 0; i < interval; i++)
          {
            response += collection.ElementAt(index - i);
          }

          response /= interval;
        }

        if (index >= interval)
        {
          response = (previous * (interval - 1) + collection.ElementAt(index)) / interval;
        }
      }

      return response;
    }

    /// <summary>
    /// Linear weighted moving average
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public virtual double LinearWeightAverage(IList<double> collection, int index, int interval)
    {
      var sum = 0.0;
      var wsum = 0.0;
      var response = 0.0;

      if (interval > 0 && index >= interval - 1)
      {
        for (var i = interval; i > 0; i--)
        {
          wsum += i;
          sum += collection.ElementAt(index - i + 1) * (interval - i + 1);
        }

        response = sum / wsum;
      }

      return response;
    }
  }
}
