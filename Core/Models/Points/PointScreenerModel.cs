using System;
using Terminal.Core.Domains;

namespace Terminal.Core.Models
{
  public class  PointScreenerModel
  {
    /// <summary>
    /// Count
    /// </summary>
    public virtual int? Count { get; set; }

    /// <summary>
    /// Start date
    /// </summary>
    public virtual DateTime? MinDate { get; set; }

    /// <summary>
    /// End date
    /// </summary>
    public virtual DateTime? MaxDate { get; set; }

    /// <summary>
    /// Asset
    /// </summary>
    public virtual InstrumentModel Instrument { get; set; }
  }
}
