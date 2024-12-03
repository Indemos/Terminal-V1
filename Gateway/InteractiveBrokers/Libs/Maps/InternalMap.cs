using IBApi;
using InteractiveBrokers.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
      var instrument = GetInstrument(message.Contract);
      var action = new TransactionModel
      {
        Instrument = instrument,
        Id = $"{message.Order.PermId}",
        Descriptor = $"{message.Contract.ConId}",
        CurrentVolume = (double)Math.Min(message.Order.FilledQuantity, message.Order.TotalQuantity),
        Volume = (double)message.Order.TotalQuantity,
        Time = DateTime.TryParse(message.Order.ActiveStartTime, out var o) ? o : DateTime.UtcNow,
        Status = GetOrderStatus(message.OrderState.Status)
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetOrderSide(message.Order.Action),
        TimeSpan = GetTimeSpan($"{message.Order.Tif}"),
        Price = message.Order.LmtPrice
      };

      switch (message.Order.OrderType)
      {
        case "STP":
          order.Type = OrderTypeEnum.Stop;
          order.Price = message.Order.AuxPrice;
          break;

        case "LMT":
          order.Type = OrderTypeEnum.Limit;
          order.Price = message.Order.LmtPrice;
          break;

        case "STP LMT":
          order.Type = OrderTypeEnum.StopLimit;
          order.Price = message.Order.LmtPrice;
          order.ActivationPrice = message.Order.AuxPrice;
          break;
      }

      return order;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(PositionMultiMessage message)
    {
      var volume = (double)Math.Abs(message.Position);
      var instrument = GetInstrument(message.Contract);
      var action = new TransactionModel
      {
        Instrument = instrument,
        Descriptor = $"{message.Contract.ConId}",
        CurrentVolume = volume,
        Volume = volume
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Price = message.AverageCost / Math.Max(1, instrument.Leverage.Value),
        Side = message.Position > 0 ? OrderSideEnum.Buy : OrderSideEnum.Sell
      };

      return order;
    }

    /// <summary>
    /// Convert remote order status to local
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static OrderStatusEnum? GetOrderStatus(string status)
    {
      switch (status)
      {
        case "ApiPending":
        case "Submitted":
        case "PreSubmitted":
        case "PendingSubmit":
        case "PendingCancel": return OrderStatusEnum.Pending;
        case "Inactive":
        case "Cancelled":
        case "ApiCancelled": return OrderStatusEnum.Canceled;
        case "Filled": return OrderStatusEnum.Filled;
      }

      return null;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetOrderSide(string side)
    {
      switch (side)
      {
        case "BUY": return OrderSideEnum.Buy;
        case "SELL": return OrderSideEnum.Sell;
      }

      return null;
    }

    /// <summary>
    /// Convert remote time in force to local
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static OrderTimeSpanEnum? GetTimeSpan(string span)
    {
      switch (span)
      {
        case "DAY": return OrderTimeSpanEnum.Day;
        case "GTC": return OrderTimeSpanEnum.Gtc;
      }

      return null;
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
        case "STK": return InstrumentEnum.Shares;
        case "OPT": return InstrumentEnum.Options;
        case "FUT": return InstrumentEnum.Futures;
        case "CFD": return InstrumentEnum.Contracts;
        case "CASH": return InstrumentEnum.Currencies;
        case "FOP": return InstrumentEnum.FuturesOptions;
      }

      return null;
    }

    /// <summary>
    /// Get instrument from contract
    /// </summary>
    /// <param name="contract"></param>
    /// <returns></returns>
    public static InstrumentModel GetInstrument(Contract contract)
    {
      var expiration = contract.LastTradeDateOrContractMonth;
      var response = new InstrumentModel
      {
        Id = $"{contract.ConId}",
        Name = contract.LocalSymbol,
        Exchange = contract.Exchange ?? "SMART",
        Type = GetInstrumentType(contract.SecType),
        Currency = new CurrencyModel { Name = contract.Currency },
        Leverage = int.TryParse(contract.Multiplier, out var leverage) ? Math.Max(1, leverage) : 1
      };

      if (string.IsNullOrEmpty(contract.Symbol) is false)
      {
        response.Basis = new InstrumentModel
        {
          Name = contract.Symbol
        };
      }

      if (string.IsNullOrEmpty(expiration) is false)
      {
        var derivative = new DerivativeModel
        {
          Strike = contract.Strike,
          Expiration = DateTime.ParseExact(expiration, "yyyyMMdd", CultureInfo.InvariantCulture)
        };

        switch (contract.Right)
        {
          case "P": derivative.Side = OptionSideEnum.Put; break;
          case "C": derivative.Side = OptionSideEnum.Call; break;
        }

        response.Derivative = derivative;
      }

      return response;
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
