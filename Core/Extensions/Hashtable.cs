using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace Terminal.Core.Extensions
{
  public static class HashtableExtensions
  {
    public static T Get<T>(this Hashtable input, string index)
    {
      return (T)(index is not null && input.ContainsKey(index) ? input[index] : default);
    }

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
  }
}
