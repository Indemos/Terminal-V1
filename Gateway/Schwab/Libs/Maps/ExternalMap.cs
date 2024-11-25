using Schwab.Messages;
using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Schwab.Mappers
{
  public class ExternalMap
  {
    public static string GetStreamingService(InstrumentModel instrument)
    {
      switch (instrument.Type)
      {
        case InstrumentEnum.Shares: return "LEVELONE_EQUITIES";
        case InstrumentEnum.Futures: return "LEVELONE_FUTURES";
        case InstrumentEnum.Currencies: return "LEVELONE_FOREX";
        case InstrumentEnum.Options: return "LEVELONE_OPTIONS";
        case InstrumentEnum.FuturesOptions: return "LEVELONE_FUTURES_OPTIONS";
      }

      return null;
    }

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
        Duration = "DAY",
        Session = "NORMAL",
        OrderType = "MARKET",
        OrderStrategyType = "SINGLE"
      };

      switch (order.Type)
      {
        case OrderTypeEnum.Stop:
          message.OrderType = "STOP";
          message.StopPrice = order.Price;
          break;

        case OrderTypeEnum.Limit:
          message.OrderType = "LIMIT";
          message.Price = order.Price;
          break;

        case OrderTypeEnum.StopLimit:
          message.OrderType = "STOP_LIMIT";
          message.Price = order.Price;
          message.StopPrice = order.ActivationPrice;
          break;
      }

      message.OrderLegCollection = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Side)
        .Select(GetOrderItem)
        .ToList();

      message.ChildOrderStrategies = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Brace)
        .Select(o =>
        {
          var subOrder = GetOrder(o);
          subOrder.OrderLegCollection = [GetOrderItem(o)];
          return subOrder;
        })
        .ToList();

      if (order?.Transaction?.Volume is not 0)
      {
        message.OrderLegCollection.Add(GetOrderItem(order));
      }

      if (message.ChildOrderStrategies.Count is not 0)
      {
        message.OrderStrategyType = "TRIGGER";
      }

      return message;
    }

    /// <summary>
    /// Create leg in a combo-order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public static OrderLegMessage GetOrderItem(OrderModel order)
    {
      var instrument = new InstrumentMessage
      {
        AssetType = GetInstrumentType(order.Transaction.Instrument.Type),
        Symbol = order.Transaction.Instrument.Name
      };

      var response = new OrderLegMessage
      {
        Instrument = instrument,
        Quantity = order.Transaction.Volume,
      };

      switch (order.Side)
      {
        case OrderSideEnum.Buy: response.Instruction = "BUY"; break;
        case OrderSideEnum.Sell: response.Instruction = "SELL"; break;
      }

      if (order.Transaction.Instrument.Type is InstrumentEnum.Options)
      {
        switch (order.Side)
        {
          case OrderSideEnum.Buy: response.Instruction = "BUY_TO_OPEN"; break;
          case OrderSideEnum.Sell: response.Instruction = "SELL_TO_OPEN"; break;
        }
      }

      return response;
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
