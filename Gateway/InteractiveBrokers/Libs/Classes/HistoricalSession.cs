/* Copyright (C) 2021 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Runtime.InteropServices;

namespace IBApi
{
    /**
     * @class HistoricalSession
     * @brief The historical session. Used when requesting historical schedule with whatToShow = SCHEDULE
     * @sa EClient, EWrapper
     */
    [ComVisible(true)]
    public class HistoricalSession
    {
        public HistoricalSession() { }

        public HistoricalSession(string startDateTime, string endDateTime, string refDate)
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            RefDate = refDate;
        }

        public string StartDateTime { get; private set; }
        public string EndDateTime { get; private set; }
        public string RefDate { get; private set; }
    }
}
