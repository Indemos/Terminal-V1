using Derivative.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Derivative.Pages.Popups
{
  public partial class PortfolioEditor : ComponentBase
  {
    [CascadingParameter] IMudDialogInstance Popup { get; set; }

    protected PortfolioInputModel InputModel { get; set; } = new PortfolioInputModel
    {
      Slope = 45,
      Count = 250,
      Duration = "Year",
      Resolution = "Daily",
      Names = "SPY,IVV,VOO",
    };

    protected EditContext InputContext { get; set; }
    protected override void OnInitialized() => InputContext = new(InputModel);
    protected void OnSuccess() => Popup.Close(DialogResult.Ok(InputModel));
    protected void OnClose() => Popup.Cancel();
  }
}
