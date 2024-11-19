/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class TickAttribBidAsk
     * @brief Tick attributes that describes additional information for bid/ask price ticks
     * @sa EWrapper::tickByTickBidAsk, EWrapper::historicalTicksBidAsk
     */
    public class TickAttribBidAsk
    {
        /**
         * @brief Used with real time tick-by-tick. Indicates if bid is lower than day's lowest low. 
         */
        public bool BidPastLow { get; set; }

        /**
         * @brief Used with real time tick-by-tick. Indicates if ask is higher than day's highest ask. 
         */
        public bool AskPastHigh { get; set; }

        /**
         * @brief Returns string to display. 
         */
        public override string ToString() => (BidPastLow ? "bidPastLow " : "") +
                                             (AskPastHigh ? "askPastHigh " : "");
    }
}
