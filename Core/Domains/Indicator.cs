using System.Collections.Generic;
using Terminal.Core.Models;

namespace Terminal.Core.Domains
{
  /// <summary>
  /// Definition
  /// </summary>
  public interface IIndicator<TInput, TOutput> where TInput : PointModel
  {
    /// <summary>
    /// Name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Point
    /// </summary>
    PointModel Point { get; set; }

    /// <summary>
    /// Calculate indicator values
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    TOutput Calculate(IList<TInput> collection);
  }

  /// <summary>
  /// Implementation
  /// </summary>
  public class Indicator<TInput, TOutput> : IIndicator<TInput, TOutput> where TInput : PointModel
  {
    /// <summary>
    /// Name
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Point
    /// </summary>
    public virtual PointModel Point { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Indicator() => Point = new();

    /// <summary>
    /// Calculate indicator values
    /// </summary>
    /// <param name="collection"></param>
    public virtual TOutput Calculate(IList<TInput> collection) => default;
  }
}
