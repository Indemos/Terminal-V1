/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    public class EMessage
    {
        private readonly byte[] buf;

        public EMessage(byte[] buf) => this.buf = buf;

        public byte[] GetBuf() => buf;
    }
}
