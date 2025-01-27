using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Terminal.Core.Extensions;
using Tradier.Messages.Account;

namespace Tradier
{
  /// <summary>
  /// The <c>Account</c> class. 
  /// </summary>
  public partial class Adapter
  {
    /// <summary>
    /// The user’s profile contains information pertaining to the user and his/her accounts
    /// </summary>
    public async Task<ProfileMessage> GetUserProfile()
    {
      var source = $"{DataUri}/user/profile";
      var response = await Send<ProfileCoreMessage>(source);
      return response.Data?.Profile;
    }

    /// <summary>
    /// Get balances information for a specific or a default user account.
    /// </summary>
    public async Task<BalanceMessage> GetBalances(string accountNumber)
    {
      var source = $"{DataUri}/accounts/{accountNumber}/balances";
      var response = await Send<BalanceCoreMessage>(source);
      return response.Data?.Balance;
    }

    /// <summary>
    /// Get the current positions being held in an account. These positions are updated intraday via trading
    /// </summary>
    public async Task<IList<PositionMessage>> GetPositions(string accountNumber)
    {
      var source = $"{DataUri}/accounts/{accountNumber}/positions";
      var response = await Send<PositionsCoreMessage>(source);
      return response.Data?.Positions?.Items;
    }

    /// <summary>
    /// Get historical activity for an account
    /// </summary>
    public async Task<HistoryMessage> GetHistory(string accountNumber, int page = 1, int itemsPerPage = 25)
    {
      var data = new Hashtable
      {
        { "page", page },
        { "limit", itemsPerPage },
      };

      var source = $"{DataUri}/accounts/{accountNumber}/history?{data.Compact()}";
      var response = await Send<HistoryCoreMessage>(source);

      return response.Data?.History;
    }

    /// <summary>
    /// Get cost basis information for a specific user account
    /// </summary>
    public async Task<GainLossMessage> GetGainLoss(string accountNumber, int page = 1, int itemsPerPage = 25)
    {
      var data = new Hashtable
      {
        { "page", page },
        { "limit", itemsPerPage },
      };

      var source = $"{DataUri}/accounts/{accountNumber}/gainloss?{data.Compact()}";
      var response = await Send<GainLossCoreMessage>(source);

      return response.Data?.GainLoss;
    }

    /// <summary>
    /// Retrieve orders placed within an account
    /// </summary>
    public async Task<IList<OrderMessage>> GetOrders(string accountNumber)
    {
      var source = $"{DataUri}/accounts/{accountNumber}/orders";
      var response = await Send<OrdersCoreMessage>(source);
      return response.Data?.Orders?.Items;
    }

    /// <summary>
    /// Get detailed information about a previously placed order
    /// </summary>
    public async Task<OrderMessage> GetOrder(string accountNumber, int orderId)
    {
      var source = $"{DataUri}/accounts/{accountNumber}/orders/{orderId}";
      var response = await Send<OrdersCoreMessage>(source);
      return response.Data?.Orders?.Items?.FirstOrDefault();
    }
  }
}
