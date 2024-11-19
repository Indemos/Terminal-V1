/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class SoftDollarTiersMessage
  {
    public int ReqId { get; private set; }
    public IBApi.SoftDollarTier[] Tiers { get; private set; }

    public SoftDollarTiersMessage(int reqId, IBApi.SoftDollarTier[] tiers)
    {
      ReqId = reqId;
      Tiers = tiers;
    }
  }
}
