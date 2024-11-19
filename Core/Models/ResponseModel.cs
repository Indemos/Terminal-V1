using System.Collections.Generic;

namespace Terminal.Core.Models
{
  public class ResponseModel<T>
  {
    /// <summary>
    /// Item
    /// </summary>
    public virtual string Cursor { get; set; }

    /// <summary>
    /// Item
    /// </summary>
    public virtual T Data { get; set; }

    /// <summary>
    /// Items
    /// </summary>
    public virtual IList<ErrorModel> Errors { get; set; } = [];
  }
}
