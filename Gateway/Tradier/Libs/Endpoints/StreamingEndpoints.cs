using System.Net.Http;
using System.Threading.Tasks;
using Tradier.Messages.Stream;

namespace Tradier
{
  public partial class Adapter
  {
    public async Task<SessionMessage> GetMarketSession()
    {
      var source = $"{SessionUri}/markets/events/session";
      var response = await Send<SessionMessage>(source, HttpMethod.Post, null, SessionToken);
      return response.Data;
    }

    public async Task<SessionMessage> GetAccountSession()
    {
      var source = $"{SessionUri}/accounts/events/session";
      var response = await Send<SessionMessage>(source, HttpMethod.Post, null, SessionToken);
      return response.Data;
    }
  }
}
