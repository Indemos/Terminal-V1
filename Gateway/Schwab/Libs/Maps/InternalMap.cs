using Schwab.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Schwab.Mappers
{
  public class InternalMap
  {
    /// <summary>
    /// Map fields in the stream
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static IDictionary<string, string> GetStreamMap(string assetType)
    {
      switch (assetType?.ToUpper())
      {
        case "LEVELONE_EQUITIES": return StreamEquityMap.Map;
        case "LEVELONE_FUTURES": return StreamFutureMap.Map;
        case "LEVELONE_FOREX": return StreamCurrencyMap.Map;
        case "LEVELONE_OPTIONS": return StreamOptionMap.Map;
        case "LEVELONE_FUTURES_OPTIONS": return StreamFutureOptionMap.Map;
      }

      return null;
    }

    /// <summary>
    /// External instrument type to local
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static InstrumentEnum? GetSubscriptionType(string assetType)
    {
      switch (assetType?.ToUpper())
      {
        case "LEVELONE_EQUITIES": return InstrumentEnum.Shares;
        case "LEVELONE_FUTURES": return InstrumentEnum.Futures;
        case "LEVELONE_FOREX": return InstrumentEnum.Currencies;
        case "LEVELONE_OPTIONS": return InstrumentEnum.Options;
        case "LEVELONE_FUTURES_OPTIONS": return InstrumentEnum.FutureOptions;
      }

      return null;
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
        case "COE":
        case "ETF":
        case "EQUITY":
        case "EXTENDED":
        case "INDICATOR":
        case "FUNDAMENTAL":
        case "MUTUAL_FUND": return InstrumentEnum.Shares;
        case "INDEX": return InstrumentEnum.Indices;
        case "BOND": return InstrumentEnum.Bonds;
        case "FOREX": return InstrumentEnum.Currencies;
        case "FUTURE": return InstrumentEnum.Futures;
        case "FUTURE_OPTION": return InstrumentEnum.FutureOptions;
        case "OPTION": return InstrumentEnum.Options;
      }

      return InstrumentEnum.Group;
    }

    /// <summary>
    /// Asset type
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static InstrumentEnum? GetOptionType(string assetType)
    {
      switch (assetType?.ToUpper())
      {
        case "COE":
        case "ETF":
        case "INDEX":
        case "EQUITY":
        case "OPTION":
        case "EXTENDED": return InstrumentEnum.Options;
        case "BOND":
        case "FUTURE":
        case "FUTURE_OPTION": return InstrumentEnum.FutureOptions;
      }

      return null;
    }

    /// <summary>
    /// Get order book
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPrice(AssetMessage message, InstrumentModel instrument)
    {
      var o = message.Quote;
      var point = new PointModel
      {
        Ask = o.AskPrice,
        Bid = o.BidPrice,
        AskSize = o.AskSize,
        BidSize = o.BidSize,
        Last = o.AskTime > o.BidTime ? o.AskPrice : o.BidPrice,
        Time = DateTimeOffset.FromUnixTimeMilliseconds(o.QuoteTime ?? DateTime.Now.Ticks).UtcDateTime,
        Instrument = instrument
      };

      return point;
    }

    /// <summary>
    /// Get internal option
    /// </summary>
    /// <param name="optionMessage"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static InstrumentModel GetOption(OptionMessage optionMessage, OptionChainMessage message)
    {
      var asset = message.Underlying;
      var price = message.UnderlyingPrice;
      var point = new PointModel
      {
        Ask = asset?.Ask ?? price,
        Bid = asset?.Bid ?? price,
        AskSize = asset?.AskSize ?? 0,
        BidSize = asset?.BidSize ?? 0,
        Last = asset?.Last ?? price
      };

      var instrument = new InstrumentModel
      {
        Type = GetInstrumentType(message.AssetType),
        Exchange = asset?.ExchangeName,
        Name = message.Symbol,
        Point = point
      };

      if (asset is not null)
      {
        point.Bar = new BarModel
        {
          Low = asset.LowPrice,
          High = asset.HighPrice,
          Open = asset.OpenPrice,
          Close = asset.Close
        };
      }

      var optionBar = new BarModel
      {
        Low = optionMessage.LowPrice,
        High = optionMessage.HighPrice,
        Open = optionMessage.OpenPrice,
        Close = optionMessage.ClosePrice
      };

      var optionPoint = new PointModel
      {
        Ask = optionMessage.Ask,
        Bid = optionMessage.Bid,
        AskSize = optionMessage.AskSize ?? 0,
        BidSize = optionMessage.BidSize ?? 0,
        Volume = optionMessage.TotalVolume ?? 0,
        Last = optionMessage.Last,
        Bar = optionBar
      };

      var optionInstrument = new InstrumentModel
      {
        Basis = instrument,
        Point = optionPoint,
        Name = optionMessage.Symbol,
        Leverage = optionMessage.Multiplier ?? 100,
        Type = GetOptionType(message.AssetType)
      };

      var variance = new VarianceModel
      {
        Rho = optionMessage.Rho ?? 0,
        Vega = optionMessage.Vega ?? 0,
        Delta = optionMessage.Delta ?? 0,
        Gamma = optionMessage.Gamma ?? 0,
        Theta = optionMessage.Theta ?? 0
      };

      var derivative = new DerivativeModel
      {
        Strike = optionMessage.StrikePrice,
        ExpirationDate = optionMessage.ExpirationDate,
        ExpirationType = GetEnum<ExpirationTypeEnum>(optionMessage.ExpirationType),
        OpenInterest = optionMessage.OpenInterest ?? 0,
        IntrinsicValue = optionMessage.IntrinsicValue ?? 0,
        Sigma = optionMessage.Volatility ?? 0,
        Variance = variance
      };

      if (optionMessage.LastTradingDay is not null)
      {
        derivative.TradeDate = DateTimeOffset
          .FromUnixTimeMilliseconds((long)optionMessage.LastTradingDay)
          .LocalDateTime;
      }

      optionInstrument.Point = optionPoint;
      optionInstrument.Derivative = derivative;

      switch (optionMessage?.PutCall?.ToUpper())
      {
        case "PUT": derivative.Side = OptionSideEnum.Put; break;
        case "CALL": derivative.Side = OptionSideEnum.Call; break;
      }

      return optionInstrument;
    }

    /// <summary>
    /// Get internal point from bar
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPrice(BarMessage message)
    {
      var point = new PointModel
      {
        Ask = message.Close,
        Bid = message.Close,
        AskSize = message.Volume,
        BidSize = message.Volume,
        Time = DateTimeOffset.FromUnixTimeMilliseconds(message.Datetime ?? DateTime.Now.Ticks).UtcDateTime,
        Last = message.Close
      };

      return point;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(PositionMessage message)
    {
      var price = message.AveragePrice + message.Instrument.NetChange;
      var volume = message.LongQuantity + message.ShortQuantity;
      var point = new PointModel
      {
        Bid = price,
        Ask = price,
        Last = price
      };

      var instrumentType = GetInstrumentType(message.Instrument.AssetType);
      var instrument = new InstrumentModel
      {
        Point = point,
        Name = message.Instrument.Symbol,
        Type = instrumentType
      };

      var action = new TransactionModel
      {
        Instrument = instrument,
        Price = message.AveragePrice,
        Descriptor = message.Instrument.Symbol,
        Volume = volume
      };

      var order = new OrderModel
      {
        Volume = volume,
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetPositionSide(message),
        Price = message.AveragePrice
      };

      return order;
    }

    /// <summary>
    /// Convert remote position side to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetPositionSide(PositionMessage message)
    {
      switch (true)
      {
        case true when message.LongQuantity > 0: return OrderSideEnum.Long;
        case true when message.ShortQuantity > 0: return OrderSideEnum.Short;
      }

      return null;
    }

    /// <summary>
    /// Convert remote order to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetOrder(OrderMessage message)
    {
      var subOrders = message?.OrderLegCollection ?? [];
      var basis = new InstrumentModel
      {
        Name = string.Join(" / ", subOrders.Select(o => o?.Instrument?.Symbol).Distinct())
      };

      var instrument = new InstrumentModel
      {
        Basis = basis,
        Name = string.Join(" / ", subOrders.Select(o => o?.Instrument?.Symbol))
      };

      var action = new TransactionModel
      {
        Id = message.OrderId,
        Instrument = instrument,
        Volume = GetValue(message.FilledQuantity, message.Quantity),
        Time = message.EnteredTime,
        Status = GetStatus(message)
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetOrderSide(message),
        TimeSpan = GetTimeSpan(message),
        Volume = GetValue(message.Quantity, message.FilledQuantity)
      };

      switch (message.OrderType.ToUpper())
      {
        case "STOP":
          order.Type = OrderTypeEnum.Stop;
          order.Price = message.Price;
          break;

        case "LIMIT":
          order.Type = OrderTypeEnum.Limit;
          order.Price = message.Price;
          break;

        case "STOP_LIMIT":
          order.Type = OrderTypeEnum.StopLimit;
          order.Price = message.Price;
          order.ActivationPrice = message.StopPrice;
          break;

        case "NET_DEBIT":
        case "NET_CREDIT":
          order.Type = OrderTypeEnum.Limit;
          order.Price = message.Price;
          break;
      }

      if (subOrders.Count is not 0)
      {
        order.Instruction = InstructionEnum.Group;

        foreach (var subOrder in subOrders)
        {
          var subInstrument = new InstrumentModel
          {
            Name = subOrder.Instrument.Symbol,
            Type = GetInstrumentType(subOrder.Instrument.AssetType)
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
            Side = GetSubOrderSide(subOrder.Instruction)
          });
        }
      }

      return order;
    }

    /// <summary>
    /// Convert remote order status to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderStatusEnum? GetStatus(OrderMessage message)
    {
      switch (message.Status.ToUpper())
      {
        case "FILLED":
        case "REPLACED": return OrderStatusEnum.Filled;
        case "REJECTED":
        case "CANCELED":
        case "EXPIRED": return OrderStatusEnum.Canceled;
        case "NEW":
        case "WORKING":
        case "PENDING_RECALL":
        case "PENDING_CANCEL":
        case "PENDING_REPLACE":
        case "PENDING_ACTIVATION":
        case "PENDING_ACKNOWLEDGEMENT":
        case "AWAITING_CONDITION":
        case "AWAITING_PARENT_ORDER":
        case "AWAITING_RELEASE_TIME":
        case "AWAITING_MANUAL_REVIEW":
        case "AWAITING_STOP_CONDITION": return OrderStatusEnum.Pending;
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
      static double getValue(OrderLegMessage o)
      {
        var units = 1.0;
        var volume = o.Quantity ?? 1.0;

        if (GetInstrumentType(o.OrderLegType) is InstrumentEnum.Options or InstrumentEnum.FutureOptions)
        {
          var derivative = GetDerivative(o.Instrument.Symbol);
          var strike = 1.0 / (derivative.Strike ?? 1.0);
          var expiration = derivative.ExpirationDate?.Ticks ?? 1.0;
          units = 100.0 * expiration * strike;
        }

        return volume * units;
      }

      if (message?.OrderLegCollection?.Count > 0)
      {
        var ups = message?.OrderLegCollection?.Where(o => GetSubOrderSide(o.Instruction) is OrderSideEnum.Long).Sum(getValue);
        var downs = message?.OrderLegCollection?.Where(o => GetSubOrderSide(o.Instruction) is OrderSideEnum.Short).Sum(getValue);

        switch (true)
        {
          case true when ups > downs: return OrderSideEnum.Long;
          case true when ups < downs: return OrderSideEnum.Short;
        }

        return OrderSideEnum.Group;
      }

      return GetSubOrderSide(message?.OrderType);
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
    /// Convert remote time in force to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderTimeSpanEnum? GetTimeSpan(OrderMessage message)
    {
      var span = message.Duration;
      var session = message.Session;

      switch (true)
      {
        case true when Same(span, "DAY"): return OrderTimeSpanEnum.Day;
        case true when Same(span, "FILL_OR_KILL"): return OrderTimeSpanEnum.Fok;
        case true when Same(span, "GOOD_TILL_CANCEL"): return OrderTimeSpanEnum.Gtc;
        case true when Same(span, "IMMEDIATE_OR_CANCEL"): return OrderTimeSpanEnum.Ioc;
        case true when Same(session, "AM"): return OrderTimeSpanEnum.Am;
        case true when Same(session, "PM"): return OrderTimeSpanEnum.Pm;
      }

      return null;
    }

    /// <summary>
    /// Get field name by code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static T? GetEnum<T>(int code) where T : struct, Enum => Enum.IsDefined(typeof(T), code) ? (T)(object)code : null;

    /// <summary>
    /// Get field name by code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static T? GetEnum<T>(string code) where T : struct, Enum => Enum.TryParse(code, true, out T o) ? o : null;

    /// <summary>
    /// Case insensitive comparison
    /// </summary>
    /// <param name="x"></param>
    /// <param name="o"></param>
    /// <returns></returns>
    public static bool Same(string x, string o) => string.Equals(x, o, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Get value or default
    /// </summary>
    /// <param name="v"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public static double? GetValue(double? v, double? origin) => v is 0 or null ? origin : v;

    /// <summary>
    /// Get value or default
    /// </summary>
    /// <param name="v"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public static double? GetValue(string v, double? origin) => double.TryParse(v, out var o) ? o : origin;
  }
}
