/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace InteractiveBrokers.Messages
{
  public class SecurityDefinitionOptionParameterMessage
  {
    public int ReqId { get; private set; }
    public string Exchange { get; private set; }
    public int UnderlyingConId { get; private set; }
    public string TradingClass { get; private set; }
    public string Multiplier { get; private set; }
    public HashSet<string> Expirations { get; private set; }
    public HashSet<double> Strikes { get; private set; }

    public SecurityDefinitionOptionParameterMessage(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
    {
      ReqId = reqId;
      Exchange = exchange;
      UnderlyingConId = underlyingConId;
      TradingClass = tradingClass;
      Multiplier = multiplier;
      Expirations = expirations;
      Strikes = strikes;
    }
  }
}
