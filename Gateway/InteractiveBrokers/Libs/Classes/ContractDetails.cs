/* Copyright (C) 2024 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;
using System.Collections.Generic;

namespace IBApi
{
    /**
     * @class ContractDetails
     * @brief extended contract details.
     * @sa Contract
     */
    public class ContractDetails
    {
        // BOND values

        /**
         * @brief A fully-defined Contract object.
         */
        public Contract Contract
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The market name for this product.
         */
        public string MarketName
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The minimum allowed price variation.
         * Note that many securities vary their minimum tick size according to their price. This value will only show the smallest of the different minimum tick sizes regardless of the product's price. Full information about the minimum increment price structure can be obtained with the reqMarketRule function or the IB Contract and Security Search site.
         */
        public double MinTick
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Allows execution and strike prices to be reported consistently with market data, historical data and the order price, i.e. Z on LIFFE is reported in Index points and not GBP.
         * In TWS versions prior to 972, the price magnifier is used in defining future option strike prices (e.g. in the API the strike is specified in dollars, but in TWS it is specified in cents).
         * In TWS versions 972 and higher, the price magnifier is not used in defining futures option strike prices so they are consistent in TWS and the API.
         */
        public int PriceMagnifier
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Supported order types for this product.
         */
        public string OrderTypes
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Valid exchange fields when placing an order for this contract.\n
         * The list of exchanges will is provided in the same order as the corresponding MarketRuleIds list.
         */
        public string ValidExchanges
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief For derivatives, the contract ID (conID) of the underlying instrument
         */
        public int UnderConId
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Descriptive name of the product.
         */
        public string LongName
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Typically the contract month of the underlying for a Future contract.
         */
        public string ContractMonth
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The industry classification of the underlying/product. For example, Financial.
         */
        public string Industry
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The industry category of the underlying. For example, InvestmentSvc.
         */
        public string Category
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The industry subcategory of the underlying. For example, Brokerage.
         */
        public string Subcategory
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The time zone for the trading hours of the product. For example, EST.
         */
        public string TimeZoneId
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The trading hours of the product.
         * This value will contain the trading hours of the current day as well as the next's. For example, 20090507:0700-1830,1830-2330;20090508:CLOSED.
         * In TWS versions 965+ there is an option in the Global Configuration API settings to return 1 month of trading hours.
         * In TWS version 970+, the format includes the date of the closing time to clarify potential ambiguity, ex: 20180323:0400-20180323:2000;20180326:0400-20180326:2000
         * The trading hours will correspond to the hours for the product on the associated exchange. The same instrument can have different hours on different exchanges.
         */
        public string TradingHours
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The liquid hours of the product.
         * This value will contain the liquid hours (regular trading hours) of the contract on the specified exchange. Format for TWS versions until 969: 20090507:0700-1830,1830-2330;20090508:CLOSED.
         * In TWS versions 965+ there is an option in the Global Configuration API settings to return 1 month of trading hours.
         * In TWS v970 and above, the format includes the date of the closing time to clarify potential ambiguity, e.g. 20180323:0930-20180323:1600;20180326:0930-20180326:1600
         */
        public string LiquidHours
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Contains the Economic Value Rule name and the respective optional argument.
         * The two values should be separated by a colon. For example, aussieBond:YearsToExpiration=3. When the optional argument is not present, the first value will be followed by a colon.
         */
        public string EvRule
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Tells you approximately how much the market value of a contract would change if the price were to change by 1.
         * It cannot be used to get market value by multiplying the price by the approximate multiplier.
         */
        public double EvMultiplier
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Aggregated group
         * Indicates the smart-routing group to which a contract belongs.
         * contracts which cannot be smart-routed have aggGroup = -1
         */
        public int AggGroup
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief A list of contract identifiers that the customer is allowed to view.
         * CUSIP/ISIN/etc. For US stocks, receiving the ISIN requires the CUSIP market data subscription.
         * For Bonds, the CUSIP or ISIN is input directly into the symbol field of the Contract class.
         */
        public List<TagValue> SecIdList
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief For derivatives, the symbol of the underlying contract.
         */
        public string UnderSymbol
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief For derivatives, returns the underlying security type.
         */
        public string UnderSecType
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The list of market rule IDs separated by comma
         * Market rule IDs can be used to determine the minimum price increment at a given price.
         */
        public string MarketRuleIds
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Real expiration date. Requires TWS 968+ and API v973.04+. Python API specifically requires API v973.06+.
         */
        public string RealExpirationDate
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Last trade time
         */
        public string LastTradeTime
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Stock type
         */
        public string StockType
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The nine-character bond CUSIP.
         * For Bonds only. Receiving CUSIPs requires a CUSIP market data subscription.
         */
        public string Cusip
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Identifies the credit rating of the issuer.
         * This field is not currently available from the TWS API.
         * For Bonds only. A higher credit rating generally indicates a less risky investment. Bond ratings are from Moody's and S&P respectively. Not currently implemented due to bond market data restrictions.
         */
        public string Ratings
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief A description string containing further descriptive information about the bond.
         * For Bonds only.
         */
        public string DescAppend
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The type of bond, such as "CORP."
         */
        public string BondType
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The type of bond coupon.
         * This field is currently not available from the TWS API.
         * For Bonds only.
         */
        public string CouponType
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief If true, the bond can be called by the issuer under certain conditions.
         * This field is currently not available from the TWS API.
         * For Bonds only.
         */
        public bool Callable
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Values are True or False. If true, the bond can be sold back to the issuer under certain conditions.
         * This field is currently not available from the TWS API.
         * For Bonds only.
         */
        public bool Putable
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The interest rate used to calculate the amount you will receive in interest payments over the course of the year.
         * This field is currently not available from the TWS API.
         * For Bonds only.
         */
        public double Coupon
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Values are True or False. If true, the bond can be converted to stock under certain conditions.
         * This field is currently not available from the TWS API.
         * For Bonds only.
         */
        public bool Convertible
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief he date on which the issuer must repay the face value of the bond.
         * This field is currently not available from the TWS API.
         * For Bonds only. Not currently implemented due to bond market data restrictions.
         */
        public string Maturity
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief The date the bond was issued.
         * This field is currently not available from the TWS API.
         * For Bonds only. Not currently implemented due to bond market data restrictions.
         */
        public string IssueDate
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Only if bond has embedded options.
         * This field is currently not available from the TWS API.
         * Refers to callable bonds and puttable bonds. Available in TWS description window for bonds.
         */
        public string NextOptionDate
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Type of embedded option.
         * This field is currently not available from the TWS API.
         * Only if bond has embedded options.
         */
        public string NextOptionType
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Only if bond has embedded options.
         * This field is currently not available from the TWS API.
         * For Bonds only.
         */
        public bool NextOptionPartial
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief If populated for the bond in IB's database.
         * For Bonds only.
         */
        public string Notes
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Order's minimal size
         */
        public decimal MinSize
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Order's size increment
         */
        public decimal SizeIncrement
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        /**
         * @brief Order's suggested size increment
         */
        public decimal SuggestedSizeIncrement
        {
            //! @cond
            get;
            set;
            //! @endcond
        }

        // FUND values

        /**
         * @brief Fund's name
         */
        public string FundName { get; set; }

        /**
         * @brief Fund's family
         */
        public string FundFamily { get; set; }

        /**
         * @brief Fund's type
         */
        public string FundType { get; set; }

        /**
         * @brief Fund's front load
         */
        public string FundFrontLoad { get; set; }

        /**
         * @brief Fund's back load
         */
        public string FundBackLoad { get; set; }

        /**
         * @brief Fund's back load time interval
         */
        public string FundBackLoadTimeInterval { get; set; }

        /**
         * @brief Fund's management fee
         */
        public string FundManagementFee { get; set; }

        /**
         * @brief Fund closed flag
         */
        public bool FundClosed { get; set; }

        /**
         * @brief Fund closed for new investors flag
         */
        public bool FundClosedForNewInvestors { get; set; }

        /**
         * @brief Fund closed for new money flag
         */
        public bool FundClosedForNewMoney { get; set; }

        /**
         * @brief Fund's notify amount
         */
        public string FundNotifyAmount { get; set; }

        /**
         * @brief Fund's minimum initial purchase
         */
        public string FundMinimumInitialPurchase { get; set; }

        /**
         * @brief Fund's subsequent minimum purchase
         */
        public string FundSubsequentMinimumPurchase { get; set; }

        /**
         * @brief Fund's blue sky states
         */
        public string FundBlueSkyStates { get; set; }

        /**
         * @brief Fund's blue sky territories
         */
        public string FundBlueSkyTerritories { get; set; }

        /**
         * @brief Fund's distribution policy indicator
         */
        public FundDistributionPolicyIndicator FundDistributionPolicyIndicator { get; set; }

        /**
         * @brief Fund's asset type
         */
        public FundAssetType FundAssetType { get; set; }

        /**
         * @brief A list of ineligibility reasons.
         */
        public List<IneligibilityReason> IneligibilityReasonList { get; set; }

        public ContractDetails()
        {
            Contract = new Contract();
            MinTick = 0;
            UnderConId = 0;
            EvMultiplier = 0;
            MinSize = decimal.MaxValue;
            SizeIncrement = decimal.MaxValue;
            SuggestedSizeIncrement = decimal.MaxValue;
        }
    }

    public enum FundDistributionPolicyIndicator
    {
        None,
        AccumulationFund,
        IncomeFund
    }

    public static class CFundDistributionPolicyIndicator
    {
        public static readonly string[] values = { "None", "N", "Y" };
        public static readonly string[] names = { "None", "Accumulation Fund", "Income Fund" };

        public static string getFundDistributionPolicyIndicatorName(this FundDistributionPolicyIndicator e) => names[(int)e];

        public static FundDistributionPolicyIndicator getFundDistributionPolicyIndicator(string value) => (FundDistributionPolicyIndicator)Array.IndexOf(values, value ?? "None");
    }

    public enum FundAssetType
    {
        None,
        Others,
        MoneyMarket,
        FixedIncome,
        MultiAsset,
        Equity,
        Sector,
        Guaranteed,
        Alternative
    }

    public static class CFundAssetType
    {
        public static readonly string[] values = { "None", "000", "001", "002", "003", "004", "005", "006", "007" };
        public static readonly string[] names = { "None", "Others", "Money Market", "Fixed Income", "Multi-asset", "Equity", "Sector", "Guaranteed", "Alternative" };

        public static string getFundAssetTypeName(this FundAssetType e) => names[(int)e];

        public static FundAssetType getFundAssetType(string value) => (FundAssetType)Array.IndexOf(values, value ?? "None");
    }
}
