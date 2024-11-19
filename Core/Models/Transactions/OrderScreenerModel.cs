using System;
using System.Collections.Generic;

namespace Terminal.Core.Models
{
  public class OrderScreenerModel
  {
    /// <summary>
    /// Order status
    /// </summary>
    public virtual string Status { get; set; }

    /// <summary>
    /// Symbols
    /// </summary>
    public virtual IList<string> Names { get; set; }
  }
}
