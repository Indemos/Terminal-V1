using System;
using Terminal.Core.Enums;

namespace Terminal.Core.Models
{
  public class DerivativeModel : ICloneable
  {
    /// <summary>
    /// Strike price
    /// </summary>
    public virtual double? Strike { get; set; }

    /// <summary>
    /// Open interest
    /// </summary>
    public virtual double? OpenInterest { get; set; }

    /// <summary>
    /// Intrinsic value
    /// </summary>
    public virtual double? IntrinsicValue { get; set; }

    /// <summary>
    /// Volatility
    /// </summary>
    public virtual double? Volatility { get; set; }

    /// <summary>
    /// CALL or PUT
    /// </summary>
    public virtual OptionSideEnum? Side { get; set; }

    /// <summary>
    /// Expiration date
    /// </summary>
    public virtual DateTime? Expiration { get; set; }

    /// <summary>
    /// Option metrics
    /// </summary>
    public virtual VariableModel Variable { get; set; }

    /// <summary>
    /// Clone
    /// </summary>
    public virtual object Clone() => MemberwiseClone() as DerivativeModel;
  }
}
