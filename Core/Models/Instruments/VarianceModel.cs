using System;

namespace Terminal.Core.Models
{
  public class VarianceModel : ICloneable
  {
    /// <summary>
    /// Delta
    /// </summary>
    public virtual double? Delta { get; set; }

    /// <summary>
    /// Gamma
    /// </summary>
    public virtual double? Gamma { get; set; }

    /// <summary>
    /// Rho
    /// </summary>
    public virtual double? Rho { get; set; }

    /// <summary>
    /// Theta
    /// </summary>
    public virtual double? Theta { get; set; }

    /// <summary>
    /// Vega
    /// </summary>
    public virtual double? Vega { get; set; }

    /// <summary>
    /// Clone
    /// </summary>
    public virtual object Clone() => MemberwiseClone() as VarianceModel;
  }
}
