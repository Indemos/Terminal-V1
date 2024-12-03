using Canvas.Core.Composers;
using Canvas.Core.Engines;
using Canvas.Core.Shapes;
using Canvas.Views.Web.Views;
using Derivative.Models;
using Derivative.Pages.Popups;
using MathNet.Numerics;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using Schwab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using static alglib;

namespace Derivative.Pages
{
  public partial class Portfolio
  {
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] IDialogService ModalService { get; set; }
    [Inject] IConfiguration Configuration { get; set; }

    protected int Count { get; set; } = 1;
    protected bool IsLoading { get; set; }
    protected Dictionary<string, Dictionary<string, CanvasView>> Groups { get; set; } = [];

    /// <summary>
    /// Clear
    /// </summary>
    public void OnClear()
    {
      Count = 1;
      Groups.Clear();
    }

    /// <summary>
    /// Bar chart editor
    /// </summary>
    /// <returns></returns>
    public async Task OnCombine()
    {
      await OnChart<PortfolioEditor>(async (caption, response, items) =>
      {
        if (Groups.ContainsKey(caption) is false)
        {
          Groups[caption] = new Dictionary<string, CanvasView> { [caption] = null };
          await InvokeAsync(StateHasChanged);
        }

        await Task.WhenAll(Groups[caption].Select(async o => await Show(o.Value, response, items)));
      });
    }

    /// <summary>
    /// API query to get options by criteria
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    protected async Task OnChart<T>(Func<string, PortfolioInputModel, IDictionary<string, IList<PointModel>>, Task> action) where T : ComponentBase
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
            var data = response.Data as PortfolioInputModel;
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

            var caption = $"{Count++} : {data.Names} : {data.Duration} : {data.Resolution}";

            await adapter.Connect();

            var names = data.Names.Split(",");
            var points = new Dictionary<string, IList<PointModel>>();

            foreach (var name in names)
            {
              var assetPoints = await adapter.GetPoints(new PointScreenerModel(), new Hashtable
              {
                ["period"] = 1,
                ["frequency"] = 1,
                ["symbol"] = name,
                ["periodType"] = data.Duration.ToLower(),
                ["frequencyType"] = data.Resolution.ToLower()
              });

              points[name] = assetPoints
                .Data
                .Where(o => o.Last is not null)
                .TakeLast((int)data.Count)
                .ToList();
            }

            await action(caption, data, points);
          }

          IsLoading = false;

          await InvokeAsync(StateHasChanged);
        });
    }

    /// <summary>
    /// Show bar charts
    /// </summary>
    /// <param name="view"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    protected async Task Show(CanvasView view, PortfolioInputModel inputModel, IDictionary<string, IList<PointModel>> items)
    {
      var modelPoints = new List<double>();
      var comPoints = GetComparablePoints(items);
      var count = Math.Min(inputModel.Count, comPoints.Min(o => o.Value.Count));

      switch (true)
      {
        case true when inputModel.Slope is 0: modelPoints = GetSpread(count, 5, 1, 1, 0); break;
        case true when inputModel.Slope is not 0: modelPoints = GetSlope(count, inputModel.Slope); break;
      }

      var numbers = GetNonLinearWeights(comPoints, [.. modelPoints]);
      var chartPoints = modelPoints.Select((o, i) =>
      {
        var sum = 0.0;

        for (var ii = 0; ii < numbers.Count; ii++)
        {
          sum += numbers[ii] * comPoints.Values.ElementAt(ii).ElementAt(i).Last.Value;
        }

        return new BarShape { X = i, Y = sum } as IShape;

      }).ToList();

      var composer = new Composer
      {
        Name = "Demo",
        Items = chartPoints,
        View = view
      };

      await view.Create<CanvasEngine>(() => composer);
      await view.Update();
    }

    /// <summary>
    /// Linear weights
    /// </summary>
    /// <param name="points"></param>
    /// <param name="modelPoints"></param>
    /// <returns></returns>
    protected IList<double> GetLinearWeightsNums(IDictionary<string, IList<PointModel>> points, IList<double> modelPoints)
    {
      var matrix = points
        .Values
        .SelectMany(items => items.Select((item, index) => new { item, index }).ToArray())
        .GroupBy(o => o.index, o => o.item.Last.Value)
        .Select(o => o.ToArray())
        .ToArray();

      var numbers = Fit.MultiDim(matrix, [.. modelPoints]);

      return numbers;
    }

    /// <summary>
    /// Linear weights
    /// </summary>
    /// <param name="points"></param>
    /// <param name="modelPoints"></param>
    /// <returns></returns>
    protected IList<double> GetLinearWeights(IDictionary<string, IList<PointModel>> points, IList<double> modelPoints)
    {
      var matrix = new double[modelPoints.Count, 2];

      for (var i = 0; i < points.Keys.Count; i++)
      {
        for (var ii = 0; ii < modelPoints.Count; ii++)
        {
          matrix[ii, i] = points.Values.ElementAt(i).ElementAt(ii).Last.Value;
        }
      }

      lsfitlinear([.. modelPoints], matrix, out int info, out double[] weights, out lsfitreport rep);

      return weights;
    }

    /// <summary>
    /// Non-linear weights
    /// </summary>
    /// <param name="points"></param>
    /// <param name="modelPoints"></param>
    /// <returns></returns>
    protected IList<double> GetNonLinearWeights(IDictionary<string, IList<PointModel>> points, IList<double> modelPoints)
    {
      var count = modelPoints.Count;
      var matrix = new double[count, points.Keys.Count];

      for (var i = 0; i < points.Keys.Count; i++)
      {
        for (var ii = 0; ii < count; ii++)
        {
          matrix[ii, i] = points.Values.ElementAt(i).ElementAt(ii).Last.Value;
        }
      }

      static void modelFunction(double[] c, double[] x, ref double func, object obj)
      {
        func = c.Select((o, i) => c.ElementAtOrDefault(i) * x.ElementAtOrDefault(i)).Sum() + c.LastOrDefault();
      }

      double epsx = 0.000001;
      int maxits = 0;

      lsfitcreatef(matrix, [.. modelPoints], points.Select(o => 1.0).ToArray(), epsx, out lsfitstate state);
      lsfitsetcond(state, epsx, maxits);
      lsfitfit(state, modelFunction, null, null);
      lsfitresults(state, out int info, out var weights, out lsfitreport rep);

      return weights;
    }

    /// <summary>
    /// Genrate trend
    /// </summary>
    /// <param name="count"></param>
    /// <param name="slope"></param>
    /// <returns></returns>
    protected List<double> GetSlope(double count, double slope)
    {
      List<double> response = [];

      for (var i = 0; i < count; i++)
      {
        response.Add(response.ElementAtOrDefault(i - 1) + slope);
      }

      return response;
    }

    /// <summary>
    /// Generate sin wave
    /// </summary>
    /// <param name="count"></param>
    /// <param name="waves"></param>
    /// <param name="amplitude"></param>
    /// <param name="frequency"></param>
    /// <param name="phase"></param>
    /// <returns></returns>
    protected List<double> GetSpread(double count, double waves, double amplitude, double frequency, double phase)
    {
      List<double> response = [];

      var xMin = 0.0;
      var xMax = 2 * Math.PI * waves;
      var step = (xMax - xMin) / count;

      for (var i = xMin; i <= xMax; i += step)
      {
        response.Add(amplitude * Math.Sin(frequency * i + phase));
      }

      return response.Take((int)count).ToList();
    }

    /// <summary>
    /// Sync points by date
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    protected IDictionary<string, IList<PointModel>> GetComparablePoints(IDictionary<string, IList<PointModel>> points)
    {
      var count = points.Max(o => o.Value.Count);
      var indices = points.ToDictionary(o => o.Key, o => 0);
      var comPoints = new Dictionary<string, IList<PointModel>>();

      for (var i = 0; i < count; i++)
      {
        var stamp = points.Max(o => o.Value.ElementAtOrDefault(indices[o.Key])?.Time);

        foreach (var series in points)
        {
          var previous = points.Get(series.Key).ElementAtOrDefault(i - 1) ?? points.Get(series.Key).FirstOrDefault();
          var current = points.Get(series.Key).ElementAtOrDefault(i) ?? previous;

          comPoints[series.Key] = comPoints.Get(series.Key) ?? [];
          comPoints[series.Key].Add(current.Time <= stamp ? current : previous);
        }
      }

      return comPoints;
    }
  }
}
