using IBApi;
using InteractiveBrokers.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace InteractiveBrokers.Mappers
{
  public class InternalMap
  {
    /// <summary>
    /// Get order book
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPrice(TickByTickBidAskMessage message)
    {
      var point = new PointModel
      {
        Ask = message.AskPrice,
        Bid = message.BidPrice,
        AskSize = (double)message.AskSize,
        BidSize = (double)message.BidSize,
        Last = message.BidPrice,
        Time = DateTimeOffset.FromUnixTimeSeconds(message.Time).UtcDateTime
      };

      return point;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetOrder(OpenOrderMessage message)
    {
      var instrument = new InstrumentModel
      {
        Name = message.Contract.Symbol,
        Type = GetInstrumentType(message.Contract.SecType)
      };

      var action = new TransactionModel
      {
        Instrument = instrument,
        Id = $"{message.Order.PermId}",
        Descriptor = $"{message.Order.OrderRef}",
        CurrentVolume = (double)Math.Min(message.Order.FilledQuantity, message.Order.TotalQuantity),
        Volume = (double)message.Order.TotalQuantity,
        Time = DateTime.TryParse(message.Order.ActiveStartTime, out var o) ? o : DateTime.UtcNow,
        Status = GetStatus(message.OrderState.Status)
      };

      var inOrder = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetOrderSide(message.Order.Action),
        TimeSpan = GetTimeSpan($"{message.Order.Duration}")
      };

      switch (message.Order.OrderType)
      {
        case "STP":
          inOrder.Type = OrderTypeEnum.Stop;
          inOrder.Price = message.Order.AdjustedStopPrice;
          break;

        case "LMT":
          inOrder.Type = OrderTypeEnum.Limit;
          inOrder.Price = message.Order.LmtPrice;
          break;

        case "STP LMT":
          inOrder.Type = OrderTypeEnum.StopLimit;
          inOrder.Price = message.Order.AdjustedStopPrice;
          inOrder.ActivationPrice = message.Order.LmtPrice;
          break;
      }

      return inOrder;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(PositionMultiMessage message)
    {
      var instrument = new InstrumentModel
      {
        Name = message.Contract.Symbol
      };

      var action = new TransactionModel
      {
        Id = instrument.Name,
        Instrument = instrument,
        Descriptor = instrument.Name,
        Price = message.AverageCost,
        CurrentVolume = (double)message.Position,
        Volume = (double)message.Position
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetPositionSide(message.Contract)
      };

      var gainLoss = 0.0;

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
        case "buy": return OrderSideEnum.Buy;
        case "sell": return OrderSideEnum.Sell;
      }

      return OrderSideEnum.None;
    }

    /// <summary>
    /// Convert remote position side to local
    /// </summary>
    /// <param name="contract"></param>
    /// <returns></returns>
    public static OrderSideEnum GetPositionSide(Contract contract)
    {
      var buys = contract.ComboLegs.Where(o => o.Action.Contains("BUY")).Sum(o => o.Ratio);
      var sells = contract.ComboLegs.Where(o => o.Action.Contains("SELL")).Sum(o => o.Ratio);

      switch (true)
      {
        case true when buys > sells: return OrderSideEnum.Buy;
        case true when buys < sells: return OrderSideEnum.Sell;
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


    /// <summary>
    /// Get external instrument type
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static InstrumentEnum? GetInstrumentType(string message)
    {
      switch (message)
      {
        case "BOND": return InstrumentEnum.Bonds;
        case "OPT": return InstrumentEnum.Shares;
        case "STK": return InstrumentEnum.Options;
        case "FUT": return InstrumentEnum.Futures;
        case "CFD": return InstrumentEnum.Contracts;
        case "CASH": return InstrumentEnum.Currencies;
      }

      return null;
    }

    /// <summary>
    /// Get option contract
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static IEnumerable<InstrumentModel> GetOptions(SecurityDefinitionOptionParameterMessage message)
    {
      var options = message.Expirations.SelectMany(expiration => message.Strikes.Select(strike =>
      {
        var point = new PointModel
        {
          Ask = 0,
          Bid = 0,
          Last = 0
        };

        var derivative = new DerivativeModel
        {
          Strike = strike,
          Expiration = DateTime.ParseExact(expiration, "yyyyMMdd", CultureInfo.InvariantCulture)
        };

        var option = new InstrumentModel
        {
          Point = point,
          Derivative = derivative
        };

        return option;
      }));

      return options;
    }
  }
}
