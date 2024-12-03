using IBApi;
using InteractiveBrokers.Enums;
using InteractiveBrokers.Messages;
using System;
using System.Collections.Generic;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;

namespace InteractiveBrokers.Mappers
{
  public class ExternalMap
  {
    /// <summary>
    /// Convert remote order from brokerage to local record
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="orderModel"></param>
    /// <param name="contracts"></param>
    /// <returns></returns>
    public static OpenOrderMessage GetOrder(int orderId, OrderModel orderModel, IDictionary<string, Contract> contracts)
    {
      var order = GetSubOrder(orderId, orderModel);
      var action = orderModel.Transaction;
      var instrument = action.Instrument;
      var contract = contracts.Get(instrument.Name) ?? GetContract(action.Instrument);

      return new OpenOrderMessage
      {
        Order = order,
        Contract = contract
      };
    }

    /// <summary>
    /// Instrument to contract
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public static Contract GetContract(InstrumentModel instrument)
    {
      if (int.TryParse(instrument.Id, out var id))
      {
        return new Contract { ConId = id };
      }

      var basis = instrument.Basis;
      var derivative = instrument.Derivative;
      var contract = new Contract
      {
        Symbol = basis?.Name,
        LocalSymbol = instrument.Name,
        Multiplier = $"{instrument.Leverage}",
        Exchange = instrument.Exchange ?? "SMART",
        SecType = GetInstrumentType(instrument.Type),
        Currency = instrument.Currency?.Name ?? nameof(CurrencyEnum.USD)
      };

      if (derivative is not null)
      {
        contract.Strike = derivative.Strike ?? 0;
        contract.LastTradeDateOrContractMonth = $"{derivative.Expiration:yyyyMMdd}";

        switch (derivative.Side)
        {
          case OptionSideEnum.Put: contract.Right = "P"; break;
          case OptionSideEnum.Call: contract.Right = "C"; break;
        }
      }

      return contract;
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
        case OrderTimeSpanEnum.Gtc: return "GTC";
      }

      return null;
    }

    /// <summary>
    /// Order side
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public static string GetSide(OrderSideEnum? side)
    {
      switch (side)
      {
        case OrderSideEnum.Buy: return "BUY";
        case OrderSideEnum.Sell: return "SELL";
      }

      return null;
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
        case OrderTypeEnum.Stop: return "STP";
        case OrderTypeEnum.Limit: return "LMT";
        case OrderTypeEnum.StopLimit: return "STP LMT";
      }

      return "MKT";
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
        case InstrumentEnum.Bonds: return "BOND";
        case InstrumentEnum.Shares: return "STK";
        case InstrumentEnum.Options: return "OPT";
        case InstrumentEnum.Futures: return "FUT";
        case InstrumentEnum.Contracts: return "CFD";
        case InstrumentEnum.Currencies: return "CASH";
        case InstrumentEnum.FuturesOptions: return "FOP";
      }

      return null;
    }

    /// <summary>
    /// Get external instrument type
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string GetExpiration(DateTime? date)
    {
      if (date is not null)
      {
        return $"{date?.Year:0000}{date?.Month:00}";
      }

      return null;
    }

    /// <summary>
    /// Get field name by code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static FieldCodeEnum? GetField(int code)
    {
      if (Enum.IsDefined(typeof(FieldCodeEnum), code))
      {
        return (FieldCodeEnum)code;
      }

      return null;
    }

    /// <summary>
    /// Basic order 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="orderModel"></param>
    /// <returns></returns>
    public static Order GetSubOrder(int id, OrderModel orderModel)
    {
      var order = new Order();

      order.OrderId = id;
      order.Action = GetSide(orderModel.Side);
      order.OrderType = GetOrderType(orderModel.Type);
      order.TotalQuantity = (decimal)orderModel.Transaction.Volume;

      switch (orderModel.Type)
      {
        case OrderTypeEnum.Stop: order.AuxPrice = orderModel.Price.Value; break;
        case OrderTypeEnum.Limit: order.LmtPrice = orderModel.Price.Value; break;
        case OrderTypeEnum.StopLimit:
          order.LmtPrice = orderModel.Price.Value;
          order.AuxPrice = orderModel.ActivationPrice.Value;
          break;
      }

      return order;
    }

    /// <summary>
    /// Bracket template
    /// </summary>
    /// <param name="order"></param>
    /// <param name="TP"></param>
    /// <param name="SL"></param>
    /// <returns></returns>
    public static List<Order> GetBraces(Order order, double? stopPrice, double? takePrice)
    {
      var orders = new List<Order> { order };

      order.Transmit = false;

      if (takePrice is not null)
      {
        var TP = new Order
        {
          OrderType = "LMT",
          OrderId = order.OrderId + 1,
          Action = order.Action.Equals("BUY") ? "SELL" : "BUY",
          TotalQuantity = order.TotalQuantity,
          LmtPrice = takePrice.Value,
          ParentId = order.OrderId,
          Transmit = false
        };

        orders.Add(TP);
      }

      if (stopPrice is not null)
      {
        var SL = new Order
        {
          OrderType = "STP",
          OrderId = order.OrderId + 2,
          Action = order.Action.Equals("BUY") ? "SELL" : "BUY",
          TotalQuantity = order.TotalQuantity,
          AuxPrice = stopPrice.Value,
          ParentId = order.OrderId,
          Transmit = true
        };

        orders.Add(SL);
      }

      return orders;
    }
  }
}
