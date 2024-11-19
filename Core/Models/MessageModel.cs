using System;
using Terminal.Core.Enums;

namespace Terminal.Core.Models
{
  public class MessageModel<T>
  {
    /// <summary>
    /// Event type
    /// </summary>
    public virtual ActionEnum Action { get; set; }

    /// <summary>
    /// Current or next value to be set
    /// </summary>
    public virtual T Next { get; set; }

    /// <summary>
    /// Previous value
    /// </summary>
    public virtual T Previous { get; set; }

    /// <summary>
    /// Current or next value to be set
    /// </summary>
    public virtual string Message { get; set; }

    /// <summary>
    /// Previous value
    /// </summary>
    public virtual Exception Error { get; set; }
  }
}
