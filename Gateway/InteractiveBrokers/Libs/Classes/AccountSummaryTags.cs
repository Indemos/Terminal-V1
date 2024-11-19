/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class AccountSummaryTags
     * @brief class containing all existing values being reported by EClientSocket::reqAccountSummary
     */
    public class AccountSummaryTags
    {
        public const string AccountType = "AccountType";
        public const string NetLiquidation = "NetLiquidation";
        public const string TotalCashValue = "TotalCashValue";
        public const string SettledCash = "SettledCash";
        public const string AccruedCash = "AccruedCash";
        public const string BuyingPower = "BuyingPower";
        public const string EquityWithLoanValue = "EquityWithLoanValue";
        public const string PreviousDayEquityWithLoanValue = "PreviousDayEquityWithLoanValue";
        public const string GrossPositionValue = "GrossPositionValue";
        public const string ReqTEquity = "ReqTEquity";
        public const string ReqTMargin = "ReqTMargin";
        public const string SMA = "SMA";
        public const string InitMarginReq = "InitMarginReq";
        public const string MaintMarginReq = "MaintMarginReq";
        public const string AvailableFunds = "AvailableFunds";
        public const string ExcessLiquidity = "ExcessLiquidity";
        public const string Cushion = "Cushion";
        public const string FullInitMarginReq = "FullInitMarginReq";
        public const string FullMaintMarginReq = "FullMaintMarginReq";
        public const string FullAvailableFunds = "FullAvailableFunds";
        public const string FullExcessLiquidity = "FullExcessLiquidity";
        public const string LookAheadNextChange = "LookAheadNextChange";
        public const string LookAheadInitMarginReq = "LookAheadInitMarginReq";
        public const string LookAheadMaintMarginReq = "LookAheadMaintMarginReq";
        public const string LookAheadAvailableFunds = "LookAheadAvailableFunds";
        public const string LookAheadExcessLiquidity = "LookAheadExcessLiquidity";
        public const string HighestSeverity = "HighestSeverity";
        public const string DayTradesRemaining = "DayTradesRemaining";
        public const string Leverage = "Leverage";

        public static string GetAllTags() => $"{AccountType},{NetLiquidation},{TotalCashValue},{SettledCash},{AccruedCash},{BuyingPower},{EquityWithLoanValue},{PreviousDayEquityWithLoanValue},{GrossPositionValue},{ReqTEquity},{ReqTMargin},{SMA},{InitMarginReq},{MaintMarginReq},{AvailableFunds},{ExcessLiquidity},{Cushion},{FullInitMarginReq},{FullMaintMarginReq},{FullAvailableFunds},{FullExcessLiquidity},{LookAheadNextChange},{LookAheadInitMarginReq},{LookAheadMaintMarginReq},{LookAheadAvailableFunds},{LookAheadExcessLiquidity},{HighestSeverity},{DayTradesRemaining},{Leverage}";
    }
}
