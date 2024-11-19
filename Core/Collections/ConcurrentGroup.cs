using System;
using System.Collections.Concurrent;

namespace Terminal.Core.Collections
{
  public class ConcurrentGroup<T> : ConcurrentStack<T> where T : IGroup
  {
    /// <summary>
    /// Groups
    /// </summary>
    protected virtual ConcurrentDictionary<long, IGroup> Groups { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ConcurrentGroup()
    {
      Groups = [];
    }

    /// <summary>
    /// Grouping implementation
    /// </summary>
    /// <param name="item"></param>
    /// <param name="span"></param>
    public virtual void Add(IGroup item, TimeSpan? span)
    {
      var index = item.GetIndex();

      if (Groups.TryGetValue(index, out var current))
      {
        item.Update(current);
        return;
      }

      Groups[index] = item;

      Push((T)item.Update(null));
    }
  }
}
