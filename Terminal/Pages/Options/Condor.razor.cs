using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Terminal.Services;

namespace Terminal.Pages.Options
{
  public partial class Condor
  {
    public virtual OptionPageComponent OptionView { get; set; }

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

        await OptionView.OnLoad(OnData);
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
      var adapter = OptionView.View.Adapters["Sim"];
      var account = adapter.Account;

      await OptionView.OnUpdate(point, 1, async options =>
      {
        if (account.Orders.Count is 0 && account.Positions.Count is 0)
        {
          var orders = GetOrders(adapter, point, options);
          var orderResponse = await adapter.CreateOrders([.. orders]);
        }
      });
    }

    /// <summary>
    /// Create short condor strategy
    /// </summary>
    /// <param name="point"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    protected static IList<OrderModel> GetOrders(IGateway adapter, PointModel point, IList<InstrumentModel> options)
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
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = longPut }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = longCall }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = shortPut }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = shortCall }
          }
        ]
      };

      return [order];
    }
  }
}
