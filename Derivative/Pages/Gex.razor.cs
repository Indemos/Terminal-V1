using Canvas.Core.Composers;
using Canvas.Core.Engines;
using Canvas.Core.Models;
using Canvas.Core.Shapes;
using Derivative.Models;
using Derivative.Pages.Popups;
using Distribution.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using Schwab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Derivative.Pages
{
  public partial class Gex
  {
    [Inject] ISnackbar Snackbar { get; set; }
    [Inject] IDialogService ModalService { get; set; }
    [Inject] IConfiguration Configuration { get; set; }

    protected int Count { get; set; } = 1;
    protected bool IsLoading { get; set; }
    protected Adapter Connector { get; set; }
    protected List<IDisposable> Subscriptions { get; set; } = [];
    protected Dictionary<string, Dictionary<string, SectionModel>> Groups { get; set; } = [];

    /// <summary>
    /// Page setup
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        Connector = new Adapter
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

        await Connector.Connect();
      }

      await base.OnAfterRenderAsync(setup);
    }

    /// <summary>
    /// Clear
    /// </summary>
    public void OnClear()
    {
      Subscriptions.ForEach(o => o.Dispose());
      Groups.Clear();
      Subscriptions.Clear();
    }

    /// <summary>
    /// Bar chart editor
    /// </summary>
    /// <returns></returns>
    public async Task OnSubscribe()
    {
      await OnChart<SubscriptionEditor>(async (caption, response) =>
      {
        Groups[caption] = new Dictionary<string, SectionModel> { [caption] = new SectionModel { Collection = [] } };
        await InvokeAsync(StateHasChanged);
        Groups[caption].ForEach(async o => await Subscribe(o.Value, response));
      });
    }

    /// <summary>
    /// API query to get options by criteria
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    protected async Task OnChart<T>(Func<string, MapInputModel, Task> action) where T : ComponentBase
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
            var data = response.Data as MapInputModel;
            var caption = $"{Count++} : {data.Name} : {data.Expression}";

            await action(caption, data);
          }

          IsLoading = false;

          await InvokeAsync(StateHasChanged);
        });
    }

    /// <summary>
    /// Show bar charts
    /// </summary>
    /// <param name="view"></param>
    /// <param name="inputModel"></param>
    /// <returns></returns>
    protected async Task Subscribe(SectionModel section, MapInputModel inputModel)
    {
      var modelPoints = new List<double>();
      var composer = section.View.Composer = new Composer
      {
        Items = [],
        Name = "Demo",
        View = section.View
      };

      await section.View.Create<CanvasEngine>(() => composer);
      await section.View.Update();

      var interval = new Timer();

      interval.Elapsed += async (o, e) => await InstanceService<ScheduleService>.Instance.Send(async () => await OnData(section, inputModel)).Task;
      interval.Interval = 5000;
      interval.Enabled = true;

      Subscriptions.Add(interval);
    }

    /// <summary>
    /// Timer
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    protected async Task OnData(SectionModel section, MapInputModel inputModel)
    {
      var items = section.Collection;
      var options = await Connector.GetOptions(new OptionScreenerModel(), new Hashtable
      {
        ["strikeCount"] = 50,
        ["symbol"] = inputModel.Name.ToUpper(),
        ["fromDate"] = $"{inputModel.Range.Start:yyyy-MM-dd}",
        ["toDate"] = $"{inputModel.Range.End:yyyy-MM-dd}"
      });

      var point = new PointModel
      {
        Last = options.Data.Sum(o => Compute(inputModel.Expression, o)),
        TimeFrame = TimeSpan.FromMinutes(1),
        Time = DateTime.Now
      };

      items.Add(point, point.TimeFrame);
      section.View.Composer.Items = items.Select((o, i) =>
      {
        var item = new CandleShape
        {
          X = i,
          Y = o.Last,
          L = o.Bar.Low,
          H = o.Bar.High,
          O = o.Bar.Open,
          C = o.Bar.Close

        } as IShape;

        return item;

      }).ToList();

      await section.View.Update(new DimensionModel { IndexDomain = [items.Count - 50, items.Count] });
    }

    /// <summary>
    /// Evaluate expression for the option
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="o"></param>
    /// <returns></returns>
    protected double Compute(string expression, InstrumentModel option)
    {
      var o = option.Derivative;
      var ce = new CalcEngine.Core.CalculationEngine();

      try
      {
        ce.Variables["Volume"] = option.Point.Volume;
        ce.Variables["Volatility"] = o.Volatility;
        ce.Variables["OpenInterest"] = o.OpenInterest;
        ce.Variables["IntrinsicValue"] = o.IntrinsicValue;
        ce.Variables["BidSize"] = option.Point.BidSize;
        ce.Variables["AskSize"] = option.Point.AskSize;
        ce.Variables["Vega"] = o.Variable.Vega;
        ce.Variables["Gamma"] = o.Variable.Gamma;
        ce.Variables["Theta"] = o.Variable.Theta;
        ce.Variables["Delta"] = o.Variable.Delta;

        if (o.Side is OptionSideEnum.Put)
        {
          ce.Variables["PVolume"] = option.Point.Volume;
          ce.Variables["PVolatility"] = o.Volatility;
          ce.Variables["POpenInterest"] = o.OpenInterest;
          ce.Variables["PIntrinsicValue"] = o.IntrinsicValue;
          ce.Variables["PBidSize"] = option.Point.BidSize;
          ce.Variables["PAskSize"] = option.Point.AskSize;
          ce.Variables["PVega"] = o.Variable.Vega;
          ce.Variables["PGamma"] = o.Variable.Gamma;
          ce.Variables["PTheta"] = o.Variable.Theta;
          ce.Variables["PDelta"] = o.Variable.Delta;

          ce.Variables["CVolume"] = 0.0;
          ce.Variables["CVolatility"] = 0.0;
          ce.Variables["COpenInterest"] = 0.0;
          ce.Variables["CIntrinsicValue"] = 0.0;
          ce.Variables["CBidSize"] = 0.0;
          ce.Variables["CAskSize"] = 0.0;
          ce.Variables["CVega"] = 0.0;
          ce.Variables["CGamma"] = 0.0;
          ce.Variables["CTheta"] = 0.0;
          ce.Variables["CDelta"] = 0.0;
        }

        if (o.Side is OptionSideEnum.Call)
        {
          ce.Variables["PVolume"] = 0.0;
          ce.Variables["PVolatility"] = 0.0;
          ce.Variables["POpenInterest"] = 0.0;
          ce.Variables["PIntrinsicValue"] = 0.0;
          ce.Variables["PBidSize"] = 0.0;
          ce.Variables["PAskSize"] = 0.0;
          ce.Variables["PVega"] = 0.0;
          ce.Variables["PGamma"] = 0.0;
          ce.Variables["PTheta"] = 0.0;
          ce.Variables["PDelta"] = 0.0;

          ce.Variables["CVolume"] = option.Point.Volume;
          ce.Variables["CVolatility"] = o.Volatility;
          ce.Variables["COpenInterest"] = o.OpenInterest;
          ce.Variables["CIntrinsicValue"] = o.IntrinsicValue;
          ce.Variables["CBidSize"] = option.Point.BidSize;
          ce.Variables["CAskSize"] = option.Point.AskSize;
          ce.Variables["CVega"] = o.Variable.Vega;
          ce.Variables["CGamma"] = o.Variable.Gamma;
          ce.Variables["CTheta"] = o.Variable.Theta;
          ce.Variables["CDelta"] = o.Variable.Delta;
        }

        return Convert.ToDouble(ce.Evaluate(expression));
      }
      catch (Exception e)
      {
        Snackbar.Add("Invalid expression: " + e.Message);
      }

      return 0;
    }
  }
}
