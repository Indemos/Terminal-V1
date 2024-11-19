using MudBlazor;

namespace Derivative.Themes
{
  public class DashboardTheme : MudTheme
  {
    public DashboardTheme()
    {
      LayoutProperties = new LayoutProperties()
      {
        DefaultBorderRadius = "5px",
      };

      Typography = new Typography()
      {
        Default = new Default()
        {
          FontFamily = ["Segoe UI", "SegoeUI", "Montserrat", "Helvetica Neue", "Helvetica", "Arial", "sans-serif"],
          FontSize = "1rem",
          FontWeight = 300,
          LineHeight = 1.5,
          LetterSpacing = "normal"
        }
      };
      Shadows = new Shadow();
      ZIndex = new ZIndex();
    }
  }
}
