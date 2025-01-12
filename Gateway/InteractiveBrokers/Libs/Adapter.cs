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
using System.Threading;
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
    /// Unique order ID
    /// </summary>
    protected int _order;

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
    /// Timeout
    /// </summary>
    public virtual TimeSpan Timeout { get; set; }

    /// <summary>
    /// Timeout
    /// </summary>
    public virtual int Interval { get; set; }

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
      Interval = 15;
      Timeout = TimeSpan.FromSeconds(10);

      _order = 1;
      _counter = 1;
      _connections = [];
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

        SubscribeToIds();
        SubscribeToErrors();
        SubscribeToOrders();
        SubscribeToStreams();

        await GetAccount([]);

        foreach (var instrument in Account.Instruments.Values)
        {
          await Subscribe(instrument);
        }

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
        var contracts = await GetContracts(instrument, Interval);
        var contract = contracts.Data.First();

        await SubscribeToPoints(id, InternalMap.GetInstrument(contract, instrument), Interval);

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
    public override async Task<ResponseModel<IList<InstrumentModel>>> GetOptions(InstrumentScreenerModel screener, Hashtable criteria)
    {
      var response = new ResponseModel<IList<InstrumentModel>>();

      try
      {
        var id = _counter++;
        var instrument = screener.Instrument;
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var contracts = await GetContracts(instrument, Interval);

        response.Data = [.. contracts
          .Data
          .Select(o => InternalMap.GetInstrument(o))
          .OrderBy(o => o.Derivative.ExpirationDate)
          .ThenBy(o => o.Derivative.Strike)
          .ThenBy(o => o.Derivative.Side)];
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
        var minDate = screener.MinDate?.ToString($"yyyyMMdd-HH:mm:ss");
        var maxDate = (screener.MaxDate ?? DateTime.Now).ToString($"yyyyMMdd-HH:mm:ss");
        var contract = ExternalMap.GetContract(instrument);
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
        _client.ClientSocket.reqHistoricalTicks(id, contract, minDate, maxDate, count, "BID_ASK", 1, false, null);

        await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => unsubscribe()));
        await Task.Delay(Interval);
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
        try
        {
          var inOrders = ComposeOrders(order);

          foreach (var inOrder in inOrders)
          {
            response.Data.Add((await CreateOrder(inOrder)).Data);
          }
        }
        catch (Exception e)
        {
          response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
        }
      }

      //await GetAccount([]);

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

      await GetAccount([]);

      return response;
    }

    /// <summary>
    /// Get contract definition
    /// </summary>
    /// <param name="instrument"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<IList<Contract>>> GetContracts(InstrumentModel instrument, int interval = 0)
    {
      var id = _counter++;
      var response = new ResponseModel<IList<Contract>> { Data = [] };
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      var contract = ExternalMap.GetContract(instrument);

      void subscribe(ContractDetailsMessage message)
      {
        if (Equals(id, message.RequestId))
        {
          response.Data.Add(message.ContractDetails.Contract);
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

      await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => unsubscribe(id)));
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

        await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => unsubscribe()));
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

        await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => unsubscribe(id)));
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
    /// <param name="id"></param>
    /// <param name="instrument"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    protected virtual async Task SubscribeToPoints(int id, InstrumentModel instrument, int interval = 0)
    {
      var max = short.MaxValue;
      var point = new PointModel();
      var contract = ExternalMap.GetContract(instrument);

      double? value(double data, double min, double max, double? original)
      {
        switch (true)
        {
          case true when data < short.MinValue:
          case true when data > short.MaxValue:
          case true when data < min:
          case true when data > max: return original;
        }

        return Math.Round(data, 2);
      }

      void subscribeToComps(TickOptionMessage message)
      {
        if (Equals(id, message.RequestId))
        {
          instrument.Derivative ??= new DerivativeModel();
          instrument.Derivative.Sigma = value(message.ImpliedVolatility, 0, max, instrument.Derivative.Sigma);

          var variance = instrument.Derivative.Variance ??= new VarianceModel();

          variance.Delta = value(message.Delta, -1, 1, variance.Delta);
          variance.Gamma = value(message.Gamma, 0, max, variance.Gamma);
          variance.Theta = value(message.Theta, 0, max, variance.Theta);
          variance.Vega = value(message.Vega, 0, max, variance.Vega);
        }
      }

      void subscribeToPrices(TickPriceMessage message)
      {
        if (Equals(id, message.RequestId))
        {
          switch (ExternalMap.GetEnum<FieldCodeEnum>(message.Field))
          {
            case FieldCodeEnum.BidSize: point.BidSize = message.Data ?? point.BidSize; break;
            case FieldCodeEnum.AskSize: point.AskSize = message.Data ?? point.AskSize; break;
            case FieldCodeEnum.BidPrice: point.Bid = message.Data ?? point.Bid; break;
            case FieldCodeEnum.AskPrice: point.Ask = message.Data ?? point.Ask; break;
            case FieldCodeEnum.LastPrice: point.Last = message.Data ?? point.Last; break;
          }

          point.Last = point.Last is 0 or null ? point.Bid ?? point.Ask : point.Last;
          point.Bid ??= point.Last;
          point.Ask ??= point.Last;

          if (point.Bid is null || point.Ask is null || point.Last is null)
          {
            return;
          }

          point.Time = DateTime.Now;
          point.Instrument = instrument;

          instrument.Points.Add(point);
          instrument.PointGroups.Add(point, instrument.TimeFrame);
          instrument.Point = instrument.PointGroups.Last();

          PointStream(new MessageModel<PointModel> { Next = instrument.Point });
        }
      }

      _client.TickPrice += subscribeToPrices;
      _client.TickOptionCommunication += subscribeToComps;
      _client.ClientSocket.reqMktData(id, contract, string.Empty, false, false, null);

      await Task.Delay(interval);
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
        if (Equals(id, message?.RequestId))
        {
          _client.AccountSummary -= subscribe;
          _client.AccountSummaryEnd -= unsubscribe;

          source.TrySetResult();
        }
      }

      _client.AccountSummary += subscribe;
      _client.AccountSummaryEnd += unsubscribe;
      _client.ClientSocket.reqAccountSummary(id, "All", AccountSummaryTags.GetAllTags());

      await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => unsubscribe(null)));

      response.Data = Account;

      return response;
    }

    /// <summary>
    /// Generate next available order ID
    /// </summary>
    /// <returns></returns>
    protected virtual void SubscribeToIds()
    {
      var source = new TaskCompletionSource();

      void subscribe(int o)
      {
        _order = o;
        _client.NextValidId -= subscribe;
        source.TrySetResult();
      }

      _client.NextValidId += subscribe;
      _client.ClientSocket.reqIds(-1);
    }

    /// <summary>
    /// Setup socket connection 
    /// </summary>
    /// <returns></returns>
    protected virtual Task<ResponseModel<EReader>> CreateReader()
    {
      var response = new ResponseModel<EReader>();
      var signal = new EReaderMonitorSignal();

      _client = new IBClient(signal);
      _client.ClientSocket.SetConnectOptions("+PACEAPI");
      _client.ClientSocket.eConnect(Host, Port, 0);

      var reader = new EReader(_client.ClientSocket, signal);

      var process = new Thread(() =>
      {
        while (_client.ClientSocket.IsConnected())
        {
          signal.waitForSignal();
          reader.processMsgs();
        }
      });

      process.Start();
      reader.Start();
      response.Data = reader;

      return Task.FromResult(response);
    }

    /// <summary>
    /// Send order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<OrderModel>> CreateOrder(OrderModel order)
    {
      Account.Orders[order.Id] = order;

      await Subscribe(order.Transaction.Instrument);

      var orderId = _order++;
      var response = new ResponseModel<OrderModel>();
      var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      var exOrders = ExternalMap.GetOrders(orderId, order, Account);
      var exResponse = null as OpenOrderMessage;

      void subscribe(OpenOrderMessage message)
      {
        if (Equals(orderId, message.OrderId))
        {
          exResponse = message;
          unsubscribe();
          source.TrySetResult();
        }
      }

      void unsubscribe()
      {
        _client.OpenOrder -= subscribe;
        _client.OpenOrderEnd -= unsubscribe;
      }

      _client.OpenOrder += subscribe;
      _client.OpenOrderEnd += unsubscribe;

      foreach (var exOrder in exOrders)
      {
        _client.ClientSocket.placeOrder(exOrder.Order.OrderId, exOrder.Contract, exOrder.Order);
        await Task.Delay(Interval);
      }

      await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => unsubscribe()));

      response.Data = order;
      response.Data.Transaction.Id = $"{orderId}";
      response.Data.Transaction.Status = InternalMap.GetOrderStatus(exResponse?.OrderState?.Status);

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

      await await Task.WhenAny(source.Task, Task.Delay(Timeout).ContinueWith(o => unsubscribe()));

      response.Data = order;
      response.Data.Transaction.Id = $"{orderId}";
      response.Data.Transaction.Status = InternalMap.GetOrderStatus(exResponse.Status);

      return response;
    }
  }
}
