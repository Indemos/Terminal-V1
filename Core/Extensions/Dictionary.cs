using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Terminal.Core.Extensions
{
  public static class DictionaryExtensions
  {
    /// <summary>
    /// Access by key
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="input"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static V Get<K, V>(this IDictionary<K, V> input, K index)
    {
      return index is not null && input.TryGetValue(index, out var value) ? value : default;
    }

    /// <summary>
    /// Concurrent dictionary
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public static ConcurrentDictionary<K, V> Concurrent<K, V>(this IDictionary<K, V> input)
    {
      return new ConcurrentDictionary<K, V>(input);
    }
  }
}
