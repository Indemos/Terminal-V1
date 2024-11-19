/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class TickOptionMessage : MarketDataMessage
  {
    public TickOptionMessage(int requestId, int field, int tickAttrib, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
        : base(requestId, field)
    {
      TickAttrib = tickAttrib;
      ImpliedVolatility = impliedVolatility;
      Delta = delta;
      OptPrice = optPrice;
      PvDividend = pvDividend;
      Gamma = gamma;
      Vega = vega;
      Theta = theta;
      UndPrice = undPrice;
    }

    public int TickAttrib { get; set; }

    public double ImpliedVolatility { get; set; }

    public double Delta { get; set; }

    public double OptPrice { get; set; }

    public double PvDividend { get; set; }

    public double Gamma { get; set; }

    public double Vega { get; set; }

    public double Theta { get; set; }

    public double UndPrice { get; set; }
  }
}
