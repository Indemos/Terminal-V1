/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class TickPriceMessage : TickGenericMessage
  {
    public TickPriceMessage(int requestId, int field, double price, TickAttrib attribs)
        : base(requestId, field, price)
    {
      Attribs = attribs;
    }

    public TickAttrib Attribs { get; set; }
  }
}
