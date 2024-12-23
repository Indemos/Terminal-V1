using Canvas.Core.Composers;
using Canvas.Core.Engines;
using Canvas.Core.Enums;
using Canvas.Core.Models;
using Canvas.Core.Services;
using Canvas.Core.Shapes;
using Canvas.Views.Web.Views;
using Derivative.Models;
using Derivative.Pages.Popups;
using Derivative.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using Schwab;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;

namespace Derivative.Pages
{
  public partial class Options
  {
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] IDialogService ModalService { get; set; }
    [Inject] IConfiguration Configuration { get; set; }
    [Inject] SyncService DataService { get; set; }

    protected int Count { get; set; } = 1;
    protected bool IsLoading { get; set; }
    protected Dictionary<string, Dictionary<string, CanvasView>> Groups { get; set; } = [];

    /// <summary>
    /// Page setup
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        await DataService.Connect();
      }

      await base.OnAfterRenderAsync(setup);
    }

    /// <summary>
    /// Clear
    /// </summary>
    public void OnClear()
    {
      Count = 1;
      Groups.Clear();
    }

    /// <summary>
    /// Description popup
    /// </summary>
    /// <returns></returns>
    public async Task OnDescription()
    {
      var props = new DialogOptions
      {
        FullWidth = true,
        MaxWidth = MaxWidth.ExtraSmall,
        CloseOnEscapeKey = true
      };

      var response = await ModalService
        .ShowAsync<Description>("Description", props)
        .ContinueWith(process => 0);
    }

    /// <summary>
    /// Bar chart editor
    /// </summary>
    /// <returns></returns>
    public async Task OnBarChart()
    {
      await OnChart<BarEditor>(async (caption, response, options) =>
      {
        var responseData = response as BarInputModel;

        await Group(caption, responseData.Group, options, async (view, records) =>
        {
          await ShowBars(
            responseData.ExpressionUp,
            responseData.ExpressionDown,
            view,
            records);
        });
      });
    }

    /// <summary>
    /// Area chart editor
    /// </summary>
    /// <returns></returns>
    public async Task OnBalanceChart()
    {
      await OnChart<BalanceEditor>(async (caption, response, options) =>
      {
        var responseData = response as BalanceInputModel;

        await Group(caption, responseData.Group, options, async (view, records) =>
        {
          await ShowBalance(
            responseData.ExpressionUp,
            responseData.ExpressionDown,
            responseData.Price,
            view,
            records);
        });
      });
    }

    /// <summary>
    /// Map chart editor
    /// </summary>
    /// <returns></returns>
    public async Task OnMapChart()
    {
      await OnChart<MapEditor>(async (caption, response, options) =>
      {
        var responseData = response as MapInputModel;

        await Group(caption, responseData.Group, options, async (view, records) =>
        {
          await ShowMaps(
            responseData.Expression,
            view,
            records);
        });
      });
    }

    /// <summary>
    /// Group by expirations 
    /// </summary>
    /// <param name="caption"></param>
    /// <param name="combine"></param>
    /// <param name="options"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task Group(
      string caption,
      string combine,
      IList<InstrumentModel> options,
      Func<CanvasView, IList<InstrumentModel>, Task> action)
    {
      if (Groups.ContainsKey(caption) is false)
      {
        Groups[caption] = new Dictionary<string, CanvasView> { [caption] = null };
        await InvokeAsync(StateHasChanged);
      }

      if (string.Equals(combine, "Yes"))
      {
        await Task.WhenAll(Groups[caption].Select(async o => await action(o.Value, options)));
        return;
      }

      var groups = options
        .GroupBy(o => $"{o.Derivative.ExpirationDate}", o => o)
        .ToDictionary(o => o.Key, o => o.ToList());

      Groups[caption] = Enumerable
        .Range(0, groups.Count)
        .ToDictionary(o => groups.Keys.ElementAt(o), o => null as CanvasView);

      await InvokeAsync(StateHasChanged);
      await Task.WhenAll(Groups[caption].Select(async o => await action(o.Value, groups[o.Key])));
    }

    /// <summary>
    /// API query to get options by criteria
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    protected async Task OnChart<T>(Func<string, dynamic, IList<InstrumentModel>, Task> action) where T : ComponentBase
    {
      var props = new DialogOptions
      {
        FullWidth = true,
        MaxWidth = MaxWidth.ExtraSmall,
        CloseOnEscapeKey = true
      };

      var response = await ModalService
        .ShowAsync<T>("Editor", props)
        .ContinueWith(async process =>
        {
          IsLoading = true;

          var popup = await process;
          var response = await popup.Result;

          if (response.Canceled is false)
          {
            dynamic data = response.Data;

            var adapter = new Adapter
            {
              Account = new Account
              {
                Descriptor = Configuration.GetValue<string>("Schwab:Account")
              },
              AccessToken = Configuration["Schwab:AccessToken"],
              RefreshToken = Configuration["Schwab:RefreshToken"],
              ClientId = Configuration["Schwab:ConsumerKey"],
              ClientSecret = Configuration["Schwab:ConsumerSecret"]
            };

            var caption = $"{Count++} : {data.Name} : {data.Range.Start:yyyy-MM-dd} : {data.Range.End:yyyy-MM-dd}";

            if (DataService.Options.TryGetValue($"{data.Name}", out var items) && items is not null)
            {
              var rangeItems = items.Where(o =>
              {
                var min = o.Derivative.ExpirationDate.Value.Date >= data.Range.Start.Date;
                var max = o.Derivative.ExpirationDate.Value.Date <= data.Range.End.Date;

                return min && max;

              }).ToList();

              await action(caption, data, rangeItems);
            }
            else
            {
              await adapter.Connect();

              var options = await adapter.GetOptions(new OptionScreenerModel(), new Hashtable
              {
                ["strikeCount"] = 100,
                ["symbol"] = data.Name?.ToUpper(),
                ["fromDate"] = $"{data.Range.Start:yyyy-MM-dd}",
                ["toDate"] = $"{data.Range.End:yyyy-MM-dd}"
              });

              await action(caption, data, options.Data);

              DataService.Assets.Add(data.Name);
            }
          }

          IsLoading = false;

          await InvokeAsync(StateHasChanged);
        });
    }

    /// <summary>
    /// Show bar charts
    /// </summary>
    /// <param name="expressionUp"></param>
    /// <param name="expressionDown"></param>
    /// <param name="view"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    protected async Task ShowBars(
      string expressionUp,
      string expressionDown,
      CanvasView view,
      IList<InstrumentModel> options)
    {
      try
      {
        var groups = options
          .OrderBy(o => o.Derivative.Strike)
          .GroupBy(o => o.Derivative.Strike, o => o)
          .ToList();

        var comUp = new ComponentModel { Color = SKColors.DeepSkyBlue };
        var comDown = new ComponentModel { Color = SKColors.OrangeRed };
        var points = groups.Select((group, i) =>
        {
          var ups = Compute(expressionUp, [.. group]);
          var downs = -Compute(expressionDown, [.. group]);

          return new Shape
          {
            X = i,
            Groups = new Dictionary<string, IShape>
            {
              ["Indicators"] = new Shape
              {
                Groups = new Dictionary<string, IShape>
                {
                  ["Ups"] = new BarShape { Y = ups, Component = comUp },
                  ["Downs"] = new BarShape { Y = downs, Component = comDown }
                }
              }
            }
          } as IShape;

        }).ToList();

        string showIndex(double o)
        {
          var index = Math.Max(Math.Min((int)Math.Round(o), groups.Count - 1), 0);
          var group = groups.ElementAtOrDefault(index) ?? default;
          var price = group?.ElementAtOrDefault(0)?.Derivative?.Strike;

          return price is null ? null : $"{price}";
        }

        var composer = new GroupComposer
        {
          Name = "Indicators",
          Items = points,
          ShowIndex = showIndex,
          View = view
        };

        await view.Create<CanvasEngine>(() => composer);
        await view.Update();
      }
      catch (Exception e)
      {
        Snackbar.Add("Invalid expression: " + e.Message);
      }
    }

    /// <summary>
    /// Show balance charts
    /// </summary>
    /// <param name="expressionUp"></param>
    /// <param name="expressionDown"></param>
    /// <param name="view"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    protected async Task ShowBalance(
      string expressionUp,
      string expressionDown,
      double price,
      CanvasView view,
      IList<InstrumentModel> options)
    {
      try
      {
        if (price is 0)
        {
          price = options.FirstOrDefault()?.Basis?.Point?.Last ?? 0;
        }

        var groups = options
          .OrderBy(o => o.Derivative.Strike)
          .GroupBy(o => o.Derivative.Strike, o => o)
          .ToList();

        var index = groups
          .Select((o, i) => new { Index = i, Data = o })
          .FirstOrDefault(o => o.Data.Key >= price)
          ?.Index ?? 0;

        if (index is 0)
        {
          index = groups.Count / 2;
        }

        var indexUp = index;
        var indexDown = index;
        var sumUp = 0.0;
        var sumDown = 0.0;
        var points = new IShape[groups.Count];
        var comUp = new ComponentModel { Color = SKColors.DeepSkyBlue };
        var comDown = new ComponentModel { Color = SKColors.OrangeRed };

        for (var i = 0; i < groups.Count; i++)
        {
          if (indexUp < groups.Count)
          {
            var sum = Compute(expressionUp, [.. groups.ElementAtOrDefault(indexUp)]);

            sumUp += sum;
            points[indexUp] = new Shape();
            points[indexUp].Groups = new Dictionary<string, IShape>();
            points[indexUp].Groups["Indicators"] = new Shape();
            points[indexUp].Groups["Indicators"].Groups = new Dictionary<string, IShape>();
            points[indexUp].Groups["Indicators"].Groups["Ups"] = new AreaShape { Y = sumUp, Component = comUp };
          }

          if (indexDown >= 0)
          {
            var sum = Compute(expressionDown, [.. groups.ElementAtOrDefault(indexDown)]);

            sumDown += sum;
            points[indexDown] = new Shape();
            points[indexDown].Groups = new Dictionary<string, IShape>();
            points[indexDown].Groups["Indicators"] = new Shape();
            points[indexDown].Groups["Indicators"].Groups = new Dictionary<string, IShape>();
            points[indexDown].Groups["Indicators"].Groups["Downs"] = new AreaShape { Y = sumDown, Component = comDown };
          }

          indexUp++;
          indexDown--;
        }

        string showIndex(double o)
        {
          var index = Math.Max(Math.Min((int)Math.Round(o), groups.Count - 1), 0);
          var group = groups.ElementAtOrDefault(index) ?? default;
          var price = group.ElementAtOrDefault(0)?.Derivative?.Strike;

          return price is null ? null : $"{price}";
        }

        var composer = new GroupComposer
        {
          Name = "Indicators",
          Items = points,
          ShowIndex = showIndex,
          View = view
        };

        await view.Create<CanvasEngine>(() => composer);
        await view.Update();
      }
      catch (Exception e)
      {
        Snackbar.Add("Invalid expression: " + e.Message);
      }
    }

    /// <summary>
    /// Show map chart
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="view"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    protected async Task ShowMaps(string expression, CanvasView view, IList<InstrumentModel> options)
    {
      try
      {
        var groups = options
          .OrderBy(o => o.Derivative.Strike)
          .GroupBy(o => o.Derivative.Strike, o => o)
          .ToDictionary(o => o.Key.Value, o => o
            .OrderBy(option => option.Derivative.ExpirationDate)
            .GroupBy(option => option.Derivative.ExpirationDate)
            .ToDictionary(group => group.Key.Value,
             group => Compute(expression, [.. group])));

        var min = groups.Min(o => o.Value.Values.Min());
        var max = groups.Max(o => o.Value.Values.Max());
        var colorService = new ColorService { Min = min, Max = max, Mode = ShadeEnum.Intensity };
        var expirationMap = new Dictionary<string, DateTime>();
        var points = groups.Select(group =>
        {
          return new ColorMapShape
          {
            Points = group.Value.Select(date =>
            {
              expirationMap[$"{date.Key}"] = date.Key;

              return new ComponentModel
              {
                Size = date.Value,
                Color = colorService.GetColor(date.Value)
              } as ComponentModel?;

            }).ToList()
          };
        }).ToArray();

        var expirations = expirationMap
          .Values
          .OrderBy(o => o)
          .Select(o => $"{o:yyyy-MM-dd}")
          .ToList();

        string showIndex(double o)
        {
          var index = Math.Min(Math.Max((int)o, 0), points.Length - 1);
          var name = groups.Keys.ElementAtOrDefault(index);

          return $"{name}";
        }

        string showValue(double o)
        {
          var index = Math.Min(Math.Max((int)o, 0), expirations.Count - 1);
          var caption = expirations.ElementAtOrDefault(index);

          return $"{caption}";
        }

        var composer = new MapComposer
        {
          Name = "Indicators",
          Items = points,
          Range = points.Max(o => o.Points.Count),
          ValueCount = Math.Min(expirations.Count, 5),
          ShowIndex = showIndex,
          ShowValue = showValue,
          View = view
        };

        await view.Create<CanvasEngine>(() => composer);
        await view.Update();
      }
      catch (Exception e)
      {
        Snackbar.Add("Invalid expression: " + e.Message);
      }
    }

    /// <summary>
    /// Evaluate expression for the option
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private double Compute(string expression, IList<InstrumentModel> options)
    {
      var ins = new InstrumentModel();
      var vars = new Dictionary<string, double>();
      var engine = new CalcEngine.Core.CalculationEngine();

      void compute(string side, InstrumentModel option)
      {
        var x = option.Point;
        var o = option.Derivative;
        var v = o.Variance;

        vars[$"{side}{nameof(v.Vega)}"] = vars.Get($"{side}{nameof(v.Vega)}") + v.Vega.Value;
        vars[$"{side}{nameof(v.Gamma)}"] = vars.Get($"{side}{nameof(v.Gamma)}") + v.Gamma.Value;
        vars[$"{side}{nameof(v.Theta)}"] = vars.Get($"{side}{nameof(v.Theta)}") + v.Theta.Value;
        vars[$"{side}{nameof(v.Delta)}"] = vars.Get($"{side}{nameof(v.Delta)}") + v.Delta.Value;
        vars[$"{side}{nameof(x.BidSize)}"] = vars.Get($"{side}{nameof(x.BidSize)}") + x.BidSize.Value;
        vars[$"{side}{nameof(x.AskSize)}"] = vars.Get($"{side}{nameof(x.AskSize)}") + x.AskSize.Value;
        vars[$"{side}{nameof(x.Volume)}"] = vars.Get($"{side}{nameof(x.Volume)}") + x.Volume.Value;
        vars[$"{side}{nameof(o.Sigma)}"] = vars.Get($"{side}{nameof(o.Sigma)}") + o.Sigma.Value;
        vars[$"{side}{nameof(o.OpenInterest)}"] = vars.Get($"{side}{nameof(o.OpenInterest)}") + o.OpenInterest.Value;
        vars[$"{side}{nameof(o.IntrinsicValue)}"] = vars.Get($"{side}{nameof(o.IntrinsicValue)}") + o.IntrinsicValue.Value;
      }

      try
      {
        foreach (var option in options)
        {
          compute("", option);

          switch (option.Derivative.Side)
          {
            case OptionSideEnum.Put: compute("P", option); break;
            case OptionSideEnum.Call: compute("C", option); break;
          }
        }

        foreach (var v in vars)
        {
          engine.Variables[v.Key] = v.Value;
        }

        return Convert.ToDouble(engine.Evaluate(expression));
      }
      catch (Exception e)
      {
        Snackbar.Add("Invalid expression: " + e.Message);
      }

      return 0;
    }
  }
}
