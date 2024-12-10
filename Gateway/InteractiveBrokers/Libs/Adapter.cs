using Distribution.Services;
using IBApi;
using InteractiveBrokers.Enums;
using InteractiveBrokers.Mappers;
using InteractiveBrokers.Messages;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using Terminal.Core.Services;
using static InteractiveBrokers.IBClient;

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
    /// Contracts
    /// </summary>
    protected ConcurrentDictionary<string, Contract> _contracts;

    /// <summary>
    /// Timeout
    /// </summary>
    public virtual TimeSpan Timeout { get; set; }

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
      Timeout = TimeSpan.FromSeconds(5);

      _counter = 1;
      _connections = [];
      _subscriptions = new ConcurrentDictionary<string, int>();
      _contracts = new ConcurrentDictionary<string, Contract>();
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
        await CreateRegister();

        SubscribeToErrors();
        SubscribeToOrders();
        SubscribeToStreams();

        await GetAccount([]);
        await Task.WhenAll(Account.Instruments.Values.Select(Subscribe));

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
        var contract = _contracts.Get(instrument.Name) ?? ExternalMap.GetContract(instrument);

        SubscribeToPoints(id, instrument, point =>
        {
          point.Time ??= DateTime.Now;
          point.Instrument = instrument;

          instrument.Points.Add(point);
          instrument.PointGroups.Add(point, instrument.TimeFrame);

          PointStream(new MessageModel<PointModel> { Next = instrument.PointGroups.Last() });
        });

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
        if (_subscriptions.TryRemove(instrument.Name, out var id))
        {
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

        await Task.FromResult(options);
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
        screener.Count ??= 1;
        screener.MaxDate ??= DateTime.Now;

        var id = _counter++;
        var point = (await GetPoints(screener, null)).Data.Last();

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
    public override async Task<ResponseModel<IList<PointModel>>> GetPoints(PointScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<PointModel>>();

      try
      {
        var id = _counter++;
        var count = screener.Count ?? 1;
        var instrument = screener.Instrument;
        var minDate = screener.MinDate?.ToString("yyyyMMdd HH:mm:ss");
        var maxDate = screener.MaxDate?.ToString("yyyyMMdd HH:mm:ss");
        var contract = _contracts.Get(instrument.Name) ?? ExternalMap.GetContract(instrument);
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void subscribe(HistoricalTicksMessage message)
        {
          if (Equals(id, message.ReqId))
          {
            response.Data = [.. message.Items.Select(InternalMap.GetPrice)];
            unsubscribe();
          }
        }

        void unsubscribe()
        {
          _client.historicalTicksList -= subscribe;
          source.TrySetResult();
        }

        _client.historicalTicksList += subscribe;
        _client.ClientSocket.reqHistoricalTicks(id, contract, minDate, maxDate, count, "BID_ASK", 1, true, null);

        await await Task.WhenAny(source.Task, Task.Delay(Timeout));
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Send orders
    /// </summary>
    /// <param name="orders"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IList<OrderModel>>> CreateOrders(params OrderModel[] orders)
    {
      var response = new ResponseModel<IList<OrderModel>> { Data = [] };

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
    /// <param name="interval"></param>
    /// <returns></returns>
    public virtual async Task<ResponseModel<Contract>> GetContract(InstrumentModel instrument, int interval = 0)
    {
      var id = _counter++;
      var response = new ResponseModel<Contract>();
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      var contract = _contracts.Get(instrument.Name) ?? ExternalMap.GetContract(instrument);

      void subscribe(ContractDetailsMessage message)
      {
        if (Equals(id, message.RequestId))
        {
          response.Data = message.ContractDetails.Contract;
          unsubscribe(id);
        }
      }

      void unsubscribe(int reqId)
      {
        _client.ContractDetails -= subscribe;
        _client.ContractDetailsEnd -= unsubscribe;

        source.TrySetResult();
      }

      _client.ContractDetails += subscribe;
      _client.ContractDetailsEnd += unsubscribe;
      _client.ClientSocket.reqContractDetails(id, contract);

      await await Task.WhenAny(source.Task, Task.Delay(Timeout));
      await Task.Delay(interval);

      return response;
    }

    /// <summary>
    /// Subscribe to account updates
    /// </summary>
    protected virtual void SubscribeToStreams()
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
    /// Sync open balance, order, and positions 
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<IAccount>> GetAccount(Hashtable criteria)
    {
      var response = new ResponseModel<IAccount>();

      await GetAccountSummary(criteria);

      var orders = await GetOrders(null, criteria);
      var positions = await GetPositions(null, criteria);

      Account.Orders = orders.Data.GroupBy(o => o.Id).ToDictionary(o => o.Key, o => o.FirstOrDefault()).Concurrent();
      Account.Positions = positions.Data.GroupBy(o => o.Name).ToDictionary(o => o.Key, o => o.FirstOrDefault()).Concurrent();

      Account
        .Positions
        .Values
        .Where(o => Account.Instruments.ContainsKey(o.Name) is false)
        .ForEach(o => Account.Instruments[o.Name] = o.Transaction.Instrument);

      foreach (var instrument in Account.Instruments.Values)
      {
        _contracts[instrument.Name] = (await GetContract(instrument, 10)).Data ?? ExternalMap.GetContract(instrument);
      }

      response.Data = Account;

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

      void subscribe(OpenOrderMessage message)
      {
        if (Equals(message.Order.Account, Account.Descriptor))
        {
          orders[$"{message.Order.PermId}"] = InternalMap.GetOrder(message);
        }
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

        await await Task.WhenAny(source.Task, Task.Delay(Timeout));
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

      void subscribe(PositionMultiMessage message)
      {
        if (Equals(id, message.ReqId) && Equals(message.Account, Account.Descriptor))
        {
          response.Data.Add(InternalMap.GetPosition(message));
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

        await await Task.WhenAny(source.Task, Task.Delay(Timeout));
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
    /// <param name="action"></param>
    /// <returns></returns>
    protected virtual void SubscribeToPoints(int id, InstrumentModel instrument, Action<PointModel> action)
    {
      var point = new PointModel();

      void subscribe(TickPriceMessage message)
      {
        if (Equals(id, message.RequestId))
        {
          switch (ExternalMap.GetField(message.Field))
          {
            case FieldCodeEnum.BidSize: point.BidSize = message.Data ?? point.BidSize; break;
            case FieldCodeEnum.AskSize: point.AskSize = message.Data ?? point.AskSize; break;
            case FieldCodeEnum.BidPrice: point.Bid = message.Data ?? point.Bid; break;
            case FieldCodeEnum.AskPrice: point.Ask = message.Data ?? point.Ask; break;
            case FieldCodeEnum.LastPrice: point.Last = message.Data ?? point.Last; break;
          }

          if (point.Bid is null || point.Ask is null || point.Last is null)
          {
            return;
          }

          point.Time = DateTime.Now;
          point.Instrument = instrument;
          action(point);
        }
      }

      var contract = _contracts.Get(instrument.Name) ?? ExternalMap.GetContract(instrument);

      _client.TickPrice += subscribe;
      _client.ClientSocket.reqMktData(id, contract, string.Empty, false, false, null);
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

      void subscribe(AccountSummaryMessage message)
      {
        if (Equals(id, message.RequestId) && Equals(message.Tag, AccountSummaryTags.NetLiquidation))
        {
          Account.Balance = double.Parse(message.Value);
        }
      }

      void unsubscribe(AccountSummaryEndMessage message)
      {
        if (Equals(id, message.RequestId))
        {
          _client.AccountSummary -= subscribe;
          _client.AccountSummaryEnd -= unsubscribe;

          source.TrySetResult();
        }
      }

      _client.AccountSummary += subscribe;
      _client.AccountSummaryEnd += unsubscribe;
      _client.ClientSocket.reqAccountSummary(id, "All", AccountSummaryTags.GetAllTags());

      await await Task.WhenAny(source.Task, Task.Delay(Timeout));

      response.Data = Account;

      return response;
    }

    /// <summary>
    /// Generate next available order ID
    /// </summary>
    /// <returns></returns>
    protected virtual async Task CreateRegister()
    {
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

      _client.NextValidId += o => source.TrySetResult();
      _client.ClientSocket.reqIds(-1);

      await source.Task;
    }

    /// <summary>
    /// Setup socket connection 
    /// </summary>
    /// <returns></returns>
    protected virtual Task<ResponseModel<EReader>> CreateReader()
    {
      var response = new ResponseModel<EReader>();
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

      while (_client.NextOrderId <= 0) ;

      return Task.FromResult(response);
    }

    /// <summary>
    /// Send order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<OrderModel>> CreateOrder(OrderModel order)
    {
      var response = new ResponseModel<OrderModel>();
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      var orderId = _client.NextOrderId++;
      var exOrder = ExternalMap.GetOrder(orderId, order, _contracts);
      var exResponse = null as OpenOrderMessage;

      void subscribe(OpenOrderMessage message)
      {
        if (Equals(orderId, message.OrderId))
        {
          exResponse = message;
          unsubscribe();
        }
      }

      void unsubscribe()
      {
        _client.OpenOrder -= subscribe;
        _client.OpenOrderEnd -= unsubscribe;
        source.TrySetResult();
      }

      _client.OpenOrder += subscribe;
      _client.OpenOrderEnd += unsubscribe;
      _client.ClientSocket.placeOrder(orderId, exOrder.Contract, exOrder.Order);

      await await Task.WhenAny(source.Task, Task.Delay(Timeout));

      response.Data = order;

      if (string.Equals(exResponse.OrderState.Status, "Submitted", StringComparison.InvariantCultureIgnoreCase))
      {
        response.Data.Transaction.Id = $"{orderId}";
        response.Data.Transaction.Status = InternalMap.GetOrderStatus(exResponse.OrderState.Status);

        Account.Orders[order.Id] = order;
      }

      return response;
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<OrderModel>> DeleteOrder(OrderModel order)
    {
      var response = new ResponseModel<OrderModel>();
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      var orderId = int.Parse(order.Transaction.Id);
      var exResponse = null as OrderStatusMessage;

      void subscribe(OrderStatusMessage message)
      {
        if (Equals(orderId, message.OrderId))
        {
          exResponse = message;
          unsubscribe();
        }
      }

      void unsubscribe()
      {
        _client.OrderStatus -= subscribe;
        source.TrySetResult();
      }

      _client.OrderStatus += subscribe;
      _client.ClientSocket.cancelOrder(orderId, string.Empty);

      await await Task.WhenAny(source.Task, Task.Delay(Timeout));

      response.Data = order;
      response.Data.Transaction.Id = $"{orderId}";
      response.Data.Transaction.Status = InternalMap.GetOrderStatus(exResponse.Status);

      return response;
    }
  }
}
