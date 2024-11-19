using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class WebSocketMessage<T> where T : SocketEventMessage
  {
    [JsonPropertyName("channel")]
    public string Channel { get; set; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("sequence_num")]
    public string SequenceNumber { get; set; }

    [JsonPropertyName("events")]
    public List<T> Events { get; set; }
  }
}
