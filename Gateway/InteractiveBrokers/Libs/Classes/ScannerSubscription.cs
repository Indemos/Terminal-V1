/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class ScannerSubscription
     * @brief Defines a market scanner request
     */
    public class ScannerSubscription
    {
        /**
         * @brief The number of rows to be returned for the query
         */
        public int NumberOfRows { get; set; } = -1;

        /**
         * @brief The instrument's type for the scan. I.e. STK, FUT.HK, etc.
         */
        public string Instrument { get; set; }

        /**
         * @brief The request's location (STK.US, STK.US.MAJOR, etc). 
         */
        public string LocationCode { get; set; }

        /**
         * @brief Same as TWS Market Scanner's "parameters" field, for example: TOP_PERC_GAIN
         */
        public string ScanCode { get; set; }

        /**
         * @brief Filters out Contracts which price is below this value
         */
        public double AbovePrice { get; set; } = double.MaxValue;

        /**
         * @brief Filters out contracts which price is above this value.
         */
        public double BelowPrice { get; set; } = double.MaxValue;

        /**
         * @brief Filters out Contracts which volume is above this value.
         */
        public int AboveVolume { get; set; } = int.MaxValue;

        /**
         * @brief Filters out Contracts which option volume is above this value.
         */
        public int AverageOptionVolumeAbove { get; set; } = int.MaxValue;

        /**
         * @brief Filters out Contracts which market cap is above this value.
         */
        public double MarketCapAbove { get; set; } = double.MaxValue;

        /**
         * @brief Filters out Contracts which market cap is below this value.
         */
        public double MarketCapBelow { get; set; } = double.MaxValue;

        /**
         * @brief Filters out Contracts which Moody's rating is below this value.
         */
        public string MoodyRatingAbove { get; set; }

        /**
         * @brief Filters out Contracts which Moody's rating is above this value.
         */
        public string MoodyRatingBelow { get; set; }

        /**
         * @brief Filters out Contracts with a S&P rating below this value.
         */
        public string SpRatingAbove { get; set; }

        /**
         * @brief Filters out Contracts with a S&P rating above this value.
         */
        public string SpRatingBelow { get; set; }

        /**
         * @brief Filter out Contracts with a maturity date earlier than this value.
         */
        public string MaturityDateAbove { get; set; }

        /**
         * @brief Filter out Contracts with a maturity date older than this value.
         */
        public string MaturityDateBelow { get; set; }

        /**
         * @brief Filter out Contracts with a coupon rate lower than this value.
         */
        public double CouponRateAbove { get; set; } = double.MaxValue;

        /**
         * @brief Filter out Contracts with a coupon rate higher than this value.
         */
        public double CouponRateBelow { get; set; } = double.MaxValue;

        /**
         * @brief Filters out Convertible bonds
         */
        public bool ExcludeConvertible { get; set; }

        /**
         * @brief For example, a pairing "Annual, true" used on the "top Option Implied Vol % Gainers" scan would return annualized volatilities.
         */
        public string ScannerSettingPairs { get; set; }

        /**
         * @brief -
         *      CORP = Corporation
         *      ADR = American Depositary Receipt
         *      ETF = Exchange Traded Fund
         *      REIT = Real Estate Investment Trust
         *      CEF = Closed End Fund
         */
        public string StockTypeFilter { get; set; }
    }
}
