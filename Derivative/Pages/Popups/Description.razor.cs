using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Derivative.Pages.Popups
{
  public partial class Description : ComponentBase
  {
    [CascadingParameter] IMudDialogInstance Popup { get; set; }

    protected void OnClose() => Popup.Cancel();
  }
}
