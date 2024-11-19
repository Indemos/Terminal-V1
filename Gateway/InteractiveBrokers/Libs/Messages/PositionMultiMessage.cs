/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class PositionMultiMessage
  {
    public PositionMultiMessage(int reqId, string account, string modelCode, Contract contract, decimal pos, double avgCost)
    {
      ReqId = reqId;
      Account = account;
      ModelCode = modelCode;
      Contract = contract;
      Position = pos;
      AverageCost = avgCost;
    }

    public int ReqId { get; set; }

    public string Account { get; set; }

    public string ModelCode { get; set; }

    public Contract Contract { get; set; }

    public decimal Position { get; set; }

    public double AverageCost { get; set; }
  }
}
