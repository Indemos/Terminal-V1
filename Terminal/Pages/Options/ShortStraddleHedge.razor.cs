using System;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Models;
using Terminal.Services;

namespace Terminal.Pages.Options
{
  public partial class ShortStraddleHedge
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
          var shareOrders = TradeService.GetShareHedge(adapter, point);

          if (shareOrders.Count > 0)
          {
            var orderResponse = await adapter.CreateOrders([.. shareOrders]);
          }
        }
      });
    }
  }
}
