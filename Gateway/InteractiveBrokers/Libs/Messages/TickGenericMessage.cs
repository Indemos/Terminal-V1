/* Copyright (C) 2022 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class TickGenericMessage : MarketDataMessage
  {
    public TickGenericMessage(int requestId, int field, double? price) : base(requestId, field)
    {
      Data = price;
      Value = price.GetValueOrDefault();
    }

    public double Value { get; set; }

    public double? Data { get; set; }
  }
}
