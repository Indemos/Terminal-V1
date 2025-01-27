using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Tradier.Mappers
{
  public class ExternalMap
  {
    /// <summary>
    /// Get external instrument type
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public static string GetSide(OrderModel order)
    {
      switch (order.Side)
      {
        case OrderSideEnum.Long: return "buy";
        case OrderSideEnum.Short: return "sell";
        case OrderSideEnum.ShortOpen: return "sell_short";
        case OrderSideEnum.ShortCover: return "buy_to_cover";
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
