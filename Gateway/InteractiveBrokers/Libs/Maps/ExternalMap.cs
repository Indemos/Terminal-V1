using IBApi;
using InteractiveBrokers.Enums;
using InteractiveBrokers.Messages;
using System;
using System.Collections.Generic;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace InteractiveBrokers.Mappers
{
  public class ExternalMap
  {
    /// <summary>
    /// Convert remote order from brokerage to local record
    /// </summary>
    /// <param name="orderModel"></param>
    /// <returns></returns>
    public static OpenOrderMessage GetOrder(OrderModel orderModel)
    {
      var action = orderModel.Transaction;
      var order = new Order();
      var contract = new Contract
      {
        Symbol = action.Instrument.Name,
        Exchange = action.Instrument.Exchange ?? "SMART",
        SecType = GetInstrumentType(action.Instrument.Type) ?? "STK",
        Currency = action.Instrument.Currency?.Name ?? nameof(CurrencyEnum.USD)
      };

      if (string.Equals(contract.SecType, "OPT"))
      {
        var option = action.Instrument;
        var derivative = option.Derivative;

        contract.Strike = derivative.Strike.Value;
        contract.LocalSymbol = option.Basis.Name;
        contract.Multiplier = $"{option.Leverage}";
        contract.LastTradeDateOrContractMonth = $"{derivative.Expiration:yyyyMMdd}";

        switch (derivative.Side)
        {
          case OptionSideEnum.Put: contract.Right = "P"; break;
          case OptionSideEnum.Call: contract.Right = "C"; break;
        }
      }

      //{
      //  Quantity = action.Volume,
      //  Symbol = action.Instrument.Name,
      //  TimeInForce = GetTimeSpan(order.TimeSpan.Value),
      //  OrderType = "market"
      //};

      switch (orderModel.Side)
      {
        case OrderSideEnum.Buy: order.Action = "BUY"; break;
        case OrderSideEnum.Sell: order.Action = "SELL"; break;
      }

      var message = new OpenOrderMessage
      {
        Order = order,
        Contract = contract
      };

      //switch (orderModel.Type)
      //{
      //  case OrderTypeEnum.Stop: message.StopPrice = orderModel.Price; break;
      //  case OrderTypeEnum.Limit: message.LimitPrice = orderModel.Price; break;
      //  case OrderTypeEnum.StopLimit: message.StopPrice = orderModel.ActivationPrice; message.LimitPrice = orderModel.Price; break;
      //}

      //if (orderModel.Orders.Any())
      //{
      //  message.OrderClass = "bracket";

      //  switch (orderModel.Side)
      //  {
      //    case OrderSideEnum.Buy:
      //      message.StopLoss = GetBracket(orderModel, 1);
      //      message.TakeProfit = GetBracket(orderModel, -1);
      //      break;

      //    case OrderSideEnum.Sell:
      //      message.StopLoss = GetBracket(orderModel, -1);
      //      message.TakeProfit = GetBracket(orderModel, 1);
      //      break;
      //  }
      //}

      return null;
    }

    /// <summary>
    /// Convert local time in force to remote
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static string GetTimeSpan(OrderTimeSpanEnum span)
    {
      switch (span)
      {
        case OrderTimeSpanEnum.Day: return "day";
        case OrderTimeSpanEnum.Fok: return "fok";
        case OrderTimeSpanEnum.Gtc: return "gtc";
        case OrderTimeSpanEnum.Ioc: return "ioc";
        case OrderTimeSpanEnum.Am: return "opg";
        case OrderTimeSpanEnum.Pm: return "cls";
      }

      return null;
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
        case InstrumentEnum.Shares: return "OPT";
        case InstrumentEnum.Options: return "STK";
        case InstrumentEnum.Futures: return "FUT";
        case InstrumentEnum.Contracts: return "CFD";
        case InstrumentEnum.Currencies: return "CASH";
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
  }
}
