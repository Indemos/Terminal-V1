using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Terminal.Core.Domains
{
  public interface IGateway : IDisposable
  {
    /// <summary>
    /// Account
    /// </summary>
    IAccount Account { get; set; }

    /// <summary>
    /// Point stream
    /// </summary>
    Action<MessageModel<PointModel>> PointStream { get; set; }

    /// <summary>
    /// Order stream
    /// </summary>
    Action<MessageModel<OrderModel>> OrderStream { get; set; }

    /// <summary>
    /// Connect
    /// </summary>
    Task<ResponseModel<StatusEnum>> Connect();

    /// <summary>
    /// Disconnect
    /// </summary>
    Task<ResponseModel<StatusEnum>> Disconnect();

    /// <summary>
    /// Subscribe
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    Task<ResponseModel<StatusEnum>> Subscribe(InstrumentModel instrument);

    /// <summary>
    /// Unsubscribe
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    Task<ResponseModel<StatusEnum>> Unsubscribe(InstrumentModel instrument);

    /// <summary>
    /// Get account state
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    Task<ResponseModel<IAccount>> GetAccount(Hashtable criteria);

    /// <summary>
    /// Get latest quote
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    Task<ResponseModel<DomModel>> GetDom(PointScreenerModel screener, Hashtable criteria);

    /// <summary>
    /// Get historical bars
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    Task<ResponseModel<IList<PointModel>>> GetPoints(PointScreenerModel screener, Hashtable criteria);

    /// <summary>
    /// Get options
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    Task<ResponseModel<IList<InstrumentModel>>> GetOptions(OptionScreenerModel screener, Hashtable criteria);

    /// <summary>
    /// Get positions
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    Task<ResponseModel<IList<OrderModel>>> GetPositions(PositionScreenerModel screener, Hashtable criteria);

    /// <summary>
    /// Get orders
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    Task<ResponseModel<IList<OrderModel>>> GetOrders(OrderScreenerModel screener, Hashtable criteria);

    /// <summary>
    /// Send new orders
    /// </summary>
    /// <param name="orders"></param>
    Task<ResponseModel<IList<OrderModel>>> CreateOrders(params OrderModel[] orders);

    /// <summary>
    /// Cancel orders
    /// </summary>
    /// <param name="orders"></param>
    Task<ResponseModel<IList<OrderModel>>> DeleteOrders(params OrderModel[] orders);
  }

  /// <summary>
  /// Implementation
  /// </summary>
  public abstract class Gateway : IGateway
  {
    /// <summary>
    /// Account
    /// </summary>
    public virtual IAccount Account { get; set; }

    /// <summary>
    /// Point stream
    /// </summary>
    public virtual Action<MessageModel<PointModel>> PointStream { get; set; }

    /// <summary>
    /// Order stream
    /// </summary>
    public virtual Action<MessageModel<OrderModel>> OrderStream { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Gateway()
    {
      PointStream = o => { };
      OrderStream = o => { };
    }

    /// <summary>
    /// Connect
    /// </summary>
    public abstract Task<ResponseModel<StatusEnum>> Connect();

    /// <summary>
    /// Disconnect
    /// </summary>
    /// <returns></returns>
    public abstract Task<ResponseModel<StatusEnum>> Disconnect();

    /// <summary>
    /// Subscribe
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<StatusEnum>> Subscribe(InstrumentModel instrument);

    /// <summary>
    /// Unsubscribe
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<StatusEnum>> Unsubscribe(InstrumentModel instrument);

    /// <summary>
    /// Get account state
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<IAccount>> GetAccount(Hashtable criteria);

    /// <summary>
    /// Get latest quote
    /// </summary>
    /// <param name="args"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<DomModel>> GetDom(PointScreenerModel args, Hashtable criteria);

    /// <summary>
    /// Get historical bars
    /// </summary>
    /// <param name="args"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<IList<PointModel>>> GetPoints(PointScreenerModel args, Hashtable criteria);

    /// <summary>
    /// Get options
    /// </summary>
    /// <param name="args"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<IList<InstrumentModel>>> GetOptions(OptionScreenerModel args, Hashtable criteria);

    /// <summary>
    /// Get positions
    /// </summary>
    /// <param name="args"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<IList<OrderModel>>> GetPositions(PositionScreenerModel args, Hashtable criteria);

    /// <summary>
    /// Get orders
    /// </summary>
    /// <param name="args"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public abstract Task<ResponseModel<IList<OrderModel>>> GetOrders(OrderScreenerModel args, Hashtable criteria);

    /// <summary>
    /// Send new orders
    /// </summary>
    /// <param name="orders"></param>
    public abstract Task<ResponseModel<IList<OrderModel>>> CreateOrders(params OrderModel[] orders);

    /// <summary>
    /// Cancel orders
    /// </summary>
    /// <param name="orders"></param>
    public abstract Task<ResponseModel<IList<OrderModel>>> DeleteOrders(params OrderModel[] orders);

    /// <summary>
    /// Dispose
    /// </summary>
    public virtual void Dispose() => Disconnect();

    /// <summary>
    /// Update points
    /// </summary>
    /// <param name="point"></param>
    protected virtual IList<IAccount> SetupAccounts(params IAccount[] accounts)
    {
      foreach (var account in accounts)
      {
        account.InitialBalance = account.Balance;
      }

      return accounts;
    }

    /// <summary>
    /// Create separate orders when combo-orders are not supported
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual IList<OrderModel> ComposeOrders(OrderModel order)
    {
      OrderModel merge(OrderModel subOrder, OrderModel group)
      {
        var nextOrder = subOrder.Clone() as OrderModel;
        var groupOrders = group
          ?.Orders
          ?.Where(o => o.Instruction is InstructionEnum.Brace)
          ?.Where(o => Equals(o.Name, nextOrder.Name)) ?? [];

        nextOrder.Price ??= nextOrder.GetOpenEstimate();
        nextOrder.Type ??= group.Type ?? OrderTypeEnum.Market;
        nextOrder.TimeSpan ??= group.TimeSpan ?? OrderTimeSpanEnum.Gtc;
        nextOrder.Instruction ??= InstructionEnum.Side;
        nextOrder.Transaction.Price ??= nextOrder.Price;
        nextOrder.Transaction.Time ??= DateTime.Now;
        nextOrder.Transaction.CurrentVolume = nextOrder.Transaction.Volume;
        nextOrder.Descriptor = group.Descriptor;
        nextOrder.Orders = [.. nextOrder.Orders, .. groupOrders];

        return nextOrder;
      }

      var nextOrders = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Side)
        .Select(o => merge(o, order))
        .ToList();

      if (order.Transaction is not null)
      {
        nextOrders.Add(merge(order, order));
      }

      order.Orders.Clear();

      return nextOrders;
    }
  }
}
