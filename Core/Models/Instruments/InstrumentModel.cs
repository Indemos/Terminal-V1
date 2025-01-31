using System;
using Terminal.Core.Collections;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Terminal.Core.Domains
{
  public class InstrumentModel : ICloneable
  {
    /// <summary>
    /// Basis name
    /// </summary>
    public virtual string BasisName => Basis?.Name;

    /// <summary>
    /// ID
    /// </summary>
    public virtual string Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Exchange
    /// </summary>
    public virtual string Exchange { get; set; }

    /// <summary>
    /// Commission
    /// </summary>
    public virtual double? Commission { get; set; }

    /// <summary>
    /// Contract size for 1 trading lot in currencies and futures
    /// </summary>
    public virtual double? ContractSize { get; set; }

    /// <summary>
    /// Contract size
    /// </summary>
    public virtual double? Leverage { get; set; }

    /// <summary>
    /// Tick size, i.e. minimum price change 
    /// </summary>
    public virtual double? StepSize { get; set; }

    /// <summary>
    /// Tick value, i.e. how much price change within one tick
    /// </summary>
    public virtual double? StepValue { get; set; }

    /// <summary>
    /// Aggregation period for the quotes
    /// </summary>
    public virtual TimeSpan? TimeFrame { get; set; }

    /// <summary>
    /// Instrument type
    /// </summary>
    public virtual InstrumentEnum? Type { get; set; }

    /// <summary>
    /// Current price data 
    /// </summary>
    public virtual PointModel Point { get; set; }

    /// <summary>
    /// Undelying symbol
    /// </summary>
    public virtual InstrumentModel Basis { get; set; }

    /// <summary>
    /// Base currency contract
    /// </summary>
    public virtual CurrencyModel Currency { get; set; }

    /// <summary>
    /// Options and futures specification
    /// </summary>
    public virtual DerivativeModel Derivative { get; set; }

    /// <summary>
    /// List of all ticks from the server
    /// </summary>
    public virtual ConcurrentGroup<PointModel> Points { get; set; }

    /// <summary>
    /// List of all ticks from the server aggregated into bars
    /// </summary>
    public virtual ConcurrentGroup<PointModel> PointGroups { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public InstrumentModel()
    {
      Leverage = 1;
      StepSize = 0.01;
      StepValue = 0.01;
      Commission = 0;
      ContractSize = 1;

      Points = [];
      PointGroups = [];
    }

    /// <summary>
    /// Clone
    /// </summary>
    public virtual object Clone() => MemberwiseClone() as InstrumentModel;
  }
}
