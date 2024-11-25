using Distribution.Services;
using Distribution.Stream;
using Distribution.Stream.Extensions;
using Schwab.Mappers;
using Schwab.Messages;
using System;
using System.Collections;
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
using Terminal.Core.Services;
using Dis = Distribution.Stream.Models;

namespace Schwab
{
  public class Adapter : Gateway
  {
    /// <summary>
    /// Request ID
    /// </summary>
    protected int _counter;

    /// <summary>
    /// Encrypted account number
    /// </summary>
    protected string _accountCode;

    /// <summary>
    /// HTTP client
    /// </summary>
    protected Service _sender;

    /// <summary>
    /// Socket connection
    /// </summary>
    protected ClientWebSocket _streamer;

    /// <summary>
    /// User preferences
    /// </summary>
    protected UserDataMessage _userData;

    /// <summary>
    /// Disposable connections
    /// </summary>
    protected IList<IDisposable> _connections;

    /// <summary>
    /// Data source
    /// </summary>
    public virtual string DataUri { get; set; }

    /// <summary>
    /// Streaming source
    /// </summary>
    public virtual string StreamUri { get; set; }

    /// <summary>
    /// Access token
    /// </summary>
    public virtual string AccessToken { get; set; }

    /// <summary>
    /// Refresh token
    /// </summary>
    public virtual string RefreshToken { get; set; }

    /// <summary>
    /// Client ID
    /// </summary>
    public virtual string ClientId { get; set; }

    /// <summary>
    /// Client secret
    /// </summary>
    public virtual string ClientSecret { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Adapter()
    {
      DataUri = "https://api.schwabapi.com";
      StreamUri = "wss://streamer-api.schwab.com/ws";

      _counter = 0;
      _connections = [];
    }

    /// <summary>
    /// Connect
    /// </summary>
    public override async Task<ResponseModel<StatusEnum>> Connect()
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        var ws = new ClientWebSocket();
        var scheduler = new ScheduleService();

        await Disconnect();

        _sender = new Service();

        await UpdateToken("/v1/oauth/token");
        await GetAccount([]);

        _streamer = await GetConnection(ws, scheduler);

        var interval = new System.Timers.Timer(TimeSpan.FromMinutes(1));

        interval.Enabled = true;
        interval.Elapsed += async (sender, e) => await UpdateToken("/v1/oauth/token");

        _connections.Add(_sender);
        _connections.Add(_streamer);
        _connections.Add(interval);

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
    /// Subscribe to data streams
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public override async Task<ResponseModel<StatusEnum>> Subscribe(InstrumentModel instrument)
    {
      var response = new ResponseModel<StatusEnum>();

      try
      {
        await Unsubscribe(instrument);

        var streamData = _userData.Streamer.FirstOrDefault();

        await SendStream(_streamer, new StreamInputMessage
        {
          Requestid = ++_counter,
          Service = ExternalMap.GetStreamingService(instrument),
          Command = "ADD",
          CustomerId = streamData.CustomerId,
          CorrelationId = $"{Guid.NewGuid()}",
          Parameters = new SrteamParamsMessage
          {
            Keys = instrument.Name,
            Fields = string.Join(",", Enumerable.Range(0, 10))
          }
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
      var response = new ResponseModel<StatusEnum>
      {
        Data = StatusEnum.Success
      };

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
        var props = new Hashtable
        {
          ["symbol"] = screener.Instrument?.Name,
          ["toDate"] = $"{screener.MaxDate:yyyy-MM-dd}",
          ["fromDate"] = $"{screener.MinDate:yyyy-MM-dd}",
          ["strikeCount"] = screener.Count ?? int.MaxValue

        }.Merge(criteria);

        var optionResponse = await SendData<OptionChainMessage>($"/marketdata/v1/chains?{props}");

        if (optionResponse.Data is not null)
        {
          response.Data = optionResponse
            .Data
            .PutExpDateMap
            ?.Concat(optionResponse.Data.CallExpDateMap)
            ?.SelectMany(dateMap => dateMap.Value.SelectMany(o => o.Value))
            ?.Select(option => InternalMap.GetOption(option, optionResponse.Data))
            ?.ToList() ?? [];
        }
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
        var props = new Hashtable
        {
          ["indicative"] = false,
          ["symbols"] = screener.Instrument.Name,
          ["fields"] = "quote,fundamental,extended,reference,regular"

        }.Merge(criteria);

        var pointResponse = await SendData<Dictionary<string, AssetMessage>>($"/marketdata/v1/quotes?{props}");
        var point = InternalMap.GetPrice(pointResponse.Data[props["symbols"]]);

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
        var props = new Hashtable
        {
          ["periodType"] = "day",
          ["period"] = 1,
          ["frequencyType"] = "minute",
          ["frequency"] = 1,
          ["symbol"] = screener.Instrument.Name

        }.Merge(criteria);

        var pointResponse = await SendData<BarsMessage>($"/marketdata/v1/pricehistory?{props}");

        response.Data = pointResponse
          .Data
          .Bars
          ?.Select(InternalMap.GetBar)?.ToList() ?? [];
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Create orders
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
      var response = new ResponseModel<IList<OrderModel>> { Data = [] };

      foreach (var order in orders)
      {
        response.Data.Add((await DeleteOrder(order)).Data);
      }

      return response;
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
        var accountProps = new Hashtable { ["fields"] = "positions" };
        var accountNumbers = await SendData<AccountNumberMessage[]>("/trader/v1/accounts/accountNumbers");

        _accountCode = accountNumbers.Data.First(o => Equals(o.AccountNumber, Account.Descriptor)).HashValue;

        var account = await SendData<AccountsMessage>($"/trader/v1/accounts/{_accountCode}?{accountProps.Query()}");
        var orders = await GetOrders(null, criteria);
        var positions = await GetPositions(null, criteria);

        Account.Balance = account.Data.AggregatedBalance.CurrentLiquidationValue;
        Account.Orders = orders.Data.GroupBy(o => o.Id).ToDictionary(o => o.Key, o => o.FirstOrDefault()).Concurrent();
        Account.Positions = positions.Data.GroupBy(o => o.Name).ToDictionary(o => o.Key, o => o.FirstOrDefault()).Concurrent();

        orders
          .Data
          .Select(o => o.Transaction.Instrument)
          .Concat(positions.Data.Select(o => o.Transaction.Instrument))
          .Where(o => Account.Instruments.ContainsKey(o.Name) is false)
          .ForEach(o => Account.Instruments[o.Name] = o);

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
        var dateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        var props = new Hashtable
        {
          ["maxResults"] = 50,
          ["toEnteredTime"] = DateTime.Now.AddDays(5).ToString(dateFormat),
          ["fromEnteredTime"] = DateTime.Now.AddDays(-100).ToString(dateFormat)

        }.Merge(criteria);

        var items = await SendData<OrderMessage[]>($"/trader/v1/accounts/{_accountCode}/orders?{props}");

        response.Data = [.. items.Data.Where(o => o.CloseTime is null).Select(InternalMap.GetOrder)];
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
        var props = new Hashtable { ["fields"] = "positions" }.Merge(criteria);
        var account = await SendData<AccountsMessage>($"/trader/v1/accounts/{_accountCode}?{props}");

        response.Data = [.. account
          .Data
          .SecuritiesAccount
          .Positions
          .Select(InternalMap.GetPosition)];
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
        WebSocketMessageType.Text,
        true,
        cancellation?.Token ?? CancellationToken.None);
    }

    /// <summary>
    /// Read response from web socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ws"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    protected virtual async Task<T> ReceiveStream<T>(ClientWebSocket ws, CancellationTokenSource source = null)
    {
      var cancellation = source?.Token ?? CancellationToken.None;
      var data = new byte[short.MaxValue];
      var response = await ws.ReceiveAsync(data, cancellation);
      var message = Encoding.ASCII.GetString(data, 0, response.Count);

      return JsonSerializer.Deserialize<T>(message, _sender.Options);
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
      var userData = await GetUserData();
      var streamData = userData.Data.Streamer.FirstOrDefault();

      await ws.ConnectAsync(source.Uri, cancellation.Token);
      await SendStream(ws, new StreamInputMessage
      {
        Service = "ADMIN",
        Command = "LOGIN",
        Requestid = ++_counter,
        CustomerId = streamData.CustomerId,
        CorrelationId = $"{Guid.NewGuid()}",
        Parameters = new StreamLoginMessage
        {
          Channel = streamData.Channel,
          FunctionId = streamData.FunctionId,
          Authorization = AccessToken
        }
      });

      var adminResponse = await ReceiveStream<StreamLoginResponseMessage>(ws);
      var adminCode = adminResponse?.Response?.FirstOrDefault()?.Content?.Code;

      if (adminCode is not 0)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Message = "No stream" });
        return ws;
      }

      scheduler.Send(async () =>
      {
        while (ws.State is WebSocketState.Open)
        {
          var data = new byte[short.MaxValue];

          await ws.ReceiveAsync(data, cancellation.Token).ContinueWith(async o =>
          {
            var response = await o;
            var content = $"{Encoding.Default.GetString(data).Trim(['\0', '[', ']'])}";
            var message = JsonNode.Parse(content);

            if (message["data"] is not null)
            {
              var streamPoints = message["data"]
                .AsArray()
                .Select(o => o.Deserialize<StreamDataMessage>());

              OnPoint(streamPoints.Where(o => InternalMap.GetSubscriptionType(o.Service) is not null));
            }
          });
        }
      });

      return ws;
    }

    /// <summary>
    /// Process quote from the stream
    /// </summary>
    /// <param name="streamPoints"></param>
    protected virtual void OnPoint(IEnumerable<StreamDataMessage> streamPoints)
    {
      var scheduler = InstanceService<ScheduleService>.Instance;

      scheduler.Send(() =>
      {
        foreach (var streamPoint in streamPoints)
        {
          var map = InternalMap.GetStreamMap(streamPoint.Service);

          foreach (var data in streamPoint.Content)
          {
            var point = new PointModel();
            var instrumentName = $"{data.Get("key")}";
            var instrument = Account.Instruments.Get(instrumentName) ?? new InstrumentModel();

            point.Instrument = instrument;
            point.Bid = double.TryParse($"{data.Get(map.Get("Bid Price"))}", out var x1) ? x1 : null;
            point.Ask = double.TryParse($"{data.Get(map.Get("Ask Price"))}", out var x2) ? x2 : null;
            point.BidSize = double.TryParse($"{data.Get(map.Get("Bid Size"))}", out var x3) ? x3 : null;
            point.AskSize = double.TryParse($"{data.Get(map.Get("Ask Size"))}", out var x4) ? x4 : null;
            point.Last = double.TryParse($"{data.Get(map.Get("Last Price"))}", out var x5) ? x5 : (point.Bid ?? point.Ask);

            instrument.Name = instrumentName;
            instrument.Point = point;
            instrument.Points.Add(point);
            instrument.PointGroups.Add(point, instrument.TimeFrame);

            PointStream(new MessageModel<PointModel>
            {
              Next = instrument.PointGroups.Last()
            });
          }
        }
      });
    }

    /// <summary>
    /// Send data to the API
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="verb"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    protected virtual async Task<Dis.ResponseModel<T>> SendData<T>(string source, HttpMethod verb = null, object content = null)
    {
      var uri = new UriBuilder(DataUri + source);
      var message = new HttpRequestMessage { Method = verb ?? HttpMethod.Get };

      switch (true)
      {
        case true when Equals(message.Method, HttpMethod.Put):
        case true when Equals(message.Method, HttpMethod.Post):
        case true when Equals(message.Method, HttpMethod.Patch):
          message.Content = new StringContent(JsonSerializer.Serialize(content, _sender.Options), Encoding.UTF8, "application/json");
          break;
      }

      message.RequestUri = uri.Uri;
      message.Headers.Add("Authorization", $"Bearer {AccessToken}");

      return await _sender.Send<T>(message, _sender.Options);
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    protected virtual async Task UpdateToken(string source)
    {
      try
      {
        var props = new Dictionary<string, string>
        {
          ["grant_type"] = "refresh_token",
          ["refresh_token"] = RefreshToken
        };

        var uri = new UriBuilder(DataUri + source);
        var content = new FormUrlEncodedContent(props);
        var message = new HttpRequestMessage();
        var basicToken = Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}");

        message.Content = content;
        message.RequestUri = uri.Uri;
        message.Method = HttpMethod.Post;
        message.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(basicToken)}");

        var response = await _sender.Send<ScopeMessage>(message, _sender.Options);

        if (response.Data is not null)
        {
          AccessToken = response.Data.AccessToken;
          RefreshToken = response.Data.RefreshToken;
        }
      }
      catch (Exception e)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      }
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    protected async Task<ResponseModel<UserDataMessage>> GetUserData()
    {
      var response = new ResponseModel<UserDataMessage>();

      try
      {
        var userResponse = await SendData<UserDataMessage>($"/trader/v1/userPreference");

        _userData = response.Data = userResponse.Data;
      }
      catch (Exception e)
      {
        response.Errors = [new ErrorModel { ErrorMessage = $"{e}" }];
      }

      return response;
    }

    /// <summary>
    /// Send order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual async Task<ResponseModel<OrderModel>> CreateOrder(OrderModel order)
    {
      var inResponse = new ResponseModel<OrderModel>();

      try
      {
        Account.Orders[order.Id] = order;

        var exOrder = ExternalMap.GetOrder(order);
        var exResponse = await SendData<OrderMessage>($"/trader/v1/accounts/{_accountCode}/orders", HttpMethod.Post, exOrder);

        inResponse.Data = order;

        if (exResponse.Message.Headers.TryGetValues("Location", out var orderData))
        {
          var orderItem = orderData.First();
          var orderId = $"{orderItem[(orderItem.LastIndexOf('/') + 1)..]}";

          if (string.IsNullOrEmpty(orderId))
          {
            inResponse.Errors.Add(new ErrorModel { ErrorMessage = $"{exResponse.Message.StatusCode}" });
            return inResponse;
          }

          inResponse.Data.Transaction.Id = orderId;
          inResponse.Data.Transaction.Status = OrderStatusEnum.Filled;
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

      try
      {
        var exResponse = await SendData<OrderMessage>($"/trader/v1/accounts/{_accountCode}/orders/{order.Transaction.Id}", HttpMethod.Delete);

        if ((int)exResponse.Message.StatusCode >= 400)
        {
          inResponse.Errors.Add(new ErrorModel { ErrorMessage = $"{exResponse.Message.StatusCode}" });
          return inResponse;
        }

        inResponse.Data = order;
        inResponse.Data.Transaction.Status = OrderStatusEnum.Canceled;
      }
      catch (Exception e)
      {
        inResponse.Errors.Add(new ErrorModel { ErrorMessage = $"{e}" });
      }

      return inResponse;
    }
  }
}
