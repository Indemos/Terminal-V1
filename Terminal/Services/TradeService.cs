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
    /// <param name="order"></param>
    /// <returns></returns>
    public static double GetDelta(OrderModel order)
    {
      var volume = order.Volume;
      var units = order.Transaction?.Instrument?.Leverage;
      var delta = order.Transaction?.Instrument?.Derivative?.Variance?.Delta;
      var side = order.Side is OrderSideEnum.Long ? 1.0 : -1.0;

      return ((delta ?? volume) * units * side) ?? 0;
    }

    /// <summary>
    /// Estimated PnL for shares or options
    /// </summary>
    /// <param name="price"></param>
    /// <param name="inputModel"></param>
    /// <returns></returns>
    public static double GetEstimate(double price, DateTime date, OptionInputModel inputModel)
    {
      var direction = inputModel.Position is OrderSideEnum.Long ? 1.0 : -1.0;

      if (inputModel.Side is not (OptionSideEnum.Put or OptionSideEnum.Call))
      {
        return (price - inputModel.Price) * inputModel.Amount * direction;
      }

      var optionSide = Enum.GetName(inputModel.Side.GetType(), inputModel.Side);
      var days = Math.Max((inputModel.Date - date).Value.TotalDays / 250.0, double.Epsilon);
      var estimate = OptionService.Price(optionSide, price, inputModel.Strike, days, 0.25, 0.05, 0);

      return (estimate - inputModel.Premium) * inputModel.Amount * direction * 100.0;
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
      var screener = new InstrumentScreenerModel
      {
        MinDate = date,
        MaxDate = date,
        Instrument = point.Instrument,
        Point = point
      };

      return (await adapter.GetOptions(screener, [])).Data;
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
          Side = position.Side is OrderSideEnum.Long ? OrderSideEnum.Short : OrderSideEnum.Long,
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
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Price = shortPut.Point.Bid,
            Transaction = new() { Instrument = shortPut }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Price = shortCall.Point.Bid,
            Transaction = new() { Instrument = shortCall }
          }
        ]
      };

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
