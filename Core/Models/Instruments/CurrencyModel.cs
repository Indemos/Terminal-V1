using System;
using Terminal.Core.Enums;

namespace Terminal.Core.Models
{
  public class CurrencyModel : ICloneable
  {
    /// <summary>
    /// Currency
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Long swap rate for keeping position overnight
    /// </summary>
    public virtual double? SwapLong { get; set; }

    /// <summary>
    /// Short swap rate for keeping position overnight
    /// </summary>
    public virtual double? SwapShort { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public CurrencyModel()
    {
      SwapLong = 0;
      SwapShort = 0;
      Name = nameof(CurrencyEnum.USD);
    }

    /// <summary>
    /// Clone
    /// </summary>
    public virtual object Clone() => MemberwiseClone() as CurrencyModel;
  }
}
