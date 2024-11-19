/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class PnLMessage
  {
    public int ReqId { get; private set; }
    public double DailyPnL { get; private set; }
    public double UnrealizedPnL { get; private set; }
    public double RealizedPnL { get; private set; }

    public PnLMessage(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL)
    {
      ReqId = reqId;
      DailyPnL = dailyPnL;
      UnrealizedPnL = unrealizedPnL;
      RealizedPnL = realizedPnL;
    }
  }
}
