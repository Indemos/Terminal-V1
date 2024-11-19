/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
    class FamilyCodesMessage
    {
        public FamilyCode[] FamilyCodes { get; private set; }

        public FamilyCodesMessage(FamilyCode[] familyCodes)
        {
            FamilyCodes = familyCodes;
        }
    }
}
