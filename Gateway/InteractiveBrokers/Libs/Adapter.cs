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

        await GetPrice(id, instrument, point =>
        {
          point.Time = DateTime.Now;
          point.TimeFrame = instrument.TimeFrame;

          instrument.Points.Add(point);
          instrument.PointGroups.Add(point, instrument.TimeFrame);

          PointStream(new MessageModel<PointModel> { Next = instrument.PointGroups.Last() });
        });

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
        var id = _counter++;
        var point = (await GetPrice(id, screener.Instrument)).Data;

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
    /// <returns></returns>
    public virtual async Task<Contract> GetContract(InstrumentModel instrument)
    {
      var id = _counter++;
      var source = new TaskCompletionSource<Contract>(TaskCreationOptions.RunContinuationsAsynchronously);
      var instrumentType = ExternalMap.GetInstrumentType(instrument.Type);
      var contract = _contracts.Get(instrument.Name) ?? ExternalMap.GetContract(instrument);

      _client.ContractDetails += message => contract = message.ContractDetails.Contract;
      _client.ContractDetailsEnd += message => source.TrySetResult(contract);
      _client.ClientSocket.reqContractDetails(id, contract);

      return await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => null as Contract));
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

      try
      {
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

        foreach (var instrument in Account.Instruments)
        {
          _contracts[instrument.Key] = await GetContract(instrument.Value);
        }

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
        if (Equals(o.Order.Account, Account.Descriptor))
        {
          orders[$"{o.Order.PermId}"] = InternalMap.GetOrder(o);
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

      void subscribe(PositionMultiMessage o)
      {
        if (Equals(id, o.ReqId) && Equals(o.Account, Account.Descriptor))
        {
          response.Data.Add(InternalMap.GetPosition(o));
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
    protected virtual async Task<ResponseModel<PointModel>> GetPrice(int id, InstrumentModel instrument, Action<PointModel> action = null)
    {
      var response = new ResponseModel<PointModel>();

      try
      {
        var point = new PointModel();
        var contract = _contracts.Get(instrument.Name) ?? ExternalMap.GetContract(instrument);
        var source = new TaskCompletionSource<PointModel>(TaskCreationOptions.RunContinuationsAsynchronously);

        _client.TickPrice += message =>
        {
          if (Equals(message.RequestId, id))
          {
            switch (ExternalMap.GetField(message.Field))
            {
              case FieldCodeEnum.BidSize: point.BidSize = message.Value; break;
              case FieldCodeEnum.AskSize: point.AskSize = message.Value; break;
              case FieldCodeEnum.BidPrice: point.Last = point.Bid = message.Value; break;
              case FieldCodeEnum.AskPrice: point.Last = point.Ask = message.Value; break;
            }

            if (point.Bid is null || point.Ask is null || point.BidSize is null || point.AskSize is null)
            {
              return;
            }

            point.Time = DateTime.Now;
            point.TimeFrame = instrument.TimeFrame;
            point.Instrument = instrument;

            if (action is not null)
            {
              action(point.Clone() as PointModel);
              return;
            }

            source.TrySetResult(point);
          }
        };

        _client.ClientSocket.reqMktData(id, contract, string.Empty, false, false, null);

        if (action is not null)
        {
          action(point.Clone() as PointModel);
          return response;
        }

        response.Data = await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => null as PointModel));

        _client.ClientSocket.cancelMktData(id);
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

      void subscribe(AccountSummaryMessage o)
      {
        if (Equals(id, o.RequestId) && Equals(o.Tag, AccountSummaryTags.NetLiquidation))
        {
          Account.Balance = double.Parse(o.Value);
        }
      }

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

        await await Task.WhenAny(source.Task, Task.Delay(Timeout));

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
    protected virtual async Task CreateRegister()
    {
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

      _client.NextValidId += o => source.TrySetResult();
      _client.ClientSocket.reqIds(-1);

      await await Task.WhenAny(source.Task, Task.Delay(Timeout));
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

        while (_client.NextOrderId <= 0) ;
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
      var response = new ResponseModel<OrderModel>();
      var source = new TaskCompletionSource<OpenOrderMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

      try
      {
        var orderId = _client.NextOrderId++;
        var exOrder = ExternalMap.GetOrder(orderId, order, _contracts);

        void subscribe(OpenOrderMessage o)
        {
          if (Equals(orderId, o.OrderId))
          {
            _client.OpenOrder -= subscribe;
            source.TrySetResult(o);
          }
        }

        _client.OpenOrder += subscribe;
        _client.ClientSocket.placeOrder(orderId, exOrder.Contract, exOrder.Order);

        var exResponse = await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => null as OpenOrderMessage));

        response.Data = order;

        if (string.Equals(exResponse.OrderState.Status, "Submitted", StringComparison.InvariantCultureIgnoreCase))
        {
          response.Data.Transaction.Id = $"{orderId}";
          response.Data.Transaction.Status = InternalMap.GetOrderStatus(exResponse.OrderState.Status);

          Account.Orders[order.Id] = order;
        }
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
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
      var source = new TaskCompletionSource<OrderStatusMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

      try
      {
        var orderId = int.Parse(order.Transaction.Id);

        void subscribe(OrderStatusMessage o)
        {
          if (Equals(orderId, o.OrderId))
          {
            _client.OrderStatus -= subscribe;
            source.TrySetResult(o);
          }
        }

        _client.OrderStatus += subscribe;
        _client.ClientSocket.cancelOrder(orderId, string.Empty);

        var exResponse = await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => null as OrderStatusMessage));

        response.Data = order;
        response.Data.Transaction.Id = $"{orderId}";
        response.Data.Transaction.Status = InternalMap.GetOrderStatus(exResponse.Status);
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return response;
    }
  }
}
