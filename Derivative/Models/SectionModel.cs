using Canvas.Views.Web.Views;
using Terminal.Core.Collections;
using Terminal.Core.Models;

namespace Derivative.Models
{
  public class SectionModel
  {
    public CanvasView View { get; set; }
    public ConcurrentGroup<PointModel> Collection { get; set; }
  }
}
