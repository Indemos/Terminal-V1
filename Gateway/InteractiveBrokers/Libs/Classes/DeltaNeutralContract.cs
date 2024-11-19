/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
    * @brief Delta-Neutral Contract.
    */
    public class DeltaNeutralContract
    {
        /**
         * @brief The unique contract identifier specifying the security. Used for Delta-Neutral Combo contracts.
         */
        public int ConId { get; set; }

        /**
        * @brief The underlying stock or future delta. Used for Delta-Neutral Combo contracts.
        */
        public double Delta { get; set; }

        /**
        * @brief The price of the underlying. Used for Delta-Neutral Combo contracts.
        */
        public double Price { get; set; }
    }
}
