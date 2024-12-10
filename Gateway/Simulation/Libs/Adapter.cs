using Distribution.Services;
using Distribution.Stream;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using Terminal.Core.Validators;

namespace Simulation
{
  public class Adapter : Gateway, IDisposable
  {
    /// <summary>
    /// HTTP service
    /// </summary>
    protected Service _sender;

    /// <summary>
    /// Disposable connections
    /// </summary>
    protected IList<IDisposable> _connections;

    /// <summary>
    /// Disposable subscriptions
    /// </summary>
    protected IDictionary<string, IDisposable> _subscriptions;

    /// <summary>
    /// Instrument streams
    /// </summary>
    protected IDictionary<string, IEnumerator<string>> _instruments;

    /// <summary>
    /// Simulation speed in milliseconds
    /// </summary>
    public virtual int Speed { get; set; }

    /// <summary>
    /// Location of the files with quotes
    /// </summary>
    public virtual string Source { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Adapter()
    {
      Speed = 1000;

      _connections = [];
      _subscriptions = new Dictionary<string, IDisposable>();
      _instruments = new Dictionary<string, IEnumerator<string>>();
      _sender = new Service();
    }

    /// <summary>
    /// Connect
    /// </summary>
    public override async Task<ResponseModel<StatusEnum>> Connect()
    {
      var response = new ResponseModel<StatusEnum>();

      await Disconnect();

      SetupAccounts(Account);

      _instruments = Account
        .Instruments
        .ToDictionary(
          o => o.Key,
          o => Directory
            .EnumerateFiles(Path.Combine(Source, o.Value.Name), "*", SearchOption.AllDirectories)
            .GetEnumerator());

      _instruments.ForEach(o => _connections.Add(o.Value));

      await Task.WhenAll(Account.Instruments.Values.Select(Subscribe));

      response.Data = StatusEnum.Success;

      return response;
    }

    /// <summary>
    /// Subscribe to streams
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<StatusEnum>> Subscribe(InstrumentModel instrument)
    {
      var response = new ResponseModel<StatusEnum>();

      await Unsubscribe(instrument);

      PointStream += OnPoint;

      var span = TimeSpan.FromMicroseconds(Speed);
      var points = new Dictionary<string, PointModel>();
      var scheduler = InstanceService<ScheduleService>.Instance;
      var interval = new Timer(span);

      interval.Enabled = true;
      interval.AutoReset = true;
      interval.Elapsed += (sender, e) => scheduler.Send(async () =>
      {
        var point = await GetState(_instruments, points);

        if (point is not null)
        {
          var instrument = Account.Instruments[point.Instrument.Name];

          if (instrument.Points.Count > 0)
          {
            instrument.Points.Last().Derivatives.Clear();
            instrument.PointGroups.Last().Derivatives.Clear();
          }

          point.Instrument = instrument;

          instrument.Point = point;
          instrument.Points.Add(point);
          instrument.PointGroups.Add(point, instrument.TimeFrame);

          PointStream(new MessageModel<PointModel>
          {
            Next = instrument.PointGroups.Last()
          });
        }
      });

      _subscriptions[instrument.Name] = interval;

      response.Data = StatusEnum.Success;

      return response;
    }

    /// <summary>
    /// Save state and dispose
    /// </summary>
    public override async Task<ResponseModel<StatusEnum>> Disconnect()
    {
      var response = new ResponseModel<StatusEnum>();

      await Task.WhenAll(Account.Instruments.Values.Select(Unsubscribe));

      _connections?.ForEach(o => o?.Dispose());
      _connections?.Clear();

      return response;
    }

    /// <summary>
    /// Unsubscribe from streams
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public override Task<ResponseModel<StatusEnum>> Unsubscribe(InstrumentModel instrument)
    {
      var response = new ResponseModel<StatusEnum>();

      PointStream -= OnPoint;

      if (_subscriptions.ContainsKey(instrument.Name))
      {
        _subscriptions[instrument.Name].Dispose();
        _subscriptions.Remove(instrument.Name);
      }

      return Task.FromResult(response);
    }

    /// <summary>
    /// Create order and depending on the account, send it to the processing queue
    /// </summary>
    /// <param name="orders"></param>
    public override Task<ResponseModel<IList<OrderModel>>> CreateOrders(params OrderModel[] orders)
    {
      var response = new ResponseModel<IList<OrderModel>> { Data = [] };
      var validator = InstanceService<OrderPriceValidator>.Instance;

      response.Errors = [.. orders.SelectMany(o => validator
        .Validate(o)
        .Errors
        .Select(error => new ErrorModel { ErrorMessage = error.ErrorMessage }))];

      if (response.Errors.Count > 0)
      {
        return Task.FromResult(response);
      }

      foreach (var order in orders)
      {
        var nextOrders = CreateGroup(order);

        foreach (var nextOrder in nextOrders)
        {
          switch (nextOrder.Type)
          {
            case OrderTypeEnum.Stop:
            case OrderTypeEnum.Limit:
            case OrderTypeEnum.StopLimit: SendPendingOrder(nextOrder); break;
            case OrderTypeEnum.Market: SendOrder(nextOrder); break;
          }

          response.Data.Add(nextOrder);
        }
      }

      return Task.FromResult(response);
    }

    /// <summary>
    /// Recursively cancel orders
    /// </summary>
    /// <param name="orders"></param>
    public override Task<ResponseModel<IList<OrderModel>>> DeleteOrders(params OrderModel[] orders)
    {
      var response = new ResponseModel<IList<OrderModel>>
      {
        Data = [.. orders.Select(order =>
        {
          if (Account.Orders.TryGetValue(order.Id, out var o))
          {
            Account.Orders.Remove(order.Id, out var item);
          }

          return o;
        })]
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// Process pending order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual OrderModel SendPendingOrder(OrderModel order)
    {
      var nextOrder = order.Clone() as OrderModel;

      nextOrder.Transaction.Id = order.Id;
      nextOrder.Transaction.Status = OrderStatusEnum.Pending;

      Account.Orders[nextOrder.Id] = nextOrder;

      return order;
    }

    /// <summary>
    /// Position opening logic 
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual OrderModel SendOrder(OrderModel order)
    {
      order.Transaction.Id = order.Id;

      if (Account.Positions.TryGetValue(order.Name, out var currentOrder))
      {
        var (nextOrder, previousOrder) = Equals(currentOrder.Side, order.Side) ?
          IncreaseSide(currentOrder, order) :
          DecreaseSide(currentOrder, order);

        Account.Positions.Remove(order.Name, out var item);

        if (previousOrder?.Transaction?.Volume.Is(0) is false)
        {
          Account.Deals.Add(previousOrder);
          Account.Balance += previousOrder.GetGainEstimate();
        }

        order = nextOrder;
      }

      if (order?.Transaction?.Volume.Is(0) is false)
      {
        Account.Positions[order.Name] = order;
      }

      return order;
    }

    /// <summary>
    /// Create position when there are no other positions
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual IList<OrderModel> CreateGroup(OrderModel order)
    {
      OrderModel updateOrder(OrderModel o, OrderModel group = null)
      {
        var nextOrder = o.Clone() as OrderModel;

        nextOrder.Price = nextOrder.GetOpenEstimate();
        nextOrder.Type ??= group?.Type ?? OrderTypeEnum.Market;
        nextOrder.TimeSpan ??= group?.TimeSpan ?? OrderTimeSpanEnum.Gtc;
        nextOrder.Instruction ??= InstructionEnum.Side;
        nextOrder.Transaction.Time ??= DateTime.Now;
        nextOrder.Transaction.Status = OrderStatusEnum.Filled;
        nextOrder.Transaction.CurrentVolume = nextOrder.Transaction.Volume;
        nextOrder.Transaction.Price = nextOrder.Price;

        return nextOrder;
      }

      var nextOrders = new List<OrderModel>();

      if (order.Transaction is not null)
      {
        nextOrders.Add(updateOrder(order));
      }

      foreach (var o in order.Orders)
      {
        o.Descriptor = order.Descriptor;

        switch (o.Instruction)
        {
          case InstructionEnum.Side: nextOrders.Add(updateOrder(o, order)); break;
          case InstructionEnum.Brace: SendPendingOrder(o); break;
        }
      }

      order.Orders.Clear();

      return nextOrders;
    }

    /// <summary>
    /// Compute aggregated position price
    /// </summary>
    /// <param name="orders"></param>
    /// <returns></returns>
    protected virtual double? GetGroupPrice(params OrderModel[] orders)
    {
      var numerator = orders.Sum(o => o.Transaction.CurrentVolume * o.Transaction.Price);
      var denominator = orders.Sum(o => o.Transaction.CurrentVolume);

      return numerator / denominator;
    }

    /// <summary>
    /// Increase size of the order
    /// </summary>
    /// <param name="order"></param>
    /// <param name="update"></param>
    /// <returns></returns>
    protected virtual (OrderModel, OrderModel) IncreaseSide(OrderModel order, OrderModel update)
    {
      var nextOrder = order.Clone() as OrderModel;
      var volume = nextOrder.Transaction.CurrentVolume + update.Transaction.Volume;

      nextOrder.Transaction.Id = update.Id;
      nextOrder.Transaction.Volume = volume;
      nextOrder.Transaction.CurrentVolume = volume;
      nextOrder.Transaction.Time = update.Transaction.Time;
      nextOrder.Transaction.Descriptor ??= update.Transaction.Descriptor;
      nextOrder.Transaction.Price = GetGroupPrice(nextOrder, update);
      nextOrder.Price = nextOrder.Transaction.Price;

      return (nextOrder, null);
    }

    /// <summary>
    /// Decrease size of the order
    /// </summary>
    /// <param name="nextOrder"></param>
    /// <param name="update"></param>
    /// <returns></returns>
    protected virtual (OrderModel, OrderModel) DecreaseSide(OrderModel order, OrderModel update)
    {
      var nextOrder = order.Clone() as OrderModel;
      var previousOrder = order.Clone() as OrderModel;
      var updateVolume = update.Transaction.Volume ?? 0;
      var previousVolume = nextOrder.Transaction.CurrentVolume ?? 0;
      var nextVolume = Math.Abs(previousVolume - updateVolume);

      nextOrder.Transaction.Id = update.Id;
      nextOrder.Transaction.Volume = nextVolume;
      nextOrder.Transaction.CurrentVolume = nextVolume;
      nextOrder.Transaction.Time = update.Transaction.Time;
      nextOrder.Transaction.Descriptor ??= update.Transaction.Descriptor;

      previousOrder.Transaction.Price = update.Price;

      switch (true)
      {
        case true when nextVolume.Is(0):
        case true when previousVolume > updateVolume:
          previousOrder.Transaction.Volume = updateVolume;
          previousOrder.Transaction.CurrentVolume = updateVolume;
          break;

        case true when previousVolume < updateVolume:
          nextOrder.Price = update.Price;
          nextOrder.Transaction.Price = update.Price;
          nextOrder.Side = nextOrder.Side is OrderSideEnum.Buy ? OrderSideEnum.Sell : OrderSideEnum.Buy;
          previousOrder.Transaction.Volume = previousVolume;
          previousOrder.Transaction.CurrentVolume = previousVolume;
          break;
      }

      return (nextVolume.Is(0) ? null : nextOrder, previousOrder);
    }

    /// <summary>
    /// Process pending orders on each quote
    /// </summary>
    /// <param name="message"></param>
    protected virtual void OnPoint(MessageModel<PointModel> message)
    {
      var estimates = Account
        .Positions
        .Select(o => o.Value.GetGainEstimate())
        .ToList();

      foreach (var order in Account.Orders.Values)
      {
        if (IsOrderExecutable(order))
        {
          SendOrder(order);
          Account.Orders = Account
            .Orders
            .Where(o => Equals(o.Value.Descriptor, order.Descriptor) is false)
            .ToDictionary(o => o.Key, o => o.Value)
            .Concurrent();
        }
      }
    }

    /// <summary>
    /// Check if pending order can be executed
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual bool IsOrderExecutable(OrderModel order)
    {
      var isExecutable = false;
      var point = order.Transaction.Instrument.Point;
      var isBuyStopLimit = order.Side is OrderSideEnum.Buy && order.Type is OrderTypeEnum.StopLimit && point.Ask >= order.ActivationPrice;
      var isSellStopLimit = order.Side is OrderSideEnum.Sell && order.Type is OrderTypeEnum.StopLimit && point.Bid <= order.ActivationPrice;

      order.Type = isBuyStopLimit || isSellStopLimit ? OrderTypeEnum.Limit : order.Type;

      var isBuyStop = order.Side is OrderSideEnum.Buy && order.Type is OrderTypeEnum.Stop;
      var isSellStop = order.Side is OrderSideEnum.Sell && order.Type is OrderTypeEnum.Stop;
      var isBuyLimit = order.Side is OrderSideEnum.Buy && order.Type is OrderTypeEnum.Limit;
      var isSellLimit = order.Side is OrderSideEnum.Sell && order.Type is OrderTypeEnum.Limit;

      isExecutable = isBuyStop || isSellLimit ? point.Ask >= order.Price : isExecutable;
      isExecutable = isSellStop || isBuyLimit ? point.Bid <= order.Price : isExecutable;

      return isExecutable;
    }

    /// <summary>
    /// Get next available point
    /// </summary>
    /// <param name="streams"></param>
    /// <param name="points"></param>
    /// <returns></returns>
    protected virtual async Task<PointModel> GetState(IDictionary<string, IEnumerator<string>> streams, IDictionary<string, PointModel> points)
    {
      var index = string.Empty;

      foreach (var stream in streams)
      {
        points.TryGetValue(stream.Key, out PointModel point);

        if (point is null)
        {
          stream.Value.MoveNext();

          if (string.IsNullOrEmpty(stream.Value.Current) is false)
          {
            points[stream.Key] = await GetStateContent(stream.Key, stream.Value.Current);
          }
        }

        points.TryGetValue(index, out PointModel min);
        points.TryGetValue(stream.Key, out PointModel current);

        var isOne = string.IsNullOrEmpty(index);
        var isMin = current is not null && min is not null && current.Time <= min.Time;

        if (isOne || isMin)
        {
          index = stream.Key;
        }
      }

      var response = points[index];

      points[index] = null;

      return response;
    }

    /// <summary>
    /// Parse snapshot document to get current symbol and options state
    /// </summary>
    /// <param name="name"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    protected virtual async Task<PointModel> GetStateContent(string name, string source)
    {
      var document = new FileInfo(source);

      if (string.Equals(document.Extension, ".zip", StringComparison.InvariantCultureIgnoreCase))
      {
        using (var stream = File.OpenRead(source))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
        using (var content = archive.Entries.First().Open())
        {
          var optionMessage = await JsonSerializer.DeserializeAsync<PointModel>(content, _sender.Options);
          optionMessage.Instrument = new InstrumentModel { Name = name };
          return optionMessage;
        }
      }

      var inputMessage = File.ReadAllText(source);
      var pointMessage = JsonSerializer.Deserialize<PointModel>(inputMessage);

      pointMessage.Instrument = new InstrumentModel { Name = name };

      return pointMessage;
    }

    /// <summary>
    /// Get depth of market when available or just a top of the book
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<DomModel>> GetDom(PointScreenerModel screener, Hashtable criteria)
    {
      var point = Account.Instruments[screener.Instrument.Name].Points.LastOrDefault();
      var response = new ResponseModel<DomModel>
      {
        Data = new DomModel
        {
          Bids = [point],
          Asks = [point]
        }
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// List of points by criteria, e.g. for specified instrument
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IList<PointModel>>> GetPoints(PointScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<PointModel>>
      {
        Data = [.. Account.Instruments[screener.Instrument.Name].Points]
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// Option chain
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IList<InstrumentModel>>> GetOptions(OptionScreenerModel screener, Hashtable criteria)
    {
      var orders = Account
        .Positions
        .Values
        .SelectMany(o => o.Orders.Append(o))
        .GroupBy(o => o.Transaction.Instrument.Name)
        .ToDictionary(o => o.Key, o => o);

      var response = new ResponseModel<IList<InstrumentModel>>
      {
        Data = screener
        .Point
        .Derivatives[nameof(InstrumentEnum.Options)]
        .Select(o =>
        {
          if (orders.TryGetValue(o.Name, out var chain))
          {
            chain.ForEach(order => order.Transaction.Instrument = o);
          }

          return o;
        })
        .Where(o => screener?.Side is null || Equals(o.Derivative.Side, screener.Side))
        .Where(o => screener?.MinDate is null || o.Derivative.Expiration >= screener.MinDate)
        .Where(o => screener?.MaxDate is null || o.Derivative.Expiration <= screener.MaxDate)
        .Where(o => screener?.MinPrice is null || o.Derivative.Strike >= screener.MinPrice)
        .Where(o => screener?.MaxPrice is null || o.Derivative.Strike <= screener.MaxPrice)
        .ToList()
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// Load account data
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IAccount>> GetAccount(Hashtable criteria)
    {
      var response = new ResponseModel<IAccount>
      {
        Data = Account
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// Get all account positions
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IList<OrderModel>>> GetPositions(PositionScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<OrderModel>>
      {
        Data = [.. Account.Positions.Values]
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// Get all account orders
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IList<OrderModel>>> GetOrders(OrderScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<OrderModel>>
      {
        Data = [.. Account.Orders.Values]
      };

      return Task.FromResult(response);
    }
  }
}
