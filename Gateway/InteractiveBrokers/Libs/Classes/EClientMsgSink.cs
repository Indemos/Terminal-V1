/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    internal interface EClientMsgSink
    {
        void serverVersion(int version, string time);
        void redirect(string host);
    }
}
