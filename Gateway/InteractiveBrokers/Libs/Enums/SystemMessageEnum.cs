namespace InteractiveBrokers.Enums
{
  public enum SystemMessageEnum : int
  {
    /// <summary>
    /// Connectivity between IB and the TWS has been lost.
    /// </summary>
    ConnectivityLost = 1100,

    /// <summary>
    /// Connectivity between IB and TWS has been restored - data lost.
    /// </summary>
    ConnectivityRestoredWithoutData = 1101,

    /// <summary>
    /// Connectivity between IB and TWS has been restored - data maintained.
    /// </summary>
    ConnectivityRestoredWithData = 1102,

    /// <summary>
    /// TWS socket port has been reset and this connection is being dropped. 
    /// Please reconnect on the new port.
    /// </summary>
    PortReset = 1300
  }
}
