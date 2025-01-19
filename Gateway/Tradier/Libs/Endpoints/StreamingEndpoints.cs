using System.Net.Http;
using System.Threading.Tasks;
using Tradier.Messages;

namespace Tradier.Endpoints
{
  public class StreamingEndpoints
  {
    public Adapter Adapter { get; set; }

    public async Task<SessionMessage> GetMarketSession()
    {
      var uri = $"{Adapter.SessionUri}/markets/events/session";
      var response = await Adapter.Send<SessionMessage>(uri, HttpMethod.Post, null, Adapter.SessionToken);
      return response.Data;
    }

    public async Task<SessionMessage> GetAccountSession()
    {
      var uri = $"{Adapter.SessionUri}/accounts/events/session";
      var response = await Adapter.Send<SessionMessage>(uri, HttpMethod.Post, null, Adapter.SessionToken);
      return response.Data;
    }
  }
}
