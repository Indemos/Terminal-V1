using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Tradier.Messages.Account;
using Tradier.Messages.MarketData;

namespace Tradier.Mappers
{
  public class InternalMap
  {
    /// <summary>
    /// Get point
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPrice(Messages.Stream.QuoteMessage message)
    {
      var point = new PointModel
      {
        Ask = message.Ask,
        Bid = message.Bid,
        Last = message.Bid,
        AskSize = message.AskSize,
        BidSize = message.BidSize,
        Time = DateTimeOffset.FromUnixTimeMilliseconds(message?.BidDate ?? 0).UtcDateTime
      };

      return point;
    }

    /// <summary>
    /// Get point
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPrice(QuoteMessage message)
    {
      var point = new PointModel
      {
        Ask = message.Ask,
        Bid = message.Bid,
        Last = message.Last,
        AskSize = message.AskSize,
        BidSize = message.BidSize,
        Volume = message.Volume,
        Time = DateTimeOffset.FromUnixTimeMilliseconds(message?.TradeDate ?? 0).UtcDateTime
      };

      point.Bar ??= new BarModel();
      point.Bar.Low = message.Low;
      point.Bar.High = message.High;
      point.Bar.Open = message.Open;
      point.Bar.Close = message.Close;

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
        Id = $"{message.Id}",
        Volume = message.ExecQuantity,
        Time = message.TransactionDate,
        Status = GetStatus(message.Status)
      };

      var order = new OrderModel
      {
        Id = $"{message.Id}",
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Volume = message.ExecQuantity
      };

      switch (message?.Type?.ToUpper())
      {
        case "STOP":
          order.Type = OrderTypeEnum.Stop;
          order.Price = message.Price;
          break;

        case "LIMIT":
          order.Type = OrderTypeEnum.Limit;
          order.Price = message.Price;
          break;
      }

      return order;
    }

    /// <summary>
    /// Get order
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetSubOrder(OrderMessage message)
    {
      var basis = new InstrumentModel
      {
        Name = message.Symbol,
        Type = GetInstrumentType(message.Class)
      };

      var instrument = new InstrumentModel
      {
        Basis = basis,
        Type = GetInstrumentType(message.Class),
        Name = message.OptionSymbol ?? message.Symbol
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

      switch (message?.Type?.ToUpper())
      {
        case "DEBIT":
        case "CREDIT":
        case "LIMIT": order.Type = OrderTypeEnum.Limit; order.Price = message.Price; break;
        case "STOP": order.Type = OrderTypeEnum.Stop; order.Price = message.StopPrice; break;
        case "STOP_LIMIT": order.Type = OrderTypeEnum.StopLimit; order.ActivationPrice = message.StopPrice; order.Price = message.Price; break;
      }

      return order;
    }

    /// <summary>
    /// Get order
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetOrders(OrderMessage message)
    {
      var orders = (message.Orders ?? []).Select(GetSubOrder).ToList();

      if (message.Quantity is null || message.Symbol is null)
      {
        return orders;
      }

      var order = GetSubOrder(message);
      var name = string.Join(" / ", orders.Select(o => o.Name).Distinct());

      order.Transaction.Instrument.Name = string.IsNullOrEmpty(name) ? order.Transaction.Instrument.Name : name;
      order.Orders = orders;

      return [order];
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(PositionMessage message)
    {
      var volume = Math.Abs(message.Quantity ?? 0);
      var instrument = new InstrumentModel
      {
        Name = message.Symbol,
        Derivative = GetDerivative(message.Symbol)
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
        Side = message.Quantity > 0 ? OrderSideEnum.Long : OrderSideEnum.Short
      };

      return order;
    }

    /// <summary>
    /// Get internal option
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static InstrumentModel GetOption(OptionMessage message)
    {
      var instrument = new InstrumentModel
      {
        Name = message.Underlying,
        Exchange = message.Exchange
      };

      var optionPoint = new PointModel
      {
        Ask = message.Ask,
        Bid = message.Bid,
        AskSize = message.AskSize ?? 0,
        BidSize = message.BidSize ?? 0,
        Volume = message.Volume,
        Last = message.Last
      };

      if (message.Open is not null)
      {
        optionPoint.Bar = new BarModel
        {
          Low = message.Low,
          High = message.High,
          Open = message.Open,
          Close = message.Close
        };
      }

      var optionInstrument = new InstrumentModel
      {
        Basis = instrument,
        Point = optionPoint,
        Name = message.Symbol,
        Exchange = message.Exchange,
        Leverage = message.ContractSize ?? 100,
        Type = GetInstrumentType(message.Type)
      };

      var derivative = new DerivativeModel
      {
        Strike = message.Strike,
        TradeDate = message.ExpirationDate,
        ExpirationDate = message.ExpirationDate,
        OpenInterest = message.OpenInterest ?? 0,
        Sigma = message?.Greeks?.SmvIV ?? 0,
      };

      var greeks = message?.Greeks;

      if (greeks is not null)
      {
        derivative.Variance = new VarianceModel
        {
          Rho = greeks.Rho ?? 0,
          Vega = greeks.Vega ?? 0,
          Delta = greeks.Delta ?? 0,
          Gamma = greeks.Gamma ?? 0,
          Theta = greeks.Theta ?? 0
        };
      }

      optionInstrument.Point = optionPoint;
      optionInstrument.Derivative = derivative;

      switch (message.OptionType.ToUpper())
      {
        case "PUT": derivative.Side = OptionSideEnum.Put; break;
        case "CALL": derivative.Side = OptionSideEnum.Call; break;
      }

      return optionInstrument;
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
        case "DEBIT":
        case "BUY_TO_OPEN":
        case "BUY_TO_CLOSE":
        case "BUY_TO_COVER":
        case "NET_DEBIT": return OrderSideEnum.Long;

        case "SELL":
        case "CREDIT":
        case "SELL_SHORT":
        case "SELL_TO_OPEN":
        case "SELL_TO_CLOSE":
        case "NET_CREDIT": return OrderSideEnum.Short;
      }

      return null;
    }

    /// <summary>
    /// Get derivative model based on option name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static DerivativeModel GetDerivative(string name)
    {
      var data = Regex.Match(name, @"^(\w{1,5})(\d{6})([CP])(\d{8})$");

      if (data.Success is false)
      {
        return null;
      }

      var strike = double.Parse(data.Groups[4].Value) / 1000.0;
      var expiration = DateTime.ParseExact(data.Groups[2].Value, "yyMMdd", null);
      var derivative = new DerivativeModel
      {
        Strike = strike,
        ExpirationDate = expiration,
        TradeDate = expiration
      };

      switch (data.Groups[3].Value.ToUpper())
      {
        case "P": derivative.Side = OptionSideEnum.Put; break;
        case "C": derivative.Side = OptionSideEnum.Call; break;
      }

      return derivative;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetOrderSide(OrderMessage message)
    {
      static double? getValue(OrderMessage o)
      {
        var units = 1.0;
        var volume = Math.Max(o.ExecQuantity ?? 0, o.Quantity ?? 0);

        if (o.OptionSymbol is not null)
        {
          var derivative = GetDerivative(o.OptionSymbol);
          var strike = derivative.Strike ?? 1.0;
          var expiration = (derivative.ExpirationDate?.Ticks ?? 1.0) / 1000000.0;
          units = expiration * strike;
        }

        return volume * units;
      }

      var side = GetSubOrderSide(message?.Side);

      if (side is not null)
      {
        return side;
      }

      var ups = message?.Orders?.Where(o => GetSubOrderSide(o.Side) is OrderSideEnum.Long).Sum(getValue);
      var downs = message?.Orders?.Where(o => GetSubOrderSide(o.Side) is OrderSideEnum.Short).Sum(getValue);

      switch (true)
      {
        case true when ups > downs: return OrderSideEnum.Long;
        case true when ups < downs: return OrderSideEnum.Short;
      }

      return OrderSideEnum.Group;
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
