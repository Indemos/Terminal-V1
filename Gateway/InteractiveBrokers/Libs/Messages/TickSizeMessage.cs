/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class TickSizeMessage : MarketDataMessage
  {
    public TickSizeMessage(int requestId, int field, double size) : base(requestId, field)
    {
      Size = size;
    }

    public double Size { get; set; }
  }
}
