using Canvas.Core.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Indicators;
using Terminal.Core.Models;
using Terminal.Services;

namespace Terminal.Pages.Options
{
  public partial class CreditSpreadReversal
  {
    public virtual OptionPageComponent OptionView { get; set; }
    public virtual RsiIndicator Rsi { get; set; }
    public virtual double Price { get; set; }

    /// <summary>
    /// Setup views and adapters
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        OptionView.Instrument = new InstrumentModel
        {
          Name = "SPY",
          TimeFrame = TimeSpan.FromMinutes(5)
        };

        Rsi = new RsiIndicator
        {
          Interval = 5,
          Name = nameof(Rsi)
        };

        await OptionView.OnLoad(OnData, ["Indicators"]);
      }

      await base.OnAfterRenderAsync(setup);
    }

    /// <summary>
    /// Process tick data
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    protected async Task OnData(PointModel point)
    {
      await OptionView.OnUpdate(point, 1, async options =>
      {
        var adapter = OptionView.View.Adapters["Sim"];
        var account = adapter.Account;
        var rsi = Rsi.Calculate(account.Instruments.Values.First().PointGroups);
        var posSide = account
          .Positions
          .FirstOrDefault()
          .Value
          ?.Transaction
          ?.Instrument
          ?.Derivative
          ?.Side;

        if (rsi.Values.Count > rsi.Interval)
        {
          if (rsi.Point.Last < 30 && posSide is not OptionSideEnum.Put)
          {
            var orders = GetOrders(adapter, point, OptionSideEnum.Put, options);

            if (orders.Count > 0)
            {
              Price = point.Last.Value;
              await TradeService.ClosePositions(adapter);
              await adapter.CreateOrders([.. orders]);
            }
          }

          if (rsi.Point.Last > 70 && posSide is not OptionSideEnum.Call)
          {
            var orders = GetOrders(adapter, point, OptionSideEnum.Call, options);

            if (orders.Count > 0)
            {
              Price = -point.Last.Value;
              await TradeService.ClosePositions(adapter);
              await adapter.CreateOrders([.. orders]);
            }
          }
        }

        OptionView.View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Indicators", "Rsi", new LineShape { Y = rsi.Point.Last });
      });
    }

    /// <summary>
    /// Create credit spread strategy
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="side"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    protected static IList<OrderModel> GetOrders(IGateway adapter, PointModel point, OptionSideEnum side, IList<InstrumentModel> options)
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
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Transaction = new ()
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new ()
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
  }
}
