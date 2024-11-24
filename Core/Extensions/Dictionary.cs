using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Terminal.Core.Extensions
{
  public static class DictionaryExtensions
  {
    public static V Get<K, V>(this IDictionary<K, V> input, K index)
    {
      return index is not null && input.TryGetValue(index, out var value) ? value : default;
    }

    public static ConcurrentDictionary<K, V> Concurrent<K, V>(this IDictionary<K, V> input)
    {
      return new ConcurrentDictionary<K, V>(input);
    }
  }
}
