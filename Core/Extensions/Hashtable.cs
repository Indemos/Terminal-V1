using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace Terminal.Core.Extensions
{
  public static class HashtableExtensions
  {
    /// <summary>
    /// Merge maps
    /// </summary>
    /// <param name="source"></param>
    /// <param name="maps"></param>
    /// <returns></returns>
    public static NameValueCollection Merge(this Hashtable source, params Hashtable[] maps)
    {
      var response = HttpUtility.ParseQueryString(string.Empty);

      foreach (DictionaryEntry o in source ?? [])
      {
        response[$"{o.Key}"] = $"{o.Value}";
      }

      foreach (Hashtable map in maps ?? [])
      {
        foreach (DictionaryEntry o in map ?? [])
        {
          response[$"{o.Key}"] = $"{o.Value}";
        }
      }

      return response;
    }

    /// <summary>
    /// Convert to query
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static NameValueCollection Compact(this Hashtable source)
    {
      var response = HttpUtility.ParseQueryString(string.Empty);

      foreach (DictionaryEntry o in source ?? [])
      {
        response[$"{o.Key}"] = $"{o.Value}";
      }

      return response;
    }
  }
}
