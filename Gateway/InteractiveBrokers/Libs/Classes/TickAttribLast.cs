/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class TickAttribLast
     * @brief Tick attributes that describes additional information for last price ticks
     * @sa EWrapper::tickByTickAllLast, EWrapper::historicalTicksLast
     */
    public class TickAttribLast
    {
        /**
         * @brief Not currently used with trade data; only applies to bid/ask data. 
         */
        public bool PastLimit { get; set; }

        /**
         * @brief Used with tick-by-tick last data or historical ticks last to indicate if a trade is classified as 'unreportable' (odd lots, combos, derivative trades, etc)
        */
        public bool Unreported { get; set; }

        /**
         * @brief Returns string to display. 
         */
        public override string ToString() => (PastLimit ? "pastLimit " : "") +
                                             (Unreported ? "unreported " : "");
    }
}
