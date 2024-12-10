using Canvas.Core.Composers;
using Canvas.Core.Engines;
using Canvas.Core.Models;
using Canvas.Core.Shapes;
using Canvas.Views.Web.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Extensions;
using Terminal.Core.Models;

namespace Terminal.Components
{
  public partial class ChartsComponent : IDisposable
  {
    /// <summary>
    /// Upside style
    /// </summary>
    protected virtual ComponentModel UpSide { get; set; }

    /// <summary>
    /// Downside style
    /// </summary>
    protected virtual ComponentModel DownSide { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    protected virtual IList<IShape> Shapes { get; set; } = [];

    /// <summary>
    /// Indices
    /// </summary>
    protected virtual IDictionary<long, IShape> Indices { get; set; } = new Dictionary<long, IShape>();

    /// <summary>
    /// Reference to view control
    /// </summary>
    public virtual CanvasGroupView View { get; set; }

    /// <summary>
    /// Labels
    /// </summary>
    public virtual IList<IComposer> Composers { get; set; } = [];

    /// <summary>
    /// Define chart model
    /// </summary>
    /// <param name="group"></param>
    public virtual async Task Create(params string[] areas)
    {
      UpSide = new ComponentModel { Color = SKColors.DeepSkyBlue };
      DownSide = new ComponentModel { Color = SKColors.OrangeRed };
      View.Item = new Shape { Groups = areas.ToDictionary(o => o, o => new Shape() as IShape) };

      Composers = await View.CreateViews<CanvasEngine>();

      await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Value to shape
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public virtual IShape GetShape<T>(double? value, SKColor? color = null) where T : IShape, new()
    {
      return GetShape<T>(new PointModel { Last = value }, color);
    }

    /// <summary>
    /// Point to shape
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public virtual IShape GetShape<T>(PointModel point, SKColor? color = null) where T : IShape, new()
    {
      if (typeof(T).Equals(typeof(CandleShape)))
      {
        return new CandleShape
        {
          L = point.Bar.Low,
          H = point.Bar.High,
          O = point.Bar.Open,
          C = point.Bar.Close,
          Component = point.Bar.Close > point.Bar.Open ? UpSide : DownSide
        };
      }

      return new T
      {
        Y = point.Last,
        Component = new ComponentModel { Color = color ?? SKColors.LimeGreen }
      };
    }

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="index"></param>
    /// <param name="inputs"></param>
    /// <param name="count"></param>
    public virtual void UpdateItems(long index, string area, string series, params IShape[] inputs)
    {
      foreach (var input in inputs)
      {
        if (Indices.TryGetValue(index, out IShape currentPoint) is false)
        {
          currentPoint = (Shapes.LastOrDefault()?.Clone() ?? new Shape()) as IShape;
          currentPoint.X = Shapes.Count;

          Shapes.Add(currentPoint);
          Indices[index] = currentPoint;
        }

        currentPoint.Groups[area] = currentPoint.Groups.Get(area) ?? new Shape();
        currentPoint.Groups[area].Groups[series] = input;
      }

      var domain = new DimensionModel
      {
        IndexDomain = [Shapes.Count - Math.Max(10, Shapes.Count), Shapes.Count]
      };

      View.Update(domain, Shapes);
    }

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="inputs"></param>
    public virtual void UpdateOrdinals(IList<IShape> shapes)
    {
      var domain = new DimensionModel
      {
        IndexDomain = [0, shapes.Count]
      };

      View.Update(domain, shapes);
    }

    /// <summary>
    /// Clear points
    /// </summary>
    public virtual void Clear()
    {
      Shapes.Clear();
      Indices.Clear();
      View.Update(new DimensionModel { IndexDomain = [0, 0] }, Shapes);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public virtual void Dispose() => View?.Dispose();
  }
}
