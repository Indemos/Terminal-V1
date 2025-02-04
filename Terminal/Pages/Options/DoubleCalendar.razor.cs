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
  public partial class DoubleCalendar
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

      await OptionView.OnUpdate(point, 30, async options =>
      {
        if (account.Orders.Count is 0 && account.Positions.Count is 0)
        {
          var upside = await GetOrders(adapter, point, OptionSideEnum.Call);
          var downside = await GetOrders(adapter, point, OptionSideEnum.Put);
          var orderResponse = await adapter.CreateOrders([..upside, ..downside]);
        }
      });
    }

    /// <summary>
    /// Create PMCC strategy
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="side"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    protected static async Task<IList<OrderModel>> GetOrders(IGateway adapter, PointModel point, OptionSideEnum side)
    {
      var account = adapter.Account;
      var curOptions = await TradeService.GetOptions(adapter, point, point.Time.Value.AddDays(0));
      var nextOptions = await TradeService.GetOptions(adapter, point, point.Time.Value.AddDays(1));
      var curSideOptions = curOptions.Where(o => Equals(o.Derivative.Side, side));
      var nextSideOptions = nextOptions.Where(o => Equals(o.Derivative.Side, side));
      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        Orders =
        [
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new ()
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Transaction = new ()
          }
        ]
      };

      switch (side)
      {
        case OptionSideEnum.Put:

          var put = order.Orders[0].Transaction.Instrument = nextSideOptions
            .Where(o => o.Derivative.Strike > point.Last)
            .FirstOrDefault();

          order.Orders[1].Transaction.Instrument = curSideOptions
            .Where(o => o.Derivative.Strike == put.Derivative.Strike)
            .LastOrDefault();

          break;

        case OptionSideEnum.Call:

          var call = order.Orders[0].Transaction.Instrument = nextSideOptions
            .Where(o => o.Derivative.Strike < point.Last)
            .LastOrDefault();

          order.Orders[1].Transaction.Instrument = curSideOptions
            .Where(o => o.Derivative.Strike == call.Derivative.Strike)
            .FirstOrDefault();

          break;
      }

      return [order];
    }
  }
}
