using System;

namespace Terminal.Core.Extensions
{
  public static class DoubleExtensions
  {
    /// <summary>
    /// Equality with precision
    /// </summary>
    /// <param name="input"></param>
    /// <param name="num"></param>
    /// <param name="precision"></param>
    /// <returns></returns>
    public static bool Is(this double input, double num, double precision = double.Epsilon)
    {
      return Math.Abs(input - num) < precision;
    }

    /// <summary>
    /// Equality with precision
    /// </summary>
    /// <param name="input"></param>
    /// <param name="num"></param>
    /// <param name="precision"></param>
    /// <returns></returns>
    public static bool Is(this double? input, double num, double precision = double.Epsilon)
    {
      return Math.Abs(input.Value - num) < precision;
    }
  }
}
