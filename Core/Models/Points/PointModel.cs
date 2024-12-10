using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using Terminal.Core.Collections;
using Terminal.Core.Domains;
using Terminal.Core.Extensions;

namespace Terminal.Core.Models
{
  public class PointModel : ICloneable, IGroup<PointModel>
  {
    /// <summary>
    /// Bid
    /// </summary>
    public virtual double? Bid { get; set; }

    /// <summary>
    /// Ask
    /// </summary>
    public virtual double? Ask { get; set; }

    /// <summary>
    /// Volume of the bid 
    /// </summary>
    public virtual double? BidSize { get; set; }

    /// <summary>
    /// Volume of the ask
    /// </summary>
    public virtual double? AskSize { get; set; }

    /// <summary>
    /// Last price or value
    /// </summary>
    public virtual double? Last { get; set; }

    /// <summary>
    /// Instrument volume
    /// </summary>
    public virtual double? Volume { get; set; }

    /// <summary>
    /// Time stamp
    /// </summary>
    public virtual DateTime? Time { get; set; }

    /// <summary>
    /// Reference to the complex data point
    /// </summary>
    public virtual BarModel Bar { get; set; }

    /// <summary>
    /// Depth of market
    /// </summary>
    public virtual DomModel Dom { get; set; }

    /// <summary>
    /// Reference to the instrument
    /// </summary>
    public virtual InstrumentModel Instrument { get; set; }

    /// <summary>
    /// Values from related series synced with the current data point, e.g. moving average or another indicator
    /// </summary>
    public virtual IDictionary<string, PointModel> Series { get; set; }

    /// <summary>
    /// List of option contracts for the current point
    /// </summary>
    public virtual IDictionary<string, IList<InstrumentModel>> Derivatives { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public PointModel()
    {
      Time = DateTime.Now;
      Series = new Dictionary<string, PointModel>();
      Derivatives = new Dictionary<string, IList<InstrumentModel>>();
    }

    /// <summary>
    /// Clone
    /// </summary>
    /// <returns></returns>
    public virtual object Clone()
    {
      var clone = MemberwiseClone() as PointModel;

      clone.Bar = Bar?.Clone() as BarModel;

      return clone;
    }

    /// <summary>
    /// Grouping index
    /// </summary>
    /// <returns></returns>
    public virtual long GetIndex()
    {
      if (Instrument.TimeFrame is not null)
      {
        return Time.Round(Instrument.TimeFrame).Value.Ticks;
      }

      return Time.Value.Ticks;
    }

    /// <summary>
    /// Grouping implementation
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public virtual PointModel Update(PointModel o)
    {
      var currentPrice = Last;
      var previousPrice = o?.Last;
      var price = (currentPrice ?? previousPrice).Value;

      Ask ??= o?.Ask ?? price;
      Bid ??= o?.Bid ?? price;
      AskSize += o?.AskSize ?? 0.0;
      BidSize += o?.BidSize ?? 0.0;
      Bar ??= new BarModel();
      Bar.Close = Last = price;
      Bar.Open = Bar.Open ?? o?.Bar?.Open ?? price;
      Bar.Low = Math.Min(Bar?.Low ?? price, o?.Bar?.Low ?? previousPrice ?? price);
      Bar.High = Math.Max(Bar?.High ?? price, o?.Bar?.High ?? previousPrice ?? price);
      Time = Time.Round(Instrument.TimeFrame) ?? o?.Time;

      Console.WriteLine(
        "### PREVIOUS ### : " +
        o?.Ask + " / " + o?.Bid + " : " + o?.Bar?.Open + " / " + o?.Bar?.High + " / " + o?.Bar?.Low + " / " + o?.Bar?.Close);

      Console.WriteLine(
        Instrument.Name + " : " +
        Ask  + " / " + Bid + " : " + Bar.Open + " / " + Bar.High + " / " + Bar.Low + " / " + Bar.Close);

      return this;
    }
  }
}
