namespace Terminal.Core.Collections
{
  public interface IGroup
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
    IGroup Update(IGroup previous);
  }
}
