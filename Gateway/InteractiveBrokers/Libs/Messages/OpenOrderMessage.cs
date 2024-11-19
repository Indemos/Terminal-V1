/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class OpenOrderMessage : OrderMessage
  {
    public OpenOrderMessage()
    { }

    public OpenOrderMessage(int orderId, Contract contract, Order order, OrderState orderState)
    {
      OrderId = orderId;
      Contract = contract;
      Order = order;
      OrderState = orderState;
    }

    public Contract Contract { get; set; }

    public Order Order { get; set; }

    public OrderState OrderState { get; set; }
  }
}
