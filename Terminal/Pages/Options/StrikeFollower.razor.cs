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
  public partial class StrikeFollower
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
    public async Task OnData(PointModel point)
    {
      var adapter = OptionView.View.Adapters["Sim"];
      var account = adapter.Account;

      await OptionView.OnUpdate(point, 1, async options =>
      {
        var strike = GetStrike(point, options, o => o.Derivative.Variable.Gamma ?? 0);
        var isSameStrike = Equals(Math.Round(point.Last.Value), Math.Round(strike.Key));

        if (account.Orders.Count is 0 && account.Positions.Count is 0)
        {
          var order = GetNextOrder(strike.Key, point, options);

          if (order is not null)
          {
            var orderResponse = await adapter.CreateOrders(order);
          }
        }

        if (account.Positions.Count > 0 && isSameStrike is false)
        {
          var order = GetNextOrder(strike.Key, point, options);

          if (order is not null)
          {
            var curOrder = account.Positions.Values.First();
            var nextSide = order.Transaction.Instrument.Derivative.Side;
            var curSide = curOrder.Transaction.Instrument.Derivative.Side;

            if (!Equals(nextSide, curSide))
            {
              await TradeService.ClosePositions(adapter);
              var orderResponse = await adapter.CreateOrders(order);
            }
          }
        }
      });
    }

    /// <summary>
    /// Get strike with max gamma
    /// </summary>
    /// <param name="point"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public virtual KeyValuePair<double, double> GetStrike(
      PointModel point,
      IList<InstrumentModel> options,
      Func<InstrumentModel, double> action)
    {
      var resPuts = options
        .Where(o => o.Derivative.Side is OptionSideEnum.Put)
        .GroupBy(o => o.Derivative.Strike)
        .ToDictionary(
          option => option.Key,
          option => option.Sum(v => action(v)))
        .OrderByDescending(o => o.Value)
        .FirstOrDefault();

      var resCalls = options
        .Where(o => o.Derivative.Side is OptionSideEnum.Call)
        .GroupBy(o => o.Derivative.Strike)
        .ToDictionary(
          option => option.Key,
          option => option.Sum(v => action(v)))
        .OrderByDescending(o => o.Value)
        .FirstOrDefault();

      var res = resCalls.Value > resPuts.Value ? resCalls : resPuts;

      // Smooth strike changes

      return KeyValuePair.Create(res.Key.Value, res.Value);
    }

    /// <summary>
    /// Create short straddle strategy
    /// </summary>
    /// <param name="strike"></param>
    /// <param name="point"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public virtual OrderModel GetNextOrder(double strike, PointModel point, IList<InstrumentModel> options)
    {
      var adapter = OptionView.View.Adapters["Sim"];
      var account = adapter.Account;
      var position = account.Positions.FirstOrDefault().Value;
      var price = point.Last;

      if (position is not null)
      {
        price = position.Transaction.Instrument.Derivative.Strike;
      }

      if (Equals(price, strike))
      {
        return null;
      }

      var side = price < strike ? OptionSideEnum.Call : OptionSideEnum.Put;
      var nextOption = options
        .Where(o => Equals(o.Derivative.Side, side))
        .Where(o => o.Derivative.Strike >= point.Last)
        .FirstOrDefault();

      var order = new OrderModel
      {
        Side = OrderSideEnum.Buy,
        Type = OrderTypeEnum.Market,
        Transaction = new() { Volume = 1, Instrument = nextOption }
      };

      return order;
    }
  }
}
