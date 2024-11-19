/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
    class AccountUpdateMultiEndMessage 
    {
        public AccountUpdateMultiEndMessage(int reqId)
        {
            ReqId = ReqId;
        }

        public int ReqId { get; set; }
    }
}
