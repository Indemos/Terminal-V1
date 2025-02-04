using Estimator.Services;
using MudBlazor;
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
  public partial class ShortStraddleDeltaLong
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
          var orders = GetShareDirection(adapter, point);

          if (orders.Count > 0)
          {
            var orderResponse = await adapter.CreateOrders([.. orders]);
          }
        }
      });
    }

    protected double GetDelta(OrderModel o, PointModel point)
    {
      var volume = o.Volume;
      var instrument = o.Transaction?.Instrument;
      var side = o.Side is OrderSideEnum.Long ? 1.0 : -1.0;

      if (instrument?.Derivative is null)
      {
        return volume * side ?? 0;
      }

      var units = instrument?.Leverage;
      var derivative = instrument?.Derivative;
      var delta = derivative?.Variance?.Delta;
      var optionSide = derivative?.Side is OptionSideEnum.Put ? "Put" : "Call";
      var exp = (instrument.Derivative.ExpirationDate - point.Time)?.TotalDays / 365.0;
      var customDelta = OptionService.Delta(optionSide, point.Last.Value, derivative.Strike.Value, exp.Value, derivative.Sigma.Value / 100.0, 0, 0);

      return (customDelta * units * side) ?? 0;
    }

    protected IList<OrderModel> GetShareDirection(IGateway adapter, PointModel point)
    {
      var account = adapter.Account;
      var basisDelta = account
        .Positions
        .Values
        .Where(o => o.Transaction.Instrument.Derivative is null)
        .Sum(o => GetDelta(o, point));

      var optionDelta = account
        .Positions
        .Values
        .Where(o => o.Transaction.Instrument.Derivative is not null)
        .Sum(o => GetDelta(o, point));

      var isOversold = basisDelta < 0 && optionDelta > 0;
      var isOverbought = basisDelta > 0 && optionDelta < 0;

      if (basisDelta is 0 || isOversold || isOverbought)
      {
        var order = new OrderModel
        {
          Volume = 100,
          Type = OrderTypeEnum.Market,
          Side = optionDelta > 0 ? OrderSideEnum.Long : OrderSideEnum.Short,
          Transaction = new() { Instrument = point.Instrument }
        };

        return [order];
      }

      return [];
    }
  }
}
