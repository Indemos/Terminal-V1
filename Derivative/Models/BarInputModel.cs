using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace Derivative.Models
{
  public class BarInputModel
  {
    public string Group { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public DateRange Range { get; set; }

    [Required]
    public string ExpressionUp { get; set; }

    [Required]
    public string ExpressionDown { get; set; }
  }
}
