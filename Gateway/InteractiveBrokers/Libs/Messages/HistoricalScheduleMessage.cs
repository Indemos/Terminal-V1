/* Copyright (C) 2021 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers.Messages
{
  public class HistoricalScheduleMessage
  {
    public int ReqId { get; set; }
    public string StartDateTime { get; set; }
    public string EndDateTime { get; set; }
    public string TimeZone { get; set; }
    public HistoricalSession[] Sessions { get; private set; }

    public HistoricalScheduleMessage(int reqId, string startDateTime, string endDateTime, string timeZone, HistoricalSession[] sessions)
    {
      ReqId = reqId;
      StartDateTime = startDateTime;
      EndDateTime = endDateTime;
      TimeZone = timeZone;
      Sessions = sessions;
    }
  }
}
