/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class DeepBookMessage
  {
    public DeepBookMessage(int tickerId, int position, int operation, int side, double price, decimal size, string marketMaker, bool isSmartDepth)
    {
      RequestId = tickerId;
      Position = position;
      Operation = operation;
      Side = side;
      Price = price;
      Size = size;
      MarketMaker = marketMaker;
      IsSmartDepth = isSmartDepth;
    }

    public int RequestId { get; set; }

    public int Position { get; set; }

    public int Operation { get; set; }

    public int Side { get; set; }

    public double Price { get; set; }

    public decimal Size { get; set; }

    public string MarketMaker { get; set; }

    public bool IsSmartDepth { get; set; }
  }
}
