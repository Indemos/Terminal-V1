using System.Linq;
using System.Threading.Tasks;
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
      var uri = $"{DataUri}/user/profile";
      var response = await Send<ProfileCoreMessage>(uri);
      return response.Data?.Profile;
    }

    /// <summary>
    /// Get balances information for a specific or a default user account.
    /// </summary>
    public async Task<BalanceMessage> GetBalances(string accountNumber)
    {
      var uri = $"{DataUri}/accounts/{accountNumber}/balances";
      var response = await Send<BalanceCoreMessage>(uri);
      return response.Data?.Balance;
    }

    /// <summary>
    /// Get the current positions being held in an account. These positions are updated intraday via trading
    /// </summary>
    public async Task<PositionsMessage> GetPositions(string accountNumber)
    {
      var uri = $"{DataUri}/accounts/{accountNumber}/positions";
      var response = await Send<PositionsCoreMessage>(uri);
      return response.Data?.Positions;
    }

    /// <summary>
    /// Get historical activity for an account
    /// </summary>
    public async Task<HistoryMessage> GetHistory(string accountNumber, int page = 1, int limitPerPage = 25)
    {
      var uri = $"{DataUri}/accounts/{accountNumber}/history?page={page}&limit={limitPerPage}";
      var response = await Send<HistoryCoreMessage>(uri);
      return response.Data?.History;
    }

    /// <summary>
    /// Get cost basis information for a specific user account
    /// </summary>
    public async Task<GainLossMessage> GetGainLoss(string accountNumber, int page = 1, int limitPerPage = 25)
    {
      var uri = $"{DataUri}/accounts/{accountNumber}/gainloss?page={page}&limit={limitPerPage}";
      var response = await Send<GainLossCoreMessage>(uri);
      return response.Data?.GainLoss;
    }

    /// <summary>
    /// Retrieve orders placed within an account
    /// </summary>
    public async Task<OrdersMessage> GetOrders(string accountNumber)
    {
      var uri = $"{DataUri}/accounts/{accountNumber}/orders";
      var response = await Send<OrdersCoreMessage>(uri);
      return response.Data?.Orders;
    }

    /// <summary>
    /// Get detailed information about a previously placed order
    /// </summary>
    public async Task<OrderMessage> GetOrder(string accountNumber, int orderId)
    {
      var uri = $"{DataUri}/accounts/{accountNumber}/orders/{orderId}";
      var response = await Send<OrdersCoreMessage>(uri);
      return response.Data?.Orders?.Items.FirstOrDefault();
    }
  }
}
