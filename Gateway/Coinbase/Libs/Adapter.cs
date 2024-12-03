using Coinbase.Classes;
using Coinbase.Mappers;
using Coinbase.Messages;
using Distribution.Services;
using Distribution.Stream;
using Distribution.Stream.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;

namespace Coinbase
{
  public class Adapter : Gateway
  {
    /// <summary>
    /// HTTP client
    /// </summary>
    protected Service _sender;

    /// <summary>
    /// Socket connection
    /// </summary>
    protected ClientWebSocket _streamer;

    /// <summary>
    /// JWT token
    /// </summary>
    protected Authenticator _authenticator;

    /// <summary>
    /// Subscription channels
    /// </summary>
    protected IList<string> _channels;

    /// <summary>
    /// Disposable connections
    /// </summary>
    protected IList<IDisposable> _connections;

    /// <summary>
    /// Disposable subscriptions
    /// </summary>
    protected IList<IDisposable> _subscriptions;

    /// <summary>
    /// Key
    /// </summary>
    public virtual string ConsumerKey { get; set; }

    /// <summary>
    /// Secret
    /// </summary>
    public virtual string ConsumerSecret { get; set; }

    /// <summary>
    /// Data source
    /// </summary>
    public virtual string DataUri { get; set; }

    /// <summary>
    /// Streaming source
    /// </summary>
    public virtual string StreamUri { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Adapter()
    {
      StreamUri = "wss://advanced-trade-ws.coinbase.com";
      DataUri = "https://api.coinbase.com";

      _subscriptions = [];
      _connections = [];
      _channels = [
        "level2",
        "ticker",
        "ticker_batch",
        "status",
        "market_trades",
        "candles",
      ];
    }

    /// <summary>
    /// Connect
    /// </summary>
    public override async Task<ResponseModel<StatusEnum>> Connect()
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        _authenticator = new Authenticator();

        var ws = new ClientWebSocket();
        var scheduler = new ScheduleService();

        await Disconnect();

        _sender = new Service();
        _streamer = await GetConnection(ws, scheduler);

        await GetAccount([]);

        _connections.Add(_sender);
        _connections.Add(_streamer);
        _connections.Add(scheduler);

        await Task.WhenAll(Account.Instruments.Values.Select(Subscribe));

        response.Data = StatusEnum.Success;
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return response;
    }

    public override async Task<ResponseModel<StatusEnum>> Subscribe(InstrumentModel instrument)
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        await Unsubscribe(instrument);

        foreach (var channel in _channels)
        {
          await SendStream(_streamer, new SubscriptionChannelMessage
          {
            Type = "subscribe",
            Channel = channel,
            ProductIds = [.. Account.Instruments.Keys]
          });
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
    /// Save state and dispose
    /// </summary>
    public override Task<ResponseModel<StatusEnum>> Disconnect()
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
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
    public override async Task<ResponseModel<StatusEnum>> Unsubscribe(InstrumentModel instrument)
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        foreach (var channel in _channels)
        {
          await SendStream(_streamer, new SubscriptionChannelMessage
          {
            Type = "unsubscribe",
            Channel = channel,
            ProductIds = [.. Account.Instruments.Keys]
          });
        }

        _subscriptions?.ForEach(o => o.Dispose());
        _subscriptions?.Clear();

        response.Data = StatusEnum.Success;
      }
      catch (Exception e)
      {
        response.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return response;
    }

    /// <summary>
    /// Get options
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IList<InstrumentModel>>> GetOptions(OptionScreenerModel screener, Hashtable criteria)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get latest quote
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<DomModel>> GetDom(PointScreenerModel screener, Hashtable criteria)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get historical bars
    /// </summary>
    /// <param name="screener"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public override Task<ResponseModel<IList<PointModel>>> GetPoints(PointScreenerModel screener, Hashtable criteria)
    {
      throw new NotImplementedException();
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
    public override Task<ResponseModel<IList<OrderModel>>> DeleteOrders(params OrderModel[] orders)
    {
      throw new NotImplementedException();
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
        var account = await SendData<AccountMessage>($"/api/v3/brokerage/accounts/{Account.Descriptor}");
        var portfolios = await SendData<PortfoliosMessage>("/api/v3/brokerage/portfolios");
        var orders = await GetOrders(null, criteria);

        foreach (var portfolio in portfolios.Data.Portfolios)
        {
          var positions = (await GetPositions(null, criteria))
            .Data
            .GroupBy(o => o.Name)
            .ToDictionary(o => o.Key, o => o.FirstOrDefault())
            .Concurrent();

          Account.Positions = positions;
        }

        Account.Balance = account.Data.AvailableBalance;
        Account.Orders = orders
          .Data
          .GroupBy(o => o.Name)
          .ToDictionary(o => o.Key, o => o.FirstOrDefault())
          .Concurrent();

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

      try
      {
        var orderProps = new Hashtable
        {
          ["order_status"] = "OPEN"
        };

        foreach (DictionaryEntry o in criteria)
        {
          orderProps[o.Key] = o.Value;
        }

        var items = await SendData<OrdersMessage>($"/api/v3/brokerage/orders/historical/batch?{orderProps.Query()}");

        response.Data = [.. items.Data.Orders.Select(InternalMap.GetOrder)];
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
      var response = new ResponseModel<IList<OrderModel>>();

      try
      {
        var portfolios = await SendData<PortfoliosMessage>($"/api/v3/brokerage/portfolios?{criteria.Query()}");

        foreach (var portfolio in portfolios.Data.Portfolios)
        {
          var positions = await SendData<PortfolioBreakdownsMessage>($"/api/v3/brokerage/portfolios/{portfolio.Uuid}");
          var activePositions = positions
            .Data
            .Breakdown
            .SpotPositions
            .Select(InternalMap.GetPosition)
            .GroupBy(o => o.Name)
            .ToDictionary(o => o.Key, o => o.FirstOrDefault())
            .Concurrent();

          Account.Positions = activePositions;
        }

      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Send data to web socket stream
    /// </summary>
    /// <param name="ws"></param>
    /// <param name="data"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    protected virtual Task SendStream(ClientWebSocket ws, object data, CancellationTokenSource cancellation = null)
    {
      var content = JsonSerializer.Serialize(data, _sender.Options);
      var message = Encoding.ASCII.GetBytes(content);

      return ws.SendAsync(
        message,
        WebSocketMessageType.Binary,
        true,
        cancellation?.Token ?? CancellationToken.None);
    }

    /// <summary>
    /// Web socket stream
    /// </summary>
    /// <param name="ws"></param>
    /// <param name="scheduler"></param>
    /// <returns></returns>
    protected virtual async Task<ClientWebSocket> GetConnection(ClientWebSocket ws, ScheduleService scheduler)
    {
      var source = new UriBuilder(StreamUri);
      var cancellation = new CancellationTokenSource();

      await ws.ConnectAsync(source.Uri, cancellation.Token);

      scheduler.Send(async () =>
      {
        while (ws.State is WebSocketState.Open)
        {
          var data = new byte[byte.MaxValue];

          await ws.ReceiveAsync(data, cancellation.Token).ContinueWith(async o =>
          {
            var response = await o;
            var content = ""; // $"[{Encoding.ASCII.GetString(data).Trim('\0', '[', ']')}]";
            var message = JsonNode
              .Parse(content)
              ?.AsArray()
              ?.FirstOrDefault();

            switch ($"{message?["T"]}".ToUpper())
            {
              case "Q": ProcessPoint(content); break;
              case "T": ProcessTrade(content); break;
            }
          });
        }
      });

      return ws;
    }

    /// <summary>
    /// Process quote from the stream
    /// </summary>
    /// <param name="content"></param>
    protected virtual void ProcessPoint(string content)
    {
      //var streamPoint = JsonSerializer
      //  .Deserialize<HistoricalQuoteMessage[]>(content, _sender.Options)
      //  .FirstOrDefault();

      //var instrument = Account.Instruments.Get(streamPoint.Symbol) ?? new Instrument();
      //var point = new PointModel
      //{
      //  Ask = streamPoint.AskPrice,
      //  Bid = streamPoint.BidPrice,
      //  AskSize = streamPoint.AskSize ?? 0,
      //  BidSize = streamPoint.BidSize ?? 0,
      //  Last = streamPoint.BidPrice ?? streamPoint.AskPrice,
      //  Time = streamPoint.TimestampUtc ?? DateTime.Now,
      //  TimeFrame = instrument.TimeFrame,
      //  Instrument = instrument
      //};

      //instrument.Name = streamPoint.Symbol;
      //instrument.Points.Add(point);
      //instrument.PointGroups.Add(point);
    }

    /// <summary>
    /// Process quote from the stream
    /// </summary>
    /// <param name="content"></param>
    protected virtual void ProcessTrade(string content)
    {
      //var orderMessage = JsonSerializer
      //  .Deserialize<HistoricalTradeMessage[]>(content, _sender.Options)
      //  .FirstOrDefault();

      //var action = new TransactionModel
      //{
      //  Id = orderMessage.TradeId,
      //  Time = orderMessage.TimestampUtc,
      //  Price = orderMessage.Price,
      //  Volume = orderMessage.Size,
      //  Instrument = new Instrument { Name = orderMessage.Symbol }
      //};

      //var order = new OrderModel
      //{
      //  Side = InternalMap.GetOrderSide(orderMessage.TakerSide)
      //};

      //var message = new StateModel<OrderModel>
      //{
      //  Next = order
      //};

      //OrderStream(message);
    }

    /// <summary>
    /// Send data to the API
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="verb"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    protected virtual async Task<Distribution.Stream.Models.ResponseModel<T>> SendData<T>(
      string source,
      HttpMethod verb = null,
      object content = null)
    {
      var uri = new UriBuilder(DataUri + source);
      var message = new HttpRequestMessage { Method = verb ?? HttpMethod.Get };
      var endpoint = $"{message.Method} {uri.Host}{uri.Path}";
      var token = _authenticator.GetToken(ConsumerKey, ConsumerSecret, endpoint);

      switch (true)
      {
        case true when Equals(message.Method, HttpMethod.Put):
        case true when Equals(message.Method, HttpMethod.Post):
        case true when Equals(message.Method, HttpMethod.Patch): message.Content = new StringContent(JsonSerializer.Serialize(content)); break;
      }

      message.RequestUri = uri.Uri;
      message.Headers.Add("Authorization", $"Bearer {token}");

      return await _sender.Send<T>(message, _sender.Options);
    }

    /// <summary>
    /// Send order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual Task<ResponseModel<OrderModel>> CreateOrder(OrderModel order)
    {
      throw new NotImplementedException();
    }
  }
}
