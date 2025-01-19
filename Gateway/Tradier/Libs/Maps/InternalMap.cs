using System;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Tradier.Client.Models.Account;
using Tradier.Messages;

namespace Tradier.Mappers
{
  public class InternalMap
  {
    /// <summary>
    /// Get point
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPoint(QuoteMessage message)
    {
      var point = new PointModel
      {
        Ask = message.Ask,
        Bid = message.Bid,
        AskSize = message.AskSize,
        BidSize = message.BidSize,
        Last = message.Bid,
        Time = DateTimeOffset.FromUnixTimeSeconds(message.BidDate).UtcDateTime
      };

      return point;
    }

    /// <summary>
    /// Get order
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetStreamOrder(OrderMessage message)
    {
      var action = new TransactionModel
      {
        Id = message.Id,
        Volume = message.ExecutedQuantity,
        Time = message.TransactionDate,
        Status = GetStatus(message.Status)
      };

      var order = new OrderModel
      {
        Id = message.Id,
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Volume = message.ExecutedQuantity
      };

      switch (message?.Type?.ToUpper())
      {
        case "STOP":
          order.Type = OrderTypeEnum.Stop;
          order.Price = message.StopPrice;
          break;

        case "LIMIT":
          order.Type = OrderTypeEnum.Limit;
          order.Price = message.Price;
          break;

        case "STOP_LIMIT":
          order.Type = OrderTypeEnum.StopLimit;
          order.Price = message.StopPrice;
          order.ActivationPrice = message.Price;
          break;
      }

      return order;
    }

    /// <summary>
    /// Get order
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetOrder(Order message)
    {
      var subOrders = message?.Leg ?? [];
      var basis = new InstrumentModel
      {
        Type = GetInstrumentType(message.Class),
        Name = string.Join(" / ", subOrders.Select(o => o.Symbol).Distinct())
      };

      var instrument = new InstrumentModel
      {
        Basis = basis,
        Type = GetInstrumentType(message.Class),
        Name = string.Join(" / ", subOrders.Select(o => o.OptionSymbol ?? o.Symbol))
      };

      var action = new TransactionModel
      {
        Id = $"{message.Id}",
        Volume = message.Quantity,
        Time = message.TransactionDate,
        Status = GetStatus(message.Status),
        Instrument = instrument
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Volume = message.Quantity,
        Side = GetOrderSide(message)
      };

      if (Equals(message?.Type?.ToUpper(), "MARKET") is false)
      {
        order.Type = OrderTypeEnum.Limit;
        order.Price = message.Price;
      }

      if (subOrders.Length is not 0)
      {
        order.Instruction = InstructionEnum.Group;

        foreach (var subOrder in subOrders)
        {
          var subInstrument = new InstrumentModel
          {
            Name = subOrder.Symbol,
            Type = GetInstrumentType(subOrder.Class)
          };

          var subAction = new TransactionModel
          {
            Instrument = subInstrument,
            Volume = subOrder.Quantity
          };

          order.Orders.Add(new OrderModel
          {
            Transaction = subAction,
            Volume = subOrder.Quantity,
            Side = GetSubOrderSide(subOrder.Side)
          });
        }
      }

      return order;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(Position message)
    {
      var volume = (double)Math.Abs(message.Quantity);
      var instrument = new InstrumentModel
      {
        Name = message.Symbol
      };

      var action = new TransactionModel
      {
        Instrument = instrument,
        Volume = volume
      };

      var order = new OrderModel
      {
        Volume = volume,
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Price = message.CostBasis / (volume * Math.Max(1, instrument.Leverage.Value)),
        Side = message.Quantity > 0 ? OrderSideEnum.Buy : OrderSideEnum.Sell
      };

      return order;
    }

    /// <summary>
    /// Convert remote order status to local
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static OrderStatusEnum? GetStatus(string status)
    {
      switch (status?.ToUpper())
      {
        case "OPEN":
        case "FILLED": return OrderStatusEnum.Filled;
        case "PARTIALLY_FILLED": return OrderStatusEnum.Partitioned;
        case "ERROR":
        case "EXPIRED":
        case "CANCELED":
        case "REJECTED": return OrderStatusEnum.Canceled;
        case "HELD":
        case "PENDING":
        case "CALCULATED":
        case "ACCEPTED_FOR_BIDDING": return OrderStatusEnum.Pending;
      }

      return null;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetSubOrderSide(string status)
    {
      switch (status?.ToUpper())
      {
        case "BUY":
        case "BUY_TO_OPEN":
        case "BUY_TO_CLOSE":
        case "NET_DEBIT": return OrderSideEnum.Buy;

        case "SELL":
        case "SELL_SHORT":
        case "SELL_TO_OPEN":
        case "SELL_TO_CLOSE":
        case "NET_CREDIT": return OrderSideEnum.Sell;
      }

      return null;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetOrderSide(Order message)
    {
      if (message.NumLegs > 0)
      {
        return OrderSideEnum.Group;
      }

      return GetSubOrderSide(message?.Side);
    }

    /// <summary>
    /// Asset type
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static InstrumentEnum? GetInstrumentType(string assetType)
    {
      switch (assetType?.ToUpper())
      {
        case "EQUITY": return InstrumentEnum.Shares;
        case "INDEX": return InstrumentEnum.Indices;
        case "FUTURE": return InstrumentEnum.Futures;
        case "OPTION": return InstrumentEnum.Options;
      }

      return InstrumentEnum.Group;
    }
  }
}
