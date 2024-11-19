/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;

namespace IBApi
{
    public class EClientException : Exception
    {
        public CodeMsgPair Err { get; private set; }
        public string Text { get; private set; }

        public EClientException(CodeMsgPair err) => Err = err;

        public EClientException(CodeMsgPair err, string text) : this(err) => Text = text;
    }
}
