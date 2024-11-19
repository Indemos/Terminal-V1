using Canvas.Core.Shapes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Schwab;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Indicators;
using Terminal.Core.Models;

namespace Terminal.Pages.Gateways
{
  public partial class Schwab
  {
    [Inject] IConfiguration Configuration { get; set; }

    protected virtual int Counter { get; set; }
    protected virtual PageComponent View { get; set; }
    protected virtual PerformanceIndicator Performance { get; set; }
    protected virtual InstrumentModel Instrument { get; set; } = new InstrumentModel
    {
      Name = "/ESZ24",
      Exchange = "CME",
      Type = InstrumentEnum.Futures,
      TimeFrame = TimeSpan.FromMinutes(1)
    };

    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        await CreateViews();

        View.OnPreConnect = CreateAccounts;
        View.OnPostConnect = () =>
        {
          var account = View.Adapters["Prime"].Account;

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
        Descriptor = Configuration["Schwab:Account"],
        Instruments = new ConcurrentDictionary<string, InstrumentModel>
        {
          [Instrument.Name] = Instrument
        }
      };

      View.Adapters["Prime"] = new Adapter
      {
        Account = account,
        AccessToken = Configuration["Schwab:AccessToken"],
        RefreshToken = Configuration["Schwab:RefreshToken"],
        ClientId = Configuration["Schwab:ConsumerKey"],
        ClientSecret = Configuration["Schwab:ConsumerSecret"]
      };

      Performance = new PerformanceIndicator { Name = "Balance" };

      View
        .Adapters
        .Values
        .ForEach(adapter => adapter.PointStream += async message =>
        {
          if (Equals(message.Next.Instrument.Name, Instrument.Name))
          {
            await OnData(message.Next);
          }
        });
    }

    private async Task OnData(PointModel point)
    {
      Counter = (Counter + 1) % 100;

      var account = View.Adapters["Prime"].Account;
      var instrument = account.Instruments[Instrument.Name];
      var performance = Performance.Calculate([account]);

      if (account.Orders.Count < 1 && account.Positions.Count < 1)
      {
        await OpenPositions(Instrument, 1);
      }

      if (Counter is 0 && account.Positions.Count > 0)
      {
        await ClosePositions();
        await OpenPositions(Instrument, account.Positions.First().Value.Side is OrderSideEnum.Buy ? -1 : 1);
      }

      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Prices", "Bars", View.ChartsView.GetShape<CandleShape>(point));
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "Balance", new AreaShape { Y = account.Balance });
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "PnL", new LineShape { Y = performance.Point.Last });
      View.DealsView.UpdateItems(account.Deals);
      View.OrdersView.UpdateItems(account.Orders.Values);
      View.PositionsView.UpdateItems(account.Positions.Values);
    }

    private async Task OpenPositions(InstrumentModel instrument, double direction)
    {
      var price = Instrument.Points.Last().Last;
      var side = direction > 0 ? OrderSideEnum.Buy : OrderSideEnum.Sell;
      var stopSide = direction < 0 ? OrderSideEnum.Buy : OrderSideEnum.Sell;
      var adapter = View.Adapters["Prime"];
      var TP = new OrderModel
      {
        Price = price + 15 * direction,
        Side = stopSide,
        Type = OrderTypeEnum.Stop,
        Instruction = InstructionEnum.Brace,
        Transaction = new()
        {
          Volume = 1,
          Instrument = instrument
        }
      };

      var SL = new OrderModel
      {
        Price = price - 15 * direction,
        Side = stopSide,
        Type = OrderTypeEnum.Limit,
        Instruction = InstructionEnum.Brace,
        Transaction = new()
        {
          Volume = 1,
          Instrument = instrument
        }
      };

      var order = new OrderModel
      {
        Price = price,
        Side = side,
        Type = OrderTypeEnum.Market,
        Orders = [SL, TP],
        Transaction = new()
        {
          Volume = 1,
          Instrument = instrument
        }
      };

      await adapter.CreateOrders(order);
    }

    private async Task ClosePositions()
    {
      var adapter = View.Adapters["Prime"];

      foreach (var position in adapter.Account.Positions)
      {
        var side = position.Value.Side is OrderSideEnum.Buy ? OrderSideEnum.Sell : OrderSideEnum.Buy;
        var order = new OrderModel
        {
          Side = side,
          Type = OrderTypeEnum.Market,
          Transaction = new()
          {
            Volume = position.Value.Transaction.Volume,
            Instrument = position.Value.Transaction.Instrument
          }
        };

        await adapter.CreateOrders(order);
      }
    }
  }
}
