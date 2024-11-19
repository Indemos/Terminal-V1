using Derivative.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;

namespace Derivative.Pages.Popups
{
  public partial class BarEditor : ComponentBase
  {
    [CascadingParameter] MudDialogInstance Popup { get; set; }

    protected BarInputModel InputModel { get; set; } = new BarInputModel
    {
      Name = "SPY",
      Group = "Yes",
      Range = new(DateTime.Now.Date, DateTime.Now.Date),
      ExpressionUp = "(CVolume - COpenInterest) * CGamma",
      ExpressionDown = "(PVolume - POpenInterest) * PGamma"
    };

    protected EditContext InputContext { get; set; }
    protected override void OnInitialized() => InputContext = new(InputModel);
    protected void OnSuccess() => Popup.Close(DialogResult.Ok(InputModel));
    protected void OnClose() => Popup.Cancel();
  }
}
