using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Enums;

namespace Terminal.Core.Models
{
  public class OrderModel : ICloneable
  {
    /// <summary>
    /// Name
    /// </summary>
    public virtual string Name => Transaction?.Instrument?.Name;

    /// <summary>
    /// Basis name
    /// </summary>
    public virtual string BasisName => Transaction?.Instrument?.Basis?.Name;

    /// <summary>
    /// Client order ID
    /// </summary>
    public virtual string Id { get; set; }

    /// <summary>
    /// Group
    /// </summary>
    public virtual string Descriptor { get; set; }

    /// <summary>
    /// Contract size
    /// </summary>
    public virtual double? Volume { get; set; }

    /// <summary>
    /// Current PnL
    /// </summary>
    public virtual double? Gain { get; set; }

    /// <summary>
    /// Min possible PnL in account's currency
    /// </summary>
    public virtual double? GainMin { get; set; }

    /// <summary>
    /// Max possible PnL in account's currency
    /// </summary>
    public virtual double? GainMax { get; set; }

    /// <summary>
    /// Price the makes order active, e.g. limit price for stop limit order
    /// </summary>
    public virtual double? ActivationPrice { get; set; }

    /// <summary>
    /// Desired price for the order to fill, e.g. stop price for stop order and limit price for limit order
    /// </summary>
    public virtual double? Price { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public virtual OrderTypeEnum? Type { get; set; }

    /// <summary>
    /// Side
    /// </summary>
    public virtual OrderSideEnum? Side { get; set; }

    /// <summary>
    /// Time in force
    /// </summary>
    public virtual OrderTimeSpanEnum? TimeSpan { get; set; }

    /// <summary>
    /// Custom order type
    /// </summary>
    public virtual InstructionEnum? Instruction { get; set; }

    /// <summary>
    /// Transaction
    /// </summary>
    public virtual TransactionModel Transaction { get; set; }

    /// <summary>
    /// List of related orders in the hierarchy
    /// </summary>
    public virtual IList<OrderModel> Orders { get; set; }

    /// <summary>
    /// Order events
    /// </summary>
    public virtual Action<MessageModel<OrderModel>> OrderStream { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public OrderModel()
    {
      Orders = [];
      OrderStream = o => { };
      Id = $"{Guid.NewGuid()}";
      Descriptor = $"{Guid.NewGuid()}";
    }

    /// <summary>
    /// Position direction
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public double? GetDirection()
    {
      switch (Side)
      {
        case OrderSideEnum.Buy: return 1;
        case OrderSideEnum.Sell: return -1;
      }

      return null;
    }

    /// <summary>
    /// Position direction
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public double? GetVolume()
    {
      var volume = Transaction?.Volume ?? 0;
      var sideVolume = Orders.Sum(o => o.Transaction?.Volume ?? 0);

      return volume + sideVolume;
    }

    /// <summary>
    /// Estimate open price for one of the instruments in the order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public virtual double? GetOpenEstimate()
    {
      var point = Transaction.Instrument.Point;

      if (point is not null)
      {
        switch (Side)
        {
          case OrderSideEnum.Buy: return point.Ask;
          case OrderSideEnum.Sell: return point.Bid;
        }
      }

      return null;
    }

    /// <summary>
    /// Estimate close price for one of the instruments in the order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public virtual double? GetCloseEstimate()
    {
      var point = Transaction.Instrument.Point;

      if (point is not null)
      {
        switch (Side)
        {
          case OrderSideEnum.Buy: return point.Bid;
          case OrderSideEnum.Sell: return point.Ask;
        }
      }

      return null;
    }

    /// <summary>
    /// Estimated PnL in points for one side of the order
    /// </summary>
    /// <param name="price"></param>
    /// <returns></returns>
    public double? GetPointsEstimate(double? price = null)
    {
      return ((price ?? GetCloseEstimate()) - Price) * GetDirection();
    }

    /// <summary>
    /// Estimated PnL in account's currency for one side of the order
    /// </summary>
    /// <param name="price"></param>
    /// <returns></returns>
    public double? GetGainEstimate(double? price = null)
    {
      var volume = Transaction.Volume;
      var instrument = Transaction.Instrument;
      var step = instrument.StepValue / instrument.StepSize;
      var estimate = volume * GetPointsEstimate(price) * step * instrument.Leverage - instrument.Commission;

      Gain = estimate ?? Gain ?? 0;
      GainMin = Math.Min(GainMin ?? 0, Gain.Value);
      GainMax = Math.Max(GainMax ?? 0, Gain.Value);

      return estimate;
    }

    /// <summary>
    /// Clone
    /// </summary>
    public virtual object Clone()
    {
      var clone = MemberwiseClone() as OrderModel;

      clone.Transaction = Transaction?.Clone() as TransactionModel;

      return clone;
    }
  }
}
