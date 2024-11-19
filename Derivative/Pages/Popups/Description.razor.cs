using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Derivative.Pages.Popups
{
  public partial class Description : ComponentBase
  {
    [CascadingParameter] MudDialogInstance Popup { get; set; }

    protected void OnClose() => Popup.Cancel();
  }
}
