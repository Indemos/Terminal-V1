using Canvas.Core.Composers;
using Canvas.Core.Engines;
using Canvas.Core.Models;
using Canvas.Core.Shapes;
using Canvas.Views.Web.Views;
using Derivative.Models;
using Derivative.Pages.Popups;
using Estimator.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Enums;

namespace Derivative.Pages
{
  public partial class Profile
  {
    [Inject] IDialogService ModalService { get; set; }

    protected int Count { get; set; } = 1;
    protected bool IsLoading { get; set; }
    protected Dictionary<double, double> Sums { get; set; } = [];
    protected Dictionary<string, Dictionary<string, CanvasView>> Groups { get; set; } = [];

    /// <summary>
    /// Clear
    /// </summary>
    public void OnClear()
    {
      Count = 1;
      Sums.Clear();
      Groups.Clear();
    }

    /// <summary>
    /// Bar chart editor
    /// </summary>
    /// <returns></returns>
    public async Task OnCombine()
    {
      await OnChart<OptionEditor>(async (caption, response) =>
      {
        if (Groups.ContainsKey(caption) is false)
        {
          Groups[caption] = new Dictionary<string, CanvasView> { [caption] = null };
          await InvokeAsync(StateHasChanged);
        }

        Groups[caption].ForEach(async o => await Show(o.Value, response));
      });
    }

    /// <summary>
    /// API query to get options by criteria
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    protected async Task OnChart<T>(Func<string, OptionInputModel, Task> action) where T : ComponentBase
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
            var inputModel = response.Data as OptionInputModel;
            var caption = $"{Count++} : {inputModel.Name}";

            await action(inputModel.Name, inputModel);
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
    protected async Task Show(CanvasView view, OptionInputModel inputModel)
    {
      var plusPercents = Enumerable.Range(0, 20).Select((o, i) => o / 2.0 / 100.0);
      var minusPercents = Enumerable.Range(1, 20).Select((o, i) => -o / 2.0 / 100.0).Reverse();
      var chartPoints = minusPercents.Concat(plusPercents).Select((o, i) =>
      {
        var step = inputModel.Price + inputModel.Price * o;
        var sum = GetEstimate(step, inputModel);

        Sums[o] = Sums.TryGetValue(o, out var s) ? s + sum : sum;

        return new LineShape
        {
          X = step,
          Y = Sums[o],
          Component = new ComponentModel { Size = 2, Color = SKColors.DeepSkyBlue }

        } as IShape;

      }).ToList();

      var composer = new Composer
      {
        Space = 0.05,
        Name = inputModel.Name,
        Items = chartPoints,
        View = view,
        ShowIndex = o => $"{chartPoints.ElementAtOrDefault((int)o)?.X}"
      };

      await view.Create<CanvasEngine>(() => composer);
      await view.Update();
    }

    /// <summary>
    /// Estimated PnL for shares or options
    /// </summary>
    /// <param name="price"></param>
    /// <param name="inputModel"></param>
    /// <returns></returns>
    protected double GetEstimate(double price, OptionInputModel inputModel)
    {
      var direction = inputModel.Position is OrderSideEnum.Buy ? 1.0 : -1.0;

      if (inputModel.Side is OptionSideEnum.Share)
      {
        return (price - inputModel.Price) * inputModel.Amount * direction;
      }

      var optionSide = Enum.GetName(inputModel.Side.GetType(), inputModel.Side);
      var days = Math.Max((DateTime.Now - inputModel.Date).Value.TotalDays / 250.0, double.Epsilon);
      var estimate = OptionService.Premium(optionSide, price, inputModel.Strike, days, 0.25, 0.05, 0);

      return (estimate - inputModel.Premium) * inputModel.Amount * direction * 100;
    }
  }
}
