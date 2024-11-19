using System;
using Terminal.Core.Domains;
using Terminal.Core.Enums;

namespace Terminal.Core.Models
{
  public class OptionScreenerModel
  {
    /// <summary>
    /// Strike count
    /// </summary>
    public virtual int? Count { get; set; }

    /// <summary>
    /// Min strike
    /// </summary>
    public virtual double? MinPrice { get; set; }

    /// <summary>
    /// Max strike
    /// </summary>
    public virtual double? MaxPrice { get; set; }

    /// <summary>
    /// Start date
    /// </summary>
    public virtual DateTime? MinDate { get; set; }

    /// <summary>
    /// End date
    /// </summary>
    public virtual DateTime? MaxDate { get; set; }

    /// <summary>
    /// Side
    /// </summary>
    public virtual OptionSideEnum? Side { get; set; }

    /// <summary>
    /// Current quote
    /// </summary>
    public virtual PointModel Point { get; set; }

    /// <summary>
    /// Instrument
    /// </summary>
    public virtual InstrumentModel Instrument { get; set; }
  }
}
