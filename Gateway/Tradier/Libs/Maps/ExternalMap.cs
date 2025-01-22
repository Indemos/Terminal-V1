using Terminal.Core.Enums;

namespace Tradier.Mappers
{
  public class ExternalMap
  {
    /// <summary>
    /// Get external instrument type
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static string GetSide(OrderSideEnum? side)
    {
      switch (side)
      {
        case OrderSideEnum.Long: return "buy";
        case OrderSideEnum.Short: return "sell";
      }

      return null;
    }

    /// <summary>
    /// Order side
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public static string GetOrderType(OrderTypeEnum? message)
    {
      switch (message)
      {
        case OrderTypeEnum.Stop: return "stop";
        case OrderTypeEnum.Limit: return "limit";
        case OrderTypeEnum.StopLimit: return "stop_limit";
      }

      return "market";
    }

    /// <summary>
    /// Convert local time in force to remote
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static string GetTimeSpan(OrderTimeSpanEnum? span)
    {
      switch (span)
      {
        case OrderTimeSpanEnum.Am: return "pre";
        case OrderTimeSpanEnum.Pm: return "post";
        case OrderTimeSpanEnum.Day: return "day";
      }

      return "gtc";
    }
  }
}
