/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class BondContractDetailsMessage
  {
    public BondContractDetailsMessage(int requestId, ContractDetails contractDetails)
    {
      RequestId = requestId;
      ContractDetails = contractDetails;
    }

    public ContractDetails ContractDetails { get; set; }

    public int RequestId { get; set; }
  }
}
