/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class ExecutionMessage
  {
    public ExecutionMessage(int reqId, Contract contract, Execution execution)
    {
      ReqId = reqId;
      Contract = contract;
      Execution = execution;
    }

    public Contract Contract { get; set; }

    public Execution Execution { get; set; }

    public int ReqId { get; set; }
  }
}
