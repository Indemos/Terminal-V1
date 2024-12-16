using Alpaca.Markets;
using System;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Alpaca.Mappers
{
  public class InternalMap
  {
    /// <summary>
    /// Get order book
    /// </summary>
    /// <param name="message"></param>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public static PointModel GetPrice(IQuote message, InstrumentModel instrument)
    {
      var point = new PointModel
      {
        Ask = (double)message.AskPrice,
        Bid = (double)message.BidPrice,
        AskSize = (double)message.AskSize,
        BidSize = (double)message.BidSize,
        Last = (double)message.BidPrice,
        Time = message.TimestampUtc
      };

      point.Instrument = instrument;

      return point;
    }

    /// <summary>
    /// Get option contract
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="message"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static InstrumentModel GetOption(OptionChainRequest screener, IOptionSnapshot message, string name)
    {
      var strikePrice = name.Substring(name.Length - 8);
      var optionType = name.Substring(name.Length - 9, 1);
      var expirationDate = name.Substring(name.Length - 15, 6);
      var o = message.Quote;
      var point = new PointModel
      {
        Ask = (double)o.AskPrice,
        Bid = (double)o.BidPrice,
        Last = (double)o.BidPrice,
        AskSize = (double)o.AskSize,
        BidSize = (double)o.BidSize
      };

      var derivative = new DerivativeModel
      {
        Strike = int.Parse(strikePrice) / 1000,
        Expiration = DateTime.ParseExact(expirationDate, "yyMMdd", null)
      };

      var basis = new InstrumentModel
      {
        Name = screener.UnderlyingSymbol
      };

      var option = new InstrumentModel
      {
        Point = point,
        Derivative = derivative,
        Name = name
      };

      return option;
    }

    /// <summary>
    /// Convert remote order to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetOrder(IOrder message)
    {
      var instrument = new InstrumentModel
      {
        Name = message.Symbol,
        Type = GetInstrumentType(message.AssetClass)
      };

      var action = new TransactionModel
      {
        Id = message.ClientOrderId,
        Instrument = instrument,
        CurrentVolume = GetValue(message.FilledQuantity, message.Quantity),
        Volume = GetValue(message.Quantity, message.FilledQuantity),
        Time = message.CreatedAtUtc,
        Status = GetStatus(message.OrderStatus)
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetOrderSide(message.OrderSide),
        TimeSpan = GetTimeSpan(message.TimeInForce),
        Id = $"{message.OrderId}"
      };

      switch (message.OrderType)
      {
        case OrderType.Stop:
          order.Type = OrderTypeEnum.Stop;
          order.Price = (double)message.StopPrice;
          break;

        case OrderType.Limit:
          order.Type = OrderTypeEnum.Limit;
          order.Price = (double)message.LimitPrice;
          break;

        case OrderType.StopLimit:
          order.Type = OrderTypeEnum.StopLimit;
          order.Price = (double)message.StopPrice;
          order.ActivationPrice = (double)message.LimitPrice;
          break;
      }

      return order;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(IPosition message)
    {
      var price = (double)message.AssetCurrentPrice;
      var point = new PointModel
      {
        Bid = price,
        Ask = price,
        Last = price
      };

      var instrument = new InstrumentModel
      {
        Point = point,
        Name = message.Symbol,
        Type = GetInstrumentType(message.AssetClass)
      };

      var action = new TransactionModel
      {
        Price = price,
        Instrument = instrument,
        Descriptor = $"{message.AssetId}",
        CurrentVolume = GetValue(message.Quantity, message.AvailableQuantity),
        Volume = GetValue(message.Quantity, message.AvailableQuantity)
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetPositionSide(message.Side),
        Price = (double)message.AverageEntryPrice,
      };

      return order;
    }

    /// <summary>
    /// Convert remote order status to local
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static OrderStatusEnum? GetStatus(OrderStatus status)
    {
      switch (status)
      {
        case OrderStatus.Fill:
        case OrderStatus.Filled: return OrderStatusEnum.Filled;
        case OrderStatus.PartialFill:
        case OrderStatus.PartiallyFilled: return OrderStatusEnum.Partitioned;
        case OrderStatus.Stopped:
        case OrderStatus.Expired:
        case OrderStatus.Rejected:
        case OrderStatus.Canceled:
        case OrderStatus.DoneForDay: return OrderStatusEnum.Canceled;
        case OrderStatus.New:
        case OrderStatus.Held:
        case OrderStatus.Accepted:
        case OrderStatus.Suspended:
        case OrderStatus.PendingNew:
        case OrderStatus.PendingCancel:
        case OrderStatus.PendingReplace: return OrderStatusEnum.Pending;
      }

      return null;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetOrderSide(OrderSide side)
    {
      switch (side)
      {
        case OrderSide.Buy: return OrderSideEnum.Buy;
        case OrderSide.Sell: return OrderSideEnum.Sell;
      }

      return null;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetTakerSide(TakerSide side)
    {
      switch (side)
      {
        case TakerSide.Buy: return OrderSideEnum.Buy;
        case TakerSide.Sell: return OrderSideEnum.Sell;
      }

      return null;
    }

    /// <summary>
    /// Convert remote position side to local
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static OrderSideEnum? GetPositionSide(PositionSide side)
    {
      switch (side)
      {
        case PositionSide.Long: return OrderSideEnum.Buy;
        case PositionSide.Short: return OrderSideEnum.Sell;
      }

      return null;
    }

    /// <summary>
    /// Convert remote time in force to local
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static OrderTimeSpanEnum? GetTimeSpan(TimeInForce span)
    {
      switch (span)
      {
        case TimeInForce.Day: return OrderTimeSpanEnum.Day;
        case TimeInForce.Fok: return OrderTimeSpanEnum.Fok;
        case TimeInForce.Gtc: return OrderTimeSpanEnum.Gtc;
        case TimeInForce.Ioc: return OrderTimeSpanEnum.Ioc;
        case TimeInForce.Opg: return OrderTimeSpanEnum.Am;
        case TimeInForce.Cls: return OrderTimeSpanEnum.Pm;
      }

      return null;
    }

    /// <summary>
    /// Asset type
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static InstrumentEnum? GetInstrumentType(AssetClass assetType)
    {
      switch (assetType)
      {
        case AssetClass.Crypto: return InstrumentEnum.Coins;
        case AssetClass.UsEquity: return InstrumentEnum.Shares;
        case AssetClass.UsOption: return InstrumentEnum.Options;
      }

      return null;
    }

    /// <summary>
    /// Get value or default
    /// </summary>
    /// <param name="price"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public static double GetValue(decimal? price, decimal? origin) => (double)(price is 0 or null ? origin : price);
  }
}
