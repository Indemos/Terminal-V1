namespace Terminal.Core.Enums
{
  public enum OrderStatusEnum : byte
  {
    None = 0,
    Filled = 1,
    Pending = 2,
    Canceled = 3,
    Partitioned = 4
  }
}
