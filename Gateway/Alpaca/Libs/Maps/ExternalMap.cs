using Alpaca.Markets;
using System.Linq;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Alpaca.Mappers
{
  public class ExternalMap
  {
    /// <summary>
    /// Send orders
    /// </summary>
    /// <param name="orders"></param>
    /// <returns></returns>
    public static OrderBase GetOrder(OrderModel order)
    {
      var instrument = order.Transaction.Instrument;
      var name = instrument.Name;
      var volume = OrderQuantity.Fractional((decimal)order.Transaction.Volume);
      var side = order.Side is OrderSideEnum.Buy ? OrderSide.Buy : OrderSide.Sell;

      SimpleOrderBase getOrder()
      {
        switch (order.Type)
        {
          case OrderTypeEnum.Stop: return side.Stop(name, volume, (decimal)order.Price);
          case OrderTypeEnum.Limit: return side.Limit(name, volume, (decimal)order.Price);
          case OrderTypeEnum.StopLimit: return side.StopLimit(name, volume, (decimal)order.ActivationPrice, (decimal)order.Price);
        }

        return side.Market(name, volume);
      }

      var exOrder = getOrder();

      exOrder.Duration = GetTimeInForce(order.TimeSpan);

      var exBrace = exOrder as OrderBase;
      var braces = order.Orders.Where(o => o.Instruction is InstructionEnum.Brace);

      if (braces.Any())
      {
        switch (order.Side)
        {
          case OrderSideEnum.Buy:
            {
              var TP = GetBracePrice(order, 1);
              var SL = GetBracePrice(order, -1);
              exBrace = GetBraces(exOrder, SL, TP);
            }
            break;

          case OrderSideEnum.Sell:
            {
              var SL = GetBracePrice(order, 1);
              var TP = GetBracePrice(order, -1);
              exBrace = GetBraces(exOrder, SL, TP);
            }
            break;
        }
      }

      return exBrace;
    }

    /// <summary>
    /// Convert child orders to brackets
    /// </summary>
    /// <param name="order"></param>
    /// <param name="SL"></param>
    /// <param name="TP"></param>
    /// <returns></returns>
    protected static OrderBase GetBraces(SimpleOrderBase order, double? SL, double? TP)
    {
      switch (true)
      {
        case true when SL is not null && TP is not null: return order.StopLoss((decimal)SL).TakeProfit((decimal)TP);
        case true when TP is not null: return order.TakeProfit((decimal)TP);
        case true when SL is not null: return order.StopLoss((decimal)SL);
      }

      return order;
    }

    /// <summary>
    /// Get price for brackets
    /// </summary>
    /// <param name="order"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    protected static double? GetBracePrice(OrderModel order, double direction)
    {
      var nextOrder = order
        .Orders
        .FirstOrDefault(o => (o.Price - order.Price) * direction > 0);

      return nextOrder?.Price;
    }

    /// <summary>
    /// Get order duration
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeInForce GetTimeInForce(OrderTimeSpanEnum? timeSpan)
    {
      switch (timeSpan)
      {
        case OrderTimeSpanEnum.Day: return TimeInForce.Day;
        case OrderTimeSpanEnum.Fok: return TimeInForce.Fok;
        case OrderTimeSpanEnum.Gtc: return TimeInForce.Gtc;
        case OrderTimeSpanEnum.Ioc: return TimeInForce.Ioc;
        case OrderTimeSpanEnum.Am: return TimeInForce.Opg;
        case OrderTimeSpanEnum.Pm: return TimeInForce.Cls;
      }

      return TimeInForce.Gtc;
    }
  }
}
