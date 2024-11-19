namespace Coinbase.Messages
{
  public class ResponseMessage<T>
  {
    public T Data { get; set; }
    public bool Success { get; set; }
    public string ExceptionType { get; set; }
    public string ExceptionMessage { get; set; }
    public string ExceptionDetails { get; set; }
  }
}
