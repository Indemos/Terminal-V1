using Distribution.Services;
using Distribution.Stream;
using Distribution.Stream.Extensions;
using Oanda.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Oanda
{
  public class Adapter : Gateway
  {
    /// <summary>
    /// HTTP client
    /// </summary>
    protected Service _sender;

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
    public virtual string Token { get; set; }

    /// <summary>
    /// Secret
    /// </summary>
    public virtual string DataUri { get; set; }

    /// <summary>
    /// Secret
    /// </summary>
    public virtual string StreamUri { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Adapter()
    {
      StreamUri = "https://stream-fxpractice.oanda.com";
      DataUri = "https://api-fxpractice.oanda.com";

      _subscriptions = [];
      _connections = [];
    }

    /// <summary>
    /// Connect
    /// </summary>
    public override async Task<IList<ErrorModel>> Connect()
    {
      var ws = new ClientWebSocket();
      var scheduler = new ScheduleService();

      await Disconnect();

      _sender = new Service();

      await GetAccountData();

      _connections.Add(_sender);
      _connections.Add(scheduler);

      Account.Instruments.ForEach(async o => await Subscribe(o.Key));

      return null;
    }

    /// <summary>
    /// Subscribe to data streams
    /// </summary>
    public override async Task<IList<ErrorModel>> Subscribe(string name)
    {
      await Unsubscribe(name);

      return null;
    }

    /// <summary>
    /// Save state and dispose
    /// </summary>
    public override Task<IList<ErrorModel>> Disconnect()
    {
      _connections?.ForEach(o => o.Dispose());
      _connections?.Clear();

      return Task.FromResult(null as IList<ErrorModel>);
    }

    /// <summary>
    /// Unsubscribe from data streams
    /// </summary>
    public override Task<IList<ErrorModel>> Unsubscribe(string name)
    {
      _subscriptions?.ForEach(o => o.Dispose());
      _subscriptions?.Clear();

      return Task.FromResult(null as IList<ErrorModel>);
    }

    /// <summary>
    /// Get quote
    /// </summary>
    /// <param name="message"></param>
    public override Task<ResponseItemModel<PointModel>> GetPoint(PointMessageModel message)
    {
      var response = new ResponseItemModel<PointModel>
      {
        Data = Account.Instruments[message.Name].Points.LastOrDefault()
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// Get option chains
    /// </summary>
    /// <param name="message"></param>
    public override Task<ResponseItemModel<IList<OptionModel>>> GetOptions(OptionMessageModel message)
    {
      throw new NotImplementedException();
    }

    public override Task<ResponseItemModel<IList<PointModel>>> GetPoints(PointMessageModel message)
    {
      var response = new ResponseItemModel<IList<PointModel>>
      {
        Data = Account.Instruments[message.Name].Points
      };

      return Task.FromResult(response);
    }

    public override async Task<ResponseMapModel<OrderModel>> CreateOrders(params OrderModel[] orders)
    {
      var response = new ResponseMapModel<OrderModel>();

      foreach (var order in orders)
      {
        response.Items.Add(await CreateOrder(order));
      }

      return response;
    }

    public override async Task<ResponseMapModel<OrderModel>> DeleteOrders(params OrderModel[] orders)
    {
      var response = new ResponseMapModel<OrderModel>();

      foreach (var order in orders)
      {
        response.Items.Add(await DeleteOrder(order));
      }

      return response;
    }

    /// <summary>
    /// Sync open balance, order, and positions 
    /// </summary>
    /// <returns></returns>
    public async Task GetAccountData()
    {
      var account = await SendData<JsonAccount>($"/v3/accounts/{Account.Descriptor}");
      var positions = await SendData<JsonPosition[]>($"/v3/accounts/{Account.Descriptor}/positions");
      var orders = await SendData<JsonOrder[]>($"/v3/accounts/{Account.Descriptor}/orders");

      //Account.Balance = account.Data.Equity;
      //Account.Descriptor = account.Data.AccountNumber;
      //Account.ActiveOrders = orders.Data.Select(GetInternalOrder).ToDictionary(o => o.Transaction.Id, o => o);
      //Account.ActivePositions = positions.Data.Select(GetInternalPosition).ToDictionary(o => o.Order.Transaction.Id, o => o);

      //Account.ActiveOrders.ForEach(async o => await Subscribe(o.Value.Transaction.Instrument.Name));
      //Account.ActivePositions.ForEach(async o => await Subscribe(o.Value.Order.Transaction.Instrument.Name));
    }

    /// <summary>
    /// Process quote from the stream
    /// </summary>
    /// <param name="content"></param>
    protected void ProcessPoint(string content)
    {
      try
      {
        //var streamPoint = JsonSerializer
        //  .Deserialize<JsonHistoricalQuote[]>(content, _sender.Options)
        //  .FirstOrDefault();

        //var instrument = Account.Instruments.Get(streamPoint.Symbol) ?? new Instrument();
        //var point = new PointModel();

        //point.Ask = streamPoint.AskPrice;
        //point.Bid = streamPoint.BidPrice;
        //point.AskSize = streamPoint.AskSize ?? 0;
        //point.BidSize = streamPoint.BidSize ?? 0;
        //point.Last = streamPoint.BidPrice ?? streamPoint.AskPrice;
        //point.Time = streamPoint.TimestampUtc ?? DateTime.Now;
        //point.TimeFrame = instrument.TimeFrame;
        //point.Instrument = instrument;

        //instrument.Name = streamPoint.Symbol;
        //instrument.Points.Add(point);
        //instrument.PointGroups.Add(point, instrument.TimeFrame, true);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    /// <summary>
    /// Process quote from the stream
    /// </summary>
    /// <param name="content"></param>
    protected void ProcessTrade(string content)
    {
      try
      {
        //var order = JsonSerializer
        //  .Deserialize<JsonHistoricalTrade[]>(content, _sender.Options)
        //  .FirstOrDefault();
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    /// <summary>
    /// Send data to the API
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="verb"></param>
    /// <param name="inputs"></param>
    /// <returns></returns>
    protected async Task<Distribution.Stream.Models.ResponseModel<T>> SendData<T>(
      string source,
      HttpMethod verb = null,
      Hashtable query = null,
      object data = null)
    {
      var uri = new UriBuilder(DataUri)
      {
        Path = source,
        Query = (query ?? []).ToQuery()
      };

      var message = new HttpRequestMessage
      {
        Method = verb ?? HttpMethod.Get
      };

      switch (true)
      {
        case true when Equals(message.Method, HttpMethod.Put):
        case true when Equals(message.Method, HttpMethod.Post):
        case true when Equals(message.Method, HttpMethod.Patch): message.Content = new StringContent(JsonSerializer.Serialize(data ?? new { })); break;
      }

      message.RequestUri = uri.Uri;
      message.Headers.Add("Authorization", Token);

      var service = InstanceService<Service>.Instance;

      return await service.Send<T>(message, service.Options).ConfigureAwait(false);
    }

    /// <summary>
    /// Send order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected async Task<ResponseItemModel<OrderModel>> CreateOrder(OrderModel order)
    {
      var inResponse = new ResponseItemModel<OrderModel>();

      try
      {
        var exOrder = GetExternalOrder(order);
        var exResponse = await SendData<JsonOrder>("/v3/orders", HttpMethod.Post, [], exOrder);

        inResponse.Data = order;
        inResponse.Data.Transaction.Id = $"{exResponse.Data?.Id}";
        inResponse.Data.Transaction.Status = GetInternalStatus(exResponse.Data?.State);

        if (exResponse.Status < 400)
        {
          Account.ActiveOrders.Add(order.Transaction.Id, order);
        }
      }
      catch (Exception e)
      {
        inResponse.Errors.Add(new ErrorModel
        {
          ErrorMessage = e.Message
        });
      }

      return inResponse;
    }


    /// <summary>
    /// Cancel order
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected async Task<ResponseItemModel<OrderModel>> DeleteOrder(OrderModel order)
    {
      await Task.CompletedTask;

      var inResponse = new ResponseItemModel<OrderModel>();

      try
      {
        //var exResponse = await SendData<JsonOrder>($"/v2/orders/{order.Transaction.Id}", HttpMethod.Delete);

        //inResponse.Data = order;
        //inResponse.Data.Transaction.Id = $"{exResponse.Data.OrderId}";
        //inResponse.Data.Transaction.Status = GetInternalStatus(exResponse.Data.OrderStatus);

        //if (exResponse.Status < 400)
        //{
        //  Account.ActiveOrders.Remove(order.Transaction.Id);
        //}
      }
      catch (Exception e)
      {
        inResponse.Errors.Add(new ErrorModel
        {
          ErrorMessage = e.Message
        });
      }

      return inResponse;
    }

    /// <summary>
    /// Convert remote order from brokerage to local record
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected JsonOrder GetExternalOrder(OrderModel order)
    {
      //var action = order.Transaction;
      //var message = new Order
      //{
      //  Quantity = action.Volume,
      //  Symbol = action.Instrument.Name,
      //  TimeInForce = GetExternalTimeSpan(order.TimeSpan.Value),
      //  OrderType = "market"
      //};

      //switch (order.Side)
      //{
      //  case OrderSideEnum.Buy: message.OrderSide = "buy"; break;
      //  case OrderSideEnum.Sell: message.OrderSide = "sell"; break;
      //}

      //switch (order.Type)
      //{
      //  case OrderTypeEnum.Stop: message.StopPrice = order.Price; break;
      //  case OrderTypeEnum.Limit: message.LimitPrice = order.Price; break;
      //  case OrderTypeEnum.StopLimit: message.StopPrice = order.ActivationPrice; message.LimitPrice = order.Price; break;
      //}

      //if (order.Orders.Any())
      //{
      //  message.OrderClass = "bracket";

      //  switch (order.Side)
      //  {
      //    case OrderSideEnum.Buy:
      //      message.StopLoss = GetExternalBracket(order, 1);
      //      message.TakeProfit = GetExternalBracket(order, -1);
      //      break;

      //    case OrderSideEnum.Sell:
      //      message.StopLoss = GetExternalBracket(order, -1);
      //      message.TakeProfit = GetExternalBracket(order, 1);
      //      break;
      //  }
      //}

      return null;
    }

    /// <summary>
    /// Convert local time in force to remote
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    protected string GetExternalTimeSpan(OrderTimeSpanEnum span)
    {
      switch (span)
      {
        case OrderTimeSpanEnum.Day: return "day";
        case OrderTimeSpanEnum.Fok: return "fok";
        case OrderTimeSpanEnum.Gtc: return "gtc";
        case OrderTimeSpanEnum.Ioc: return "ioc";
        case OrderTimeSpanEnum.Omo: return "opg";
        case OrderTimeSpanEnum.Omc: return "cls";
      }

      return null;
    }

    /// <summary>
    /// Convert remote order to local
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected OrderModel GetInternalOrder(JsonOrder order)
    {
      //var instrument = new Terminal.Core.Domains.Instrument
      //{
      //  Name = order.Symbol
      //};

      //var action = new TransactionModel
      //{
      //  Id = $"{order.OrderId}",
      //  Descriptor = order.ClientOrderId,
      //  Instrument = instrument,
      //  CurrentVolume = order.FilledQuantity,
      //  Volume = order.Quantity,
      //  Time = order.CreatedAtUtc,
      //  Status = GetInternalStatus(order.OrderStatus)
      //};

      //var inOrder = new OrderModel
      //{
      //  Transaction = action,
      //  Type = OrderTypeEnum.Market,
      //  Side = GetInternalOrderSide(order.OrderSide),
      //  TimeSpan = GetInternalTimeSpan(order.TimeInForce)
      //};

      //switch (order.OrderType)
      //{
      //  case "stop":
      //    inOrder.Type = OrderTypeEnum.Stop;
      //    inOrder.Price = order.StopPrice;
      //    break;

      //  case "limit":
      //    inOrder.Type = OrderTypeEnum.Limit;
      //    inOrder.Price = order.LimitPrice;
      //    break;

      //  case "stop_limit":
      //    inOrder.Type = OrderTypeEnum.StopLimit;
      //    inOrder.Price = order.StopPrice;
      //    inOrder.ActivationPrice = order.LimitPrice;
      //    break;
      //}

      //return inOrder;

      return null;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected PositionModel GetInternalPosition(JsonPosition position)
    {
      //var instrument = new Terminal.Core.Domains.Instrument
      //{
      //  Name = position.Instrument
      //};

      //var action = new TransactionModel
      //{
      //  Descriptor = position.Instrument,
      //  Instrument = instrument,
      //  CurrentVolume = position.AvailableQuantity,
      //  Volume = position.Long
      //};

      //var order = new OrderModel
      //{
      //  Transaction = action,
      //  Type = OrderTypeEnum.Market,
      //  Side = GetInternalPositionSide(position)
      //};

      //var gainLossPoints = position.AverageEntryPrice - position.AssetCurrentPrice;
      //var gainLoss = position.CostBasis - position.MarketValue;

      //return new PositionModel
      //{
      //  GainLossPointsMax = gainLossPoints,
      //  GainLossPointsMin = gainLossPoints,
      //  GainLossPoints = gainLossPoints,
      //  GainLossMax = gainLoss,
      //  GainLossMin = gainLoss,
      //  GainLoss = gainLoss,
      //  Order = order,
      //  Orders = [order]
      //};

      return null;
    }

    /// <summary>
    /// Convert remote order status to local
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    protected OrderStatusEnum GetInternalStatus(string status)
    {
      switch (status)
      {
        case "fill":
        case "filled": return OrderStatusEnum.Filled;
        case "partial_fill":
        case "partially_filled": return OrderStatusEnum.Partitioned;
        case "stopped":
        case "expired":
        case "rejected":
        case "canceled":
        case "done_for_day": return OrderStatusEnum.Canceled;
        case "new":
        case "held":
        case "accepted":
        case "suspended":
        case "pending_new":
        case "pending_cancel":
        case "pending_replace": return OrderStatusEnum.Pending;
      }

      return OrderStatusEnum.None;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    protected OrderSideEnum GetInternalOrderSide(string side)
    {
      switch (side)
      {
        case "buy": return OrderSideEnum.Buy;
        case "sell": return OrderSideEnum.Sell;
      }

      return OrderSideEnum.None;
    }

    /// <summary>
    /// Convert remote position side to local
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected OrderSideEnum GetInternalPositionSide(JsonPosition position)
    {
      var buy = position.Long.Units.GetValueOrDefault();
      var sell = position.Short.Units.GetValueOrDefault();

      switch (true)
      {
        case true when buy.IsEqual(0) is false: return OrderSideEnum.Buy;
        case true when sell.IsEqual(0) is false: return OrderSideEnum.Sell;
      }

      return OrderSideEnum.None;
    }

    /// <summary>
    /// Convert remote time in force to local
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    protected OrderTimeSpanEnum GetInternalTimeSpan(string span)
    {
      switch (span)
      {
        case "day": return OrderTimeSpanEnum.Day;
        case "fok": return OrderTimeSpanEnum.Fok;
        case "gtc": return OrderTimeSpanEnum.Gtc;
        case "ioc": return OrderTimeSpanEnum.Ioc;
        case "opg": return OrderTimeSpanEnum.Omo;
        case "cls": return OrderTimeSpanEnum.Omc;
      }

      return OrderTimeSpanEnum.None;
    }
  }
}
