/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class TickReqParamsMessage
  {
    public int TickerId { get; private set; }
    public double MinTick { get; private set; }
    public string BboExchange { get; private set; }
    public int SnapshotPermissions { get; private set; }

    public TickReqParamsMessage(int tickerId, double minTick, string bboExchange, int snapshotPermissions)
    {
      TickerId = tickerId;
      MinTick = minTick;
      BboExchange = bboExchange;
      SnapshotPermissions = snapshotPermissions;
    }
  }
}
