using Canvas.Core.Shapes;
using InteractiveBrokers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Indicators;
using Terminal.Core.Models;
using Terminal.Services;

namespace Terminal.Pages.Gateways
{
  public partial class InteractiveBrokersDemo
  {
    [Inject] IConfiguration Configuration { get; set; }

    protected PageComponent View { get; set; }
    protected PerformanceIndicator Performance { get; set; }
    protected virtual InstrumentModel Instrument { get; set; } = new InstrumentModel
    {
      Name = "ESH5",
      Exchange = "CME",
      Type = InstrumentEnum.Futures,
      TimeFrame = TimeSpan.FromMinutes(1),
      Basis = new InstrumentModel { Name = "ES" }
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
        Descriptor = Configuration["InteractiveBrokers:PaperAccount"],
        Instruments = new ConcurrentDictionary<string, InstrumentModel>
        {
          [Instrument.Name] = Instrument
        }
      };

      View.Adapters["Prime"] = new Adapter
      {
        Account = account,
        Port = int.Parse(Configuration["InteractiveBrokers:PaperPort"])
      };

      Performance = new PerformanceIndicator { Name = "Balance" };

      View
        .Adapters
        .Values
        .ForEach(adapter => adapter.DataStream += async message =>
        {
          if (Equals(message.Next.Instrument.Name, Instrument.Name))
          {
            await OnData(message.Next);
          }
        });
    }

    protected async Task OnData(PointModel point)
    {
      var name = Instrument.Name;
      var account = View.Adapters["Prime"].Account;
      var instrument = account.Instruments[name];
      var performance = Performance.Calculate([account]);
      var openOrders = account.Orders.Values.Where(o => Equals(o.Name, name));
      var openPositions = account.Positions.Values.Where(o => Equals(o.Name, name));

      if (openOrders.IsEmpty() && openPositions.IsEmpty())
      {
        await OpenPositions(Instrument, 1);
        await TradeService.Done(async () =>
        {
          var position = account
            .Positions
            .Values
            .Where(o => Equals(o.BasisName ?? o.Name, name))
            .FirstOrDefault();

          if (position is not null)
          {
            await ClosePositions(position.Name);
          }

        }, 10000);
      }

      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Prices", "Bars", View.ChartsView.GetShape<CandleShape>(point));
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "Balance", new AreaShape { Y = account.Balance });
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "PnL", new LineShape { Y = performance.Point.Last });
      View.DealsView.UpdateItems(account.Deals);
      View.OrdersView.UpdateItems(account.Orders.Values);
      View.PositionsView.UpdateItems(account.Positions.Values);
    }

    protected double? GetPrice(double direction) => direction > 0 ?
      Instrument.Point.Ask :
      Instrument.Point.Bid;

    protected async Task OpenPositions(InstrumentModel instrument, double direction)
    {
      var adapter = View.Adapters["Prime"];
      var side = direction > 0 ? OrderSideEnum.Long : OrderSideEnum.Short;
      var stopSide = direction < 0 ? OrderSideEnum.Long : OrderSideEnum.Short;

      var TP = new OrderModel
      {
        Volume = 1,
        Side = stopSide,
        Type = OrderTypeEnum.Limit,
        Instruction = InstructionEnum.Brace,
        Price = GetPrice(direction) + 15 * direction,
        Transaction = new() { Instrument = instrument }
      };

      var SL = new OrderModel
      {
        Volume = 1,
        Side = stopSide,
        Type = OrderTypeEnum.Stop,
        Instruction = InstructionEnum.Brace,
        Price = GetPrice(-direction) - 15 * direction,
        Transaction = new() { Instrument = instrument }
      };

      var order = new OrderModel
      {
        Volume = 1,
        Side = side,
        Price = GetPrice(direction),
        Type = OrderTypeEnum.Market,
        Transaction = new() { Instrument = instrument },
        Orders = [SL, TP]
      };

      await adapter.CreateOrders(order);
    }

    protected async Task ClosePositions(string name)
    {
      var adapter = View.Adapters["Prime"];

      foreach (var position in adapter.Account.Positions.Values.Where(o => Equals(name, o.Name)))
      {
        var side = position.Side is OrderSideEnum.Long ? OrderSideEnum.Short : OrderSideEnum.Long;
        var order = new OrderModel
        {
          Side = side,
          Type = OrderTypeEnum.Market,
          Volume = position.Volume,
          Transaction = new()
          {
            Instrument = position.Transaction.Instrument
          }
        };

        await adapter.CreateOrders(order);
      }
    }
  }
}
