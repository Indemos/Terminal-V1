using Canvas.Core.Models;
using Canvas.Core.Shapes;
using SkiaSharp;
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
  public partial class DebitSpreadReversal
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
          .Values
          .FirstOrDefault(o => o.Transaction.Instrument.Derivative is not null)
          ?.Transaction
          ?.Instrument
          ?.Derivative
          ?.Side;

        if (rsi.Values.Count > rsi.Interval)
        {
          if (rsi.Point.Last < 30 && posSide is not OptionSideEnum.Call)
          {
            var orders = TradeService.GetDebigSpread(adapter, point, OptionSideEnum.Call, options);

            if (orders.Count > 0)
            {
              Price = point.Last.Value;
              await TradeService.ClosePositions(adapter);
              await adapter.CreateOrders([.. orders]);
            }
          }

          if (rsi.Point.Last > 70 && posSide is not OptionSideEnum.Put)
          {
            var orders = TradeService.GetDebigSpread(adapter, point, OptionSideEnum.Put, options);

            if (orders.Count > 0)
            {
              Price = -point.Last.Value;
              await TradeService.ClosePositions(adapter);
              await adapter.CreateOrders([.. orders]);
            }
          }
        }

        if (account.Positions.Count > 0)
        {
          var optionDelta = Math.Round(account
            .Positions
            .Values
            .Where(o => o.Transaction.Instrument.Derivative is not null)
            .Sum(TradeService.GetDelta), MidpointRounding.ToZero);
        }

        OptionView.View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Indicators", "Rsi", new LineShape { Y = rsi.Point.Last });
      });
    }
  }
}
