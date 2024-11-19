/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class DepthMktDataDescription
     * @brief A class for storing depth market data desctiption
     */
    public class DepthMktDataDescription
    {
        /**
         * @brief The exchange name
         */
        public string Exchange { get; set; }

        /**
         * @brief The security type
         */
        public string SecType { get; set; }

        /**
         * @brief The listing exchange name
         */
        public string ListingExch { get; set; }

        /**
         * @brief The service data type
         */
        public string ServiceDataType { get; set; }

        /**
         * @brief The aggregated group
         */
        public int AggGroup { get; set; }

        public DepthMktDataDescription() { }

        public DepthMktDataDescription(string exchange, string secType, string listingExch, string serviceDataType, int aggGroup)
        {
            Exchange = exchange;
            SecType = secType;
            ListingExch = listingExch;
            ServiceDataType = serviceDataType;
            AggGroup = aggGroup;
        }
    }
}
