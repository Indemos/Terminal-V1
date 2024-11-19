/* Copyright (C) 2024 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IBApi
{
  /**
   * @class EClient
   * @brief TWS/Gateway client class
   * This client class contains all the available methods to communicate with IB. Up to thirty-two clients can be connected to a single instance of the TWS/Gateway simultaneously. From herein, the TWS/Gateway will be referred to as the Host.
   */
  public abstract class EClient
  {
    protected int serverVersion;

    protected ETransport socketTransport;

    protected EWrapper wrapper;

    protected volatile bool isConnected;
    protected int clientId;
    protected bool extraAuth;
    protected bool useV100Plus = true;

    internal bool UseV100Plus => useV100Plus;

    private string connectOptions = "";
    protected bool allowRedirect;

    /**
     * @brief Constructor
     * @param wrapper EWrapper's implementing class instance. Every message being delivered by IB to the API client will be forwarded to the EWrapper's implementing class.
     * @sa EWrapper
     */
    public EClient(EWrapper wrapper)
    {
      this.wrapper = wrapper;
      clientId = -1;
      extraAuth = false;
      isConnected = false;
      optionalCapabilities = "";
      AsyncEConnect = false;
    }

    /**
     * @brief Ignore. Used for IB's internal purposes.
     */
    public void SetConnectOptions(string connectOptions)
    {
      if (IsConnected())
      {
        wrapper.error(clientId, EClientErrors.AlreadyConnected.Code, EClientErrors.AlreadyConnected.Message, "");
        return;
      }

      this.connectOptions = connectOptions;
    }

    public void SetOptionalCapabilities(string optionalCapabilities)
    {
      this.optionalCapabilities = optionalCapabilities;
    }

    /**
     * @brief Allows to switch between different current (V100+) and previous connection mechanisms.
     */
    public void DisableUseV100Plus()
    {
      useV100Plus = false;
      connectOptions = "";
    }

    /**
     * @brief Reference to the EWrapper implementing object.
     */
    public EWrapper Wrapper => wrapper;

    public bool AllowRedirect
    {
      get => allowRedirect;
      set => allowRedirect = value;
    }

    /**
     * @brief returns the Host's version. Some of the API functionality might not be available in older Hosts and therefore it is essential to keep the TWS/Gateway as up to date as possible.
     */
    public int ServerVersion => serverVersion;

    /**
     * @brief Indicates whether the API-TWS connection has been closed.
     * Note: This function is not automatically invoked and must be by the API client.
     * @returns true if connection has been established, false if it has not.
     */
    public bool IsConnected() => isConnected;

    public string ServerTime { get; protected set; }

    private static readonly string encodedVersion = Constants.MinVersion + (Constants.MaxVersion != Constants.MinVersion ? $"..{Constants.MaxVersion}" : string.Empty);
    protected Stream tcpStream;

    protected abstract uint prepareBuffer(BinaryWriter paramsList);

    protected void sendConnectRequest()
    {
      try
      {
        if (useV100Plus)
        {
          var paramsList = new BinaryWriter(new MemoryStream());

          paramsList.AddParameter("API");

          var lengthPos = prepareBuffer(paramsList);

          paramsList.Write(Encoding.ASCII.GetBytes($"v{encodedVersion}{' '}{connectOptions}"));

          CloseAndSend(paramsList, lengthPos);
        }
        else
        {
          var buf = new List<byte>();

          buf.AddRange(Encoding.UTF8.GetBytes(Constants.ClientVersion.ToString()));
          buf.Add(Constants.EOL);
          socketTransport.Send(new EMessage(buf.ToArray()));
        }
      }
      catch (IOException)
      {
        wrapper.error(clientId, EClientErrors.CONNECT_FAIL.Code, EClientErrors.CONNECT_FAIL.Message, "");
        throw;
      }
    }

    protected void validateInvalidSymbols(string host)
    {
      if (host != null && !IBParamsList.isAsciiPrintable(host))
      {
        throw new EClientException(EClientErrors.INVALID_SYMBOL, host);
      }
      if (connectOptions != null && !IBParamsList.isAsciiPrintable(connectOptions))
      {
        throw new EClientException(EClientErrors.INVALID_SYMBOL, connectOptions);
      }
      if (optionalCapabilities != null && !IBParamsList.isAsciiPrintable(optionalCapabilities))
      {
        throw new EClientException(EClientErrors.INVALID_SYMBOL, optionalCapabilities);
      }
    }
    /**
     * @brief Initiates the message exchange between the client application and the TWS/IB Gateway
     */
    public void startApi()
    {
      if (!CheckConnection()) return;

      const int VERSION = 2;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.StartApi);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(clientId);

        if (serverVersion >= MinServerVer.OPTIONAL_CAPABILITIES) paramsList.AddParameter(optionalCapabilities);
      }
      catch (EClientException e)
      {
        wrapper.error(IncomingMessage.NotValid, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(paramsList, lengthPos);
    }


    public string optionalCapabilities { get; set; }

    /**
     * @brief Terminates the connection and notifies the EWrapper implementing class.
     * @sa EWrapper::connectionClosed, eDisconnect
     */
    public void Close() => eDisconnect();

    /**
     * @brief Closes the socket connection and terminates its thread.
     */
    public virtual void eDisconnect(bool resetState = true)
    {
      if (socketTransport == null) return;

      if (resetState)
      {
        isConnected = false;
        extraAuth = false;
        clientId = -1;
        serverVersion = 0;
        optionalCapabilities = "";
      }

      tcpStream?.Close();

      if (resetState) wrapper.connectionClosed();
    }

    /**
     * @brief Requests completed orders.\n
     * @param apiOnly - request only API orders.\n
     * @sa EWrapper::completedOrder, EWrapper::completedOrdersEnd
     */
    public void reqCompletedOrders(bool apiOnly)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.COMPLETED_ORDERS, " It does not support completed orders requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.ReqCompletedOrders);
      paramsList.AddParameter(apiOnly);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQCOMPLETEDORDERS);
    }

    /**
     * @brief Cancels tick-by-tick data.\n
     * @param reqId - unique identifier of the request.\n
     */
    public void cancelTickByTickData(int requestId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.TICK_BY_TICK, " It does not support tick-by-tick cancels.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelTickByTickData);
      paramsList.AddParameter(requestId);

      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_CANCELTICKBYTICKDATA);
    }

    /**
     * @brief Requests tick-by-tick data.\n
     * @param reqId - unique identifier of the request.\n
     * @param contract - the contract for which tick-by-tick data is requested.\n
     * @param tickType - tick-by-tick data type: "Last", "AllLast", "BidAsk" or "MidPoint".\n
     * @param numberOfTicks - number of ticks.\n
     * @param ignoreSize - ignore size flag.\n
     * @sa EWrapper::tickByTickAllLast, EWrapper::tickByTickBidAsk, EWrapper::tickByTickMidPoint, Contract
     */
    public void reqTickByTickData(int requestId, Contract contract, string tickType, int numberOfTicks, bool ignoreSize)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.TICK_BY_TICK, " It does not support tick-by-tick requests.")) return;
      if ((numberOfTicks != 0 || ignoreSize) && !CheckServerVersion(MinServerVer.TICK_BY_TICK_IGNORE_SIZE, " It does not support ignoreSize and numberOfTicks parameters in tick-by-tick requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqTickByTickData);
        paramsList.AddParameter(requestId);
        paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        paramsList.AddParameter(contract.TradingClass);
        paramsList.AddParameter(tickType);

        if (serverVersion >= MinServerVer.TICK_BY_TICK_IGNORE_SIZE)
        {
          paramsList.AddParameter(numberOfTicks);
          paramsList.AddParameter(ignoreSize);
        }
      }
      catch (EClientException e)
      {
        wrapper.error(requestId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQTICKBYTICKDATA);
    }

    /**
     * @brief Cancels a historical data request.
     * @param reqId the request's identifier.
     * @sa reqHistoricalData
     */
    public void cancelHistoricalData(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(24, " It does not support historical data cancelations.")) return;
      const int VERSION = 1;
      //No server version validation takes place here since minimum is already higher
      SendCancelRequest(OutgoingMessages.CancelHistoricalData, VERSION, reqId, EClientErrors.FAIL_SEND_CANHISTDATA);
    }

    /**
     * @brief Calculate the volatility for an option.\n
     * Request the calculation of the implied volatility based on hypothetical option and its underlying prices.\n The calculation will be return in EWrapper's tickOptionComputation callback.\n
     * @param reqId unique identifier of the request.\n
     * @param contract the option's contract for which the volatility wants to be calculated.\n
     * @param optionPrice hypothetical option price.\n
     * @param underPrice hypothetical option's underlying price.\n
     * @sa EWrapper::tickOptionComputation, cancelCalculateImpliedVolatility, Contract
     */
    public void calculateImpliedVolatility(int reqId, Contract contract, double optionPrice, double underPrice,
        //reserved for future use, must be blank
        List<TagValue> impliedVolatilityOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_CALC_IMPLIED_VOLAT, " It does not support calculate implied volatility.")) return;
      if (!Util.StringIsEmpty(contract.TradingClass) && !CheckServerVersion(MinServerVer.TRADING_CLASS, "")) return;

      const int version = 3;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqCalcImpliedVolat);
        paramsList.AddParameter(version);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        paramsList.AddParameter(optionPrice);
        paramsList.AddParameter(underPrice);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(impliedVolatilityOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQCALCIMPLIEDVOLAT);
    }

    /**
     * @brief Calculates an option's price based on the provided volatility and its underlying's price. \n
     * The calculation will be return in EWrapper's tickOptionComputation callback.\n
     * @param reqId request's unique identifier.\n
     * @param contract the option's contract for which the price wants to be calculated.\n
     * @param volatility hypothetical volatility.\n
     * @param underPrice hypothetical underlying's price.\n
     * @sa EWrapper::tickOptionComputation, cancelCalculateOptionPrice, Contract
     */
    public void calculateOptionPrice(int reqId, Contract contract, double volatility, double underPrice,
        //reserved for future use, must be blank
        List<TagValue> optionPriceOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_CALC_OPTION_PRICE, " It does not support calculation price requests.")) return;
      if (!Util.StringIsEmpty(contract.TradingClass) && !CheckServerVersion(MinServerVer.REQ_CALC_OPTION_PRICE, " It does not support tradingClass parameter in calculateOptionPrice.")) return;

      const int version = 3;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqCalcOptionPrice);
        paramsList.AddParameter(version);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        paramsList.AddParameter(volatility);
        paramsList.AddParameter(underPrice);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(optionPriceOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQCALCOPTIONPRICE);
    }

    /**
     * @brief Cancels the account's summary request.
     * After requesting an account's summary, invoke this function to cancel it.
     * @param reqId the identifier of the previously performed account request
     * @sa reqAccountSummary
     */
    public void cancelAccountSummary(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.ACCT_SUMMARY, " It does not support account summary cancellation.")) return;
      SendCancelRequest(OutgoingMessages.CancelAccountSummary, 1, reqId, EClientErrors.FAIL_SEND_CANACCOUNTDATA);
    }

    /**
     * @brief Cancels an option's implied volatility calculation request
     * @param reqId the identifier of the implied volatility's calculation request.
     * @sa calculateImpliedVolatility
     */
    public void cancelCalculateImpliedVolatility(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.CANCEL_CALC_IMPLIED_VOLAT, " It does not support calculate implied volatility cancellation.")) return;
      SendCancelRequest(OutgoingMessages.CancelImpliedVolatility, 1, reqId, EClientErrors.FAIL_SEND_CANCALCIMPLIEDVOLAT);
    }

    /**
     * @brief Cancels an option's price calculation request
     * @param reqId the identifier of the option's price's calculation request.
     * @sa calculateOptionPrice
     */
    public void cancelCalculateOptionPrice(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.CANCEL_CALC_OPTION_PRICE, " It does not support calculate option price cancellation.")) return;
      SendCancelRequest(OutgoingMessages.CancelOptionPrice, 1, reqId, EClientErrors.FAIL_SEND_CANCALCOPTIONPRICE);
    }

    /**
     * @brief Cancels Fundamental data request
     * @param reqId the request's identifier.
     * @sa reqFundamentalData
     */
    public void cancelFundamentalData(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.FUNDAMENTAL_DATA, " It does not support fundamental data requests.")) return;
      SendCancelRequest(OutgoingMessages.CancelFundamentalData, 1, reqId, EClientErrors.FAIL_SEND_CANFUNDDATA);
    }

    /**
     * @brief Cancels a RT Market Data request
     * @param tickerId request's identifier
     * @sa reqMktData
     */
    public void cancelMktData(int tickerId)
    {
      if (!CheckConnection()) return;

      SendCancelRequest(OutgoingMessages.CancelMarketData, 1, tickerId, EClientErrors.FAIL_SEND_CANMKT);
    }

    /**
     * @brief Cancel's market depth's request.
     * @param tickerId request's identifier.
     * @sa reqMarketDepth
     */
    public void cancelMktDepth(int tickerId, bool isSmartDepth)
    {
      if (!CheckConnection()) return;

      if (isSmartDepth && !CheckServerVersion(tickerId, MinServerVer.SMART_DEPTH, " It does not support SMART depth cancel.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelMarketDepth);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(tickerId);
      if (serverVersion >= MinServerVer.SMART_DEPTH) paramsList.AddParameter(isSmartDepth);

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_SEND_CANMKTDEPTH);
    }

    /**
     * @brief Cancels IB's news bulletin subscription
     * @sa reqNewsBulletins
     */
    public void cancelNewsBulletin()
    {
      if (!CheckConnection()) return;
      SendCancelRequest(OutgoingMessages.CancelNewsBulletin, 1, EClientErrors.FAIL_SEND_CORDER);
    }

    /**
     * @brief Cancels an active order placed by from the same API client ID.\n
     * Note: API clients cannot cancel individual orders placed by other clients. Only reqGlobalCancel is available.\n
     * @param orderId the order's client id
     * @sa placeOrder, reqGlobalCancel
     */
    public void cancelOrder(int orderId, string manualOrderCancelTime)
    {
      if (!CheckConnection()) return;
      if (!IsEmpty(manualOrderCancelTime) && !CheckServerVersion(orderId, MinServerVer.MANUAL_ORDER_TIME, " It does not support manual order cancel time attribute")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.CancelOrder);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(orderId);
        if (serverVersion >= MinServerVer.MANUAL_ORDER_TIME) paramsList.AddParameter(manualOrderCancelTime);
      }
      catch (EClientException e)
      {
        wrapper.error(orderId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_CANCELPNL);
    }

    /**
     * @brief Cancels a previous position subscription request made with reqPositions
     * @sa reqPositions
     */
    public void cancelPositions()
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.ACCT_SUMMARY, " It does not support position cancellation.")) return;

      SendCancelRequest(OutgoingMessages.CancelPositions, 1, EClientErrors.FAIL_SEND_CANPOSITIONS);
    }

    /**
     * @brief Cancels Real Time Bars' subscription
     * @param tickerId the request's identifier.
     * @sa reqRealTimeBars
     */
    public void cancelRealTimeBars(int tickerId)
    {
      if (!CheckConnection()) return;

      SendCancelRequest(OutgoingMessages.CancelRealTimeBars, 1, tickerId, EClientErrors.FAIL_SEND_CANRTBARS);
    }

    /**
     * @brief Cancels Scanner Subscription
     * @param tickerId the subscription's unique identifier.
     * @sa reqScannerSubscription, ScannerSubscription, reqScannerParameters
     */
    public void cancelScannerSubscription(int tickerId)
    {
      if (!CheckConnection()) return;

      SendCancelRequest(OutgoingMessages.CancelScannerSubscription, 1, tickerId, EClientErrors.FAIL_SEND_CANSCANNER);
    }

    /**
     * @brief Exercises an options contract\n
     * Note: this function is affected by a TWS setting which specifies if an exercise request must be finalized
     * @param tickerId exercise request's identifier
     * @param contract the option Contract to be exercised.
     * @param exerciseAction set to 1 to exercise the option, set to 2 to let the option lapse.
     * @param exerciseQuantity number of contracts to be exercised
     * @param account destination account
     * @param ovrd Specifies whether your setting will override the system's natural action. For example, if your action is "exercise" and the option is not in-the-money, by natural action the option would not exercise. If you have override set to "yes" the natural action would be overridden and the out-of-the money option would be exercised. Set to 1 to override, set to 0 not to.
     * @param manualOrderTime Manual Order Time
     * @param customerAccount Customer Account
     * @param professionalCustomer Professional Customer
     */
    public void exerciseOptions(int tickerId, Contract contract, int exerciseAction, int exerciseQuantity, string account, int ovrd, string manualOrderTime, string customerAccount, bool professionalCustomer)
    {
      //WARN needs to be tested!
      if (!CheckConnection()) return;
      if (!CheckServerVersion(21, " It does not support options exercise from the API.")) return;
      if ((!Util.StringIsEmpty(contract.TradingClass) || contract.ConId > 0) &&
          !CheckServerVersion(MinServerVer.TRADING_CLASS, " It does not support conId not tradingClass parameter when exercising options."))
      {
        return;
      }

      if ((!Util.StringIsEmpty(manualOrderTime)) &&
          !CheckServerVersion(MinServerVer.MIN_SERVER_VER_MANUAL_ORDER_TIME_EXERCISE_OPTIONS, " It does not support manual order time parameter when exercising options."))
      {
        return;
      }

      if ((!Util.StringIsEmpty(customerAccount)) &&
          !CheckServerVersion(MinServerVer.MIN_SERVER_VER_CUSTOMER_ACCOUNT, " It does not support customer account parameter when exercising options."))
      {
        return;
      }

      if (professionalCustomer &&
          !CheckServerVersion(MinServerVer.MIN_SERVER_VER_PROFESSIONAL_CUSTOMER, " It does not support professional customer parameter when exercising options."))
      {
        return;
      }

      var VERSION = 2;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ExerciseOptions);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(tickerId);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        paramsList.AddParameter(exerciseAction);
        paramsList.AddParameter(exerciseQuantity);
        paramsList.AddParameter(account);
        paramsList.AddParameter(ovrd);
        if (serverVersion >= MinServerVer.MIN_SERVER_VER_MANUAL_ORDER_TIME_EXERCISE_OPTIONS) paramsList.AddParameter(manualOrderTime);
        if (serverVersion >= MinServerVer.MIN_SERVER_VER_CUSTOMER_ACCOUNT) paramsList.AddParameter(customerAccount);
        if (serverVersion >= MinServerVer.MIN_SERVER_VER_PROFESSIONAL_CUSTOMER) paramsList.AddParameter(professionalCustomer);
      }
      catch (EClientException e)
      {
        wrapper.error(tickerId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_GENERIC);
    }

    /**
     * @brief Places or modifies an order
     * @param id the order's unique identifier. Use a sequential id starting with the id received at the nextValidId method. If a new order is placed with an order ID less than or equal to the order ID of a previous order an error will occur.
     * @param contract the order's contract
     * @param order the order
     * @sa EWrapper::nextValidId, reqAllOpenOrders, reqAutoOpenOrders, reqOpenOrders, cancelOrder, reqGlobalCancel, EWrapper::openOrder, EWrapper::orderStatus, Order, Contract
     */
    public void placeOrder(int id, Contract contract, Order order)
    {
      if (!CheckConnection()) return;
      if (!VerifyOrder(order, id, StringsAreEqual(Constants.BagSecType, contract.SecType))) return;
      if (!VerifyOrderContract(contract, id)) return;

      var MsgVersion = serverVersion < MinServerVer.NOT_HELD ? 27 : 45;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.PlaceOrder);
        if (serverVersion < MinServerVer.ORDER_CONTAINER) paramsList.AddParameter(MsgVersion);
        paramsList.AddParameter(id);
        if (serverVersion >= MinServerVer.PLACE_ORDER_CONID) paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        if (serverVersion >= 15) paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        if (serverVersion >= 14) paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        if (serverVersion >= 2) paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        if (serverVersion >= MinServerVer.SEC_ID_TYPE)
        {
          paramsList.AddParameter(contract.SecIdType);
          paramsList.AddParameter(contract.SecId);
        }

        // paramsList.AddParameter main order fields
        paramsList.AddParameter(order.Action);

        if (ServerVersion >= MinServerVer.FRACTIONAL_POSITIONS) paramsList.AddParameter(order.TotalQuantity);
        else paramsList.AddParameter((int)order.TotalQuantity);

        paramsList.AddParameter(order.OrderType);
        if (serverVersion < MinServerVer.ORDER_COMBO_LEGS_PRICE) paramsList.AddParameter(order.LmtPrice == double.MaxValue ? 0 : order.LmtPrice);
        else paramsList.AddParameterMax(order.LmtPrice);
        if (serverVersion < MinServerVer.TRAILING_PERCENT) paramsList.AddParameter(order.AuxPrice == double.MaxValue ? 0 : order.AuxPrice);
        else paramsList.AddParameterMax(order.AuxPrice);

        // paramsList.AddParameter extended order fields
        paramsList.AddParameter(order.Tif);
        paramsList.AddParameter(order.OcaGroup);
        paramsList.AddParameter(order.Account);
        paramsList.AddParameter(order.OpenClose);
        paramsList.AddParameter(order.Origin);
        paramsList.AddParameter(order.OrderRef);
        paramsList.AddParameter(order.Transmit);
        if (serverVersion >= 4) paramsList.AddParameter(order.ParentId);

        if (serverVersion >= 5)
        {
          paramsList.AddParameter(order.BlockOrder);
          paramsList.AddParameter(order.SweepToFill);
          paramsList.AddParameter(order.DisplaySize);
          paramsList.AddParameter(order.TriggerMethod);
          if (serverVersion < 38) paramsList.AddParameter( /* order.ignoreRth */ false); // will never happen
          else paramsList.AddParameter(order.OutsideRth);
        }
        if (serverVersion >= 7) paramsList.AddParameter(order.Hidden);

        // paramsList.AddParameter combo legs for BAG requests
        var isBag = StringsAreEqual(Constants.BagSecType, contract.SecType);
        if (serverVersion >= 8 && isBag)
        {
          if (contract.ComboLegs == null)
          {
            paramsList.AddParameter(0);
          }
          else
          {
            paramsList.AddParameter(contract.ComboLegs.Count);

            ComboLeg comboLeg;
            for (var i = 0; i < contract.ComboLegs.Count; i++)
            {
              comboLeg = contract.ComboLegs[i];
              paramsList.AddParameter(comboLeg.ConId);
              paramsList.AddParameter(comboLeg.Ratio);
              paramsList.AddParameter(comboLeg.Action);
              paramsList.AddParameter(comboLeg.Exchange);
              paramsList.AddParameter(comboLeg.OpenClose);

              if (serverVersion >= MinServerVer.SSHORT_COMBO_LEGS)
              {
                paramsList.AddParameter(comboLeg.ShortSaleSlot);
                paramsList.AddParameter(comboLeg.DesignatedLocation);
              }
              if (serverVersion >= MinServerVer.SSHORTX_OLD)
              {
                paramsList.AddParameter(comboLeg.ExemptCode);
              }
            }
          }
        }

        // add order combo legs for BAG requests
        if (serverVersion >= MinServerVer.ORDER_COMBO_LEGS_PRICE && isBag)
        {
          if (order.OrderComboLegs == null)
          {
            paramsList.AddParameter(0);
          }
          else
          {
            paramsList.AddParameter(order.OrderComboLegs.Count);

            for (var i = 0; i < order.OrderComboLegs.Count; i++)
            {
              var orderComboLeg = order.OrderComboLegs[i];
              paramsList.AddParameterMax(orderComboLeg.Price);
            }
          }
        }

        if (serverVersion >= MinServerVer.SMART_COMBO_ROUTING_PARAMS && isBag)
        {
          var smartComboRoutingParams = order.SmartComboRoutingParams;
          var smartComboRoutingParamsCount = smartComboRoutingParams?.Count ?? 0;
          paramsList.AddParameter(smartComboRoutingParamsCount);
          if (smartComboRoutingParamsCount > 0)
          {
            for (var i = 0; i < smartComboRoutingParamsCount; ++i)
            {
              var tagValue = smartComboRoutingParams[i];
              paramsList.AddParameter(tagValue.Tag);
              paramsList.AddParameter(tagValue.Value);
            }
          }
        }

        if (serverVersion >= 9)
        {
          // paramsList.AddParameter deprecated sharesAllocation field
          paramsList.AddParameter("");
        }

        if (serverVersion >= 10) paramsList.AddParameter(order.DiscretionaryAmt);
        if (serverVersion >= 11) paramsList.AddParameter(order.GoodAfterTime);
        if (serverVersion >= 12) paramsList.AddParameter(order.GoodTillDate);
        if (serverVersion >= 13)
        {
          paramsList.AddParameter(order.FaGroup);
          paramsList.AddParameter(order.FaMethod);
          paramsList.AddParameter(order.FaPercentage);
          if (serverVersion < MinServerVer.MIN_SERVER_VER_FA_PROFILE_DESUPPORT)
          {
            paramsList.AddParameter(""); // send deprecated faProfile field
          }
        }
        if (serverVersion >= MinServerVer.MODELS_SUPPORT) paramsList.AddParameter(order.ModelCode);
        if (serverVersion >= 18)
        { // institutional short sale slot fields.
          paramsList.AddParameter(order.ShortSaleSlot);      // 0 only for retail, 1 or 2 only for institution.
          paramsList.AddParameter(order.DesignatedLocation); // only populate when order.shortSaleSlot = 2.
        }
        if (serverVersion >= MinServerVer.SSHORTX_OLD) paramsList.AddParameter(order.ExemptCode);
        if (serverVersion >= 19)
        {
          paramsList.AddParameter(order.OcaType);
          if (serverVersion < 38)
          {
            // will never happen
            paramsList.AddParameter( /* order.rthOnly */ false);
          }
          paramsList.AddParameter(order.Rule80A);
          paramsList.AddParameter(order.SettlingFirm);
          paramsList.AddParameter(order.AllOrNone);
          paramsList.AddParameterMax(order.MinQty);
          paramsList.AddParameterMax(order.PercentOffset);
          paramsList.AddParameter(false);
          paramsList.AddParameter(false);
          paramsList.AddParameterMax(double.MaxValue);
          paramsList.AddParameterMax(order.AuctionStrategy);
          paramsList.AddParameterMax(order.StartingPrice);
          paramsList.AddParameterMax(order.StockRefPrice);
          paramsList.AddParameterMax(order.Delta);
          // Volatility orders had specific watermark price attribs in server version 26
          var lower = serverVersion == 26 && Util.IsVolOrder(order.OrderType) ? double.MaxValue : order.StockRangeLower;
          var upper = serverVersion == 26 && Util.IsVolOrder(order.OrderType) ? double.MaxValue : order.StockRangeUpper;
          paramsList.AddParameterMax(lower);
          paramsList.AddParameterMax(upper);
        }
        if (serverVersion >= 22) paramsList.AddParameter(order.OverridePercentageConstraints);
        if (serverVersion >= 26)
        { // Volatility orders
          paramsList.AddParameterMax(order.Volatility);
          paramsList.AddParameterMax(order.VolatilityType);
          if (serverVersion < 28)
          {
            var isDeltaNeutralTypeMKT = string.Compare("MKT", order.DeltaNeutralOrderType, true) == 0;
            paramsList.AddParameter(isDeltaNeutralTypeMKT);
          }
          else
          {
            paramsList.AddParameter(order.DeltaNeutralOrderType);
            paramsList.AddParameterMax(order.DeltaNeutralAuxPrice);

            if (serverVersion >= MinServerVer.DELTA_NEUTRAL_CONID && !IsEmpty(order.DeltaNeutralOrderType))
            {
              paramsList.AddParameter(order.DeltaNeutralConId);
              paramsList.AddParameter(order.DeltaNeutralSettlingFirm);
              paramsList.AddParameter(order.DeltaNeutralClearingAccount);
              paramsList.AddParameter(order.DeltaNeutralClearingIntent);
            }

            if (serverVersion >= MinServerVer.DELTA_NEUTRAL_OPEN_CLOSE && !IsEmpty(order.DeltaNeutralOrderType))
            {
              paramsList.AddParameter(order.DeltaNeutralOpenClose);
              paramsList.AddParameter(order.DeltaNeutralShortSale);
              paramsList.AddParameter(order.DeltaNeutralShortSaleSlot);
              paramsList.AddParameter(order.DeltaNeutralDesignatedLocation);
            }
          }
          paramsList.AddParameter(order.ContinuousUpdate);
          if (serverVersion == 26)
          {
            // Volatility orders had specific watermark price attribs in server version 26
            var lower = Util.IsVolOrder(order.OrderType) ? order.StockRangeLower : double.MaxValue;
            var upper = Util.IsVolOrder(order.OrderType) ? order.StockRangeUpper : double.MaxValue;
            paramsList.AddParameterMax(lower);
            paramsList.AddParameterMax(upper);
          }
          paramsList.AddParameterMax(order.ReferencePriceType);
        }

        if (serverVersion >= 30) paramsList.AddParameterMax(order.TrailStopPrice); // TRAIL_STOP_LIMIT stop price
        if (serverVersion >= MinServerVer.TRAILING_PERCENT) paramsList.AddParameterMax(order.TrailingPercent);
        if (serverVersion >= MinServerVer.SCALE_ORDERS)
        {
          if (serverVersion >= MinServerVer.SCALE_ORDERS2)
          {
            paramsList.AddParameterMax(order.ScaleInitLevelSize);
            paramsList.AddParameterMax(order.ScaleSubsLevelSize);
          }
          else
          {
            paramsList.AddParameter("");
            paramsList.AddParameterMax(order.ScaleInitLevelSize);
          }
          paramsList.AddParameterMax(order.ScalePriceIncrement);
        }

        if (serverVersion >= MinServerVer.SCALE_ORDERS3 && order.ScalePriceIncrement > 0.0 && order.ScalePriceIncrement != double.MaxValue)
        {
          paramsList.AddParameterMax(order.ScalePriceAdjustValue);
          paramsList.AddParameterMax(order.ScalePriceAdjustInterval);
          paramsList.AddParameterMax(order.ScaleProfitOffset);
          paramsList.AddParameter(order.ScaleAutoReset);
          paramsList.AddParameterMax(order.ScaleInitPosition);
          paramsList.AddParameterMax(order.ScaleInitFillQty);
          paramsList.AddParameter(order.ScaleRandomPercent);
        }

        if (serverVersion >= MinServerVer.SCALE_TABLE)
        {
          paramsList.AddParameter(order.ScaleTable);
          paramsList.AddParameter(order.ActiveStartTime);
          paramsList.AddParameter(order.ActiveStopTime);
        }

        if (serverVersion >= MinServerVer.HEDGE_ORDERS)
        {
          paramsList.AddParameter(order.HedgeType);
          if (!IsEmpty(order.HedgeType)) paramsList.AddParameter(order.HedgeParam);
        }

        if (serverVersion >= MinServerVer.OPT_OUT_SMART_ROUTING) paramsList.AddParameter(order.OptOutSmartRouting);

        if (serverVersion >= MinServerVer.PTA_ORDERS)
        {
          paramsList.AddParameter(order.ClearingAccount);
          paramsList.AddParameter(order.ClearingIntent);
        }

        if (serverVersion >= MinServerVer.NOT_HELD) paramsList.AddParameter(order.NotHeld);

        if (serverVersion >= MinServerVer.DELTA_NEUTRAL)
        {
          if (contract.DeltaNeutralContract != null)
          {
            var deltaNeutralContract = contract.DeltaNeutralContract;
            paramsList.AddParameter(true);
            paramsList.AddParameter(deltaNeutralContract.ConId);
            paramsList.AddParameter(deltaNeutralContract.Delta);
            paramsList.AddParameter(deltaNeutralContract.Price);
          }
          else
          {
            paramsList.AddParameter(false);
          }
        }

        if (serverVersion >= MinServerVer.ALGO_ORDERS)
        {
          paramsList.AddParameter(order.AlgoStrategy);
          if (!IsEmpty(order.AlgoStrategy))
          {
            var algoParams = order.AlgoParams;
            var algoParamsCount = algoParams?.Count ?? 0;
            paramsList.AddParameter(algoParamsCount);
            if (algoParamsCount > 0)
            {
              for (var i = 0; i < algoParamsCount; ++i)
              {
                var tagValue = algoParams[i];
                paramsList.AddParameter(tagValue.Tag);
                paramsList.AddParameter(tagValue.Value);
              }
            }
          }
        }

        if (serverVersion >= MinServerVer.ALGO_ID) paramsList.AddParameter(order.AlgoId);
        if (serverVersion >= MinServerVer.WHAT_IF_ORDERS) paramsList.AddParameter(order.WhatIf);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(order.OrderMiscOptions);
        if (serverVersion >= MinServerVer.ORDER_SOLICITED) paramsList.AddParameter(order.Solicited);
        if (serverVersion >= MinServerVer.RANDOMIZE_SIZE_AND_PRICE)
        {
          paramsList.AddParameter(order.RandomizeSize);
          paramsList.AddParameter(order.RandomizePrice);
        }

        if (serverVersion >= MinServerVer.PEGGED_TO_BENCHMARK)
        {
          if (Util.IsPegBenchOrder(order.OrderType))
          {
            paramsList.AddParameter(order.ReferenceContractId);
            paramsList.AddParameter(order.IsPeggedChangeAmountDecrease);
            paramsList.AddParameter(order.PeggedChangeAmount);
            paramsList.AddParameter(order.ReferenceChangeAmount);
            paramsList.AddParameter(order.ReferenceExchange);
          }

          paramsList.AddParameter(order.Conditions.Count);

          if (order.Conditions.Count > 0)
          {
            foreach (var item in order.Conditions)
            {
              paramsList.AddParameter((int)item.Type);
              item.Serialize(paramsList);
            }

            paramsList.AddParameter(order.ConditionsIgnoreRth);
            paramsList.AddParameter(order.ConditionsCancelOrder);
          }

          paramsList.AddParameter(order.AdjustedOrderType);
          paramsList.AddParameter(order.TriggerPrice);
          paramsList.AddParameter(order.LmtPriceOffset);
          paramsList.AddParameter(order.AdjustedStopPrice);
          paramsList.AddParameter(order.AdjustedStopLimitPrice);
          paramsList.AddParameter(order.AdjustedTrailingAmount);
          paramsList.AddParameter(order.AdjustableTrailingUnit);
        }

        if (serverVersion >= MinServerVer.EXT_OPERATOR) paramsList.AddParameter(order.ExtOperator);

        if (serverVersion >= MinServerVer.SOFT_DOLLAR_TIER)
        {
          paramsList.AddParameter(order.Tier.Name);
          paramsList.AddParameter(order.Tier.Value);
        }

        if (serverVersion >= MinServerVer.CASH_QTY) paramsList.AddParameterMax(order.CashQty);

        if (serverVersion >= MinServerVer.DECISION_MAKER)
        {
          paramsList.AddParameter(order.Mifid2DecisionMaker);
          paramsList.AddParameter(order.Mifid2DecisionAlgo);
        }

        if (serverVersion >= MinServerVer.MIFID_EXECUTION)
        {
          paramsList.AddParameter(order.Mifid2ExecutionTrader);
          paramsList.AddParameter(order.Mifid2ExecutionAlgo);
        }

        if (serverVersion >= MinServerVer.AUTO_PRICE_FOR_HEDGE) paramsList.AddParameter(order.DontUseAutoPriceForHedge);
        if (serverVersion >= MinServerVer.ORDER_CONTAINER) paramsList.AddParameter(order.IsOmsContainer);
        if (serverVersion >= MinServerVer.D_PEG_ORDERS) paramsList.AddParameter(order.DiscretionaryUpToLimitPrice);
        if (serverVersion >= MinServerVer.PRICE_MGMT_ALGO) paramsList.AddParameter(order.UsePriceMgmtAlgo);
        if (serverVersion >= MinServerVer.DURATION) paramsList.AddParameter(order.Duration);
        if (serverVersion >= MinServerVer.POST_TO_ATS) paramsList.AddParameter(order.PostToAts);
        if (serverVersion >= MinServerVer.AUTO_CANCEL_PARENT) paramsList.AddParameter(order.AutoCancelParent);
        if (serverVersion >= MinServerVer.ADVANCED_ORDER_REJECT) paramsList.AddParameter(order.AdvancedErrorOverride);
        if (serverVersion >= MinServerVer.MANUAL_ORDER_TIME) paramsList.AddParameter(order.ManualOrderTime);
        if (serverVersion >= MinServerVer.PEGBEST_PEGMID_OFFSETS)
        {
          if (contract.Exchange == "IBKRATS") paramsList.AddParameterMax(order.MinTradeQty);
          var sendMidOffsets = false;
          if (Util.IsPegBestOrder(order.OrderType))
          {
            paramsList.AddParameterMax(order.MinCompeteSize);
            paramsList.AddParameterMax(order.CompeteAgainstBestOffset);
            if (order.CompeteAgainstBestOffset == Order.COMPETE_AGAINST_BEST_OFFSET_UP_TO_MID) sendMidOffsets = true;
          }
          else if (Util.IsPegMidOrder(order.OrderType))
          {
            sendMidOffsets = true;
          }

          if (sendMidOffsets)
          {
            paramsList.AddParameterMax(order.MidOffsetAtWhole);
            paramsList.AddParameterMax(order.MidOffsetAtHalf);
          }
        }
        if (serverVersion >= MinServerVer.MIN_SERVER_VER_CUSTOMER_ACCOUNT) paramsList.AddParameter(order.CustomerAccount);
        if (serverVersion >= MinServerVer.MIN_SERVER_VER_PROFESSIONAL_CUSTOMER) paramsList.AddParameter(order.ProfessionalCustomer);
      }
      catch (EClientException e)
      {
        wrapper.error(id, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(id, paramsList, lengthPos, EClientErrors.FAIL_SEND_ORDER);
    }

    /**
     * @brief Replaces Financial Advisor's settings
     * A Financial Advisor can define three different configurations:
     *    1. Groups: offer traders a way to create a group of accounts and apply a single allocation method to all accounts in the group.
     *    3. Account Aliases: let you easily identify the accounts by meaningful names rather than account numbers.
     * More information at https://www.interactivebrokers.com/en/?f=%2Fen%2Fsoftware%2Fpdfhighlights%2FPDF-AdvisorAllocations.php%3Fib_entity%3Dllc
     * @param faDataType the configuration to change. Set to 1 or 3 as defined above.
     * @param xml the xml-formatted configuration string
     * @sa requestFA
     */
    public void replaceFA(int reqId, int faDataType, string xml)
    {
      if (!CheckConnection()) return;
      if (serverVersion >= MinServerVer.MIN_SERVER_VER_FA_PROFILE_DESUPPORT && faDataType == 2)
      {
        wrapper.error(reqId, EClientErrors.FA_PROFILE_NOT_SUPPORTED.Code, EClientErrors.FA_PROFILE_NOT_SUPPORTED.Message, "");
        return;
      }

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReplaceFA);
        paramsList.AddParameter(1);
        paramsList.AddParameter(faDataType);
        paramsList.AddParameter(xml);
        if (serverVersion >= MinServerVer.REPLACE_FA_END)
        {
          paramsList.AddParameter(reqId);
        }
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_FA_REPLACE);
    }

    /**
     * @brief Requests the FA configuration
     * A Financial Advisor can define three different configurations:
     *      1. Groups: offer traders a way to create a group of accounts and apply a single allocation method to all accounts in the group.
     *      3. Account Aliases: let you easily identify the accounts by meaningful names rather than account numbers.
     * More information at https://www.interactivebrokers.com/en/?f=%2Fen%2Fsoftware%2Fpdfhighlights%2FPDF-AdvisorAllocations.php%3Fib_entity%3Dllc
     * @param faDataType the configuration to change. Set to 1 or 3 as defined above.
     * @sa replaceFA
     */
    public void requestFA(int faDataType)
    {
      if (!CheckConnection()) return;
      if (serverVersion >= MinServerVer.MIN_SERVER_VER_FA_PROFILE_DESUPPORT && faDataType == 2)
      {
        wrapper.error(IncomingMessage.NotValid, EClientErrors.FA_PROFILE_NOT_SUPPORTED.Code, EClientErrors.FA_PROFILE_NOT_SUPPORTED.Message, "");
        return;
      }

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestFA);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(faDataType);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_FA_REQUEST);
    }

    /**
     * @brief Requests a specific account's summary.\n
     * This method will subscribe to the account summary as presented in the TWS' Account Summary tab. The data is returned at EWrapper::accountSummary\n
     * https://www.interactivebrokers.com/en/software/tws/accountwindowtop.htm
     * @param reqId the unique request identifier.\n
     * @param group set to "All" to return account summary data for all accounts, or set to a specific Advisor Account Group name that has already been created in TWS Global Configuration.\n
     * @param tags a comma separated list with the desired tags:
     *      - AccountType — Identifies the IB account structure
     *      - NetLiquidation — The basis for determining the price of the assets in your account. Total cash value + stock value + options value + bond value
     *      - TotalCashValue — Total cash balance recognized at the time of trade + futures PNL
     *      - SettledCash — Cash recognized at the time of settlement - purchases at the time of trade - commissions - taxes - fees
     *      - AccruedCash — Total accrued cash value of stock, commodities and securities
     *      - BuyingPower — Buying power serves as a measurement of the dollar value of securities that one may purchase in a securities account without depositing additional funds
     *      - EquityWithLoanValue — Forms the basis for determining whether a client has the necessary assets to either initiate or maintain security positions. Cash + stocks + bonds + mutual funds
     *      - PreviousEquityWithLoanValue — Marginable Equity with Loan value as of 16:00 ET the previous day
     *      - GrossPositionValue — The sum of the absolute value of all stock and equity option positions
     *      - RegTEquity — Regulation T equity for universal account
     *      - RegTMargin — Regulation T margin for universal account
     *      - SMA — Special Memorandum Account: Line of credit created when the market value of securities in a Regulation T account increase in value
     *      - InitMarginReq — Initial Margin requirement of whole portfolio
     *      - MaintMarginReq — Maintenance Margin requirement of whole portfolio
     *      - AvailableFunds — This value tells what you have available for trading
     *      - ExcessLiquidity — This value shows your margin cushion, before liquidation
     *      - Cushion — Excess liquidity as a percentage of net liquidation value
     *      - FullInitMarginReq — Initial Margin of whole portfolio with no discounts or intraday credits
     *      - FullMaintMarginReq — Maintenance Margin of whole portfolio with no discounts or intraday credits
     *      - FullAvailableFunds — Available funds of whole portfolio with no discounts or intraday credits
     *      - FullExcessLiquidity — Excess liquidity of whole portfolio with no discounts or intraday credits
     *      - LookAheadNextChange — Time when look-ahead values take effect
     *      - LookAheadInitMarginReq — Initial Margin requirement of whole portfolio as of next period's margin change
     *      - LookAheadMaintMarginReq — Maintenance Margin requirement of whole portfolio as of next period's margin change
     *      - LookAheadAvailableFunds — This value reflects your available funds at the next margin change
     *      - LookAheadExcessLiquidity — This value reflects your excess liquidity at the next margin change
     *      - HighestSeverity — A measure of how close the account is to liquidation
     *      - DayTradesRemaining — The Number of Open/Close trades a user could put on before Pattern Day Trading is detected. A value of "-1" means that the user can put on unlimited day trades.
     *      - Leverage — GrossPositionValue / NetLiquidation
     *      - $LEDGER — Single flag to relay all cash balance tags*, only in base currency.
     *      - $LEDGER:CURRENCY — Single flag to relay all cash balance tags*, only in the specified currency.
     *      - $LEDGER:ALL — Single flag to relay all cash balance tags* in all currencies.
     * @sa cancelAccountSummary, EWrapper::accountSummary, EWrapper::accountSummaryEnd
     */
    public void reqAccountSummary(int reqId, string group, string tags)
    {
      var VERSION = 1;
      if (!CheckConnection()) return;
      if (!CheckServerVersion(reqId, MinServerVer.ACCT_SUMMARY, " It does not support account summary requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestAccountSummary);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(group);
        paramsList.AddParameter(tags);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQACCOUNTDATA);
    }

    /**
     * @brief Subscribes to a specific account's information and portfolio.
     * Through this method, a single account's subscription can be started/stopped. As a result from the subscription, the account's information, portfolio and last update time will be received at EWrapper::updateAccountValue, EWrapper::updateAccountPortfolio, EWrapper::updateAccountTime respectively. All account values and positions will be returned initially, and then there will only be updates when there is a change in a position, or to an account value every 3 minutes if it has changed.
     * Only one account can be subscribed at a time. A second subscription request for another account when the previous one is still active will cause the first one to be canceled in favour of the second one. Consider user reqPositions if you want to retrieve all your accounts' portfolios directly.
     * @param subscribe set to true to start the subscription and to false to stop it.
     * @param acctCode the account id (i.e. U123456) for which the information is requested.
     * @sa reqPositions, EWrapper::updateAccountValue, EWrapper::updatePortfolio, EWrapper::updateAccountTime
     */
    public void reqAccountUpdates(bool subscribe, string acctCode)
    {
      var VERSION = 2;
      if (!CheckConnection()) return;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestAccountData);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(subscribe);
        if (serverVersion >= 9) paramsList.AddParameter(acctCode);
      }
      catch (EClientException e)
      {
        wrapper.error(IncomingMessage.NotValid, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQACCOUNTDATA);
    }

    /**
     * @brief Requests all *current* open orders in associated accounts at the current moment. The existing orders will be received via the openOrder and orderStatus events.
     * Open orders are returned once; this function does not initiate a subscription
     * @sa reqAutoOpenOrders, reqOpenOrders, EWrapper::openOrder, EWrapper::orderStatus, EWrapper::openOrderEnd
     */
    public void reqAllOpenOrders()
    {
      var VERSION = 1;
      if (!CheckConnection()) return;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestAllOpenOrders);
      paramsList.AddParameter(VERSION);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_OORDER);
    }

    /**
     * @brief Requests status updates about future orders placed from TWS. Can only be used with client ID 0.
     * @param autoBind if set to true, the newly created orders will be assigned an API order ID and implicitly associated with this client. If set to false, future orders will not be.
     * @sa reqAllOpenOrders, reqOpenOrders, cancelOrder, reqGlobalCancel, EWrapper::openOrder, EWrapper::orderStatus
     */
    public void reqAutoOpenOrders(bool autoBind)
    {
      var VERSION = 1;
      if (!CheckConnection()) return;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestAutoOpenOrders);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(autoBind);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_OORDER);
    }

    /**
     * @brief Requests contract information.\n
     * This method will provide all the contracts matching the contract provided. It can also be used to retrieve complete options and futures chains. This information will be returned at EWrapper:contractDetails. Though it is now (in API version > 9.72.12) advised to use reqSecDefOptParams for that purpose. \n
     * @param reqId the unique request identifier.\n
     * @param contract the contract used as sample to query the available contracts. Typically, it will contain the Contract::Symbol, Contract::Currency, Contract::SecType, Contract::Exchange\n
     * @sa EWrapper::contractDetails, EWrapper::contractDetailsEnd
     */
    public void reqContractDetails(int reqId, Contract contract)
    {
      if (!CheckConnection()) return;

      if ((!IsEmpty(contract.SecIdType) || !IsEmpty(contract.SecId)) && !CheckServerVersion(reqId, MinServerVer.SEC_ID_TYPE, " It does not support secIdType not secId attributes")) return;
      if (!IsEmpty(contract.TradingClass) && !CheckServerVersion(reqId, MinServerVer.TRADING_CLASS, " It does not support the TradingClass parameter when requesting contract details.")) return;
      if (!IsEmpty(contract.PrimaryExch) && !CheckServerVersion(reqId, MinServerVer.LINKING, " It does not support PrimaryExch parameter when requesting contract details.")) return;
      if (!IsEmpty(contract.IssuerId) && !CheckServerVersion(reqId, MinServerVer.MIN_SERVER_VER_BOND_ISSUERID, " It does not support IssuerId parameter when requesting contract details.")) return;

      var VERSION = 8;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestContractData);
        paramsList.AddParameter(VERSION); //version
        if (serverVersion >= MinServerVer.CONTRACT_DATA_CHAIN) paramsList.AddParameter(reqId);
        if (serverVersion >= MinServerVer.CONTRACT_CONID) paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        if (serverVersion >= 15) paramsList.AddParameter(contract.Multiplier);
        if (serverVersion >= MinServerVer.PRIMARYEXCH)
        {
          paramsList.AddParameter(contract.Exchange);
          paramsList.AddParameter(contract.PrimaryExch);
        }
        else if (serverVersion >= MinServerVer.LINKING)
        {
          if (!IsEmpty(contract.PrimaryExch) && (contract.Exchange == "BEST" || contract.Exchange == "SMART"))
          {
            paramsList.AddParameter($"{contract.Exchange}:{contract.PrimaryExch}");
          }
          else
          {
            paramsList.AddParameter(contract.Exchange);
          }
        }

        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        if (serverVersion >= 31) paramsList.AddParameter(contract.IncludeExpired);
        if (serverVersion >= MinServerVer.SEC_ID_TYPE)
        {
          paramsList.AddParameter(contract.SecIdType);
          paramsList.AddParameter(contract.SecId);
        }
        if (serverVersion >= MinServerVer.MIN_SERVER_VER_BOND_ISSUERID) paramsList.AddParameter(contract.IssuerId);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQCONTRACT);
    }

    /**
     * @brief Requests TWS's current time.
     * @sa EWrapper::currentTime
     */
    public void reqCurrentTime()
    {
      var VERSION = 1;
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.CURRENT_TIME, " It does not support current time requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestCurrentTime);
      paramsList.AddParameter(VERSION); //version
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQCURRTIME);
    }

    /**
     * @brief Requests current day's (since midnight) executions matching the filter.
     * Only the current day's executions can be retrieved. Along with the executions, the CommissionReport will also be returned. The execution details will arrive at EWrapper:execDetails
     * @param reqId the request's unique identifier.
     * @param filter the filter criteria used to determine which execution reports are returned.
     * @sa EWrapper::execDetails, EWrapper::commissionReport, ExecutionFilter
     */
    public void reqExecutions(int reqId, ExecutionFilter filter)
    {
      if (!CheckConnection()) return;

      var VERSION = 3;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestExecutions);
        paramsList.AddParameter(VERSION); //version
        if (serverVersion >= MinServerVer.EXECUTION_DATA_CHAIN) paramsList.AddParameter(reqId);

        //Send the execution rpt filter data
        if (serverVersion >= 9)
        {
          paramsList.AddParameter(filter.ClientId);
          paramsList.AddParameter(filter.AcctCode);

          // Note that the valid format for time is "yyyyMMdd-HH:mm:ss" (UTC) or "yyyyMMdd HH:mm:ss timezone"
          paramsList.AddParameter(filter.Time);
          paramsList.AddParameter(filter.Symbol);
          paramsList.AddParameter(filter.SecType);
          paramsList.AddParameter(filter.Exchange);
          paramsList.AddParameter(filter.Side);
        }
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_EXEC);
    }

    /**
     * @brief Legacy/DEPRECATED. Requests the contract's fundamental data.
     * Fundamental data is returned at EWrapper::fundamentalData
     * @param reqId the request's unique identifier.
     * @param contract the contract's description for which the data will be returned.
     * @param reportType there are three available report types:
     *      - ReportSnapshot: Company overview
     *      - ReportsFinSummary: Financial summary
     *      - ReportRatios: Financial ratios
     *      - ReportsFinStatements: Financial statements
     *      - RESC: Analyst estimates
     * @sa EWrapper::fundamentalData
     */
    public void reqFundamentalData(int reqId, Contract contract, string reportType,
        //reserved for future use, must be blank
        List<TagValue> fundamentalDataOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(reqId, MinServerVer.FUNDAMENTAL_DATA, " It does not support Fundamental Data requests.")) return;
      if ((!IsEmpty(contract.TradingClass) || contract.ConId > 0 || !IsEmpty(contract.Multiplier)) && !CheckServerVersion(reqId, MinServerVer.TRADING_CLASS, "")) return;

      const int VERSION = 3;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestFundamentalData);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(reqId);

        if (serverVersion >= MinServerVer.TRADING_CLASS)
        {
          //WARN: why are we checking the trading class and multiplier above never send them?
          paramsList.AddParameter(contract.ConId);
        }

        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.Exchange);
        paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        paramsList.AddParameter(reportType);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(fundamentalDataOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQFUNDDATA);
    }

    /**
     * @brief Cancels all active orders.\n
     * This method will cancel ALL open orders including those placed directly from TWS.
     * @sa cancelOrder
     */
    public void reqGlobalCancel()
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_GLOBAL_CANCEL, "It does not support global cancel requests.")) return;

      const int VERSION = 1;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestGlobalCancel);
      paramsList.AddParameter(VERSION);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQGLOBALCANCEL);
    }

    /**
     * @brief Requests contracts' historical data.
     * When requesting historical data, a finishing time and date is required along with a duration string. For example, having:
     *      - endDateTime: 20130701 23:59:59 GMT
     *      - durationStr: 3 D
     * will return three days of data counting backwards from July 1st 2013 at 23:59:59 GMT resulting in all the available bars of the last three days until the date and time specified. It is possible to specify a timezone optionally. The resulting bars will be returned in EWrapper::historicalData
     * @param tickerId the request's unique identifier.
     * @param contract the contract for which we want to retrieve the data.
     * @param endDateTime request's ending time with format yyyyMMdd HH:mm:ss {TMZ}
     * @param durationStr the amount of time for which the data needs to be retrieved:
     *      - " S (seconds)
     *      - " D (days)
     *      - " W (weeks)
     *      - " M (months)
     *      - " Y (years)
     * @param barSizeSetting the size of the bar:
     *      - 1 sec
     *      - 5 secs
     *      - 15 secs
     *      - 30 secs
     *      - 1 min
     *      - 2 mins
     *      - 3 mins
     *      - 5 mins
     *      - 15 mins
     *      - 30 mins
     *      - 1 hour
     *      - 1 day
     * @param whatToShow the kind of information being retrieved:
     *      - TRADES
     *      - MIDPOINT
     *      - BID
     *      - ASK
     *      - BID_ASK
     *      - HISTORICAL_VOLATILITY
     *      - OPTION_IMPLIED_VOLATILITY
     *      - FEE_RATE
     *      - SCHEDULE
     * @param useRTH set to 0 to obtain the data which was also generated outside of the Regular Trading Hours, set to 1 to obtain only the RTH data
     * @param formatDate set to 1 to obtain the bars' time as yyyyMMdd HH:mm:ss, set to 2 to obtain it like system time format in seconds
     * @param keepUpToDate set to True to received continuous updates on most recent bar data. If True, and endDateTime cannot be specified.
     * @sa EWrapper::historicalData
     */
    public void reqHistoricalData(int tickerId, Contract contract, string endDateTime,
        string durationStr, string barSizeSetting, string whatToShow, int useRTH, int formatDate, bool keepUpToDate, List<TagValue> chartOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(tickerId, 16)) return;
      if ((!IsEmpty(contract.TradingClass) || contract.ConId > 0) && !CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support conId nor trading class parameters when requesting historical data.")) return;
      if (!IsEmpty(whatToShow) && whatToShow.Equals("SCHEDULE") && !CheckServerVersion(tickerId, MinServerVer.HISTORICAL_SCHEDULE, " It does not support requesting of historical schedule.")) return;

      const int VERSION = 6;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestHistoricalData);
        if (serverVersion < MinServerVer.SYNT_REALTIME_BARS) paramsList.AddParameter(VERSION);
        paramsList.AddParameter(tickerId);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        paramsList.AddParameter(contract.IncludeExpired ? 1 : 0);
        paramsList.AddParameter(endDateTime);
        paramsList.AddParameter(barSizeSetting);
        paramsList.AddParameter(durationStr);
        paramsList.AddParameter(useRTH);
        paramsList.AddParameter(whatToShow);
        paramsList.AddParameter(formatDate);

        if (StringsAreEqual(Constants.BagSecType, contract.SecType))
        {
          if (contract.ComboLegs != null)
          {
            paramsList.AddParameter(contract.ComboLegs.Count);

            ComboLeg comboLeg;
            for (var i = 0; i < contract.ComboLegs.Count; i++)
            {
              comboLeg = contract.ComboLegs[i];
              paramsList.AddParameter(comboLeg.ConId);
              paramsList.AddParameter(comboLeg.Ratio);
              paramsList.AddParameter(comboLeg.Action);
              paramsList.AddParameter(comboLeg.Exchange);
            }
          }
          else
          {
            paramsList.AddParameter(0);
          }
        }
        if (serverVersion >= MinServerVer.SYNT_REALTIME_BARS) paramsList.AddParameter(keepUpToDate);

        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(chartOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(tickerId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQHISTDATA);
    }

    /**
     * @brief Requests the next valid order ID at the current moment.
     * @param numIds deprecated- this parameter will not affect the value returned to nextValidId
     * @sa EWrapper::nextValidId
     */
    public void reqIds(int numIds)
    {
      if (!CheckConnection()) return;
      const int VERSION = 1;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestIds);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(numIds);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_GENERIC);
    }

    /**
     * @brief Requests the accounts to which the logged user has access to.
     * @sa EWrapper::managedAccounts
     */
    public void reqManagedAccts()
    {
      if (!CheckConnection()) return;
      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestManagedAccounts);
      paramsList.AddParameter(VERSION);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_GENERIC);
    }

    /**
     * @brief Requests real time market data.
     * Returns market data for an instrument either in real time or 10-15 minutes delayed (depending on the market data type specified)
     * @param tickerId the request's identifier
     * @param contract the Contract for which the data is being requested
     * @param genericTickList comma separated ids of the available generic ticks:
     *      - 100 Option Volume (currently for stocks)
     *      - 101 Option Open Interest (currently for stocks)
     *      - 104 Historical Volatility (currently for stocks)
     *      - 105 Average Option Volume (currently for stocks)
     *      - 106 Option Implied Volatility (currently for stocks)
     *      - 162 Index Future Premium
     *      - 165 Miscellaneous Stats
     *      - 221 Mark Price (used in TWS P&L computations)
     *      - 225 Auction values (volume, price and imbalance)
     *      - 233 RTVolume - contains the last trade price, last trade size, last trade time, total volume, VWAP, and single trade flag.
     *      - 236 Shortable
     *      - 256 Inventory
     *      - 258 Fundamental Ratios
     *      - 411 Realtime Historical Volatility
     *      - 456 IBDividends
     * @param snapshot for users with corresponding real time market data subscriptions. A true value will return a one-time snapshot, while a false value will provide streaming data.
     * @param regulatory snapshot for US stocks requests NBBO snapshots for users which have "US Securities Snapshot Bundle" subscription but not corresponding Network A, B, or C subscription necessary for streaming market data. One-time snapshot of current market price that will incur a fee of 1 cent to the account per snapshot.
     * @sa cancelMktData, EWrapper::tickPrice, EWrapper::tickSize, EWrapper::tickString, EWrapper::tickEFP, EWrapper::tickGeneric, EWrapper::tickOptionComputation, EWrapper::tickSnapshotEnd
     */
    public void reqMktData(int tickerId, Contract contract, string genericTickList, bool snapshot, bool regulatorySnaphsot, List<TagValue> mktDataOptions)
    {
      if (!CheckConnection()) return;
      if (snapshot && !CheckServerVersion(tickerId, MinServerVer.SNAPSHOT_MKT_DATA, "It does not support snapshot market data requests.")) return;
      if (contract.DeltaNeutralContract != null && !CheckServerVersion(tickerId, MinServerVer.DELTA_NEUTRAL, " It does not support delta-neutral orders")) return;
      if (contract.ConId > 0 && !CheckServerVersion(tickerId, MinServerVer.CONTRACT_CONID, " It does not support ConId parameter")) return;
      if (!Util.StringIsEmpty(contract.TradingClass) && !CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support trading class parameter in reqMktData.")) return;

      var version = 11;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestMarketData);
        paramsList.AddParameter(version);
        paramsList.AddParameter(tickerId);
        if (serverVersion >= MinServerVer.CONTRACT_CONID) paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        if (serverVersion >= 15) paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        if (serverVersion >= 14) paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        if (serverVersion >= 2) paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        if (serverVersion >= 8 && Constants.BagSecType.Equals(contract.SecType))
        {
          if (contract.ComboLegs != null)
          {
            paramsList.AddParameter(contract.ComboLegs.Count);
            for (var i = 0; i < contract.ComboLegs.Count; i++)
            {
              var leg = contract.ComboLegs[i];
              paramsList.AddParameter(leg.ConId);
              paramsList.AddParameter(leg.Ratio);
              paramsList.AddParameter(leg.Action);
              paramsList.AddParameter(leg.Exchange);
            }
          }
          else
          {
            paramsList.AddParameter(0);
          }
        }

        if (serverVersion >= MinServerVer.DELTA_NEUTRAL)
        {
          if (contract.DeltaNeutralContract != null)
          {
            paramsList.AddParameter(true);
            paramsList.AddParameter(contract.DeltaNeutralContract.ConId);
            paramsList.AddParameter(contract.DeltaNeutralContract.Delta);
            paramsList.AddParameter(contract.DeltaNeutralContract.Price);
          }
          else
          {
            paramsList.AddParameter(false);
          }
        }
        if (serverVersion >= 31) paramsList.AddParameter(genericTickList);
        if (serverVersion >= MinServerVer.SNAPSHOT_MKT_DATA) paramsList.AddParameter(snapshot);
        if (serverVersion >= MinServerVer.SMART_COMPONENTS) paramsList.AddParameter(regulatorySnaphsot);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(mktDataOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(tickerId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQMKT);
    }

    /**
     * @brief Switches data type returned from reqMktData request to "frozen", "delayed" or "delayed-frozen" market data. Requires TWS/IBG v963+.\n
     * The API can receive frozen market data from Trader Workstation. Frozen market data is the last data recorded in our system.\n During normal trading hours, the API receives real-time market data. Invoking this function with argument 2 requests a switch to frozen data immediately or after the close.\n When the market reopens, the market data type will automatically switch back to real time if available.
     * @param marketDataType:
     *      by default only real-time (1) market data is enabled
     *      sending 1 (real-time) disables frozen, delayed and delayed-frozen market data
     *      sending 2 (frozen) enables frozen market data
     *      sending 3 (delayed) enables delayed and disables delayed-frozen market data
     *      sending 4 (delayed-frozen) enables delayed and delayed-frozen market data
     */
    public void reqMarketDataType(int marketDataType)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_MARKET_DATA_TYPE, " It does not support market data type requests.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestMarketDataType);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(marketDataType);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQMARKETDATATYPE);
    }

    /**
     * @brief Requests the contract's market depth (order book).\n This request must be direct-routed to an exchange and not smart-routed. The number of simultaneous market depth requests allowed in an account is calculated based on a formula that looks at an accounts equity, commissions, and quote booster packs.
     * @param tickerId the request's identifier
     * @param contract the Contract for which the depth is being requested
     * @param numRows the number of rows on each side of the order book
     * @param isSmartDepth flag indicates that this is smart depth request
     * @sa cancelMktDepth, EWrapper::updateMktDepth, EWrapper::updateMktDepthL2
     */
    public void reqMarketDepth(int tickerId, Contract contract, int numRows, bool isSmartDepth, List<TagValue> mktDepthOptions)
    {
      if (!CheckConnection()) return;
      if ((!IsEmpty(contract.TradingClass) || contract.ConId > 0) && !CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support ConId nor TradingClass parameters in reqMktDepth.")) return;
      if (isSmartDepth && !CheckServerVersion(tickerId, MinServerVer.SMART_DEPTH, " It does not support SMART depth request.")) return;
      if (!IsEmpty(contract.PrimaryExch) && !CheckServerVersion(tickerId, MinServerVer.MKT_DEPTH_PRIM_EXCHANGE, " It does not support PrimaryExch parameter in reqMktDepth.")) return;

      const int VERSION = 5;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestMarketDepth);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(tickerId);
        // paramsList.AddParameter contract fields
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        if (serverVersion >= 15) paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        if (serverVersion >= MinServerVer.MKT_DEPTH_PRIM_EXCHANGE) paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        if (serverVersion >= 19) paramsList.AddParameter(numRows);
        if (serverVersion >= MinServerVer.SMART_DEPTH) paramsList.AddParameter(isSmartDepth);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(mktDepthOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(tickerId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQMKTDEPTH);
    }

    /**
     * @brief Subscribes to IB's News Bulletins
     * @param allMessages if set to true, will return all the existing bulletins for the current day, set to false to receive only the new bulletins.
     * @sa cancelNewsBulletin, EWrapper::updateNewsBulletin
     */
    public void reqNewsBulletins(bool allMessages)
    {
      if (!CheckConnection()) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestNewsBulletins);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(allMessages);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_GENERIC);
    }

    /**
     * @brief Requests all open orders places by this specific API client (identified by the API client id). For client ID 0, this will bind previous manual TWS orders.
     * @sa reqAllOpenOrders, reqAutoOpenOrders, placeOrder, cancelOrder, reqGlobalCancel, EWrapper::openOrder, EWrapper::orderStatus, EWrapper::openOrderEnd
     */
    public void reqOpenOrders()
    {
      var VERSION = 1;
      if (!CheckConnection()) return;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestOpenOrders);
      paramsList.AddParameter(VERSION);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_OORDER);
    }

    /**
     * @brief Subscribes to position updates for all accessible accounts. All positions sent initially, and then only updates as positions change.
     * @sa cancelPositions, EWrapper::position, EWrapper::positionEnd
     */
    public void reqPositions()
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.ACCT_SUMMARY, " It does not support position requests.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestPositions);
      paramsList.AddParameter(VERSION);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQPOSITIONS);
    }

    /**
     * @brief Requests real time bars\n
     * Currently, only 5 seconds bars are provided. This request is subject to the same pacing as any historical data request: no more than 60 API queries in more than 600 seconds.\n Real time bars subscriptions are also included in the calculation of the number of Level 1 market data subscriptions allowed in an account.
     * @param tickerId the request's unique identifier.
     * @param contract the Contract for which the depth is being requested
     * @param barSize currently being ignored
     * @param whatToShow the nature of the data being retrieved:
     *      - TRADES
     *      - MIDPOINT
     *      - BID
     *      - ASK
     * @param useRTH set to 0 to obtain the data which was also generated ourside of the Regular Trading Hours, set to 1 to obtain only the RTH data
     * @sa cancelRealTimeBars, EWrapper::realtimeBar
     */
    public void reqRealTimeBars(int tickerId, Contract contract, int barSize, string whatToShow, bool useRTH, List<TagValue> realTimeBarsOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(tickerId, MinServerVer.REAL_TIME_BARS, " It does not support real time bars.")) return;
      if ((!IsEmpty(contract.TradingClass) || contract.ConId > 0) && !CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support ConId nor TradingClass parameters in reqRealTimeBars.")) return;

      const int VERSION = 3;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestRealTimeBars);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(tickerId);
        // paramsList.AddParameter contract fields
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.ConId);
        paramsList.AddParameter(contract.Symbol);
        paramsList.AddParameter(contract.SecType);
        paramsList.AddParameter(contract.LastTradeDateOrContractMonth);
        paramsList.AddParameter(contract.Strike);
        paramsList.AddParameter(contract.Right);
        paramsList.AddParameter(contract.Multiplier);
        paramsList.AddParameter(contract.Exchange);
        paramsList.AddParameter(contract.PrimaryExch);
        paramsList.AddParameter(contract.Currency);
        paramsList.AddParameter(contract.LocalSymbol);
        if (serverVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
        paramsList.AddParameter(barSize); // this parameter is not currently used
        paramsList.AddParameter(whatToShow);
        paramsList.AddParameter(useRTH);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(realTimeBarsOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(tickerId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQRTBARS);
    }

    /**
     * @brief Requests an XML list of scanner parameters valid in TWS. \n
     * Not all parameters are valid from API scanner.
     * @sa reqScannerSubscription
     */
    public void reqScannerParameters()
    {
      if (!CheckConnection()) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestScannerParameters);
      paramsList.AddParameter(VERSION);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQSCANNERPARAMETERS);
    }

    /**
     * @brief Starts a subscription to market scan results based on the provided parameters.
     * @param reqId the request's identifier
     * @param subscription summary of the scanner subscription including its filters.
     * @sa reqScannerParameters, ScannerSubscription, EWrapper::scannerData
     */
    public void reqScannerSubscription(int reqId, ScannerSubscription subscription, List<TagValue> scannerSubscriptionOptions, List<TagValue> scannerSubscriptionFilterOptions) => reqScannerSubscription(reqId, subscription, Util.TagValueListToString(scannerSubscriptionOptions), Util.TagValueListToString(scannerSubscriptionFilterOptions));

    public void reqScannerSubscription(int reqId, ScannerSubscription subscription, string scannerSubscriptionOptions, string scannerSubscriptionFilterOptions)
    {
      if (!CheckConnection()) return;
      if (scannerSubscriptionFilterOptions != null && !CheckServerVersion(MinServerVer.SCANNER_GENERIC_OPTS, " It does not support API scanner subscription generic filter options")) return;

      const int VERSION = 4;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestScannerSubscription);
        if (serverVersion < MinServerVer.SCANNER_GENERIC_OPTS) paramsList.AddParameter(VERSION);
        paramsList.AddParameter(reqId);
        paramsList.AddParameterMax(subscription.NumberOfRows);
        paramsList.AddParameter(subscription.Instrument);
        paramsList.AddParameter(subscription.LocationCode);
        paramsList.AddParameter(subscription.ScanCode);

        paramsList.AddParameterMax(subscription.AbovePrice);
        paramsList.AddParameterMax(subscription.BelowPrice);
        paramsList.AddParameterMax(subscription.AboveVolume);
        paramsList.AddParameterMax(subscription.MarketCapAbove);
        paramsList.AddParameterMax(subscription.MarketCapBelow);
        paramsList.AddParameter(subscription.MoodyRatingAbove);
        paramsList.AddParameter(subscription.MoodyRatingBelow);
        paramsList.AddParameter(subscription.SpRatingAbove);
        paramsList.AddParameter(subscription.SpRatingBelow);
        paramsList.AddParameter(subscription.MaturityDateAbove);
        paramsList.AddParameter(subscription.MaturityDateBelow);
        paramsList.AddParameterMax(subscription.CouponRateAbove);
        paramsList.AddParameterMax(subscription.CouponRateBelow);
        paramsList.AddParameter(subscription.ExcludeConvertible);

        if (serverVersion >= 25)
        {
          paramsList.AddParameterMax(subscription.AverageOptionVolumeAbove);
          paramsList.AddParameter(subscription.ScannerSettingPairs);
        }

        if (serverVersion >= 27) paramsList.AddParameter(subscription.StockTypeFilter);
        if (serverVersion >= MinServerVer.SCANNER_GENERIC_OPTS) paramsList.AddParameter(scannerSubscriptionFilterOptions);
        if (serverVersion >= MinServerVer.LINKING) paramsList.AddParameter(scannerSubscriptionOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQSCANNER);
    }

    /**
     * @brief Changes the TWS/GW log level.
     * The default is 2 = ERROR\n
     * 5 = DETAIL is required for capturing all API messages and troubleshooting API programs\n
     * Valid values are:\n
     * 1 = SYSTEM\n
     * 2 = ERROR\n
     * 3 = WARNING\n
     * 4 = INFORMATION\n
     * 5 = DETAIL\n
     */
    public void setServerLogLevel(int logLevel)
    {
      if (!CheckConnection()) return;

      const int VERSION = 1;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.ChangeServerLog);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(logLevel);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_SERVER_LOG_LEVEL);
    }

    /**
     * @brief For IB's internal purpose. Allows to provide means of verification between the TWS and third party programs.
     */
    public void verifyRequest(string apiName, string apiVersion)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING, " It does not support verification request.")) return;
      if (!extraAuth)
      {
        ReportError(IncomingMessage.NotValid, EClientErrors.FAIL_SEND_VERIFYMESSAGE, " Intent to authenticate needs to be expressed during initial connect request.");
        return;
      }

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.VerifyRequest);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(apiName);
        paramsList.AddParameter(apiVersion);
      }
      catch (EClientException e)
      {
        wrapper.error(IncomingMessage.NotValid, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_VERIFYREQUEST);
    }

    /**
     * @brief For IB's internal purpose. Allows to provide means of verification between the TWS and third party programs.
     */
    public void verifyMessage(string apiData)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING, " It does not support verification message sending.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.VerifyMessage);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(apiData);
      }
      catch (EClientException e)
      {
        wrapper.error(IncomingMessage.NotValid, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_VERIFYMESSAGE);
    }

    /**
     * @brief For IB's internal purpose. Allows to provide means of verification between the TWS and third party programs.
     */
    public void verifyAndAuthRequest(string apiName, string apiVersion, string opaqueIsvKey)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING_AUTH, " It does not support verification request.")) return;
      if (!extraAuth)
      {
        ReportError(IncomingMessage.NotValid, EClientErrors.FAIL_SEND_VERIFYANDAUTHMESSAGE, " Intent to authenticate needs to be expressed during initial connect request.");
        return;
      }

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.VerifyAndAuthRequest);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(apiName);
        paramsList.AddParameter(apiVersion);
        paramsList.AddParameter(opaqueIsvKey);
      }
      catch (EClientException e)
      {
        wrapper.error(IncomingMessage.NotValid, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_VERIFYANDAUTHREQUEST);
    }

    /**
     * @brief For IB's internal purpose. Allows to provide means of verification between the TWS and third party programs.
     */
    public void verifyAndAuthMessage(string apiData, string xyzResponse)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING_AUTH, " It does not support verification message sending.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.VerifyAndAuthMessage);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(apiData);
        paramsList.AddParameter(xyzResponse);
      }
      catch (EClientException e)
      {
        wrapper.error(IncomingMessage.NotValid, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_VERIFYANDAUTHMESSAGE);
    }

    /**
     * @brief Requests all available Display Groups in TWS
     * @param requestId is the ID of this request
     */
    public void queryDisplayGroups(int requestId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING, " It does not support queryDisplayGroups request.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.QueryDisplayGroups);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(requestId);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_QUERYDISPLAYGROUPS);
    }

    /**
     * @brief Integrates API client and TWS window grouping.
     * @param requestId is the Id chosen for this subscription request
     * @param groupId is the display group for integration
     */
    public void subscribeToGroupEvents(int requestId, int groupId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING, " It does not support subscribeToGroupEvents request.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.SubscribeToGroupEvents);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(requestId);
      paramsList.AddParameter(groupId);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_SUBSCRIBETOGROUPEVENTS);
    }

    /**
     * @brief Updates the contract displayed in a TWS Window Group
     * @param requestId is the ID chosen for this request
     * @param contractInfo is an encoded value designating a unique IB contract. Possible values include:
     * 1. none = empty selection
     * 2. contractID@exchange - any non-combination contract. Examples 8314@SMART for IBM SMART; 8314@ARCA for IBM ARCA
     * 3. combo= if any combo is selected
     * Note: This request from the API does not get a TWS response unless an error occurs.
     */
    public void updateDisplayGroup(int requestId, string contractInfo)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING, " It does not support updateDisplayGroup request.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.UpdateDisplayGroup);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(requestId);
        paramsList.AddParameter(contractInfo);
      }
      catch (EClientException e)
      {
        wrapper.error(requestId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_UPDATEDISPLAYGROUP);
    }

    /**
     * @brief Cancels a TWS Window Group subscription
     */
    public void unsubscribeFromGroupEvents(int requestId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.LINKING, " It does not support unsubscribeFromGroupEvents request.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.UnsubscribeFromGroupEvents);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(requestId);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_UNSUBSCRIBEFROMGROUPEVENTS);
    }

    /**
     * @brief Requests position subscription for account and/or model
     * Initially all positions are returned, and then updates are returned for any position changes in real time.
     * @param requestId - Request's identifier
     * @param account - If an account Id is provided, only the account's positions belonging to the specified model will be delivered
     * @param modelCode - The code of the model's positions we are interested in.
     * @sa cancelPositionsMulti, EWrapper::positionMulti, EWrapper::positionMultiEnd
     */
    public void reqPositionsMulti(int requestId, string account, string modelCode)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.MODELS_SUPPORT, " It does not support positions multi requests.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestPositionsMulti);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(requestId);
        paramsList.AddParameter(account);
        paramsList.AddParameter(modelCode);
      }
      catch (EClientException e)
      {
        wrapper.error(requestId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQPOSITIONSMULTI);
    }

    /**
     * @brief Cancels positions request for account and/or model
     * @param requestId - the identifier of the request to be canceled.
     * @sa reqPositionsMulti
     */
    public void cancelPositionsMulti(int requestId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.MODELS_SUPPORT, " It does not support positions multi cancellation.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelPositionsMulti);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(requestId);
      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_CANPOSITIONSMULTI);
    }

    /**
     * @brief Requests account updates for account and/or model
     * @param reqId identifier to label the request
     * @param account account values can be requested for a particular account
     * @param modelCode values can also be requested for a model
     * @param ledgerAndNLV returns light-weight request; only currency positions as opposed to account values and currency positions
     * @sa cancelAccountUpdatesMulti, EWrapper::accountUpdateMulti, EWrapper::accountUpdateMultiEnd
     */
    public void reqAccountUpdatesMulti(int requestId, string account, string modelCode, bool ledgerAndNLV)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.MODELS_SUPPORT, " It does not support account updates multi requests.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestAccountUpdatesMulti);
        paramsList.AddParameter(VERSION);
        paramsList.AddParameter(requestId);
        paramsList.AddParameter(account);
        paramsList.AddParameter(modelCode);
        paramsList.AddParameter(ledgerAndNLV);
      }
      catch (EClientException e)
      {
        wrapper.error(requestId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQACCOUNTUPDATESMULTI);
    }

    /**
     * @brief Cancels account updates request for account and/or model
     * @param requestId account subscription to cancel
     * @sa reqAccountUpdatesMulti
     */
    public void cancelAccountUpdatesMulti(int requestId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.MODELS_SUPPORT, " It does not support account updates multi cancellation.")) return;

      const int VERSION = 1;
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelAccountUpdatesMulti);
      paramsList.AddParameter(VERSION);
      paramsList.AddParameter(requestId);
      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_CANACCOUNTUPDATESMULTI);
    }

    /**
     * @brief Requests security definition option parameters for viewing a contract's option chain
     * @param reqId the ID chosen for the request
     * @param underlyingSymbol
     * @param futFopExchange The exchange on which the returned options are trading. Can be set to the empty string "" for all exchanges.
     * @param underlyingSecType The type of the underlying security, i.e. STK
     * @param underlyingConId the contract ID of the underlying security
     * @sa EWrapper::securityDefinitionOptionParameter
     */
    public void reqSecDefOptParams(int reqId, string underlyingSymbol, string futFopExchange, string underlyingSecType, int underlyingConId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.SEC_DEF_OPT_PARAMS_REQ, " It does not support security definition option parameters.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestSecurityDefinitionOptionalParameters);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(underlyingSymbol);
        paramsList.AddParameter(futFopExchange);
        paramsList.AddParameter(underlyingSecType);
        paramsList.AddParameter(underlyingConId);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQSECDEFOPTPARAMS);
    }

    /**
     * @brief Requests pre-defined Soft Dollar Tiers. This is only supported for registered professional advisors and hedge and mutual funds who have configured Soft Dollar Tiers in Account Management. Refer to: https://www.interactivebrokers.com/en/software/am/am/manageaccount/requestsoftdollars.htm?Highlight=soft%20dollar%20tier
     * @sa EWrapper::softDollarTiers
     */
    public void reqSoftDollarTiers(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.SOFT_DOLLAR_TIER, " It does not support soft dollar tier.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestSoftDollarTiers);
      paramsList.AddParameter(reqId);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQSOFTDOLLARTIERS);
    }

    /**
     * @brief Requests family codes for an account, for instance if it is a FA, IBroker, or associated account.
     * @sa EWrapper::familyCodes
     */
    public void reqFamilyCodes()
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_FAMILY_CODES, " It does not support family codes requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestFamilyCodes);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQFAMILYCODES);
    }

    /**
     * @brief Requests matching stock symbols
     * @param reqId id to specify the request
     * @param pattern - either start of ticker symbol or (for larger strings) company name
     * @sa EWrapper::symbolSamples
     */
    public void reqMatchingSymbols(int reqId, string pattern)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_MATCHING_SYMBOLS, " It does not support mathing symbols requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestMatchingSymbols);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(pattern);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQMATCHINGSYMBOLS);
    }

    /**
     * @brief Requests venues for which market data is returned to updateMktDepthL2 (those with market makers)
     * @sa EWrapper::mktDepthExchanges
     */
    public void reqMktDepthExchanges()
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_MKT_DEPTH_EXCHANGES, " It does not support market depth exchanges requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestMktDepthExchanges);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQMKTDEPTHEXCHANGES);
    }

    /**
     * @brief Returns the mapping of single letter codes to exchange names given the mapping identifier
     * @param reqId id of the request
     * @param bboExchange mapping identifier received from EWrapper.tickReqParams
     * @sa EWrapper::smartComponents
     */
    public void reqSmartComponents(int reqId, string bboExchange)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_MKT_DEPTH_EXCHANGES, " It does not support smart components request.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestSmartComponents);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(bboExchange);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQSMARTCOMPONENTS);
    }

    /**
     * @brief Requests news providers which the user has subscribed to.
     * @sa EWrapper::newsProviders
     */
    public void reqNewsProviders()
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_NEWS_PROVIDERS, " It does not support news providers requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestNewsProviders);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQNEWSPROVIDERS);
    }

    /**
     * @brief Requests news article body given articleId.
     * @param requestId id of the request
     * @param providerCode short code indicating news provider, e.g. FLY
     * @param articleId id of the specific article
     * @param newsArticleOptions reserved for internal use. Should be defined as null.
     * @sa EWrapper::newsArticle,
     */
    public void reqNewsArticle(int requestId, string providerCode, string articleId, List<TagValue> newsArticleOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_NEWS_ARTICLE, " It does not support news article requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestNewsArticle);
        paramsList.AddParameter(requestId);
        paramsList.AddParameter(providerCode);
        paramsList.AddParameter(articleId);
        if (serverVersion >= MinServerVer.NEWS_QUERY_ORIGINS) paramsList.AddParameter(newsArticleOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(requestId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQNEWSARTICLE);
    }

    /**
     * @brief Requests historical news headlines
     * @param requestId
     * @param conId - contract id of ticker
     * @param providerCodes - a '+'-separated list of provider codes
     * @param startDateTime - marks the (exclusive) start of the date range. The format is yyyy-MM-dd HH:mm:ss.0
     * @param endDateTime - marks the (inclusive) end of the date range. The format is yyyy-MM-dd HH:mm:ss.0
     * @param totalResults - the maximum number of headlines to fetch (1 - 300)
     * @param historicalNewsOptions reserved for internal use. Should be defined as null.
     * @sa EWrapper::historicalNews, EWrapper::historicalNewsEnd
     */
    public void reqHistoricalNews(int requestId, int conId, string providerCodes, string startDateTime, string endDateTime, int totalResults, List<TagValue> historicalNewsOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_HISTORICAL_NEWS, " It does not support historical news requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestHistoricalNews);
        paramsList.AddParameter(requestId);
        paramsList.AddParameter(conId);
        paramsList.AddParameter(providerCodes);
        paramsList.AddParameter(startDateTime);
        paramsList.AddParameter(endDateTime);
        paramsList.AddParameter(totalResults);
        if (serverVersion >= MinServerVer.NEWS_QUERY_ORIGINS) paramsList.AddParameter(historicalNewsOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(requestId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(requestId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQHISTORICALNEWS);
    }

    /**
     * @brief Returns the timestamp of earliest available historical data for a contract and data type
     * @param tickerId - an identifier for the request
     * @param contract - contract object for which head timestamp is being requested
     * @param whatToShow - type of data for head timestamp - "BID", "ASK", "TRADES", etc
     * @param useRTH - use regular trading hours only, 1 for yes or 0 for no
     * @param formatDate - @param formatDate set to 1 to obtain the bars' time as yyyyMMdd HH:mm:ss, set to 2 to obtain it like system time format in seconds
     * @sa headTimeStamp
     */
    public void reqHeadTimestamp(int tickerId, Contract contract, string whatToShow, int useRTH, int formatDate)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_HEAD_TIMESTAMP, " It does not support head time stamp requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestHeadTimestamp);
        paramsList.AddParameter(tickerId);
        paramsList.AddParameter(contract);
        paramsList.AddParameter(useRTH);
        paramsList.AddParameter(whatToShow);
        paramsList.AddParameter(formatDate);
      }
      catch (EClientException e)
      {
        wrapper.error(tickerId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQHEADTIMESTAMP);
    }

    /**
     * @brief Cancels a pending reqHeadTimeStamp request\n
     * @param tickerId Id of the request
     */
    public void cancelHeadTimestamp(int tickerId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.CANCEL_HEADTIMESTAMP, " It does not support head time stamp requests canceling.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelHeadTimestamp);
      paramsList.AddParameter(tickerId);
      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_CANCELHEADTIMESTAMP);
    }


    /**
     * @brief Returns data histogram of specified contract\n
     * @param tickerId - an identifier for the request\n
     * @param contract - Contract object for which histogram is being requested\n
     * @param useRTH - use regular trading hours only, 1 for yes or 0 for no\n
     * @param period - period of which data is being requested, e.g. "3 days"\n
     * @sa histogramData
     */
    public void reqHistogramData(int tickerId, Contract contract, bool useRTH, string period)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_HISTOGRAM_DATA, " It does not support histogram data requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.RequestHistogramData);
        paramsList.AddParameter(tickerId);
        paramsList.AddParameter(contract);
        paramsList.AddParameter(useRTH);
        paramsList.AddParameter(period);
      }
      catch (EClientException e)
      {
        wrapper.error(tickerId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(tickerId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQHISTOGRAMDATA);
    }

    /**
     * @brief Cancels an active data histogram request
     * @param tickerId - identifier specified in reqHistogramData request
     * @sa reqHistogramData, histogramData
     */
    public void cancelHistogramData(int tickerId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.REQ_HISTOGRAM_DATA, " It does not support histogram data requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelHistogramData);
      paramsList.AddParameter(tickerId);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_CANCELHISTOGRAMDATA);
    }

    /**
     * @brief Requests details about a given market rule\n
     * The market rule for an instrument on a particular exchange provides details about how the minimum price increment changes with price\n
     * A list of market rule ids can be obtained by invoking reqContractDetails on a particular contract. The returned market rule ID list will provide the market rule ID for the instrument in the correspond valid exchange list in contractDetails.\n
     * @param marketRuleId - the id of market rule\n
     * @sa EWrapper::marketRule
     */
    public void reqMarketRule(int marketRuleId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.MARKET_RULES, " It does not support market rule requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.RequestMarketRule);
      paramsList.AddParameter(marketRuleId);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQMARKETRULE);
    }

    /**
     * @brief Creates subscription for real time daily PnL and unrealized PnL updates
     * @param account account for which to receive PnL updates
     * @param modelCode specify to request PnL updates for a specific model
     */
    public void reqPnL(int reqId, string account, string modelCode)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.PNL, "  It does not support PnL requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqPnL);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(account);
        paramsList.AddParameter(modelCode);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQPNL);
    }

    /**
     * @brief cancels subscription for real time updated daily PnL
     * params reqId
     */
    public void cancelPnL(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.PNL, "  It does not support PnL requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelPnL);
      paramsList.AddParameter(reqId);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_CANCELPNL);
    }

    /**
     * @brief Requests real time updates for daily PnL of individual positions
     * @param reqId
     * @param account account in which position exists
     * @param modelCode model in which position exists
     * @param conId contract ID (conId) of contract to receive daily PnL updates for.
     * Note: does not return message if invalid conId is entered
     */
    public void reqPnLSingle(int reqId, string account, string modelCode, int conId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.PNL, "  It does not support PnL requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqPnLSingle);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(account);
        paramsList.AddParameter(modelCode);
        paramsList.AddParameter(conId);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQPNLSINGLE);
    }

    /**
     * @brief Cancels real time subscription for a positions daily PnL information
     * @param reqId
     */
    public void cancelPnLSingle(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.PNL, "  It does not support PnL requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelPnLSingle);
      paramsList.AddParameter(reqId);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_REQPNLSINGLE);
    }

    /**
     * @brief Requests historical Time&Sales data for an instrument
     * @param reqId id of the request
     * @param contract Contract object that is subject of query
     * @param startDateTime ,i.e. "20170701 12:01:00". Uses TWS timezone specified at login.
     * @param endDateTime ,i.e. "20170701 13:01:00". In TWS timezone. Exactly one of start time and end time has to be defined.
     * @param numberOfTicks Number of distinct data points. Max currently 1000 per request.
     * @param whatToShow (Bid_Ask, Midpoint, Trades) Type of data requested.
     * @param useRth Data from regular trading hours (1), or all available hours (0)
     * @param ignoreSize A filter only used when the source price is Bid_Ask
     * @param miscOptions should be defined as <i>null</i>, reserved for internal use
     */
    public void reqHistoricalTicks(int reqId, Contract contract, string startDateTime,
                                   string endDateTime, int numberOfTicks, string whatToShow, int useRth, bool ignoreSize,
                                   List<TagValue> miscOptions)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.HISTORICAL_TICKS, "  It does not support historical ticks request.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqHistoricalTicks);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(contract);
        paramsList.AddParameter(startDateTime);
        paramsList.AddParameter(endDateTime);
        paramsList.AddParameter(numberOfTicks);
        paramsList.AddParameter(whatToShow);
        paramsList.AddParameter(useRth);
        paramsList.AddParameter(ignoreSize);
        paramsList.AddParameter(miscOptions);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQHISTORICALTICKS);
    }

    /**
     * @brief Requests metadata from the WSH calendar
     * @param reqId
     */
    public void reqWshMetaData(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.WSHE_CALENDAR, "  It does not support WSHE Calendar API.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqWshMetaData);
        paramsList.AddParameter(reqId);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQ_WSH_META_DATA);
    }

    /**
     * @brief Cancels pending request for WSH metadata
     * @param reqId
     */
    public void cancelWshMetaData(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.WSHE_CALENDAR, "  It does not support WSHE Calendar API.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelWshMetaData);
      paramsList.AddParameter(reqId);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_CAN_WSH_META_DATA);
    }

    /**
     * @brief Requests event data from the wSH calendar
     * @param reqId
     * @param conId contract ID (conId) of contract to receive WSH Event Data for.
     */
    public void reqWshEventData(int reqId, WshEventData wshEventData)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.WSHE_CALENDAR, "  It does not support WSHE Calendar API.")) return;

      if (serverVersion < MinServerVer.MIN_SERVER_VER_WSH_EVENT_DATA_FILTERS)
      {
        if (!IsEmpty(wshEventData.Filter) || wshEventData.FillWatchlist || wshEventData.FillPortfolio || wshEventData.FillCompetitors)
        {
          ReportError(reqId, EClientErrors.UPDATE_TWS, "  It does not support WSH event data filters.");
          return;
        }
      }

      if (serverVersion < MinServerVer.MIN_SERVER_VER_WSH_EVENT_DATA_FILTERS_DATE)
      {
        if (!IsEmpty(wshEventData.StartDate) || !IsEmpty(wshEventData.EndDate) || wshEventData.TotalLimit != int.MaxValue)
        {
          ReportError(reqId, EClientErrors.UPDATE_TWS, "  It does not support WSH event data date filters.");
          return;
        }
      }

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqWshEventData);
        paramsList.AddParameter(reqId);
        paramsList.AddParameter(wshEventData.ConId);

        if (serverVersion >= MinServerVer.MIN_SERVER_VER_WSH_EVENT_DATA_FILTERS)
        {
          paramsList.AddParameter(wshEventData.Filter);
          paramsList.AddParameter(wshEventData.FillWatchlist);
          paramsList.AddParameter(wshEventData.FillPortfolio);
          paramsList.AddParameter(wshEventData.FillCompetitors);
        }

        if (serverVersion >= MinServerVer.MIN_SERVER_VER_WSH_EVENT_DATA_FILTERS_DATE)
        {
          paramsList.AddParameter(wshEventData.StartDate);
          paramsList.AddParameter(wshEventData.EndDate);
          paramsList.AddParameter(wshEventData.TotalLimit);
        }
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQ_WSH_EVENT_DATA);
    }

    /**
     * @brief Cancels pending WSH event data request
     * @param reqId
     */
    public void cancelWshEventData(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.WSHE_CALENDAR, "  It does not support WSHE Calendar API.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      paramsList.AddParameter(OutgoingMessages.CancelWshEventData);
      paramsList.AddParameter(reqId);

      CloseAndSend(paramsList, lengthPos, EClientErrors.FAIL_SEND_CAN_WSH_EVENT_DATA);
    }

    /**
     * @brief Requests user info
     * @param reqId
     */
    public void reqUserInfo(int reqId)
    {
      if (!CheckConnection()) return;
      if (!CheckServerVersion(MinServerVer.USER_INFO, " It does not support user info requests.")) return;

      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(OutgoingMessages.ReqUserInfo);
        paramsList.AddParameter(reqId);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      CloseAndSend(reqId, paramsList, lengthPos, EClientErrors.FAIL_SEND_REQ_USER_INFO);
    }

    protected bool CheckServerVersion(int requiredVersion) => CheckServerVersion(requiredVersion, "");

    protected bool CheckServerVersion(int requestId, int requiredVersion) => CheckServerVersion(requestId, requiredVersion, "");

    protected bool CheckServerVersion(int requiredVersion, string updatetail) => CheckServerVersion(IncomingMessage.NotValid, requiredVersion, updatetail);

    protected bool CheckServerVersion(int tickerId, int requiredVersion, string updatetail)
    {
      if (serverVersion >= requiredVersion) return true;
      ReportUpdateTWS(tickerId, updatetail);
      return false;
    }

    protected void CloseAndSend(BinaryWriter paramsList, uint lengthPos, CodeMsgPair error)
    {
      CloseAndSend(IncomingMessage.NotValid, paramsList, lengthPos, error);
    }

    protected void CloseAndSend(int reqId, BinaryWriter paramsList, uint lengthPos, CodeMsgPair error)
    {
      try
      {
        CloseAndSend(paramsList, lengthPos);
      }
      catch (Exception)
      {
        wrapper.error(reqId, error.Code, error.Message, "");
        Close();
      }
    }

    protected abstract void CloseAndSend(BinaryWriter request, uint lengthPos);

    protected bool CheckConnection()
    {
      if (isConnected) return true;
      wrapper.error(IncomingMessage.NotValid, EClientErrors.NOT_CONNECTED.Code, EClientErrors.NOT_CONNECTED.Message, "");
      return false;
    }

    protected void ReportError(int reqId, CodeMsgPair error, string tail) => ReportError(reqId, error.Code, error.Message + tail);

    protected void ReportUpdateTWS(int reqId, string tail) => ReportError(reqId, EClientErrors.UPDATE_TWS.Code, EClientErrors.UPDATE_TWS.Message + tail);

    protected void ReportUpdateTWS(string tail) => ReportError(IncomingMessage.NotValid, EClientErrors.UPDATE_TWS.Code, EClientErrors.UPDATE_TWS.Message + tail);

    protected void ReportError(int reqId, int code, string message) => wrapper.error(reqId, code, message, "");

    protected void SendCancelRequest(OutgoingMessages msgType, int version, int reqId, CodeMsgPair errorMessage)
    {
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(msgType);
        paramsList.AddParameter(version);
        paramsList.AddParameter(reqId);
      }
      catch (EClientException e)
      {
        wrapper.error(reqId, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      try
      {
        CloseAndSend(paramsList, lengthPos);
      }
      catch (Exception)
      {
        wrapper.error(reqId, errorMessage.Code, errorMessage.Message, "");
        Close();
      }
    }

    protected void SendCancelRequest(OutgoingMessages msgType, int version, CodeMsgPair errorMessage)
    {
      var paramsList = new BinaryWriter(new MemoryStream());
      var lengthPos = prepareBuffer(paramsList);

      try
      {
        paramsList.AddParameter(msgType);
        paramsList.AddParameter(version);
      }
      catch (EClientException e)
      {
        wrapper.error(IncomingMessage.NotValid, e.Err.Code, e.Err.Message + e.Text, "");
        return;
      }

      try
      {
        CloseAndSend(paramsList, lengthPos);
      }
      catch (Exception)
      {
        wrapper.error(IncomingMessage.NotValid, errorMessage.Code, errorMessage.Message, "");
        Close();
      }
    }

    protected bool VerifyOrderContract(Contract contract, int id)
    {
      if (serverVersion < MinServerVer.SSHORT_COMBO_LEGS)
      {
        if (contract.ComboLegs.Count > 0)
        {
          ComboLeg comboLeg;
          for (var i = 0; i < contract.ComboLegs.Count; ++i)
          {
            comboLeg = contract.ComboLegs[i];
            if (comboLeg.ShortSaleSlot == 0 && IsEmpty(comboLeg.DesignatedLocation)) continue;
            ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support SSHORT flag for combo legs.");
            return false;
          }
        }
      }

      if (serverVersion < MinServerVer.DELTA_NEUTRAL && contract.DeltaNeutralContract != null)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support delta-neutral orders.");
        return false;
      }

      if (serverVersion < MinServerVer.PLACE_ORDER_CONID && contract.ConId > 0)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support conId parameter.");
        return false;
      }

      if (serverVersion < MinServerVer.SEC_ID_TYPE && (!IsEmpty(contract.SecIdType) || !IsEmpty(contract.SecId)))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support secIdType and secId parameters.");
        return false;
      }
      if (serverVersion < MinServerVer.SSHORTX && contract.ComboLegs.Count > 0)
      {
        ComboLeg comboLeg;
        for (var i = 0; i < contract.ComboLegs.Count; ++i)
        {
          comboLeg = contract.ComboLegs[i];
          if (comboLeg.ExemptCode == -1) continue;
          ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support exemptCode parameter.");
          return false;
        }
      }
      if (serverVersion < MinServerVer.TRADING_CLASS && !IsEmpty(contract.TradingClass))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support tradingClass parameters in placeOrder.");
        return false;
      }
      return true;
    }

    protected bool VerifyOrder(Order order, int id, bool isBagOrder)
    {
      if (serverVersion < MinServerVer.SCALE_ORDERS && (order.ScaleInitLevelSize != int.MaxValue || order.ScalePriceIncrement != double.MaxValue))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support Scale orders.");
        return false;
      }
      if (serverVersion < MinServerVer.WHAT_IF_ORDERS && order.WhatIf)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support what-if orders.");
        return false;
      }

      if (serverVersion < MinServerVer.SCALE_ORDERS2 && order.ScaleSubsLevelSize != int.MaxValue)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support Subsequent Level Size for Scale orders.");
        return false;
      }

      if (serverVersion < MinServerVer.ALGO_ORDERS && !IsEmpty(order.AlgoStrategy))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support algo orders.");
        return false;
      }

      if (serverVersion < MinServerVer.NOT_HELD && order.NotHeld)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support notHeld parameter.");
        return false;
      }

      if (serverVersion < MinServerVer.SSHORTX && order.ExemptCode != -1)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support exemptCode parameter.");
        return false;
      }

      if (serverVersion < MinServerVer.HEDGE_ORDERS && !IsEmpty(order.HedgeType))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support hedge orders.");
        return false;
      }

      if (serverVersion < MinServerVer.OPT_OUT_SMART_ROUTING && order.OptOutSmartRouting)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support optOutSmartRouting parameter.");
        return false;
      }

      if (serverVersion < MinServerVer.DELTA_NEUTRAL_CONID && (order.DeltaNeutralConId > 0
                                                               || !IsEmpty(order.DeltaNeutralSettlingFirm)
                                                               || !IsEmpty(order.DeltaNeutralClearingAccount)
                                                               || !IsEmpty(order.DeltaNeutralClearingIntent)))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support deltaNeutral parameters: ConId, SettlingFirm, ClearingAccount, ClearingIntent");
        return false;
      }

      if (serverVersion < MinServerVer.DELTA_NEUTRAL_OPEN_CLOSE && (!IsEmpty(order.DeltaNeutralOpenClose)
                                                                    || order.DeltaNeutralShortSale
                                                                    || order.DeltaNeutralShortSaleSlot > 0
                                                                    || !IsEmpty(order.DeltaNeutralDesignatedLocation)))
      {
        ReportError(id, EClientErrors.UPDATE_TWS,
            "  It does not support deltaNeutral parameters: OpenClose, ShortSale, ShortSaleSlot, DesignatedLocation");
        return false;
      }

      if (serverVersion < MinServerVer.SCALE_ORDERS3 && order.ScalePriceIncrement > 0 && order.ScalePriceIncrement != double.MaxValue)
      {
        if (order.ScalePriceAdjustValue != double.MaxValue ||
            order.ScalePriceAdjustInterval != int.MaxValue ||
            order.ScaleProfitOffset != double.MaxValue ||
            order.ScaleAutoReset ||
            order.ScaleInitPosition != int.MaxValue ||
            order.ScaleInitFillQty != int.MaxValue ||
            order.ScaleRandomPercent)
        {
          ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support Scale order parameters: PriceAdjustValue, PriceAdjustInterval, ProfitOffset, AutoReset, InitPosition, InitFillQty and RandomPercent");
          return false;
        }
      }

      if (serverVersion < MinServerVer.ORDER_COMBO_LEGS_PRICE && isBagOrder && order.OrderComboLegs.Count > 0)
      {
        OrderComboLeg orderComboLeg;
        for (var i = 0; i < order.OrderComboLegs.Count; ++i)
        {
          orderComboLeg = order.OrderComboLegs[i];
          if (orderComboLeg.Price == double.MaxValue) continue;
          ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support per-leg prices for order combo legs.");
          return false;
        }
      }

      if (serverVersion < MinServerVer.TRAILING_PERCENT && order.TrailingPercent != double.MaxValue)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support trailing percent parameter.");
        return false;
      }

      if (serverVersion < MinServerVer.ALGO_ID && !IsEmpty(order.AlgoId))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support algoId parameter");
        return false;
      }

      if (serverVersion < MinServerVer.SCALE_TABLE && (!IsEmpty(order.ScaleTable) || !IsEmpty(order.ActiveStartTime) || !IsEmpty(order.ActiveStopTime)))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support scaleTable, activeStartTime nor activeStopTime parameters.");
        return false;
      }

      if (serverVersion < MinServerVer.EXT_OPERATOR && !IsEmpty(order.ExtOperator))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support extOperator parameter");
        return false;
      }

      if (serverVersion < MinServerVer.CASH_QTY && order.CashQty != double.MaxValue)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support cashQty parameter");
        return false;
      }

      if (serverVersion < MinServerVer.DECISION_MAKER && (!IsEmpty(order.Mifid2DecisionMaker)
                                                          || !IsEmpty(order.Mifid2DecisionAlgo)))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support MIFID II decision maker parameters");
        return false;
      }

      if (serverVersion < MinServerVer.DECISION_MAKER && (!IsEmpty(order.Mifid2ExecutionTrader)
                                                          || !IsEmpty(order.Mifid2ExecutionAlgo)))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support MIFID II execution parameters");
        return false;
      }

      if (serverVersion < MinServerVer.AUTO_PRICE_FOR_HEDGE && order.DontUseAutoPriceForHedge)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support don't use auto price for hedge parameter");
        return false;
      }

      if (serverVersion < MinServerVer.ORDER_CONTAINER && order.IsOmsContainer)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support oms container parameter.");
        return false;
      }

      if (serverVersion < MinServerVer.D_PEG_ORDERS && order.DiscretionaryUpToLimitPrice)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support D-Peg orders.");
        return false;
      }

      if (serverVersion < MinServerVer.PRICE_MGMT_ALGO && order.UsePriceMgmtAlgo.HasValue)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support Use Price Management Algo requests.");
        return false;
      }

      if (serverVersion < MinServerVer.DURATION && order.Duration != int.MaxValue)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support duration attribute.");
        return false;
      }

      if (serverVersion < MinServerVer.POST_TO_ATS && order.PostToAts != int.MaxValue)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support postToAts attribute.");
        return false;
      }

      if (serverVersion < MinServerVer.AUTO_CANCEL_PARENT && order.AutoCancelParent)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support autoCancelParent attribute.");
        return false;
      }

      if (serverVersion < MinServerVer.ADVANCED_ORDER_REJECT && !IsEmpty(order.AdvancedErrorOverride))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support advanced error override attribute.");
        return false;
      }

      if (serverVersion < MinServerVer.MANUAL_ORDER_TIME && !IsEmpty(order.ManualOrderTime))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support manual order time attribute.");
        return false;
      }

      if (serverVersion < MinServerVer.PEGBEST_PEGMID_OFFSETS && (order.MinTradeQty != int.MaxValue ||
                                                                  order.MinCompeteSize != int.MaxValue ||
                                                                  order.CompeteAgainstBestOffset != double.MaxValue ||
                                                                  order.MidOffsetAtWhole != double.MaxValue ||
                                                                  order.MidOffsetAtHalf != double.MaxValue))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, "  It does not support PEG BEST / PEG MID order parameters: minTradeQty, minCompeteSize, competeAgainstBestOffset, midOffsetAtWhole and midOffsetAtHalf");
        return false;
      }

      if (serverVersion < MinServerVer.MIN_SERVER_VER_CUSTOMER_ACCOUNT && !IsEmpty(order.CustomerAccount))
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support customer account parameter.");
        return false;
      }

      if (serverVersion < MinServerVer.MIN_SERVER_VER_PROFESSIONAL_CUSTOMER && order.ProfessionalCustomer)
      {
        ReportError(id, EClientErrors.UPDATE_TWS, " It does not support professional customer parameter.");
        return false;
      }

      return true;
    }

    private bool IsEmpty(string str) => Util.StringIsEmpty(str);

    private bool StringsAreEqual(string a, string b) => string.Compare(a, b, true) == 0;

    public bool IsDataAvailable()
    {
      if (!isConnected) return false;
      return !(tcpStream is NetworkStream networkStream) || networkStream.DataAvailable;
    }

    public int ReadInt() => IPAddress.NetworkToHostOrder(new BinaryReader(tcpStream).ReadInt32());

    public byte[] ReadAtLeastNBytes(int msgSize)
    {
      var buf = new byte[msgSize];
      return buf.Take(tcpStream.Read(buf, 0, msgSize)).ToArray();
    }

    public byte[] ReadByteArray(int msgSize) => new BinaryReader(tcpStream).ReadBytes(msgSize);

    public bool AsyncEConnect { get; set; }
  }
}
