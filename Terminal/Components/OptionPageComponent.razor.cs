using Canvas.Core.Models;
using Canvas.Core.Shapes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Indicators;
using Terminal.Core.Models;
using Terminal.Models;
using Terminal.Services;
using Sim = Simulation;

namespace Terminal.Components
{
  public partial class OptionPageComponent
  {
    [Inject] IConfiguration Configuration { get; set; }

    public virtual PageComponent View { get; set; }
    public virtual ChartsComponent FramesView { get; set; }
    public virtual ChartsComponent StrikesView { get; set; }
    public virtual ChartsComponent PremiumsView { get; set; }
    public virtual ChartsComponent PositionsView { get; set; }
    public virtual PerformanceIndicator Performance { get; set; }
    public virtual InstrumentModel Instrument { get; set; }

    /// <summary>
    /// Setup views and adapters
    /// </summary>
    /// <param name="action"></param>
    /// <param name="span"></param>
    /// <returns></returns>
    public virtual async Task OnLoad(Func<PointModel, Task> action, params string[] groups)
    {
      await CreateViews(groups);

      Performance = new PerformanceIndicator { Name = nameof(Performance) };

      View.OnPreConnect = () =>
      {
        View.Adapters["Sim"] = CreateSimAccount();
        View.Adapters["Sim"].PointStream += o => action(o.Next);
      };

      View.OnPostConnect = () =>
      {
        var account = View.Adapters["Sim"].Account;

        View.DealsView.UpdateItems(account.Deals);
        View.OrdersView.UpdateItems(account.Orders.Values);
        View.PositionsView.UpdateItems(account.Positions.Values);
      };

      View.OnDisconnect = () =>
      {
        FramesView.Clear();
        PremiumsView.Clear();
        PositionsView.Clear();
        StrikesView.Clear();
      };
    }

    /// <summary>
    /// Charts setup
    /// </summary>
    /// <returns></returns>
    public virtual async Task CreateViews(params string[] groups)
    {
      await View.ReportsView.Create("Performance");
      await View.ChartsView.Create([.. groups, "Volume", "Delta", "Vega", "Theta", "Ratios"]);
      await PositionsView.Create("Assets", "Volume", "Delta", "Gamma", "Vega", "Iv", "Theta");
      await StrikesView.Create("Gamma", "Theta", "Volume", "OI", "Ratios");
      await PremiumsView.Create("Estimates");
      await FramesView.Create("Prices");
    }

    /// <summary>
    /// Setup simulation account
    /// </summary>
    /// <returns></returns>
    public virtual Sim.Adapter CreateSimAccount()
    {
      var account = new Account
      {
        Balance = 25000,
        Instruments = new ConcurrentDictionary<string, InstrumentModel>
        {
          [Instrument.Name] = Instrument
        }
      };

      return new Sim.Adapter
      {
        Account = account,
        Source = Configuration["Simulation:Source"]
      };
    }

    /// <summary>
    /// Process tick data
    /// </summary>
    /// <param name="point"></param>
    /// <param name="days"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public virtual async Task OnUpdate(PointModel point, int days, Action<IList<InstrumentModel>> action)
    {
      var adapter = View.Adapters["Sim"];
      var account = adapter.Account;
      var options = await TradeService.GetOptions(adapter, point, point.Time.Value.AddDays(days));

      action([.. options]);

      var com = new ComponentModel { Color = SKColors.LimeGreen };
      var comUp = new ComponentModel { Color = SKColors.DeepSkyBlue };
      var comDown = new ComponentModel { Color = SKColors.OrangeRed };
      var puts = options.Where(o => o.Derivative.Side is OptionSideEnum.Put);
      var calls = options.Where(o => o.Derivative.Side is OptionSideEnum.Call);
      var putBids = puts.Sum(o => o.Point.BidSize ?? 0);
      var putAsks = puts.Sum(o => o.Point.AskSize ?? 0);
      var callBids = calls.Sum(o => o.Point.BidSize ?? 0);
      var callAsks = calls.Sum(o => o.Point.AskSize ?? 0);

      FramesView.UpdateItems(point.Time.Value.Ticks, "Prices", "Bars", View.ReportsView.GetShape<CandleShape>(point));

      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Volume", "CallPutVolume", new AreaShape { Y = calls.Sum(o => o.Point.Volume) - puts.Sum(o => o.Point.Volume), Component = com });
      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Delta", "CallPutDelta", new AreaShape { Y = calls.Sum(o => o.Derivative.Variance.Delta) + puts.Sum(o => o.Derivative.Variance.Delta), Component = com });
      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Vega", "CallPutVega", new AreaShape { Y = calls.Sum(o => o.Derivative.Variance.Vega) - puts.Sum(o => o.Derivative.Variance.Vega), Component = com });
      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Theta", "CallPutTheta", new AreaShape { Y = calls.Sum(o => o.Derivative.Variance.Theta) - puts.Sum(o => o.Derivative.Variance.Theta), Component = com });
      View.ChartsView.UpdateItems(point.Time.Value.Ticks, "Ratios", "CallPutDomRatio", new AreaShape { Y = (callBids - callAsks) - (putBids - putAsks), Component = com });

      var performance = Performance.Calculate([account]);

      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "Balance", new AreaShape { Y = account.Balance });
      View.ReportsView.UpdateItems(point.Time.Value.Ticks, "Performance", "PnL", new LineShape { Y = performance.Point.Last, Component = comDown });

      var positions = account.Positions.Values;
      var basisPositions = positions.Where(o => o.Transaction.Instrument.Derivative is null);
      var optionPositions = positions.Where(o => o.Transaction.Instrument.Derivative is not null);
      var posPuts = optionPositions.Where(o => o.Transaction.Instrument.Derivative.Side is OptionSideEnum.Put);
      var posCalls = optionPositions.Where(o => o.Transaction.Instrument.Derivative.Side is OptionSideEnum.Call);

      var longs = basisPositions
        .Where(o => o.Side is OrderSideEnum.Buy)
        .Concat(posCalls.Where(o => o.Side is OrderSideEnum.Buy))
        .Concat(posPuts.Where(o => o.Side is OrderSideEnum.Sell));

      var shorts = basisPositions
        .Where(o => o.Side is OrderSideEnum.Sell)
        .Concat(posPuts.Where(o => o.Side is OrderSideEnum.Buy))
        .Concat(posCalls.Where(o => o.Side is OrderSideEnum.Sell));

      PositionsView.UpdateItems(point.Time.Value.Ticks, "Assets", "BasisDelta", new AreaShape { Y = basisPositions.Sum(TradeService.GetDelta), Component = comUp });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Assets", "OptionDelta", new AreaShape { Y = optionPositions.Sum(TradeService.GetDelta), Component = comDown });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Volume", "LongVolume", new AreaShape { Y = longs.Sum(o => o.Transaction.Instrument.Point.Volume), Component = comUp });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Volume", "ShortVolume", new AreaShape { Y = -shorts.Sum(o => o.Transaction.Instrument.Point.Volume), Component = comDown });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Delta", "LongDelta", new AreaShape { Y = longs.Sum(TradeService.GetDelta), Component = comUp });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Delta", "ShortDelta", new AreaShape { Y = shorts.Sum(TradeService.GetDelta), Component = comDown });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Gamma", "LongGamma", new AreaShape { Y = longs.Sum(o => o.Transaction.Instrument.Derivative?.Variance?.Gamma ?? 0), Component = comUp });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Gamma", "ShortGamma", new AreaShape { Y = -shorts.Sum(o => o.Transaction.Instrument.Derivative?.Variance?.Gamma ?? 0), Component = comDown });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Theta", "LongTheta", new AreaShape { Y = -longs.Sum(o => o.Transaction.Instrument.Derivative?.Variance?.Theta ?? 0), Component = comUp });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Theta", "ShortTheta", new AreaShape { Y = shorts.Sum(o => o.Transaction.Instrument.Derivative?.Variance?.Theta ?? 0), Component = comDown });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Vega", "LongVega", new AreaShape { Y = longs.Sum(o => o.Transaction.Instrument.Derivative?.Variance?.Vega ?? 0), Component = comUp });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Vega", "ShortVega", new AreaShape { Y = -shorts.Sum(o => o.Transaction.Instrument.Derivative?.Variance?.Vega ?? 0), Component = comDown });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Iv", "LongIv", new AreaShape { Y = longs.Sum(o => o.Transaction.Instrument.Derivative?.Sigma ?? 0), Component = comUp });
      PositionsView.UpdateItems(point.Time.Value.Ticks, "Iv", "ShortIv", new AreaShape { Y = -shorts.Sum(o => o.Transaction.Instrument.Derivative?.Sigma ?? 0), Component = comDown });

      View.DealsView.UpdateItems(account.Deals);
      View.OrdersView.UpdateItems(account.Orders.Values);
      View.PositionsView.UpdateItems(account.Positions.Values);

      ShowEstimates(point);
      ShowStrikes(point, options);
    }

    /// <summary>
    /// Render estimated position gain
    /// </summary>
    /// <param name="point"></param>
    protected void ShowEstimates(PointModel point)
    {
      var adapter = View.Adapters["Sim"];
      var account = adapter.Account;
      var sums = new Dictionary<double, double>();

      foreach (var pos in account.Positions.Values)
      {
        var plusPercents = Enumerable.Range(0, 20).Select((o, i) => o / 2.0 / 100.0);
        var minusPercents = Enumerable.Range(1, 20).Select((o, i) => -o / 2.0 / 100.0).Reverse();
        var inputModel = new OptionInputModel
        {
          Price = point.Last.Value,
          Amount = pos.Volume ?? 0,
          Strike = pos.Transaction.Instrument?.Derivative?.Strike ?? 0,
          Premium = pos.Transaction.Instrument?.Point?.Last ?? 0,
          Date = pos.Transaction.Instrument?.Derivative?.ExpirationDate,
          Side = pos.Transaction.Instrument?.Derivative?.Side ?? 0,
          Position = pos.Side.Value
        };

        var chartPoints = minusPercents.Concat(plusPercents).Select((o, i) =>
        {
          var step = inputModel.Price + inputModel.Price * o;
          var sum = TradeService.GetEstimate(step, point.Time.Value, inputModel);
          var shape = new Shape();

          sums[o] = sums.TryGetValue(o, out var s) ? s + sum : sum;

          shape.X = step;
          shape.Groups = new Dictionary<string, IShape>();
          shape.Groups["Estimates"] = new Shape();
          shape.Groups["Estimates"].Groups = new Dictionary<string, IShape>();
          shape.Groups["Estimates"].Groups["Estimate"] = new LineShape { Name = "Estimate", Y = sums[o] };

          return shape as IShape;

        }).ToList();

        PremiumsView.Composers.ForEach(composer => composer.ShowIndex = o => $"{chartPoints.ElementAtOrDefault((int)o)?.X:0.00}");
        PremiumsView.UpdateOrdinals(chartPoints);
      }
    }

    /// <summary>
    /// Render metrics per strike
    /// </summary>
    /// <param name="point"></param>
    /// <param name="options"></param>
    protected void ShowStrikes(PointModel point, IList<InstrumentModel> options)
    {
      var adapter = View.Adapters["Sim"];
      var account = adapter.Account;
      var chartPoints = new List<IShape>();
      var groups = options
        .OrderBy(o => o.Derivative.Strike)
        .GroupBy(o => o.Derivative.Strike, o => o)
        .ToList();

      foreach (var group in groups)
      {
        var puts = group.Where(o => o.Derivative.Side is OptionSideEnum.Put);
        var calls = group.Where(o => o.Derivative.Side is OptionSideEnum.Call);
        var shape = new Shape();

        shape.Groups = new Dictionary<string, IShape>();

        shape.Groups["Gamma"] = new Shape();
        shape.Groups["Gamma"].Groups = new Dictionary<string, IShape>();
        shape.Groups["Gamma"].Groups["CallGamma"] = new BarShape { X = group.Key, Y = calls.Sum(o => o?.Derivative?.Variance.Gamma ?? 0), Component = new ComponentModel { Color = SKColors.DeepSkyBlue } };
        shape.Groups["Gamma"].Groups["PutGamma"] = new BarShape { X = group.Key, Y = -puts.Sum(o => o?.Derivative?.Variance.Gamma ?? 0), Component = new ComponentModel { Color = SKColors.OrangeRed } };

        shape.Groups["Theta"] = new Shape();
        shape.Groups["Theta"].Groups = new Dictionary<string, IShape>();
        shape.Groups["Theta"].Groups["CallTheta"] = new BarShape { X = group.Key, Y = -calls.Sum(o => o?.Derivative?.Variance.Theta ?? 0), Component = new ComponentModel { Color = SKColors.DeepSkyBlue } };
        shape.Groups["Theta"].Groups["PutTheta"] = new BarShape { X = group.Key, Y = puts.Sum(o => o?.Derivative?.Variance.Theta ?? 0), Component = new ComponentModel { Color = SKColors.OrangeRed } };

        shape.Groups["Volume"] = new Shape();
        shape.Groups["Volume"].Groups = new Dictionary<string, IShape>();
        shape.Groups["Volume"].Groups["CallVolume"] = new BarShape { X = group.Key, Y = calls.Sum(o => o?.Point?.Volume ?? 0), Component = new ComponentModel { Color = SKColors.DeepSkyBlue } };
        shape.Groups["Volume"].Groups["PutVolume"] = new BarShape { X = group.Key, Y = -puts.Sum(o => o?.Point?.Volume ?? 0), Component = new ComponentModel { Color = SKColors.OrangeRed } };

        shape.Groups["OI"] = new Shape();
        shape.Groups["OI"].Groups = new Dictionary<string, IShape>();
        shape.Groups["OI"].Groups["CallOpenInterest"] = new BarShape { X = group.Key, Y = calls.Sum(o => o?.Derivative?.OpenInterest ?? 0), Component = new ComponentModel { Color = SKColors.DeepSkyBlue } };
        shape.Groups["OI"].Groups["PutOpenInterest"] = new BarShape { X = group.Key, Y = -puts.Sum(o => o?.Derivative?.OpenInterest ?? 0), Component = new ComponentModel { Color = SKColors.OrangeRed } };

        var putBids = puts.Sum(o => o?.Point?.Bid ?? 0);
        var putAsks = puts.Sum(o => o?.Point?.Ask ?? 0);
        var callBids = calls.Sum(o => o?.Point?.Bid ?? 0);
        var callAsks = calls.Sum(o => o?.Point?.Ask ?? 0);

        shape.Groups["Ratios"] = new Shape();
        shape.Groups["Ratios"].Groups = new Dictionary<string, IShape>();
        shape.Groups["Ratios"].Groups["PcDomRatio"] = new BarShape { X = group.Key, Y = (callBids + putAsks) - (callAsks + putBids), Component = new ComponentModel { Color = SKColors.LimeGreen } };

        chartPoints.Add(shape);
      }

      StrikesView.Composers.ForEach(composer => composer.ShowIndex = o => $"{groups.ElementAtOrDefault((int)o)?.Key}");
      StrikesView.UpdateOrdinals(chartPoints);
    }
  }
}
