using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terminal.Core.Collections
{
  public class ObservableGroup<T> : ObservableCollection<T> where T : IGroup
  {
    /// <summary>
    /// Groups
    /// </summary>
    protected virtual IDictionary<long, int> Groups { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ObservableGroup()
    {
      Groups = new Dictionary<long, int>();
    }

    /// <summary>
    /// Grouping implementation
    /// </summary>
    /// <param name="item"></param>
    /// <param name="span"></param>
    public virtual void Add(IGroup item, TimeSpan? span)
    {
      var index = item.GetIndex();

      if (Groups.TryGetValue(index, out var position))
      {
        this[position] = (T)item.Update(this[position]);
        return;
      }

      Groups[index] = Count;

      Add((T)item.Update(null));
    }
  }
}
