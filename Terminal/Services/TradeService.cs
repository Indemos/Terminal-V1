using Estimator.Services;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Terminal.Models;

namespace Terminal.Services
{
  public class TradeService
  {
    /// <summary>
    /// Get position delta
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static double GetDelta(OrderModel o)
    {
      var volume = o.Volume;
      var leverage = o.Transaction?.Instrument?.Leverage;
      var delta = o.Transaction?.Instrument?.Derivative?.Variance?.Delta;
      var side = o.Side is OrderSideEnum.Buy ? 1.0 : -1.0;

      return ((delta ?? volume) * leverage * side) ?? 0;
    }

    /// <summary>
    /// Estimated PnL for shares or options
    /// </summary>
    /// <param name="price"></param>
    /// <param name="inputModel"></param>
    /// <returns></returns>
    public static double GetEstimate(double price, DateTime date, OptionInputModel inputModel)
    {
      var direction = inputModel.Position is OrderSideEnum.Buy ? 1.0 : -1.0;

      if (inputModel.Side is not OptionSideEnum.Put && inputModel.Side is not OptionSideEnum.Call)
      {
        return (price - inputModel.Price) * inputModel.Amount * direction;
      }

      var optionSide = Enum.GetName(inputModel.Side.GetType(), inputModel.Side);
      var days = Math.Max((inputModel.Date - date).Value.TotalDays / 250.0, double.Epsilon);
      var estimate = OptionService.Premium(optionSide, price, inputModel.Strike, days, 0.25, 0.05, 0);

      return (estimate - inputModel.Premium) * inputModel.Amount * direction * 100;
    }

    /// <summary>
    /// Get option chain
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public static async Task<IList<InstrumentModel>> GetOptions(IGateway adapter, PointModel point, DateTime date)
    {
      var account = adapter.Account;
      var screener = new OptionScreenerModel
      {
        MinDate = date,
        MaxDate = date,
        Instrument = point.Instrument,
        Point = point
      };

      var options = await adapter.GetOptions(screener, []);
      var nextOptions = options
        .Data
        .OrderBy(o => o.Derivative.ExpirationDate)
        .ThenBy(o => o.Derivative.Strike)
        .ThenBy(o => o.Derivative.Side)
        .ToList();

      return nextOptions;
    }

    /// <summary>
    /// Close positions
    /// </summary>
    /// <param name="instrumentType"></param>
    /// <returns></returns>
    public static async Task ClosePositions(IGateway adapter, InstrumentEnum? instrumentType = null)
    {
      foreach (var position in adapter.Account.Positions.Values.ToList())
      {
        var order = new OrderModel
        {
          Volume = position.Volume,
          Side = position.Side is OrderSideEnum.Buy ? OrderSideEnum.Sell : OrderSideEnum.Buy,
          Type = OrderTypeEnum.Market,
          Transaction = new()
          {
            Instrument = position.Transaction.Instrument
          }
        };

        var insEmpty = instrumentType is null;
        var insShares = instrumentType is InstrumentEnum.Shares && position.Transaction.Instrument.Derivative is null;
        var insOptions = instrumentType is InstrumentEnum.Options && position.Transaction.Instrument.Derivative is not null;

        if (insEmpty || insShares || insOptions)
        {
          await adapter.CreateOrders(order);
        }
      }
    }

    /// <summary>
    /// Create short condor strategy
    /// </summary>
    /// <param name="point"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetCondor(IGateway adapter, PointModel point, IList<InstrumentModel> options)
    {
      var range = point.Last * 0.01;
      var shortPut = options
        ?.Where(o => o.Derivative.Side is OptionSideEnum.Put)
        ?.Where(o => o.Derivative.Strike <= point.Last)
        ?.LastOrDefault();

      var longPut = options
        ?.Where(o => o.Derivative.Side is OptionSideEnum.Put)
        ?.Where(o => o.Derivative.Strike < shortPut.Derivative.Strike - range)
        ?.LastOrDefault();

      var shortCall = options
        ?.Where(o => o.Derivative.Side is OptionSideEnum.Call)
        ?.Where(o => o.Derivative.Strike >= point.Last)
        ?.FirstOrDefault();

      var longCall = options
        ?.Where(o => o.Derivative.Side is OptionSideEnum.Call)
        ?.Where(o => o.Derivative.Strike > shortCall.Derivative.Strike + range)
        ?.FirstOrDefault();

      if (shortPut is null || shortCall is null || longPut is null || longCall is null)
      {
        return [];
      }

      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        Instruction = InstructionEnum.Group,
        Orders =
        [
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Buy,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = longPut }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Buy,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = longCall }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Sell,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = shortPut }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Sell,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = shortCall }
          }
        ]
      };

      return [order];
    }

    /// <summary>
    /// Create short straddle strategy
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetShortStraddle(IGateway adapter, PointModel point, IList<InstrumentModel> options)
    {
      var shortPut = options
        ?.Where(o => o.Derivative.Side is OptionSideEnum.Put)
        ?.Where(o => o.Derivative.Strike >= point.Last)
        ?.FirstOrDefault();

      var shortCall = options
        ?.Where(o => o.Derivative.Side is OptionSideEnum.Call)
        ?.Where(o => o.Derivative.Strike >= point.Last)
        ?.FirstOrDefault();

      if (shortPut is null || shortCall is null)
      {
        return [];
      }

      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        Instruction = InstructionEnum.Group,
        Orders =
        [
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Sell,
            Instruction = InstructionEnum.Side,
            Price = shortPut.Point.Bid,
            Transaction = new() { Instrument = shortPut }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Sell,
            Instruction = InstructionEnum.Side,
            Price = shortCall.Point.Bid,
            Transaction = new() { Instrument = shortCall }
          }
        ]
      };

      return [order];
    }

    /// <summary>
    /// Hedge each delta change with shares
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetShareHedge(IGateway adapter, PointModel point)
    {
      var account = adapter.Account;
      var basisDelta = Math.Round(account
        .Positions
        .Values
        .Where(o => o.Transaction.Instrument.Derivative is null)
        .Sum(GetDelta), MidpointRounding.ToZero);

      var optionDelta = Math.Round(account
        .Positions
        .Values
        .Where(o => o.Transaction.Instrument.Derivative is not null)
        .Sum(GetDelta), MidpointRounding.ToZero);

      var delta = optionDelta + basisDelta;

      if (Math.Abs(delta) > 0)
      {
        var order = new OrderModel
        {
          Volume = Math.Abs(delta),
          Type = OrderTypeEnum.Market,
          Side = delta < 0 ? OrderSideEnum.Buy : OrderSideEnum.Sell,
          Transaction = new() { Instrument = point.Instrument }
        };

        return [order];
      }

      return [];
    }

    /// <summary>
    /// Open share position in the direction of option delta
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetShareDirection(IGateway adapter, PointModel point)
    {
      var account = adapter.Account;
      var basisDelta = account
        .Positions
        .Values
        .Where(o => o.Transaction.Instrument.Derivative is null)
        .Sum(GetDelta);

      var optionDelta = account
        .Positions
        .Values
        .Where(o => o.Transaction.Instrument.Derivative is not null)
        .Sum(GetDelta);

      var isOversold = basisDelta < 0 && optionDelta > 0;
      var isOverbought = basisDelta > 0 && optionDelta < 0;

      if (basisDelta is 0 || isOversold || isOverbought)
      {
        var order = new OrderModel
        {
          Volume = 100,
          Type = OrderTypeEnum.Market,
          Side = optionDelta > 0 ? OrderSideEnum.Buy : OrderSideEnum.Sell,
          Transaction = new() { Instrument = point.Instrument }
        };

        return [order];
      }

      return [];
    }

    /// <summary>
    /// Create credit spread strategy
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="side"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetCreditSpread(IGateway adapter, PointModel point, OptionSideEnum side, IList<InstrumentModel> options)
    {
      var account = adapter.Account;
      var sideOptions = options.Where(o => Equals(o.Derivative.Side, side));
      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        Orders =
        [
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Sell,
            Instruction = InstructionEnum.Side
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Buy,
            Instruction = InstructionEnum.Side
          }
        ]
      };

      switch (side)
      {
        case OptionSideEnum.Put:

          var put = order.Orders[0].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike <= point.Last - point.Last * 0.001)
            .LastOrDefault();

          order.Orders[1].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike <= point.Last - point.Last * 0.005)
            .LastOrDefault();

          break;

        case OptionSideEnum.Call:

          var call = order.Orders[0].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike >= point.Last + point.Last * 0.001)
            .FirstOrDefault();

          order.Orders[1].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike >= point.Last + point.Last * 0.005)
            .FirstOrDefault();

          break;
      }

      return [order];
    }

    /// <summary>
    /// Create debit spread strategy
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="side"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetDebigSpread(IGateway adapter, PointModel point, OptionSideEnum side, IList<InstrumentModel> options)
    {
      var account = adapter.Account;
      var sideOptions = options.Where(o => Equals(o.Derivative.Side, side));
      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        Orders =
        [
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Buy,
            Instruction = InstructionEnum.Side
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Sell,
            Instruction = InstructionEnum.Side
          }
        ]
      };

      switch (side)
      {
        case OptionSideEnum.Put:

          var put = order.Orders[0].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike >= point.Last + point.Last * 0.001)
            .FirstOrDefault();

          order.Orders[1].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike <= point.Last - point.Last * 0.005)
            .LastOrDefault();

          break;

        case OptionSideEnum.Call:

          var call = order.Orders[0].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike <= point.Last - point.Last * 0.001)
            .LastOrDefault();

          order.Orders[1].Transaction.Instrument = sideOptions
            .Where(o => o.Derivative.Strike >= point.Last + point.Last * 0.005)
            .FirstOrDefault();

          break;
      }

      return [order];
    }

    /// <summary>
    /// Create PMCC strategy
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="side"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IList<OrderModel> GetPmCover(IGateway adapter, PointModel point, OptionSideEnum side, IList<InstrumentModel> options)
    {
      var account = adapter.Account;
      var sideOptions = options.Where(o => Equals(o.Derivative.Side, side));
      var minDate = options.First().Derivative.ExpirationDate;
      var maxDate = options.Last().Derivative.ExpirationDate;
      var longOptions = sideOptions.Where(o => o.Derivative.ExpirationDate >= maxDate);
      var shortOptions = sideOptions.Where(o => o.Derivative.ExpirationDate <= minDate);
      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        Orders =
        [
          new OrderModel
          {
            Volume = 2,
            Side = OrderSideEnum.Buy,
            Instruction = InstructionEnum.Side
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Sell,
            Instruction = InstructionEnum.Side
          }
        ]
      };

      switch (side)
      {
        case OptionSideEnum.Put:

          var put = order.Orders[0].Transaction.Instrument = longOptions
            .Where(o => o.Derivative.Strike > point.Last)
            .FirstOrDefault();

          order.Orders[1].Transaction.Instrument = shortOptions
            .Where(o => o.Derivative.Strike < point.Last)
            .LastOrDefault();

          break;

        case OptionSideEnum.Call:

          var call = order.Orders[0].Transaction.Instrument = longOptions
            .Where(o => o.Derivative.Strike < point.Last)
            .LastOrDefault();

          order.Orders[1].Transaction.Instrument = shortOptions
            .Where(o => o.Derivative.Strike > point.Last)
            .FirstOrDefault();

          break;
      }

      return [order];
    }

    /// <summary>
    /// Run with delay
    /// </summary>
    /// <param name="action"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static async Task Done(Action action, int interval)
    {
      await Task.Delay(interval);
      action();
    }
  }
}
