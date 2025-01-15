namespace InteractiveBrokers.Enums
{
  public enum ClientErrorEnum : int
  {
    /// <summary>
    /// Already Connected.
    /// </summary>
    AlreadyConnected = 501,

    /// <summary>
    /// Couldn't connect to TWS.
    /// </summary>
    ConnectionError = 502,

    /// <summary>
    /// The TWS is out of date and must be upgraded.
    /// </summary>
    UpgradeRequired = 503,

    /// <summary>
    /// Not connected.
    /// </summary>
    NoConnection = 504,

    /// <summary>
    /// An operation was attempted on something that is not a socket. (WinError 10038)
    /// </summary>
    InvalidSocketOperation = 10038
  }
}
