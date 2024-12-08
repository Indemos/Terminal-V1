namespace Terminal.Core.Collections
{
  public interface IGroup<T>
  {
    /// <summary>
    /// Group index
    /// </summary>
    /// <returns></returns>
    long GetIndex();

    /// <summary>
    /// Grouping implementation
    /// </summary>
    /// <param name="previous"></param>
    /// <returns></returns>
    T Update(T previous);
  }
}
