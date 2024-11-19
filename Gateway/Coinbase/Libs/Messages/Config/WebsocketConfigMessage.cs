namespace Coinbase.Messages
{
  public class WebsocketConfigMessage
  {
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string WebSocketUrl { get; set; }
  }
}
