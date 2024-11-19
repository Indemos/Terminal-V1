using System;
using System.ComponentModel.DataAnnotations;
using Terminal.Core.Enums;

namespace Derivative.Models
{
  public class OptionInputModel
  {
    [Required]
    public string Name { get; set; }

    [Required]
    public double Price { get; set; }

    [Required]
    public double Strike { get; set; }

    [Required]
    public double Premium { get; set; }

    [Required]
    public double Amount { get; set; }

    [Required]
    public DateTime? Date { get; set; }

    [Required]
    public OptionSideEnum Side { get; set; }

    [Required]
    public OrderSideEnum Position { get; set; }
  }
}
