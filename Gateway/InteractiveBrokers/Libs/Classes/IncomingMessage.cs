/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    public class IncomingMessage
    {
        public const int NotValid = -1;
        public const int TickPrice = 1;
        public const int TickSize = 2;
        public const int OrderStatus = 3;
        public const int Error = 4;
        public const int OpenOrder = 5;
        public const int AccountValue = 6;
        public const int PortfolioValue = 7;
        public const int AccountUpdateTime = 8;
        public const int NextValidId = 9;
        public const int ContractData = 10;
        public const int ExecutionData = 11;
        public const int MarketDepth = 12;
        public const int MarketDepthL2 = 13;
        public const int NewsBulletins = 14;
        public const int ManagedAccounts = 15;
        public const int ReceiveFA = 16;
        public const int HistoricalData = 17;
        public const int BondContractData = 18;
        public const int ScannerParameters = 19;
        public const int ScannerData = 20;
        public const int TickOptionComputation = 21;
        public const int TickGeneric = 45;
        public const int Tickstring = 46;
        public const int TickEFP = 47; //TICK EFP 47
        public const int CurrentTime = 49;
        public const int RealTimeBars = 50;
        public const int FundamentalData = 51;
        public const int ContractDataEnd = 52;
        public const int OpenOrderEnd = 53;
        public const int AccountDownloadEnd = 54;
        public const int ExecutionDataEnd = 55;
        public const int DeltaNeutralValidation = 56;
        public const int TickSnapshotEnd = 57;
        public const int MarketDataType = 58;
        public const int CommissionsReport = 59;
        public const int Position = 61;
        public const int PositionEnd = 62;
        public const int AccountSummary = 63;
        public const int AccountSummaryEnd = 64;
        public const int VerifyMessageApi = 65;
        public const int VerifyCompleted = 66;
        public const int DisplayGroupList = 67;
        public const int DisplayGroupUpdated = 68;
        public const int VerifyAndAuthMessageApi = 69;
        public const int VerifyAndAuthCompleted = 70;
        public const int PositionMulti = 71;
        public const int PositionMultiEnd = 72;
        public const int AccountUpdateMulti = 73;
        public const int AccountUpdateMultiEnd = 74;
        public const int SecurityDefinitionOptionParameter = 75;
        public const int SecurityDefinitionOptionParameterEnd = 76;
        public const int SoftDollarTier = 77;
        public const int FamilyCodes = 78;
        public const int SymbolSamples = 79;
        public const int MktDepthExchanges = 80;
        public const int TickReqParams = 81;
        public const int SmartComponents = 82;
        public const int NewsArticle = 83;
        public const int TickNews = 84;
        public const int NewsProviders = 85;
        public const int HistoricalNews = 86;
        public const int HistoricalNewsEnd = 87;
        public const int HeadTimestamp = 88;
        public const int HistogramData = 89;
        public const int HistoricalDataUpdate = 90;
        public const int RerouteMktDataReq = 91;
        public const int RerouteMktDepthReq = 92;
        public const int MarketRule = 93;
        public const int PnL = 94;
        public const int PnLSingle = 95;
        public const int HistoricalTick = 96;
        public const int HistoricalTickBidAsk = 97;
        public const int HistoricalTickLast = 98;
        public const int TickByTick = 99;
        public const int OrderBound = 100;
        public const int CompletedOrder = 101;
        public const int CompletedOrdersEnd = 102;
        public const int ReplaceFAEnd = 103;
        public const int WshMetaData = 104;
        public const int WshEventData = 105;
        public const int HistoricalSchedule = 106;
        public const int UserInfo = 107;
    }
}
