using System.Collections.Generic;

namespace Terminal.Core.Models
{
  public class DomModel
  {
    public virtual IList<PointModel> Asks { get; set; }
    public virtual IList<PointModel> Bids { get; set; }
  }
}
