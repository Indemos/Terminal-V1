using Schwab.Messages;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Schwab.Mappers
{
  public class ExternalMap
  {
    /// <summary>
    /// Convert remote order from brokerage to local record
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public static OrderMessage GetOrder(OrderModel order)
    {
      var action = order.Transaction;
      var message = new OrderMessage
      {
        Session = "NORMAL",
        OrderStrategyType = "SINGLE",
        OrderType = GetOrderType(order.Type),
        Duration = GetTimeSpan(order.TimeSpan)
      };

      switch (order.Type)
      {
        case OrderTypeEnum.Stop: message.StopPrice = order.Price; break;
        case OrderTypeEnum.Limit: message.Price = order.Price; break;
        case OrderTypeEnum.StopLimit:
          message.Price = order.Price;
          message.StopPrice = order.ActivationPrice;
          break;
      }

      message.OrderLegCollection = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Side)
        .Select(o => GetSubOrder(o, order))
        .ToList();

      message.ChildOrderStrategies = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Brace)
        .Where(o => Equals(o.Name, order.Name))
        .Select(o =>
        {
          var subOrder = GetOrder(o);
          subOrder.OrderLegCollection = [GetSubOrder(o, order)];
          return subOrder;
        })
        .ToList();

      if (order?.Volume is not 0)
      {
        message.OrderLegCollection.Add(GetSubOrder(order));
      }

      if (message.ChildOrderStrategies.Count is not 0)
      {
        message.OrderStrategyType = "TRIGGER";
      }

      return message;
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
        case OrderTypeEnum.Stop: return "STOP";
        case OrderTypeEnum.Limit: return "LIMIT";
        case OrderTypeEnum.StopLimit: return "STOP_LIMIT";
      }

      return "MARKET";
    }

    /// <summary>
    /// Service name based on asset type
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public static string GetStreamingService(InstrumentModel instrument)
    {
      switch (instrument.Type)
      {
        case InstrumentEnum.Futures: return "LEVELONE_FUTURES";
        case InstrumentEnum.Currencies: return "LEVELONE_FOREX";
        case InstrumentEnum.Options: return "LEVELONE_OPTIONS";
        case InstrumentEnum.FutureOptions: return "LEVELONE_FUTURES_OPTIONS";
      }

      return "LEVELONE_EQUITIES";
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
        case OrderTimeSpanEnum.Day: return "DAY";
        case OrderTimeSpanEnum.Fok: return "FILL_OR_KILL";
      }

      return "GOOD_TILL_CANCEL";
    }

    /// <summary>
    /// Create leg in a combo-order
    /// </summary>
    /// <param name="order"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static OrderLegMessage GetSubOrder(OrderModel order, OrderModel group = null)
    {
      var action = order?.Transaction ?? group?.Transaction;
      var assetType = action?.Instrument?.Type ?? group?.Transaction?.Instrument?.Type;
      var side = order?.Side ?? group?.Side;
      var instrument = new InstrumentMessage
      {
        AssetType = GetInstrumentType(assetType),
        Symbol = action.Instrument.Name
      };

      var response = new OrderLegMessage
      {
        Instrument = instrument,
        Quantity = order.Volume,
        Instruction = GetSide(assetType, side)
      };

      return response;
    }

    /// <summary>
    /// Get external instrument type
    /// </summary>
    /// <param name="assetType"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public static string GetSide(InstrumentEnum? assetType, OrderSideEnum? side)
    {
      switch (side)
      {
        case OrderSideEnum.Long: return "BUY"; 
        case OrderSideEnum.Short: return "SELL"; 
      }

      if (assetType is InstrumentEnum.Options)
      {
        switch (side)
        {
          case OrderSideEnum.Long: return "BUY_TO_OPEN"; 
          case OrderSideEnum.Short: return "SELL_TO_OPEN";
        }
      }

      return null;
    }

    /// <summary>
    /// Get external instrument type
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string GetInstrumentType(InstrumentEnum? message)
    {
      switch (message)
      {
        case InstrumentEnum.Shares: return "EQUITY";
        case InstrumentEnum.Options: return "OPTION";
      }

      return null;
    }
  }
}
