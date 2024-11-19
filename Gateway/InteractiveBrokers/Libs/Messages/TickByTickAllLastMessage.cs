/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class TickByTickAllLastMessage
  {
    public int ReqId { get; private set; }
    public int TickType { get; private set; }
    public long Time { get; private set; }
    public double Price { get; private set; }
    public decimal Size { get; private set; }
    public TickAttribLast TickAttribLast { get; private set; }
    public string Exchange { get; private set; }
    public string SpecialConditions { get; private set; }

    public TickByTickAllLastMessage(int reqId, int tickType, long time, double price, decimal size, TickAttribLast tickAttribLast, string exchange, string specialConditions)
    {
      ReqId = reqId;
      TickType = tickType;
      Time = time;
      Price = price;
      Size = size;
      TickAttribLast = tickAttribLast;
      Exchange = exchange;
      SpecialConditions = specialConditions;
    }
  }
}
