/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;

namespace InteractiveBrokers.Messages
{
  public class RealTimeBarMessage : HistoricalDataMessage
  {
    public long Timestamp { get; set; }

    public RealTimeBarMessage(int reqId, long date, double open, double high, double low, double close, decimal volume, decimal WAP, int count)
        : base(reqId, new IBApi.Bar(UnixTimestampToDateTime(date).ToString("yyyyMMdd-HH:mm:ss"), open, high, low, close, volume, count, WAP))
    {
      Timestamp = date;
    }

    static DateTime UnixTimestampToDateTime(long unixTimestamp)
    {
      DateTime unixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      return unixBaseTime.AddSeconds(unixTimestamp);
    }
  }
}
