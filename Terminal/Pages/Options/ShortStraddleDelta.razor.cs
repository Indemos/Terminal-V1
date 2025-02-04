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
  public partial class ShortStraddleDelta
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
          var orders = TradeService.GetShortStraddle(adapter, point, options);
          var orderResponse = await adapter.CreateOrders([.. orders]);
        }

        if (account.Positions.Count > 0)
        {
          var shareOrders = GetShareHedge(adapter, point);

          if (shareOrders.Count > 0)
          {
            var orderResponse = await adapter.CreateOrders([.. shareOrders]);
          }
        }
      });
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
        .Sum(TradeService.GetDelta), MidpointRounding.ToZero);

      var optionDelta = Math.Round(account
        .Positions
        .Values
        .Where(o => o.Transaction.Instrument.Derivative is not null)
        .Sum(TradeService.GetDelta), MidpointRounding.ToZero);

      var delta = optionDelta + basisDelta;

      if (Math.Abs(delta) > 0)
      {
        var order = new OrderModel
        {
          Volume = Math.Abs(delta),
          Type = OrderTypeEnum.Market,
          Side = delta < 0 ? OrderSideEnum.Long : OrderSideEnum.Short,
          Transaction = new() { Instrument = point.Instrument }
        };

        return [order];
      }

      return [];
    }
  }
}
