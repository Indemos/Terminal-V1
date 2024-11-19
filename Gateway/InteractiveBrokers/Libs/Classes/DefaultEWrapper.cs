/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;

namespace IBApi
{
    public class DefaultEWrapper : EWrapper
    {
        //
        // Note to updaters:
        //
        //
        // Please ensure that implementations of new EWrapper methods are declared
        // as virtual, since the only purpose for this class to be public is so that
        // API clients that only wish to consume a subset of the EWrapper interface
        // can create a class that inherits from it and then override just the methods
        // needed (ie Adapter pattern), rather than implementing EWrapper directly.
        //

        public virtual void error(Exception e) { }

        public virtual void error(string str) { }

        public virtual void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson) { }

        public virtual void currentTime(long time) { }

        public virtual void tickPrice(int tickerId, int field, double price, TickAttrib attribs) { }

        public virtual void tickSize(int tickerId, int field, decimal size) { }

        public virtual void tickString(int tickerId, int field, string value) { }

        public virtual void tickGeneric(int tickerId, int field, double value) { }

        public virtual void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate) { }

        public virtual void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract) { }

        public virtual void tickOptionComputation(int tickerId, int field, int tickAttrib, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice) { }

        public virtual void tickSnapshotEnd(int tickerId) { }

        public virtual void nextValidId(int orderId) { }

        public virtual void managedAccounts(string accountsList) { }

        public virtual void connectionClosed() { }

        public virtual void accountSummary(int reqId, string account, string tag, string value, string currency) { }

        public virtual void accountSummaryEnd(int reqId) { }

        public virtual void bondContractDetails(int reqId, ContractDetails contract) { }

        public virtual void updateAccountValue(string key, string value, string currency, string accountName) { }

        public virtual void updatePortfolio(Contract contract, decimal position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName) { }

        public virtual void updateAccountTime(string timestamp) { }

        public virtual void accountDownloadEnd(string account) { }

        public virtual void orderStatus(int orderId, string status, decimal filled, decimal remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice) { }

        public virtual void openOrder(int orderId, Contract contract, Order order, OrderState orderState) { }

        public virtual void openOrderEnd() { }

        public virtual void contractDetails(int reqId, ContractDetails contractDetails) { }

        public virtual void contractDetailsEnd(int reqId) { }

        public virtual void execDetails(int reqId, Contract contract, Execution execution) { }

        public virtual void execDetailsEnd(int reqId) { }

        public virtual void commissionReport(CommissionReport commissionReport) { }

        public virtual void fundamentalData(int reqId, string data) { }

        public virtual void historicalData(int reqId, Bar bar) { }

        public virtual void historicalDataEnd(int reqId, string start, string end) { }

        public virtual void marketDataType(int reqId, int marketDataType) { }

        public virtual void updateMktDepth(int tickerId, int position, int operation, int side, double price, decimal size) { }

        public virtual void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, decimal size, bool isSmartDepth) { }

        public virtual void updateNewsBulletin(int msgId, int msgType, string message, string origExchange) { }

        public virtual void position(string account, Contract contract, decimal pos, double avgCost) { }

        public virtual void positionEnd() { }

        public virtual void realtimeBar(int reqId, long time, double open, double high, double low, double close, decimal volume, decimal WAP, int count) { }

        public virtual void scannerParameters(string xml) { }

        public virtual void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr) { }

        public virtual void scannerDataEnd(int reqId) { }

        public virtual void receiveFA(int faDataType, string faXmlData) { }

        public virtual void verifyMessageAPI(string apiData) { }

        public virtual void verifyCompleted(bool isSuccessful, string errorText) { }

        public virtual void verifyAndAuthMessageAPI(string apiData, string xyzChallenge) { }

        public virtual void verifyAndAuthCompleted(bool isSuccessful, string errorText) { }

        public virtual void displayGroupList(int reqId, string groups) { }

        public virtual void displayGroupUpdated(int reqId, string contractInfo) { }

        public virtual void connectAck() { }

        public virtual void positionMulti(int requestId, string account, string modelCode, Contract contract, decimal pos, double avgCost) { }

        public virtual void positionMultiEnd(int requestId) { }

        public virtual void accountUpdateMulti(int requestId, string account, string modelCode, string key, string value, string currency) { }

        public virtual void accountUpdateMultiEnd(int requestId) { }


        public virtual void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes) { }

        public virtual void securityDefinitionOptionParameterEnd(int reqId) { }

        public virtual void softDollarTiers(int reqId, SoftDollarTier[] tiers) { }

        public virtual void familyCodes(FamilyCode[] familyCodes) { }

        public virtual void symbolSamples(int reqId, ContractDescription[] contractDescriptions) { }

        public virtual void mktDepthExchanges(DepthMktDataDescription[] descriptions) { }

        public virtual void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData) { }

        public virtual void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap) { }

        public virtual void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions) { }

        public virtual void newsProviders(NewsProvider[] newsProviders) { }

        public virtual void newsArticle(int requestId, int articleType, string articleText) { }

        public virtual void historicalNews(int requestId, string time, string providerCode, string articleId, string headline) { }

        public virtual void historicalNewsEnd(int requestId, bool hasMore) { }

        public virtual void headTimestamp(int reqId, string headTimestamp) { }


        public virtual void histogramData(int reqId, HistogramEntry[] data) { }

        public virtual void historicalDataUpdate(int reqId, Bar bar) { }

        public virtual void rerouteMktDataReq(int reqId, int conId, string exchange) { }

        public virtual void rerouteMktDepthReq(int reqId, int conId, string exchange) { }

        public virtual void marketRule(int marketRuleId, PriceIncrement[] priceIncrements) { }


        public virtual void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL) { }

        public virtual void pnlSingle(int reqId, decimal pos, double dailyPnL, double realizedPnL, double value, double unrealizedPnL) { }

        public virtual void historicalTicks(int reqId, HistoricalTick[] ticks, bool done) { }

        public virtual void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done) { }

        public virtual void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done) { }

        public virtual void tickByTickAllLast(int reqId, int tickType, long time, double price, decimal size, TickAttribLast tickAttribLast, string exchange, string specialConditions) { }

        public virtual void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, decimal bidSize, decimal askSize, TickAttribBidAsk tickAttribBidAsk) { }

        public virtual void tickByTickMidPoint(int reqId, long time, double midPoint) { }

        public virtual void orderBound(long orderId, int apiClientId, int apiOrderId) { }

        public virtual void completedOrder(Contract contract, Order order, OrderState orderState) { }

        public virtual void completedOrdersEnd() { }

        public virtual void replaceFAEnd(int reqId, string text) { }

        public virtual void wshMetaData(int reqId, string dataJson) { }

        public virtual void wshEventData(int reqId, string dataJson) { }

        public void historicalSchedule(int reqId, string startDateTime, string endDateTime, string timeZone, HistoricalSession[] sessions) { }

        public void userInfo(int reqId, string whiteBrandingId) { }
    }
}
