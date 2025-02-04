using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using Terminal.Services;

namespace Terminal.Pages.Options
{
  public partial class ShortStraddleVolumeLong
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
        var puts = options.Where(o => o.Derivative.Side is OptionSideEnum.Put).Sum(o => o.Point.Volume);
        var calls = options.Where(o => o.Derivative.Side is OptionSideEnum.Call).Sum(o => o.Point.Volume);
        var position = account.Positions.Values?.FirstOrDefault();
        var isClear = account.Orders.Count is 0 && account.Positions.Count is 0;
        var isTop = puts > calls && (position?.Transaction?.Instrument?.Derivative?.Side is OptionSideEnum.Call || isClear);
        var isBottom = puts < calls && (position?.Transaction?.Instrument?.Derivative?.Side is OptionSideEnum.Put || isClear);

        if (isTop || isBottom)
        {
          var orders = Array.Empty<OrderModel>();

          await TradeService.ClosePositions(adapter);

          switch (true)
          {
            case true when puts > calls: orders = [.. GetOrders(adapter, point, OptionSideEnum.Put, options)]; break;
            case true when puts < calls: orders = [.. GetOrders(adapter, point, OptionSideEnum.Call, options)]; break;
          }

          var orderResponse = await adapter.CreateOrders([.. orders]);
        }
      });
    }

    /// <summary>
    /// Create debit spread strategy
    /// </summary>
    /// <param name="adapter"></param>
    /// <param name="point"></param>
    /// <param name="side"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    protected static IList<OrderModel> GetOrders(IGateway adapter, PointModel point, OptionSideEnum side, IList<InstrumentModel> options)
    {
      var account = adapter.Account;
      var sideOptions = options.Where(o => Equals(o.Derivative.Side, side));
      var chainCenter = sideOptions.OrderBy(o => Math.Abs(Math.Abs(o.Derivative.Variance.Delta.Value) - 0.5));
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
            Transaction = new TransactionModel { Instrument = chainCenter.First() }
          }
        ]
      };

      return [order];
    }
  }
}
