using Canvas.Core.Shapes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Simulation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Indicators;
using Terminal.Core.Models;
using Terminal.Services;

namespace Terminal.Pages.Shares
{
  public partial class RangeBar
  {
    [Inject] IConfiguration Configuration { get; set; }

    protected virtual double Price { get; set; }
    protected virtual PageComponent View { get; set; }
    protected virtual PerformanceIndicator Performance { get; set; }
    protected virtual InstrumentModel Instrument { get; set; } = new InstrumentModel
    {
      Name = "SPY",
      Type = InstrumentEnum.Shares
    };

    /// <summary>
    /// Setup views and adapters
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        await CreateViews();

        Performance = new PerformanceIndicator { Name = nameof(Performance) };

        View.OnPreConnect = CreateAccounts;
        View.OnPostConnect = () =>
        {
          var account = View.Adapters["Sim"].Account;

          View.DealsView.UpdateItems(account.Deals);
          View.OrdersView.UpdateItems(account.Orders.Values);
          View.PositionsView.UpdateItems(account.Positions.Values);
        };
      }

      await base.OnAfterRenderAsync(setup);
    }

    /// <summary>
    /// Charts setup
    /// </summary>
    /// <returns></returns>
    protected virtual async Task CreateViews()
    {
      await View.ChartsView.Create("Prices");
      await View.ReportsView.Create("Performance");
    }

    /// <summary>
    /// Setup simulation account
    /// </summary>
    /// <returns></returns>
    protected virtual void CreateAccounts()
    {
      var account = new Account
      {
        Balance = 25000,
        Instruments = new Dictionary<string, InstrumentModel>
        {
          [Instrument.Name] = Instrument
        }
      };

      View.Adapters["Sim"] = new Adapter
      {
        Speed = 1,
        Account = account,
        Source = Configuration["Simulation:Source"]
      };

      View
        .Adapters
        .Values
        .ForEach(adapter => adapter.PointStream += async message => await OnData(message.Next));
    }

    /// <summary>
    /// Process tick data
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    protected async Task OnData(PointModel point)
    {
      var step = 0.50;
      var adapter = View.Adapters["Sim"];
      var account = adapter.Account;
      var performance = Performance.Calculate([account]);

      Price = Price.Is(0) ? point.Last.Value : Price;

      if (Math.Abs((point.Last - Price).Value) > step)
      {
        var side = OrderSideEnum.Buy;
        var position = account.Positions.FirstOrDefault().Value;

        if (point.Last < Price)
        {
          side = OrderSideEnum.Sell;
        }

        if (position is not null && Equals(side, position.Side) is false)
        {
          await TradeService.ClosePositions(adapter);
        }

        var order = new OrderModel
        {
          Side = side,
          Type = OrderTypeEnum.Market,
          Transaction = new() { Volume = 100, Instrument = Instrument }
        };

        Price = point.Last.Value;

        await adapter.CreateOrders(order);

        View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Prices", "Price", new AreaShape { Y = Price });
      }

      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "Balance", new AreaShape { Y = account.Balance });
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "PnL", new LineShape { Y = performance.Point.Last });
      View.DealsView.UpdateItems(account.Deals);
      View.OrdersView.UpdateItems(account.Orders.Values);
      View.PositionsView.UpdateItems(account.Positions.Values);
    }
  }
}
