using System;
using Terminal.Core.Enums;

namespace Terminal.Models
{
  public class OptionInputModel
  {
    public double Price { get; set; }
    public double Strike { get; set; }
    public double Premium { get; set; }
    public double Amount { get; set; }
    public DateTime? Date { get; set; }
    public OptionSideEnum Side { get; set; }
    public OrderSideEnum Position { get; set; }
  }
}
