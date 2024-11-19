/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class PriceIncrement
     * @brief Class describing price increment
     * @sa EClient::reqMarketRule, EWrapper::marketRule
     */
    public class PriceIncrement
    {
        /**
         * @brief The low edge
         */
        public double LowEdge { get; set; }

        /**
         * @brief The increment
         */
        public double Increment { get; set; }

        public PriceIncrement() { }

        public PriceIncrement(double lowEdge, double increment)
        {
            LowEdge = lowEdge;
            Increment = increment;
        }
    }
}
