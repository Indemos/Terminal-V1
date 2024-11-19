using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Derivative.Services;

namespace Derivative
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.WebHost.UseStaticWebAssets();
      builder.Services.AddSingleton<SyncService>();
      builder.Services.AddRazorPages();
      builder.Services.AddServerSideBlazor();
      builder.Services.AddMudServices(o =>
      {
        o.SnackbarConfiguration.NewestOnTop = true;
        o.SnackbarConfiguration.ShowCloseIcon = true;
        o.SnackbarConfiguration.PreventDuplicates = true;
        o.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
        o.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
      });

      var app = builder.Build();

      app.UseStaticFiles();
      app.UseRouting();
      app.MapBlazorHub();
      app.MapFallbackToPage("/Host");
      app.Run();
    }
  }
}
