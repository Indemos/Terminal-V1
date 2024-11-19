/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class OrderBoundMessage
  {
    public long OrderId { get; private set; }
    public int ApiClientId { get; private set; }
    public int ApiOrderId { get; private set; }

    public OrderBoundMessage(long orderId, int apiClientId, int apiOrderId)
    {
      OrderId = orderId;
      ApiClientId = apiClientId;
      ApiOrderId = apiOrderId;
    }
  }
}
