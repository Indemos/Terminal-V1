/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class SymbolSamplesMessage
  {
    public int ReqId { get; private set; }
    public ContractDescription[] ContractDescriptions { get; private set; }

    public SymbolSamplesMessage(int reqId, ContractDescription[] contractDescriptions)
    {
      ReqId = reqId;
      ContractDescriptions = contractDescriptions;
    }
  }
}
