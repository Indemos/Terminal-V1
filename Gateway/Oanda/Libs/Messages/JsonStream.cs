namespace Oanda.Messages
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public partial class JsonStream
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("asks")]
    public List<JsonTick> Asks { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("bids")]
    public List<JsonTick> Bids { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("closeoutAsk")]
    public string CloseoutAsk { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("closeoutBid")]
    public string CloseoutBid { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("instrument")]
    public string Instrument { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }
  }

  public partial class JsonTick
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("liquidity")]
    public long? Liquidity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("price")]
    public string Price { get; set; }
  }
}
