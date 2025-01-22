using System;
using System.Linq;
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
    public static PointModel GetPrice(QuoteMessage message)
    {
      var point = new PointModel
      {
        Ask = message.Ask,
        Bid = message.Bid,
        Last = message.Last,
        AskSize = message.Asksize,
        BidSize = message.Bidsize,
        Volume = message.Volume,
        Time = DateTimeOffset.FromUnixTimeSeconds(message?.TradeDate?.Ticks ?? 0).UtcDateTime
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
    public static OrderModel GetOrder(OrderMessage message)
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
    public static OrderModel GetPosition(PositionMessage message)
    {
      var volume = Math.Abs(message.Quantity ?? 0);
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

      switch (message.Type.ToUpper())
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
      var strike = name.Substring(name.Length - 8);
      var side = name.Substring(name.Length - 9, 1);
      var expiration = name.Substring(name.Length - 15, 6);
      var expirationDate = DateTime.ParseExact(expiration, "yyMMdd", null);
      var derivative = new DerivativeModel
      {
        Strike = double.Parse(strike) / 1000.0,
        ExpirationDate = expirationDate,
        TradeDate = expirationDate
      };

      switch (side?.ToUpper())
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
      static double? getValue(LegMessage o)
      {
        var volume = o.ExecQuantity ?? o.Quantity;
        var units = 1.0;

        if (o.OptionSymbol is not null)
        {
          var derivative = GetDerivative(o.OptionSymbol);
          var strike = 1.0 / (derivative.Strike ?? 1.0);
          var expiration = derivative.ExpirationDate?.Ticks ?? 1.0;
          units = 100.0 * expiration * strike;
        }

        return volume * units;
      }

      if (message.NumLegs > 0)
      {
        var ups = message?.Leg?.Where(o => GetSubOrderSide(o.Side) is OrderSideEnum.Long).Sum(getValue);
        var downs = message?.Leg?.Where(o => GetSubOrderSide(o.Side) is OrderSideEnum.Short).Sum(getValue);

        switch (true)
        {
          case true when ups > downs: return OrderSideEnum.Long;
          case true when ups < downs: return OrderSideEnum.Short;
        }

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
