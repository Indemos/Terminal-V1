/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class PnLSingleMessage
  {
    public int ReqId { get; private set; }
    public decimal Pos { get; private set; }
    public double DailyPnL { get; private set; }
    public double Value { get; private set; }
    public double UnrealizedPnL { get; private set; }
    public double RealizedPnL { get; private set; }

    public PnLSingleMessage(int reqId, decimal pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value)
    {
      ReqId = reqId;
      Pos = pos;
      DailyPnL = dailyPnL;
      Value = value;
      UnrealizedPnL = unrealizedPnL;
      RealizedPnL = realizedPnL;
    }
  }
}
