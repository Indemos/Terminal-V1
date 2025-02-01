using System;
using System.Text.Json.Serialization;
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
    /// Implied volatility
    /// </summary>
    public virtual double? Sigma { get; set; }

    /// <summary>
    /// CALL or PUT
    /// </summary>
    public virtual OptionSideEnum? Side { get; set; }

    /// <summary>
    /// Expiration type
    /// </summary>
    public virtual ExpirationTypeEnum? ExpirationType { get; set; }

    /// <summary>
    /// Expiration date
    /// </summary>
    public virtual DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Last trading date
    /// </summary>
    public virtual DateTime? TradeDate { get; set; }

    /// <summary>
    /// Option metrics
    /// </summary>
    public virtual VarianceModel Variance { get; set; }

    /// <summary>
    /// Clone
    /// </summary>
    public virtual object Clone() => MemberwiseClone() as DerivativeModel;
  }
}
