/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class HistoricalTickLastMessage
  {
    public int ReqId { get; private set; }
    public long Time { get; private set; }
    public TickAttribLast TickAttribLast { get; private set; }
    public double Price { get; private set; }
    public decimal Size { get; private set; }
    public string Exchange { get; private set; }
    public string SpecialConditions { get; private set; }

    public HistoricalTickLastMessage(int reqId, long time, TickAttribLast tickAttribLast, double price, decimal size, string exchange, string specialConditions)
    {
      ReqId = reqId;
      Time = time;
      TickAttribLast = tickAttribLast;
      Price = price;
      Size = size;
      Exchange = exchange;
      SpecialConditions = specialConditions;
    }
  }
}
