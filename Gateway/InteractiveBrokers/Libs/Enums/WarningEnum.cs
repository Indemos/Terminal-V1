namespace InteractiveBrokers.Enums
{
  public enum WarningEnum : int
  {
    /// <summary>
    /// New account data requested from TWS. API client has been unsubscribed from account data.
    /// </summary>
    AccountDataRequested = 2100,

    /// <summary>
    /// Unable to subscribe to account as the following clients are subscribed to a different account.
    /// </summary>
    UnableToSubscribeToAccount = 2101,

    /// <summary>
    /// Unable to modify this order as it is still being processed.
    /// </summary>
    UnableToModifyOrder = 2102,

    /// <summary>
    /// A market data farm is disconnected.
    /// </summary>
    MarketDataFarmDisconnected = 2103,

    /// <summary>
    /// Market data farm connection is OK
    /// </summary>
    MarketDataFarmConnected = 2104,

    /// <summary>
    /// Sec-def data farm connection is OK
    /// </summary>
    SecDefDataFarmConnected = 2158,

    /// <summary>
    /// A historical data farm is disconnected.
    /// </summary>
    HistoricalDataFarmDisconnected = 2105,

    /// <summary>
    /// A historical data farm is connected.
    /// </summary>
    HistoricalDataFarmConnected = 2106,

    /// <summary>
    /// A historical data farm connection has become inactive but should be available upon demand.
    /// </summary>
    HistoricalDataFarmInactive = 2107,

    /// <summary>
    /// A market data farm connection has become inactive but should be available upon demand.
    /// </summary>
    MarketDataFarmInactive = 2108,

    /// <summary>
    /// Order Event Warning: Attribute "Outside Regular Trading Hours" is ignored based on the order type and destination. PlaceOrder is now processed.
    /// </summary>
    OutsideRthIgnored = 2109,

    /// <summary>
    /// Connectivity between TWS and server is broken. It will be restored automatically.
    /// </summary>
    ConnectivityBroken = 2110,

    /// <summary>
    /// Cross Side Warning
    /// </summary>
    CrossSideWarning = 2137,

    /// <summary>
    /// Etrade Only Not Supported Warning
    /// </summary>
    EtradeOnlyNotSupported = 2168,

    /// <summary>
    /// Firm Quote Only Not Supported Warning
    /// </summary>
    FirmQuoteOnlyNotSupported = 2169
  }
}
