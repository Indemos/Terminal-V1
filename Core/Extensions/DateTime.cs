using System;

namespace Terminal.Core.Extensions
{
  public static class DateTimeExtensions
  {
    /// <summary>
    /// Round by interval
    /// </summary>
    /// <param name="input"></param>
    /// <param name="span"></param>
    /// <returns></returns>
    public static DateTime? Round(this DateTime? input, TimeSpan? span)
    {
      if (input is null)
      {
        return null;
      }

      var date = input.Value.Ticks;
      var excess = Math.Max(span?.Ticks ?? 1, 1);

      return new DateTime(date - (date % excess), input.Value.Kind);
    }

    /// <summary>
    /// Date without time
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static DateOnly? AsDate(this DateTime? input)
    {
      if (input is null)
      {
        return null;
      }

      return DateOnly.FromDateTime(input.Value);
    }
  }
}
