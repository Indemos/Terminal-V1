using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
      var data = new Dictionary<string, string>
      {
        { "class", "option" },
        { "symbol", order.BasisName },
        { "option_symbol", order.Name },
        { "side", ExternalMap.GetSide(order.Side) },
        { "quantity", $"{order.Volume}" },
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", $"{order.Price}" },
        { "stop", $"{order.ActivationPrice ?? order.Price}" },
        { "tag", $"{order.Descriptor}" },
        { "preview", $"{preview}" }
      };

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Post, data);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a multileg order with up to 4 legs
    /// </summary>
    public async Task<OrderResponseMessage> SendGroupOrder(OrderModel order, bool preview = false)
    {
      var data = new Dictionary<string, string>
      {
        { "class", "multileg" },
        { "symbol", order.Name },
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", $"{order.Price}" }
      };

      var index = 0;

      foreach (var item in order.Orders)
      {
        data.Add($"option_symbol[{index}]", item.Name);
        data.Add($"side[{index}]", ExternalMap.GetSide(item.Side));
        data.Add($"quantity[{index}]", $"{item.Volume}");

        index++;
      }

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Post, data);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place an order to trade an equity security
    /// </summary>
    public async Task<OrderResponseMessage> SendEquityOrder(OrderModel order, bool preview = false)
    {
      var data = new Dictionary<string, string>
      {
        { "account_id", Account.Descriptor },
        { "class", "equity" },
        { "symbol", order.Name },
        { "side", ExternalMap.GetSide(order.Side) },
        { "quantity", $"{order.Volume}"},
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", $"{order.Price}" },
        { "stop", $"{order.ActivationPrice ?? order.Price}" },
        { "preview", $"{preview}" }
      };

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Post, data);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a combo order. This is a specialized type of order consisting of one equity leg and one option leg
    /// </summary>
    public async Task<OrderResponseMessage> SendComboOrder(OrderModel order, bool preview = false)
    {
      var data = new Dictionary<string, string>
      {
        { "class", "combo" },
        { "symbol", order.Name },
        { "type", ExternalMap.GetOrderType(order.Type) },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "price", $"{order.Price}" },
      };

      var index = 0;

      foreach (var item in order.Orders)
      {
        data.Add($"option_symbol[{index}]", item.Name);
        data.Add($"side[{index}]", ExternalMap.GetSide(item.Side));
        data.Add($"quantity[{index}]", $"{item.Volume}");

        index++;
      }

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Post, data);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a one-triggers-other order. This order type is composed of two separate orders sent simultaneously
    /// </summary>
    public async Task<OrderResponseMessage> SendOtoOrder(OrderModel order, bool preview = false)
    {
      var data = new Dictionary<string, string>
      {
        { "class", "oto" },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "preview", $"{preview}" }
      };

      var index = 0;

      foreach (var item in order.Orders)
      {
        data.Add($"symbol[{index}]", item.BasisName);
        data.Add($"quantity[{index}]", $"{item.Volume}");
        data.Add($"type[{index}]", ExternalMap.GetOrderType(item.Type));
        data.Add($"option_symbol[{index}]", item.Name);
        data.Add($"side[{index}]", ExternalMap.GetSide(order.Side));
        data.Add($"price[{index}]", $"{item.Price}");
        data.Add($"stop[{index}]", $"{item.ActivationPrice ?? item.Price}");

        index++;
      }

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Post, data);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a one-cancels-other order. This order type is composed of two separate orders sent simultaneously
    /// </summary>
    public async Task<OrderResponseMessage> SendOcoOrder(OrderModel order, bool preview = false)
    {
      var data = new Dictionary<string, string>
      {
        { "class", "oco" },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "preview", $"{preview}" }
      };

      var index = 0;

      foreach (var item in order.Orders)
      {
        data.Add($"symbol[{index}]", item.BasisName);
        data.Add($"quantity[{index}]", $"{item.Volume}");
        data.Add($"type[{index}]", ExternalMap.GetOrderType(item.Type));
        data.Add($"option_symbol[{index}]", item.Name);
        data.Add($"side[{index}]", ExternalMap.GetSide(order.Side));
        data.Add($"price[{index}]", $"{item.Price}");
        data.Add($"stop[{index}]", $"{item.ActivationPrice ?? item.Price}");

        index++;
      }

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Post, data);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Place a one-triggers-one-cancels-other order. This order type is composed of three separate orders sent simultaneously
    /// </summary>
    public async Task<OrderResponseMessage> SendOtocoOrder(OrderModel order, bool preview = false)
    {
      var data = new Dictionary<string, string>
      {
        { "class", "otoco" },
        { "duration", ExternalMap.GetTimeSpan(order.TimeSpan) },
        { "preview", $"{preview}" }
      };

      var index = 0;

      foreach (var item in order.Orders)
      {
        data.Add($"symbol[{index}]", item.BasisName);
        data.Add($"quantity[{index}]", $"{item.Volume}");
        data.Add($"type[{index}]", ExternalMap.GetOrderType(item.Type));
        data.Add($"option_symbol[{index}]", item.Name);
        data.Add($"side[{index}]", ExternalMap.GetSide(item.Side));
        data.Add($"price[{index}]", $"{item.Price}");
        data.Add($"stop[{index}]", $"{item.ActivationPrice ?? item.Price}");

        index++;
      }

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Post, data);

      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Modify an order. You may change some or all of these parameters.
    /// </summary>
    public async Task<OrderResponseMessage> UpdateOrder(string orderId, string type = null, string duration = null, double? price = null, double? stop = null)
    {
      var data = new Dictionary<string, string>
      {
        { "type", type },
        { "duration", duration },
        { "price", $"{price}" },
        { "stop", $"{stop}" },
      };

      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders/{orderId}";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Put, data);
      return response.Data?.OrderReponse;
    }

    /// <summary>
    /// Cancel an order using the default account number
    /// </summary>
    public async Task<OrderResponseMessage> DeleteOrder(string orderId)
    {
      var uri = $"{DataUri}/accounts/{Account.Descriptor}/orders/{orderId}";
      var response = await Send<OrderResponseCoreMessage>(uri, HttpMethod.Delete);
      return response.Data?.OrderReponse;
    }
  }
}
