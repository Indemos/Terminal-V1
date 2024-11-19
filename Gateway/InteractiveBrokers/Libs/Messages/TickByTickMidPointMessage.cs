/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class TickByTickMidPointMessage
  {
    public int ReqId { get; private set; }
    public long Time { get; private set; }
    public double MidPoint { get; private set; }

    public TickByTickMidPointMessage(int reqId, long time, double midPoint)
    {
      ReqId = reqId;
      Time = time;
      MidPoint = midPoint;
    }
  }
}
