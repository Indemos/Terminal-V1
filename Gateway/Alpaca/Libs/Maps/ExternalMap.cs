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
    public static NewOrderRequest GetOrder(OrderModel order)
    {
      var instrument = order.Transaction.Instrument;
      var name = instrument.Name;
      var volume = OrderQuantity.Fractional((decimal)order.Transaction.Volume);
      var side = order.Side is OrderSideEnum.Buy ? OrderSide.Buy : OrderSide.Sell;
      var orderType = GetOrderType(order.Type);
      var duration = GetTimeInForce(order.TimeSpan);
      var exOrder = new NewOrderRequest(name, volume, side, orderType, duration);
      var braces = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Brace)
        .Where(o => Equals(o.Name, order.Name));

      exOrder.ClientOrderId = order.Id;

      switch (order.Type)
      {
        case OrderTypeEnum.Stop: exOrder.StopPrice = (decimal)order.Price; break;
        case OrderTypeEnum.Limit: exOrder.LimitPrice = (decimal)order.Price; break;
        case OrderTypeEnum.StopLimit:
          exOrder.LimitPrice = (decimal)order.Price;
          exOrder.StopPrice = (decimal)order.ActivationPrice;
          break;
      }

      if (braces.Any())
      {
        var TP = GetBracePrice(order, order.Side is OrderSideEnum.Buy ? 1 : -1);
        var SL = GetBracePrice(order, order.Side is OrderSideEnum.Buy ? -1 : 1);

        exOrder.OrderClass = OrderClass.Bracket;
        exOrder.StopLossStopPrice = SL is null ? null : (decimal)SL;
        exOrder.TakeProfitLimitPrice = TP is null ? null : (decimal)TP;
      }

      return exOrder;
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

    /// <summary>
    /// Get order type
    /// </summary>
    /// <param name="orderType"></param>
    /// <returns></returns>
    public static OrderType GetOrderType(OrderTypeEnum? orderType)
    {
      switch (orderType)
      {
        case OrderTypeEnum.Stop: return OrderType.Stop;
        case OrderTypeEnum.Limit: return OrderType.Limit;
        case OrderTypeEnum.StopLimit: return OrderType.StopLimit;
      }

      return OrderType.Market;
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
  }
}
