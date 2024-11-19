/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class HeadTimestampMessage
  {
    public int ReqId { get; private set; }
    public string HeadTimestamp { get; private set; }

    public HeadTimestampMessage(int reqId, string headTimestamp)
    {
      ReqId = reqId;
      HeadTimestamp = headTimestamp;
    }
  }
}
