/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
    public class MarketRuleMessage
    {
        public int MarketruleId { get; private set; }
        public PriceIncrement[] PriceIncrements { get; private set; }

        public MarketRuleMessage(int marketRuleId, PriceIncrement[] priceIncrements)
        {
            MarketruleId = marketRuleId;
            PriceIncrements = priceIncrements;
        }
    }
}
