using Coinbase.Messages;
using System;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Coinbase.Mappers
{
  public class InternalMap
  {
    /// <summary>
    /// Convert remote order to local
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public static OrderModel GetOrder(OrderMessage order)
    {
      var instrument = new InstrumentModel
      {
        //Name = order.Symbol
      };

      var action = new TransactionModel
      {
        //Id = order.OrderId,
        //Descriptor = order.ClientOrderId,
        //Instrument = instrument,
        //CurrentVolume = order.FilledQuantity,
        //Volume = order.Quantity,
        //Time = order.CreatedAtUtc,
        //Status = GetStatus(order.OrderStatus)
      };

      var inOrder = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        //Side = GetOrderSide(order.OrderSide),
        //TimeSpan = GetTimeSpan(order.TimeInForce)
      };

      //switch (order.OrderType)
      //{
      //  case "stop":
      //    inOrder.Type = OrderTypeEnum.Stop;
      //    inOrder.Price = order.StopPrice;
      //    break;

      //  case "limit":
      //    inOrder.Type = OrderTypeEnum.Limit;
      //    inOrder.Price = order.LimitPrice;
      //    break;

      //  case "stop_limit":
      //    inOrder.Type = OrderTypeEnum.StopLimit;
      //    inOrder.Price = order.StopPrice;
      //    inOrder.ActivationPrice = order.LimitPrice;
      //    break;
      //}

      return inOrder;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(SpotPositionMessage position)
    {
      var instrument = new InstrumentModel
      {
        //Name = position.Symbol
      };

      var action = new TransactionModel
      {
        //Id = position.AssetId,
        //Descriptor = position.Symbol,
        //Instrument = instrument,
        //Price = position.AverageEntryPrice,
        //CurrentVolume = position.AvailableQuantity,
        //Volume = position.Quantity
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        //Side = GetPositionSide(position.Side)
      };

      var gainLoss = 0; // position.CostBasis - position.MarketValue;

      return new OrderModel
      {
        GainMax = gainLoss,
        GainMin = gainLoss
      };
    }

    /// <summary>
    /// Convert remote order status to local
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static OrderStatusEnum GetStatus(string status)
    {
      switch (status)
      {
        case "fill":
        case "filled": return OrderStatusEnum.Filled;
        case "partial_fill":
        case "partially_filled": return OrderStatusEnum.Partitioned;
        case "stopped":
        case "expired":
        case "rejected":
        case "canceled":
        case "done_for_day": return OrderStatusEnum.Canceled;
        case "new":
        case "held":
        case "accepted":
        case "suspended":
        case "pending_new":
        case "pending_cancel":
        case "pending_replace": return OrderStatusEnum.Pending;
      }

      return OrderStatusEnum.None;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static OrderSideEnum GetOrderSide(string side)
    {
      switch (side)
      {
        case "buy": return OrderSideEnum.Long;
        case "sell": return OrderSideEnum.Short;
      }

      return OrderSideEnum.None;
    }

    /// <summary>
    /// Convert remote position side to local
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static OrderSideEnum GetPositionSide(string side)
    {
      switch (side)
      {
        case "long": return OrderSideEnum.Long;
        case "short": return OrderSideEnum.Short;
      }

      return OrderSideEnum.None;
    }

    /// <summary>
    /// Convert remote time in force to local
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static OrderTimeSpanEnum GetTimeSpan(string span)
    {
      switch (span)
      {
        case "day": return OrderTimeSpanEnum.Day;
        case "fok": return OrderTimeSpanEnum.Fok;
        case "gtc": return OrderTimeSpanEnum.Gtc;
        case "ioc": return OrderTimeSpanEnum.Ioc;
        case "opg": return OrderTimeSpanEnum.Am;
        case "cls": return OrderTimeSpanEnum.Pm;
      }

      return OrderTimeSpanEnum.None;
    }
  }
}
