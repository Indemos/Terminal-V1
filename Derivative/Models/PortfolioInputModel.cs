using System.ComponentModel.DataAnnotations;

namespace Derivative.Models
{
  public class PortfolioInputModel
  {
    [Required]
    public double Count { get; set; }

    [Required]
    public double Slope { get; set; }

    [Required]
    public string Names { get; set; }

    [Required]
    public string Duration { get; set; }

    [Required]
    public string Resolution { get; set; }
  }
}
