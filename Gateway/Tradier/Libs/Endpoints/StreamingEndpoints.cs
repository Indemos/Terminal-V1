using System.Net.Http;
using System.Threading.Tasks;
using Tradier.Messages;

namespace Tradier
{
  public partial class Adapter
  {
    public async Task<SessionMessage> GetMarketSession()
    {
      var uri = $"{SessionUri}/markets/events/session";
      var response = await Send<SessionMessage>($"{SessionUri}/markets/events/session", HttpMethod.Post, null, SessionToken);
      return response.Data;
    }

    public async Task<SessionMessage> GetAccountSession()
    {
      var uri = $"{SessionUri}/accounts/events/session";
      var response = await Send<SessionMessage>($"{SessionUri}/accounts/events/session", HttpMethod.Post, null, SessionToken);
      return response.Data;
    }
  }
}
