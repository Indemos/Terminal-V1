using System;
using Terminal.Core.Enums;

namespace Terminal.Records
{
  /// <summary>
  /// Template record
  /// </summary>
  public struct OrderRecord
  {
    public string Name { get; set; }
    public string Group { get; set; }
    public string Type { get; set; }
    public double Size { get; set; }
    public double Price { get; set; }
    public DateTime? Time { get; set; }
    public OrderSideEnum? Side { get; set; }
  }
}
