using Distribution.Services;
using IBApi;
using InteractiveBrokers.Enums;
using InteractiveBrokers.Mappers;
using InteractiveBrokers.Messages;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using Terminal.Core.Services;

namespace InteractiveBrokers
{
  public class Adapter : Gateway
  {
    /// <summary>
    /// Unique request ID
    /// </summary>
    protected int _counter;

    /// <summary>
    /// IB client
    /// </summary>
    protected IBClient _client;

    /// <summary>
    /// Disposable connections
    /// </summary>
    protected IList<IDisposable> _connections;

    /// <summary>
    /// Asset subscriptions
    /// </summary>
    protected ConcurrentDictionary<string, int> _subscriptions;

    /// <summary>
    /// Reference to the most recent tick for each symbol
    /// </summary>
    protected ConcurrentDictionary<int, PointModel> _points;

    /// <summary>
    /// Host
    /// </summary>
    public virtual string Host { get; set; }

    /// <summary>
    /// Port
    /// </summary>
    public virtual int Port { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Adapter()
    {
      Port = 7497;
      Host = "127.0.0.1";

      _counter = 1;
      _connections = [];
      _points = new ConcurrentDictionary<int, PointModel>();
      _subscriptions = new ConcurrentDictionary<string, int>();
    }

    /// <summary>
    /// Connect
    /// </summary>
    public override async Task<ResponseModel<StatusEnum>> Connect()
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        await Disconnect();
        await CreateReader();
        await CreateOrderSystem();

        SubscribeToErrors();
        SubscribeToOrders();
        SubscribeToConnections();

        await GetAccount([]);

        Account.Instruments.ForEach(async o => await Subscribe(o.Value));

        response.Data = StatusEnum.Success;
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

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

      try
      {
        await Unsubscribe(instrument);

        var id = _subscriptions[instrument.Name] = _counter++;
        var contract = new Contract
        {
          Symbol = instrument.Name,
          Exchange = instrument.Exchange,
          SecType = ExternalMap.GetInstrumentType(instrument.Type),
          LastTradeDateOrContractMonth = ExternalMap.GetExpiration(instrument?.Derivative?.Expiration),
          Currency = nameof(CurrencyEnum.USD)
        };

        void sendPoint(PointModel map)
        {
          if (map.Bid is null || map.Ask is null || map.BidSize is null || map.AskSize is null)
          {
            return;
          }

          var point = map.Clone() as PointModel;

          point.Time = DateTime.Now;
          point.TimeFrame = instrument.TimeFrame;

          instrument.Points.Add(point);
          instrument.PointGroups.Add(point, instrument.TimeFrame);

          PointStream(new MessageModel<PointModel>
          {
            Next = instrument.PointGroups.Last()
          });
        }

        _client.TickPrice += message =>
        {
          if (_points.TryGetValue(message.RequestId, out var map))
          {
            switch (ExternalMap.GetField(message.Field))
            {
              case FieldCodeEnum.BidSize: map.BidSize = message.Value; sendPoint(map); break;
              case FieldCodeEnum.AskSize: map.AskSize = message.Value; sendPoint(map); break;
              case FieldCodeEnum.BidPrice: map.Last = map.Bid = message.Value; sendPoint(map); break;
              case FieldCodeEnum.AskPrice: map.Last = map.Ask = message.Value; sendPoint(map); break;
            }
          }
        };

        _points[id] = new PointModel { Instrument = instrument };
        _client.ClientSocket.reqMktData(id, contract, string.Empty, false, false, null);

        response.Data = StatusEnum.Success;
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return response;
    }

    /// <summary>
    /// Save state and dispose
    /// </summary>
    public override Task<ResponseModel<StatusEnum>> Disconnect()
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        _client?.ClientSocket?.eDisconnect();
        _client?.Dispose();
        _connections?.ForEach(o => o?.Dispose());
        _connections?.Clear();

        response.Data = StatusEnum.Success;
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return Task.FromResult(response);
    }

    /// <summary>
    /// Unsubscribe from data streams
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public override Task<ResponseModel<StatusEnum>> Unsubscribe(InstrumentModel instrument)
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        if (_subscriptions.TryGetValue(instrument.Name, out var id))
        {
          _subscriptions.TryRemove(instrument.Name, out var o);
          _client.ClientSocket.cancelMktData(id);
        }

        response.Data = StatusEnum.Success;
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return Task.FromResult(response);
    }

    /// <summary>
    /// Get options
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IList<InstrumentModel>>> GetOptions(OptionScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<InstrumentModel>>();

      try
      {
        var id = _counter++;
        var instrument = screener.Instrument;
        var options = new List<InstrumentModel>().AsEnumerable();
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var contract = await GetContract(instrument);
        var exchange = instrument.Exchange;

        if (instrument.Type is InstrumentEnum.Futures)
        {
          exchange = "GLOBEX";
        }

        _client.SecurityDefinitionOptionParameter += message => options = options.Concat(InternalMap.GetOptions(message));
        _client.SecurityDefinitionOptionParameterEnd += message => source.TrySetResult();
        _client.ClientSocket.reqSecDefOptParams(
          id,
          contract.Symbol,
          exchange,
          contract.SecType,
          contract.ConId);

        await source.Task;

        response.Data = options
          .DistinctBy(o => new { o.Derivative.Strike, o.Derivative.Expiration })
          .ToList();
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Get latest quote
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<DomModel>> GetDom(PointScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<DomModel>();

      try
      {
        var point = (await GetPrice(screener, criteria)).Data;

        response.Data = new DomModel
        {
          Asks = [point],
          Bids = [point]
        };
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Get historical bars
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IList<PointModel>>> GetPoints(PointScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<PointModel>>();

      try
      {
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return Task.FromResult(response);
    }

    /// <summary>
    /// Send orders
    /// </summary>
    /// <param name="orders"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IList<OrderModel>>> CreateOrders(params OrderModel[] orders)
    {
      var response = new ResponseModel<IList<OrderModel>>();

      foreach (var order in orders)
      {
        response.Data.Add((await CreateOrder(order)).Data);
      }

      return response;
    }

    /// <summary>
    /// Cancel orders
    /// </summary>
    /// <param name="orders"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IList<OrderModel>>> DeleteOrders(params OrderModel[] orders)
    {
      var response = new ResponseModel<IList<OrderModel>>();

      foreach (var order in orders)
      {
        response.Data.Add((await DeleteOrder(order)).Data);
      }

      return response;
    }

    /// <summary>
    /// Get contract definition
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    protected virtual Task<Contract> GetContract(InstrumentModel instrument)
    {
      var id = _counter++;
      var source = new TaskCompletionSource<Contract>(TaskCreationOptions.RunContinuationsAsynchronously);
      var instrumentType = ExternalMap.GetInstrumentType(instrument.Type);

      if (instrument.Type is InstrumentEnum.Futures)
      {
        instrumentType = "IND";
      }

      var contract = new Contract
      {
        Symbol = instrument.Name,
        Exchange = instrument.Exchange,
        SecType = instrumentType,
        Currency = nameof(CurrencyEnum.USD)
      };

      _client.ContractDetails += message => contract = message.ContractDetails.Contract;
      _client.ContractDetailsEnd += message => source.TrySetResult(contract);
      _client.ClientSocket.reqContractDetails(id, contract);

      return source.Task;
    }

    /// <summary>
    /// Subscribe to account updates
    /// </summary>
    protected virtual void SubscribeToConnections()
    {
      _client.ConnectionClosed += () =>
      {
        var message = new MessageModel<string>
        {
          Message = "No connection",
          Action = ActionEnum.Disconnect
        };

        InstanceService<MessageService>.Instance.OnMessage(message);
      };
    }

    /// <summary>
    /// Subscribe errors
    /// </summary>
    protected virtual void SubscribeToErrors()
    {
      _client.Error += (id, code, message, error, e) =>
      {
        Console.WriteLine(id + " : " + code + " : " + message + " : " + error + " : " + e?.Message);
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      };
    }

    /// <summary>
    /// Subscribe orders
    /// </summary>
    protected virtual void SubscribeToOrders()
    {
      _client.OpenOrder += o =>
      {
        OrderStream(new MessageModel<OrderModel> { Next = InternalMap.GetOrder(o) });
      };

      _client.ClientSocket.reqAutoOpenOrders(true);
    }

    /// <summary>
    /// Limit actions to current account only
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="action"></param>
    protected virtual void ScreenAccount(string descriptor, Action action)
    {
      if (string.Equals(descriptor, Account.Descriptor, StringComparison.InvariantCultureIgnoreCase))
      {
        action();
      }
    }

    /// <summary>
    /// Sync open balance, order, and positions 
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IAccount>> GetAccount(Hashtable criteria)
    {
      var response = new ResponseModel<IAccount>();

      try
      {
        await GetAccountSummary(criteria);

        var orders = await GetOrders(null, criteria);
        var positions = await GetPositions(null, criteria);

        Account.Orders = orders.Data.GroupBy(o => o.Id).ToDictionary(o => o.Key, o => o.FirstOrDefault());
        Account.Positions = positions.Data.GroupBy(o => o.Name).ToDictionary(o => o.Key, o => o.FirstOrDefault());

        Account
          .Orders
          .Values
          .Select(o => o.Transaction.Instrument.Name)
          .Concat(Account.Positions.Select(o => o.Value.Transaction.Instrument.Name))
          .Where(o => Account.Instruments.ContainsKey(o) is false)
          .ForEach(o => Account.Instruments[o] = new InstrumentModel { Name = o });

        response.Data = Account;
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Get orders
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IList<OrderModel>>> GetOrders(OrderScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<OrderModel>>();
      var orders = new ConcurrentDictionary<string, OrderModel>();
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

      void subscribe(OpenOrderMessage o)
      {
        ScreenAccount(o.Order.Account, () => orders[$"{o.OrderId}"] = InternalMap.GetOrder(o));
      }

      void unsubscribe()
      {
        _client.OpenOrder -= subscribe;
        _client.OpenOrderEnd -= unsubscribe;

        response.Data = [.. orders.Values];
        source.TrySetResult();
      }

      try
      {
        _client.OpenOrder += subscribe;
        _client.OpenOrderEnd += unsubscribe;
        _client.ClientSocket.reqAllOpenOrders();

        await source.Task;
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Get positions 
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IList<OrderModel>>> GetPositions(PositionScreenerModel screener, Hashtable criteria)
    {
      var id = _counter++;
      var response = new ResponseModel<IList<OrderModel>> { Data = [] };
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

      void subscribe(PositionMultiMessage o)
      {
        if (Equals(id, o.ReqId))
        {
          ScreenAccount(o.Account, () => response.Data.Add(InternalMap.GetPosition(o)));
        }
      }

      void unsubscribe(int reqId)
      {
        if (Equals(id, reqId))
        {
          _client.PositionMulti -= subscribe;
          _client.PositionMultiEnd -= unsubscribe;
          source.TrySetResult();
        }
      }

      try
      {
        _client.PositionMulti += subscribe;
        _client.PositionMultiEnd += unsubscribe;
        _client.ClientSocket.reqPositionsMulti(id, Account.Descriptor, string.Empty);

        await source.Task;
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Sync open balance, order, and positions 
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<IAccount>> GetAccountSummary(Hashtable criteria)
    {
      var id = _counter++;
      var response = new ResponseModel<IAccount>();
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

      void subscribe(AccountSummaryMessage o) => ScreenAccount(o.Account, () =>
      {
        if (Equals(id, o.RequestId))
        {
          switch (o.Tag)
          {
            case AccountSummaryTags.NetLiquidation: Account.Balance = double.Parse(o.Value); break;
          }
        }
      });

      void unsubscribe(AccountSummaryEndMessage o)
      {
        if (Equals(id, o.RequestId))
        {
          _client.AccountSummary -= subscribe;
          _client.AccountSummaryEnd -= unsubscribe;
          source.TrySetResult();
        }
      }

      try
      {
        _client.AccountSummary += subscribe;
        _client.AccountSummaryEnd += unsubscribe;
        _client.ClientSocket.reqAccountSummary(id, "All", AccountSummaryTags.GetAllTags());

        await source.Task;

        response.Data = Account;
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Generate next available order ID
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<int>> CreateOrderSystem()
    {
      var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
      var response = new ResponseModel<int>();

      _client.NextValidId += o => source.TrySetResult(o);
      _client.ClientSocket.reqIds(-1);

      response.Data = await source.Task;

      return response;
    }

    /// <summary>
    /// Setup socket connection 
    /// </summary>
    /// <returns></returns>
    protected virtual Task<ResponseModel<EReader>> CreateReader()
    {
      var response = new ResponseModel<EReader>();

      try
      {
        var scheduler = new ScheduleService();
        var signal = new EReaderMonitorSignal();

        _client = new IBClient(signal);
        _client.ClientSocket.SetConnectOptions("+PACEAPI");
        _client.ClientSocket.eConnect(Host, Port, 0);

        var reader = new EReader(_client.ClientSocket, signal);

        scheduler.Send(() =>
        {
          while (_client.ClientSocket.IsConnected())
          {
            signal.waitForSignal();
            reader.processMsgs();
          }
        });

        _connections.Add(scheduler);

        reader.Start();
        response.Data = reader;
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return Task.FromResult(response);
    }

    /// <summary>
    /// Send order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<OrderModel>> CreateOrder(OrderModel order)
    {
      var inResponse = new ResponseModel<OrderModel>();
      var source = new TaskCompletionSource<OpenOrderMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

      try
      {
        var exOrder = ExternalMap.GetOrder(order);
        var orderId = _client.NextOrderId++;

        void subscribe(OpenOrderMessage o)
        {
          if (Equals(orderId, o.OrderId))
          {
            source.TrySetResult(o);
            _client.OpenOrder -= subscribe;
          }
        }

        _client.OpenOrder += subscribe;
        _client.ClientSocket.placeOrder(orderId, exOrder.Contract, exOrder.Order);

        var exResponse = await source.Task;

        inResponse.Data = order;

        if (string.Equals(exResponse.OrderState.Status, "Submitted", StringComparison.InvariantCultureIgnoreCase))
        {
          inResponse.Data.Transaction.Id = $"{orderId}";
          inResponse.Data.Transaction.Status = InternalMap.GetStatus(exResponse.OrderState.Status);

          Account.Orders[order.Id] = order;
        }
      }
      catch (Exception e)
      {
        inResponse.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return inResponse;
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<OrderModel>> DeleteOrder(OrderModel order)
    {
      var inResponse = new ResponseModel<OrderModel>();
      var source = new TaskCompletionSource<OrderStatusMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

      try
      {
        var exOrder = ExternalMap.GetOrder(order);
        var orderId = _client.NextOrderId++;

        void subscribe(OrderStatusMessage o)
        {
          if (Equals(orderId, o.OrderId))
          {
            source.TrySetResult(o);
            _client.OrderStatus -= subscribe;
          }
        }

        _client.OrderStatus += subscribe;
        _client.ClientSocket.cancelOrder(orderId, string.Empty);

        var exResponse = await source.Task;

        inResponse.Data = order;
        inResponse.Data.Transaction.Id = $"{orderId}";
        inResponse.Data.Transaction.Status = InternalMap.GetStatus(exResponse.Status);
      }
      catch (Exception e)
      {
        inResponse.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return inResponse;
    }

    /// <summary>
    /// Get latest quote
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public virtual async Task<ResponseModel<PointModel>> GetPrice(PointScreenerModel screener, Hashtable criteria)
    {
      var id = _counter++;
      var response = new ResponseModel<PointModel>();
      var source = new TaskCompletionSource<TickByTickBidAskMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

      void subscribe(TickByTickBidAskMessage o)
      {
        if (Equals(id, o.ReqId))
        {
          _client.tickByTickBidAsk -= subscribe;
          source.TrySetResult(o);
        }
      }

      try
      {
        var contract = new Contract
        {
          Symbol = screener?.Instrument?.Name ?? criteria.Get<string>("symbol"),
          SecType = ExternalMap.GetInstrumentType(screener?.Instrument?.Type) ?? criteria.Get<string>("secType"),
          Exchange = screener?.Instrument?.Exchange ?? criteria.Get<string>("exchange") ?? "SMART",
          Currency = nameof(CurrencyEnum.USD)
        };

        _client.tickByTickBidAsk += subscribe;
        _client.ClientSocket.reqTickByTickData(id, contract, "BidAsk", 0, false);

        response.Data = InternalMap.GetPrice(await source.Task);
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }
  }
}
