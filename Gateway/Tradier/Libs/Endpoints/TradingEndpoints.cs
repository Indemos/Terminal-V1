using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Terminal.Core.Enums;
using Terminal.Core.Extensions;
using Terminal.Core.Models;
using Tradier.Mappers;
using Tradier.Messages.Trading;

namespace Tradier
{
  public partial class Adapter
  {
    /// <summary>
    /// Place an order to trade a single option
    /// </summary>
    public async Task<OrderResponseMessage> SendOptionOrder(OrderModel order, bool preview = false)
    {
      var data = new Hashtable
      {
        { "class", "option" },
        { "symbol", order.BasisName ?? order.Name },
        { "option_symbol", GetOptionName(order) },
        { "side", ExternalMap.GetSide(order, Account) },
        { "quantity", order.Volume },
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", order.Price },
        { "stop", order.ActivationPrice ?? order.Price },
        { "tag", order.Descriptor },
        { "preview", preview }
      };

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Post);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a multileg order with up to 4 legs
    /// </summary>
    public async Task<OrderResponseMessage> SendGroupOrder(OrderModel order, bool preview = false)
    {
      var index = 0;
      var data = new Hashtable
      {
        { "class", "multileg" },
        { "symbol", order.BasisName ?? order.Name },
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", order.Price },
        { "tag", order.Descriptor }
      };

      foreach (var item in order.Orders)
      {
        data[$"option_symbol[{index}]"] = GetOptionName(item);
        data[$"side[{index}]"] = ExternalMap.GetSide(item, Account);
        data[$"quantity[{index}]"] = item.Volume;

        index++;
      }

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Post);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place an order to trade an equity security
    /// </summary>
    public async Task<OrderResponseMessage> SendEquityOrder(OrderModel order, bool preview = false)
    {
      var data = new Hashtable
      {
        { "account_id", Account.Descriptor },
        { "class", "equity" },
        { "symbol", order.BasisName ?? order.Name },
        { "side", ExternalMap.GetSide(order, Account) },
        { "quantity", order.Volume},
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", order.Price },
        { "stop", order.ActivationPrice ?? order.Price },
        { "preview", preview },
        { "tag", order.Descriptor }
      };

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Post);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a combo order. This is a specialized type of order consisting of one equity leg and one option leg
    /// </summary>
    public async Task<OrderResponseMessage> SendComboOrder(OrderModel order, bool preview = false)
    {
      var index = 0;
      var data = new Hashtable
      {
        { "class", "combo" },
        { "symbol", order.BasisName ?? order.Name },
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", order.Price },
        { "tag", order.Descriptor }
      };

      foreach (var item in order.Orders)
      {
        data[$"option_symbol[{index}]"] = GetOptionName(item);
        data[$"side[{index}]"] = ExternalMap.GetSide(item, Account);
        data[$"quantity[{index}]"] = item.Volume;

        index++;
      }

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Post);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a one-triggers-other order. This order type is composed of two separate orders sent simultaneously
    /// </summary>
    public async Task<OrderResponseMessage> SendOtoOrder(OrderModel order, bool preview = false)
    {
      var index = 0;
      var data = new Hashtable
      {
        { "class", "oto" },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "preview", preview },
        { "tag", order.Descriptor }
      };

      var subOrders = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Brace)
        .Prepend(order);

      foreach (var item in subOrders)
      {
        data[$"symbol[{index}]"] = item.BasisName ?? item.Name;
        data[$"quantity[{index}]"] = item.Volume;
        data[$"type[{index}]"] = ExternalMap.GetOrderType(item.Type);
        data[$"option_symbol[{index}]"] = GetOptionName(item);
        data[$"side[{index}]"] = ExternalMap.GetSide(order, Account);
        data[$"price[{index}]"] = item.Price;
        data[$"stop[{index}]"] = item.ActivationPrice ?? item.Price;

        index++;
      }

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Post);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a one-cancels-other order. This order type is composed of two separate orders sent simultaneously
    /// </summary>
    public async Task<OrderResponseMessage> SendOcoOrder(OrderModel order, bool preview = false)
    {
      var index = 0;
      var data = new Hashtable
      {
        { "class", "oco" },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "preview", preview },
        { "tag", order.Descriptor }
      };

      var subOrders = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Brace)
        .Prepend(order);

      foreach (var item in subOrders)
      {
        data[$"symbol[{index}]"] = item.BasisName ?? item.Name;
        data[$"quantity[{index}]"] = item.Volume;
        data[$"type[{index}]"] = ExternalMap.GetOrderType(item.Type);
        data[$"option_symbol[{index}]"] = GetOptionName(item);
        data[$"side[{index}]"] = ExternalMap.GetSide(order, Account);
        data[$"price[{index}]"] = item.Price;
        data[$"stop[{index}]"] = item.ActivationPrice ?? item.Price;

        index++;
      }

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Post);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a one-triggers-one-cancels-other order. This order type is composed of three separate orders sent simultaneously
    /// </summary>
    public async Task<OrderResponseMessage> SendBraceOrder(OrderModel order, bool preview = false)
    {
      var index = 0;
      var data = new Hashtable
      {
        { "class", "otoco" },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "preview", preview },
        { "tag", order.Descriptor }
      };

      var subOrders = order
        .Orders
        .Where(o => o.Instruction is InstructionEnum.Brace)
        .Prepend(order);

      foreach (var item in subOrders)
      {
        data[$"symbol[{index}]"] = item.BasisName ?? item.Name;
        data[$"quantity[{index}]"] = item.Volume;
        data[$"type[{index}]"] = ExternalMap.GetOrderType(item.Type);
        data[$"option_symbol[{index}]"] = GetOptionName(item);
        data[$"side[{index}]"] = ExternalMap.GetSide(item, Account);
        data[$"price[{index}]"] = item.Price;
        data[$"stop[{index}]"] = item.ActivationPrice ?? item.Price;

        index++;
      }

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Post);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Modify an order. You may change some or all of these parameters.
    /// </summary>
    public async Task<OrderResponseMessage> UpdateOrder(string orderId, string type = null, string duration = null, double? price = null, double? stop = null)
    {
      var data = new Hashtable
      {
        { "type", type },
        { "duration", duration },
        { "price", price },
        { "stop", stop }
      };

      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders/{orderId}?{data.Compact()}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Put);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Cancel an order using the default account number
    /// </summary>
    public virtual async Task<OrderResponseMessage> DeleteOrder(string orderId)
    {
      var source = $"{DataUri}/accounts/{Account.Descriptor}/orders/{orderId}";
      var response = await Send<OrderResponseCoreMessage>(source, HttpMethod.Delete);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Return name only for options
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    protected virtual string GetOptionName(OrderModel order)
    {
      switch (order.Transaction.Instrument.Type)
      {
        case InstrumentEnum.Options: 
        case InstrumentEnum.FutureOptions: return order.Name;
      }

      return null;
    }
  }
}
