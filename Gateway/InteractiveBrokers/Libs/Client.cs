using Distribution.Models;
using Distribution.Services;
using IBApi;
using InteractiveBrokers.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace InteractiveBrokers
{
  public class IBClient : EWrapper, IDisposable
  {
    public ScheduleService Scheduler { get; protected set; }

    public Task<Contract> ResolveContractAsync(int conId, string refExch)
    {
      var reqId = new Random(DateTime.Now.Millisecond).Next();
      var resolveResult = new TaskCompletionSource<Contract>();
      var resolveContract_Error = new Action<int, int, string, string, Exception>((id, code, msg, advancedOrderRejectJson, ex) =>
      {
        if (reqId != id)
          return;

        resolveResult.SetResult(null);
      });
      var resolveContract = new Action<ContractDetailsMessage>(msg =>
      {
        if (msg.RequestId == reqId)
          resolveResult.SetResult(msg.ContractDetails.Contract);
      });
      var contractDetailsEnd = new Action<int>(id =>
      {
        if (reqId == id && !resolveResult.Task.IsCompleted)
          resolveResult.SetResult(null);
      });

      var cbError = Error;
      var cbContractDetails = ContractDetails;
      var cbContractDetailsEnd = ContractDetailsEnd;

      Error = resolveContract_Error;
      ContractDetails = resolveContract;
      ContractDetailsEnd = contractDetailsEnd;

      resolveResult.Task.ContinueWith(o =>
      {
        Error = cbError;
        ContractDetails = cbContractDetails;
        ContractDetailsEnd = cbContractDetailsEnd;
      });

      ClientSocket.reqContractDetails(reqId, new Contract
      {
        ConId = conId,
        Exchange = refExch
      });

      return resolveResult.Task;
    }

    public Task<Contract[]> ResolveContractAsync(string secType, string symbol, string currency, string exchange)
    {
      var reqId = new Random(DateTime.Now.Millisecond).Next();
      var res = new TaskCompletionSource<Contract[]>();
      var contractList = new List<Contract>();
      var resolveContract_Error = new Action<int, int, string, string, Exception>((id, code, msg, advancedOrderRejectJson, ex) =>
      {
        if (reqId != id)
          return;

        res.SetResult([]);
      });
      var contractDetails = new Action<ContractDetailsMessage>(msg =>
      {
        if (reqId != msg.RequestId)
          return;

        contractList.Add(msg.ContractDetails.Contract);
      });
      var contractDetailsEnd = new Action<int>(id =>
      {
        if (reqId == id)
          res.SetResult(contractList.ToArray());
      });

      var cbError = Error;
      var cbContractDetails = ContractDetails;
      var cbContractDetailsEnd = ContractDetailsEnd;

      Error = resolveContract_Error;
      ContractDetails = contractDetails;
      ContractDetailsEnd = contractDetailsEnd;

      res.Task.ContinueWith(o =>
      {
        Error = cbError;
        ContractDetails = cbContractDetails;
        ContractDetailsEnd = cbContractDetailsEnd;
      });

      ClientSocket.reqContractDetails(reqId, new Contract
      {
        SecType = secType,
        Symbol = symbol,
        Currency = currency,
        Exchange = exchange
      });

      return res.Task;
    }

    public int ClientId { get; set; }

    public IBClient(EReaderSignal signal)
    {
      ClientSocket = new EClientSocket(this, signal);
      Scheduler = new ScheduleService();
    }

    public EClientSocket ClientSocket { get; private set; }

    public int NextOrderId { get; set; }

    public event Action<int, int, string, string, Exception> Error;

    void EWrapper.error(Exception e)
    {
      var cb = Error;

      if (cb != null)
        Run(() => cb(0, 0, null, null, e), null);
    }

    void EWrapper.error(string str)
    {
      var cb = Error;

      if (cb != null)
        Run(() => cb(0, 0, str, null, null), null);
    }

    void EWrapper.error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson)
    {
      var cb = Error;

      if (cb != null)
        Run(() => cb(id, errorCode, errorMsg, advancedOrderRejectJson, null), null);
    }

    public event Action ConnectionClosed;

    void EWrapper.connectionClosed()
    {
      var cb = ConnectionClosed;

      if (cb != null)
        Run(() => cb(), null);
    }

    public event Action<long> CurrentTime;

    void EWrapper.currentTime(long time)
    {
      var cb = CurrentTime;

      if (cb != null)
        Run(() => cb(time), null);
    }

    public event Action<TickPriceMessage> TickPrice;

    void EWrapper.tickPrice(int tickerId, int field, double price, TickAttrib attribs)
    {
      var cb = TickPrice;

      if (cb != null)
        Run(() => cb(new TickPriceMessage(tickerId, field, price, attribs)), null);
    }

    //public event Action<TickSizeMessage> TickSize;

    void EWrapper.tickSize(int tickerId, int field, decimal size)
    {
      var cb = TickPrice;

      if (cb != null)
        Run(() => cb(new TickPriceMessage(tickerId, field, (double)size, null)), null);
    }

    public event Action<int, int, string> TickString;

    void EWrapper.tickString(int tickerId, int tickType, string value)
    {
      var cb = TickString;

      if (cb != null)
        Run(() => cb(tickerId, tickType, value), null);
    }

    public event Action<TickGenericMessage> TickGeneric;

    void EWrapper.tickGeneric(int tickerId, int field, double value)
    {
      var cb = TickGeneric;

      if (cb != null)
        Run(() => cb(new TickGenericMessage(tickerId, field, value)), null);
    }

    public event Action<int, int, double, string, double, int, string, double, double> TickEFP;

    void EWrapper.tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate)
    {
      var cb = TickEFP;

      if (cb != null)
        Run(() => cb(tickerId, tickType, basisPoints, formattedBasisPoints, impliedFuture, holdDays, futureLastTradeDate, dividendImpact, dividendsToLastTradeDate), null);
    }

    public event Action<int> TickSnapshotEnd;

    void EWrapper.tickSnapshotEnd(int tickerId)
    {
      var cb = TickSnapshotEnd;

      if (cb != null)
        Run(() => cb(tickerId), null);
    }

    public event Action<int> NextValidId;

    void EWrapper.nextValidId(int orderId)
    {
      var cb = NextValidId;

      if (cb != null)
        Run(() => cb(orderId), null);

      NextOrderId = orderId;
    }

    public event Action<int, DeltaNeutralContract> DeltaNeutralValidation;

    void EWrapper.deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract)
    {
      var cb = DeltaNeutralValidation;

      if (cb != null)
        Run(() => cb(reqId, deltaNeutralContract), null);
    }

    public event Action<ManagedAccountsMessage> ManagedAccounts;

    void EWrapper.managedAccounts(string accountsList)
    {
      var cb = ManagedAccounts;

      if (cb != null)
        Run(() => cb(new ManagedAccountsMessage(accountsList)), null);
    }

    public event Action<TickOptionMessage> TickOptionCommunication;

    void EWrapper.tickOptionComputation(int tickerId, int field, int tickAttrib, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
    {
      var cb = TickOptionCommunication;

      if (cb != null)
        Run(() => cb(new TickOptionMessage(tickerId, field, tickAttrib, impliedVolatility, delta, optPrice, pvDividend, gamma, vega, theta, undPrice)), null);
    }

    public event Action<AccountSummaryMessage> AccountSummary;

    void EWrapper.accountSummary(int reqId, string account, string tag, string value, string currency)
    {
      var cb = AccountSummary;

      if (cb != null)
        Run(() => cb(new AccountSummaryMessage(reqId, account, tag, value, currency)), null);
    }

    public event Action<AccountSummaryEndMessage> AccountSummaryEnd;

    void EWrapper.accountSummaryEnd(int reqId)
    {
      var cb = AccountSummaryEnd;

      if (cb != null)
        Run(() => cb(new AccountSummaryEndMessage(reqId)), null);
    }

    public event Action<AccountValueMessage> UpdateAccountValue;

    void EWrapper.updateAccountValue(string key, string value, string currency, string accountName)
    {
      var cb = UpdateAccountValue;

      if (cb != null)
        Run(() => cb(new AccountValueMessage(key, value, currency, accountName)), null);
    }

    public event Action<UpdatePortfolioMessage> UpdatePortfolio;

    void EWrapper.updatePortfolio(Contract contract, decimal position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName)
    {
      var cb = UpdatePortfolio;

      if (cb != null)
        Run(() => cb(new UpdatePortfolioMessage(contract, position, marketPrice, marketValue, averageCost, unrealizedPNL, realizedPNL, accountName)), null);
    }

    public event Action<UpdateAccountTimeMessage> UpdateAccountTime;

    void EWrapper.updateAccountTime(string timestamp)
    {
      var cb = UpdateAccountTime;

      if (cb != null)
        Run(() => cb(new UpdateAccountTimeMessage(timestamp)), null);
    }

    public event Action<AccountDownloadEndMessage> AccountDownloadEnd;

    void EWrapper.accountDownloadEnd(string account)
    {
      var cb = AccountDownloadEnd;

      if (cb != null)
        Run(() => cb(new AccountDownloadEndMessage(account)), null);
    }

    public event Action<OrderStatusMessage> OrderStatus;

    void EWrapper.orderStatus(int orderId, string status, decimal filled, decimal remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
    {
      var cb = OrderStatus;

      if (cb != null)
        Run(() => cb(new OrderStatusMessage(orderId, status, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld, mktCapPrice)), null);
    }

    public event Action<OpenOrderMessage> OpenOrder;

    void EWrapper.openOrder(int orderId, Contract contract, Order order, OrderState orderState)
    {
      var cb = OpenOrder;

      if (cb != null)
        Run(() => cb(new OpenOrderMessage(orderId, contract, order, orderState)), null);
    }

    public event Action OpenOrderEnd;

    void EWrapper.openOrderEnd()
    {
      var cb = OpenOrderEnd;

      if (cb != null)
        Run(() => cb(), null);
    }

    public event Action<ContractDetailsMessage> ContractDetails;

    void EWrapper.contractDetails(int reqId, ContractDetails contractDetails)
    {
      var cb = ContractDetails;

      if (cb != null)
        Run(() => cb(new ContractDetailsMessage(reqId, contractDetails)), null);
    }

    public event Action<int> ContractDetailsEnd;

    void EWrapper.contractDetailsEnd(int reqId)
    {
      var cb = ContractDetailsEnd;

      if (cb != null)
        Run(() => cb(reqId), null);
    }

    public event Action<ExecutionMessage> ExecDetails;

    void EWrapper.execDetails(int reqId, Contract contract, Execution execution)
    {
      var cb = ExecDetails;

      if (cb != null)
        Run(() => cb(new ExecutionMessage(reqId, contract, execution)), null);
    }

    public event Action<int> ExecDetailsEnd;

    void EWrapper.execDetailsEnd(int reqId)
    {
      var cb = ExecDetailsEnd;

      if (cb != null)
        Run(() => cb(reqId), null);
    }

    public event Action<CommissionReport> CommissionReport;

    void EWrapper.commissionReport(CommissionReport commissionReport)
    {
      var cb = CommissionReport;

      if (cb != null)
        Run(() => cb(commissionReport), null);
    }

    public event Action<FundamentalsMessage> FundamentalData;

    void EWrapper.fundamentalData(int reqId, string data)
    {
      var cb = FundamentalData;

      if (cb != null)
        Run(() => cb(new FundamentalsMessage(data)), null);
    }

    public event Action<HistoricalDataMessage> HistoricalData;

    void EWrapper.historicalData(int reqId, Bar bar)
    {
      var cb = HistoricalData;

      if (cb != null)
        Run(() => cb(new HistoricalDataMessage(reqId, bar)), null);
    }

    public event Action<HistoricalDataEndMessage> HistoricalDataEnd;

    void EWrapper.historicalDataEnd(int reqId, string startDate, string endDate)
    {
      var cb = HistoricalDataEnd;

      if (cb != null)
        Run(() => cb(new HistoricalDataEndMessage(reqId, startDate, endDate)), null);
    }

    public event Action<MarketDataTypeMessage> MarketDataType;

    void EWrapper.marketDataType(int reqId, int marketDataType)
    {
      var cb = MarketDataType;

      if (cb != null)
        Run(() => cb(new MarketDataTypeMessage(reqId, marketDataType)), null);
    }

    public event Action<DeepBookMessage> UpdateMktDepth;

    void EWrapper.updateMktDepth(int tickerId, int position, int operation, int side, double price, decimal size)
    {
      var cb = UpdateMktDepth;

      if (cb != null)
        Run(() => cb(new DeepBookMessage(tickerId, position, operation, side, price, size, "", false)), null);
    }

    public event Action<DeepBookMessage> UpdateMktDepthL2;

    void EWrapper.updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, decimal size, bool isSmartDepth)
    {
      var cb = UpdateMktDepthL2;

      if (cb != null)
        Run(() => cb(new DeepBookMessage(tickerId, position, operation, side, price, size, marketMaker, isSmartDepth)), null);
    }

    public event Action<int, int, string, string> UpdateNewsBulletin;

    void EWrapper.updateNewsBulletin(int msgId, int msgType, string message, string origExchange)
    {
      var cb = UpdateNewsBulletin;

      if (cb != null)
        Run(() => cb(msgId, msgType, message, origExchange), null);
    }

    public event Action<PositionMessage> Position;

    void EWrapper.position(string account, Contract contract, decimal pos, double avgCost)
    {
      var cb = Position;

      if (cb != null)
        Run(() => cb(new PositionMessage(account, contract, pos, avgCost)), null);
    }

    public event Action PositionEnd;

    void EWrapper.positionEnd()
    {
      var cb = PositionEnd;

      if (cb != null)
        Run(() => cb(), null);
    }

    public event Action<RealTimeBarMessage> RealtimeBar;

    void EWrapper.realtimeBar(int reqId, long time, double open, double high, double low, double close, decimal volume, decimal WAP, int count)
    {
      var cb = RealtimeBar;

      if (cb != null)
        Run(() => cb(new RealTimeBarMessage(reqId, time, open, high, low, close, volume, WAP, count)), null);
    }

    public event Action<string> ScannerParameters;

    void EWrapper.scannerParameters(string xml)
    {
      var cb = ScannerParameters;

      if (cb != null)
        Run(() => cb(xml), null);
    }

    public event Action<ScannerMessage> ScannerData;

    void EWrapper.scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
    {
      var cb = ScannerData;

      if (cb != null)
        Run(() => cb(new ScannerMessage(reqId, rank, contractDetails, distance, benchmark, projection, legsStr)), null);
    }

    public event Action<int> ScannerDataEnd;

    void EWrapper.scannerDataEnd(int reqId)
    {
      var cb = ScannerDataEnd;

      if (cb != null)
        Run(() => cb(reqId), null);
    }

    public event Action<AdvisorDataMessage> ReceiveFA;

    void EWrapper.receiveFA(int faDataType, string faXmlData)
    {
      var cb = ReceiveFA;

      if (cb != null)
        Run(() => cb(new AdvisorDataMessage(faDataType, faXmlData)), null);
    }

    public event Action<BondContractDetailsMessage> BondContractDetails;

    void EWrapper.bondContractDetails(int requestId, ContractDetails contractDetails)
    {
      var cb = BondContractDetails;

      if (cb != null)
        Run(() => cb(new BondContractDetailsMessage(requestId, contractDetails)), null);
    }

    public event Action<string> VerifyMessageAPI;

    void EWrapper.verifyMessageAPI(string apiData)
    {
      var cb = VerifyMessageAPI;

      if (cb != null)
        Run(() => cb(apiData), null);
    }
    public event Action<bool, string> VerifyCompleted;

    void EWrapper.verifyCompleted(bool isSuccessful, string errorText)
    {
      var cb = VerifyCompleted;

      if (cb != null)
        Run(() => cb(isSuccessful, errorText), null);
    }

    public event Action<string, string> VerifyAndAuthMessageAPI;

    void EWrapper.verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
    {
      var cb = VerifyAndAuthMessageAPI;

      if (cb != null)
        Run(() => cb(apiData, xyzChallenge), null);
    }

    public event Action<bool, string> VerifyAndAuthCompleted;

    void EWrapper.verifyAndAuthCompleted(bool isSuccessful, string errorText)
    {
      var cb = VerifyAndAuthCompleted;

      if (cb != null)
        Run(() => cb(isSuccessful, errorText), null);
    }

    public event Action<int, string> DisplayGroupList;

    void EWrapper.displayGroupList(int reqId, string groups)
    {
      var cb = DisplayGroupList;

      if (cb != null)
        Run(() => cb(reqId, groups), null);
    }

    public event Action<int, string> DisplayGroupUpdated;

    void EWrapper.displayGroupUpdated(int reqId, string contractInfo)
    {
      var cb = DisplayGroupUpdated;

      if (cb != null)
        Run(() => cb(reqId, contractInfo), null);
    }


    void EWrapper.connectAck()
    {
      if (ClientSocket.AsyncEConnect)
        ClientSocket.startApi();
    }

    public event Action<PositionMultiMessage> PositionMulti;

    void EWrapper.positionMulti(int reqId, string account, string modelCode, Contract contract, decimal pos, double avgCost)
    {
      var cb = PositionMulti;

      if (cb != null)
        Run(() => cb(new PositionMultiMessage(reqId, account, modelCode, contract, pos, avgCost)), null);
    }

    public event Action<int> PositionMultiEnd;

    void EWrapper.positionMultiEnd(int reqId)
    {
      var cb = PositionMultiEnd;

      if (cb != null)
        Run(() => cb(reqId), null);
    }

    public event Action<AccountUpdateMultiMessage> AccountUpdateMulti;

    void EWrapper.accountUpdateMulti(int reqId, string account, string modelCode, string key, string value, string currency)
    {
      var cb = AccountUpdateMulti;

      if (cb != null)
        Run(() => cb(new AccountUpdateMultiMessage(reqId, account, modelCode, key, value, currency)), null);
    }

    public event Action<int> AccountUpdateMultiEnd;

    void EWrapper.accountUpdateMultiEnd(int reqId)
    {
      var cb = AccountUpdateMultiEnd;

      if (cb != null)
        Run(() => cb(reqId), null);
    }

    public event Action<SecurityDefinitionOptionParameterMessage> SecurityDefinitionOptionParameter;

    void EWrapper.securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
    {
      var cb = SecurityDefinitionOptionParameter;

      if (cb != null)
        Run(() => cb(new SecurityDefinitionOptionParameterMessage(reqId, exchange, underlyingConId, tradingClass, multiplier, expirations, strikes)), null);
    }

    public event Action<int> SecurityDefinitionOptionParameterEnd;

    void EWrapper.securityDefinitionOptionParameterEnd(int reqId)
    {
      var cb = SecurityDefinitionOptionParameterEnd;

      if (cb != null)
        Run(() => cb(reqId), null);
    }

    public event Action<SoftDollarTiersMessage> SoftDollarTiers;

    void EWrapper.softDollarTiers(int reqId, SoftDollarTier[] tiers)
    {
      var cb = SoftDollarTiers;

      if (cb != null)
        Run(() => cb(new SoftDollarTiersMessage(reqId, tiers)), null);
    }

    public event Action<FamilyCode[]> FamilyCodes;

    void EWrapper.familyCodes(FamilyCode[] familyCodes)
    {
      var cb = FamilyCodes;

      if (cb != null)
        Run(() => cb(familyCodes), null);
    }

    public event Action<SymbolSamplesMessage> SymbolSamples;

    void EWrapper.symbolSamples(int reqId, ContractDescription[] contractDescriptions)
    {
      var cb = SymbolSamples;

      if (cb != null)
        Run(() => cb(new SymbolSamplesMessage(reqId, contractDescriptions)), null);
    }


    public event Action<DepthMktDataDescription[]> MktDepthExchanges;

    void EWrapper.mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions)
    {
      var cb = MktDepthExchanges;

      if (cb != null)
        Run(() => cb(depthMktDataDescriptions), null);
    }

    public event Action<TickNewsMessage> TickNews;

    void EWrapper.tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData)
    {
      var cb = TickNews;

      if (cb != null)
        Run(() => cb(new TickNewsMessage(tickerId, timeStamp, providerCode, articleId, headline, extraData)), null);
    }

    public event Action<int, Dictionary<int, KeyValuePair<string, char>>> SmartComponents;

    void EWrapper.smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap)
    {
      var cb = SmartComponents;

      if (cb != null)
        Run(() => cb(reqId, theMap), null);
    }

    public event Action<TickReqParamsMessage> TickReqParams;

    void EWrapper.tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions)
    {
      var cb = TickReqParams;

      if (cb != null)
        Run(() => cb(new TickReqParamsMessage(tickerId, minTick, bboExchange, snapshotPermissions)), null);
    }

    public event Action<NewsProvider[]> NewsProviders;

    void EWrapper.newsProviders(NewsProvider[] newsProviders)
    {
      var cb = NewsProviders;

      if (cb != null)
        Run(() => cb(newsProviders), null);
    }

    public event Action<NewsArticleMessage> NewsArticle;

    void EWrapper.newsArticle(int requestId, int articleType, string articleText)
    {
      var cb = NewsArticle;

      if (cb != null)
        Run(() => cb(new NewsArticleMessage(requestId, articleType, articleText)), null);
    }

    public event Action<HistoricalNewsMessage> HistoricalNews;

    void EWrapper.historicalNews(int requestId, string time, string providerCode, string articleId, string headline)
    {
      var cb = HistoricalNews;

      if (cb != null)
        Run(() => cb(new HistoricalNewsMessage(requestId, time, providerCode, articleId, headline)), null);
    }

    public event Action<HistoricalNewsEndMessage> HistoricalNewsEnd;

    void EWrapper.historicalNewsEnd(int requestId, bool hasMore)
    {
      var cb = HistoricalNewsEnd;

      if (cb != null)
        Run(() => cb(new HistoricalNewsEndMessage(requestId, hasMore)), null);
    }

    public event Action<HeadTimestampMessage> HeadTimestamp;

    void EWrapper.headTimestamp(int reqId, string headTimestamp)
    {
      var cb = HeadTimestamp;

      if (cb != null)
        Run(() => cb(new HeadTimestampMessage(reqId, headTimestamp)), null);
    }

    public event Action<HistogramDataMessage> HistogramData;

    void EWrapper.histogramData(int reqId, HistogramEntry[] data)
    {
      var cb = HistogramData;

      if (cb != null)
        Run(() => cb(new HistogramDataMessage(reqId, data)), null);
    }

    public event Action<HistoricalDataMessage> HistoricalDataUpdate;

    void EWrapper.historicalDataUpdate(int reqId, Bar bar)
    {
      var cb = HistoricalDataUpdate;

      if (cb != null)
        Run(() => cb(new HistoricalDataMessage(reqId, bar)), null);
    }

    public event Action<int, int, string> RerouteMktDataReq;

    void EWrapper.rerouteMktDataReq(int reqId, int conId, string exchange)
    {
      var cb = RerouteMktDataReq;

      if (cb != null)
        Run(() => cb(reqId, conId, exchange), null);
    }

    public event Action<int, int, string> RerouteMktDepthReq;

    void EWrapper.rerouteMktDepthReq(int reqId, int conId, string exchange)
    {
      var cb = RerouteMktDepthReq;

      if (cb != null)
        Run(() => cb(reqId, conId, exchange), null);
    }

    public event Action<MarketRuleMessage> MarketRule;

    void EWrapper.marketRule(int marketRuleId, PriceIncrement[] priceIncrements)
    {
      var cb = MarketRule;

      if (cb != null)
        Run(() => cb(new MarketRuleMessage(marketRuleId, priceIncrements)), null);
    }

    public event Action<PnLMessage> Pnl;

    void EWrapper.pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL)
    {
      var cb = Pnl;

      if (cb != null)
        Run(() => cb(new PnLMessage(reqId, dailyPnL, unrealizedPnL, realizedPnL)), null);
    }

    public event Action<PnLSingleMessage> PnlSingle;

    void EWrapper.pnlSingle(int reqId, decimal pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value)
    {
      var cb = PnlSingle;

      if (cb != null)
        Run(() => cb(new PnLSingleMessage(reqId, pos, dailyPnL, unrealizedPnL, realizedPnL, value)), null);
    }

    public event Action<HistoricalTickMessage> historicalTick;

    void EWrapper.historicalTicks(int reqId, HistoricalTick[] ticks, bool done)
    {
      var cb = historicalTick;

      if (cb != null)
        ticks.ToList().ForEach(tick => Run(() => cb(new HistoricalTickMessage(reqId, tick.Time, tick.Price, tick.Size)), null));
    }

    public event Action<HistoricalTicksMessage> historicalTicksList;
    public event Action<HistoricalTickBidAskMessage> historicalTickBidAsk;
    public class HistoricalTicksMessage
    {
      public int ReqId { get; set; }
      public HistoricalTickBidAsk[] Items { get; set; } = [];
    }

    void EWrapper.historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done)
    {
      var cb = historicalTickBidAsk;
      var cbs = historicalTicksList;

      if (cbs != null)
        Run(() => cbs(new HistoricalTicksMessage { ReqId = reqId, Items = ticks }), null);

      //if (cb != null)
      //  ticks.ToList().ForEach(tick => Run(() =>
      //      cb(new HistoricalTickBidAskMessage(reqId, tick.Time, tick.TickAttribBidAsk, tick.PriceBid, tick.PriceAsk, tick.SizeBid, tick.SizeAsk)), null));
    }

    public event Action<HistoricalTickLastMessage> historicalTickLast;

    void EWrapper.historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done)
    {
      var cb = historicalTickLast;

      if (cb != null)
        ticks.ToList().ForEach(tick => Run(() =>
            cb(new HistoricalTickLastMessage(reqId, tick.Time, tick.TickAttribLast, tick.Price, tick.Size, tick.Exchange, tick.SpecialConditions)), null));
    }

    public event Action<TickByTickAllLastMessage> tickByTickAllLast;

    void EWrapper.tickByTickAllLast(int reqId, int tickType, long time, double price, decimal size, TickAttribLast tickAttribLast, string exchange, string specialConditions)
    {
      var cb = tickByTickAllLast;

      if (cb != null)
        Run(() => cb(new TickByTickAllLastMessage(reqId, tickType, time, price, size, tickAttribLast, exchange, specialConditions)), null);
    }

    public event Action<TickByTickBidAskMessage> tickByTickBidAsk;

    void EWrapper.tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, decimal bidSize, decimal askSize, TickAttribBidAsk tickAttribBidAsk)
    {
      var cb = tickByTickBidAsk;

      if (cb != null)
        Run(() => cb(new TickByTickBidAskMessage(reqId, time, bidPrice, askPrice, bidSize, askSize, tickAttribBidAsk)), null);
    }

    public event Action<TickByTickMidPointMessage> tickByTickMidPoint;

    void EWrapper.tickByTickMidPoint(int reqId, long time, double midPoint)
    {
      var cb = tickByTickMidPoint;

      if (cb != null)
        Run(() => cb(new TickByTickMidPointMessage(reqId, time, midPoint)), null);
    }

    public event Action<OrderBoundMessage> OrderBound;

    void EWrapper.orderBound(long orderId, int apiClientId, int apiOrderId)
    {
      var cb = OrderBound;

      if (cb != null)
        Run(() => cb(new OrderBoundMessage(orderId, apiClientId, apiOrderId)), null);
    }

    public event Action<CompletedOrderMessage> CompletedOrder;

    void EWrapper.completedOrder(Contract contract, Order order, OrderState orderState)
    {
      var cb = CompletedOrder;

      if (cb != null)
        Run(() => cb(new CompletedOrderMessage(contract, order, orderState)), null);
    }

    public event Action CompletedOrdersEnd;

    void EWrapper.completedOrdersEnd()
    {
      var cb = CompletedOrdersEnd;

      if (cb != null)
        Run(() => cb(), null);
    }

    public event Action<int, string> ReplaceFAEnd;

    void EWrapper.replaceFAEnd(int reqId, string text)
    {
      var cb = ReplaceFAEnd;

      if (cb != null)
        Run(() => cb(reqId, text), null);
    }

    public event Action<int, string> WshMetaData;

    public void wshMetaData(int reqId, string dataJson)
    {
      var cb = WshMetaData;

      if (cb != null)
        Run(() => cb(reqId, dataJson), null);
    }

    public event Action<int, string> WshEventData;

    public void wshEventData(int reqId, string dataJson)
    {
      var cb = WshEventData;

      if (cb != null)
        Run(() => cb(reqId, dataJson), null);
    }

    public event Action<HistoricalScheduleMessage> HistoricalSchedule;

    public void historicalSchedule(int reqId, string startDateTime, string endDateTime, string timeZone, HistoricalSession[] sessions)
    {
      var cb = HistoricalSchedule;

      if (cb != null)
        Run(() => cb(new HistoricalScheduleMessage(reqId, startDateTime, endDateTime, timeZone, sessions)), null);
    }

    public event Action<string> UserInfo;

    void EWrapper.userInfo(int reqId, string whiteBrandingId)
    {
      var cb = UserInfo;
      if (cb != null)
        Run(() => cb(whiteBrandingId), null);
    }

    protected void Run(Action cb, object state) => Scheduler.Send(cb, new OptionModel { IsRemovable = false });
    
    public void Dispose() => Scheduler.Dispose();
  }
}
