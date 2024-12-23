using Canvas.Core.Models;
using Canvas.Core.Shapes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Simulation;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Indicators;
using Terminal.Core.Models;
using Terminal.Services;

namespace Terminal.Pages.Shares
{
  public partial class Lock
  {
    [Inject] IConfiguration Configuration { get; set; }

    /// <summary>
    /// Strategy
    /// </summary>
    const string _assetX = "GOOG";
    const string _assetY = "GOOGL";

    protected virtual ComponentModel UpSide { get; set; }
    protected virtual ComponentModel DownSide { get; set; }
    protected virtual PageComponent View { get; set; }
    protected virtual PerformanceIndicator Performance { get; set; }

    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        await CreateViews();

        UpSide = new ComponentModel { Color = SKColors.DeepSkyBlue };
        DownSide = new ComponentModel { Color = SKColors.OrangeRed };
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

    protected virtual async Task CreateViews()
    {
      await View.ChartsView.Create("Prices");
      await View.ReportsView.Create("Performance");
    }

    protected virtual void CreateAccounts()
    {
      var account = new Account
      {
        Balance = 25000,
        Instruments = new ConcurrentDictionary<string, InstrumentModel>
        {
          [_assetX] = new InstrumentModel { Name = _assetX },
          [_assetY] = new InstrumentModel { Name = _assetY }
        }
      };

      View.Adapters["Sim"] = new Adapter
      {
        Speed = 1,
        Account = account,
        Source = Configuration["Simulation:Source"]
      };

      Performance = new PerformanceIndicator { Name = "Balance" };

      View
        .Adapters
        .Values
        .ForEach(adapter => adapter.PointStream += async message => await OnData(message.Next));
    }

    protected async Task OnData(PointModel point)
    {
      var adapter = View.Adapters["Sim"];
      var account = adapter.Account;
      var instrumentX = account.Instruments[_assetX];
      var instrumentY = account.Instruments[_assetY];
      var seriesX = instrumentX.Points;
      var seriesY = instrumentY.Points;

      if (seriesX.Count is 0 || seriesY.Count is 0)
      {
        return;
      }

      var pointX = seriesX.Last().Last;
      var pointY = seriesY.Last().Last;
      var performance = Performance.Calculate([account]);

      if (account.Positions.Count is 0)
      {
        await OpenPositions(100, 90);
      }

      if (account.Positions.Count > 0)
      {
        if (performance.Point.Last - account.Balance > 5)
        {
          await TradeService.ClosePositions(adapter);
        }
      }

      View.DealsView.UpdateItems(account.Deals);
      View.OrdersView.UpdateItems(account.Orders.Values);
      View.PositionsView.UpdateItems(account.Positions.Values);
      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Prices", "Combination", new LineShape { Y = pointX + pointY, Component = UpSide });
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "Balance", new AreaShape { Y = account.Balance });
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "PnL", View.ReportsView.GetShape<LineShape>(performance.Point, SKColors.OrangeRed));
    }

    protected async Task OpenPositions(double? amountX, double? amountY)
    {
      var adapter = View.Adapters["Sim"];
      var account = adapter.Account;
      var instrumentX = account.Instruments[_assetX];
      var instrumentY = account.Instruments[_assetY];

      await TradeService.ClosePositions(adapter);
      await adapter.CreateOrders(
      [
        new OrderModel
        {
          Volume = amountX,
          Side = OrderSideEnum.Buy,
          Type = OrderTypeEnum.Market,
          Transaction = new() { Instrument = instrumentX }
        },
        new OrderModel
        {
          Volume = amountY,
          Side = OrderSideEnum.Sell,
          Type = OrderTypeEnum.Market,
          Transaction = new() { Instrument = instrumentY }
        }
      ]);
    }
  }
}
