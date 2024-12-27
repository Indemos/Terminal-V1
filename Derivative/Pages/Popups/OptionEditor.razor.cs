using Derivative.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using Terminal.Core.Enums;

namespace Derivative.Pages.Popups
{
  public partial class OptionEditor : ComponentBase
  {
    [CascadingParameter] IMudDialogInstance Popup { get; set; }

    protected OptionInputModel InputModel { get; set; } = new OptionInputModel
    {
      Name = "SPY",
      Price = 200,
      Strike = 205,
      Premium = 1.25,
      Amount = 1,
      Date = DateTime.Now.Date,
      Side = OptionSideEnum.Call,
      Position = OrderSideEnum.Buy
    };

    protected EditContext InputContext { get; set; }
    protected override void OnInitialized() => InputContext = new(InputModel);
    protected void OnSuccess() => Popup.Close(DialogResult.Ok(InputModel));
    protected void OnClose() => Popup.Cancel();
  }
}
