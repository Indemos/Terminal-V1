/* Copyright (C) 2022 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    public class WshEventData
    {
        public int ConId { get; set; }

        public string Filter { get; set; }

        public bool FillWatchlist { get; set; }

        public bool FillPortfolio { get; set; }

        public bool FillCompetitors { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public int TotalLimit { get; set; }


        public WshEventData()
        {
            ConId = int.MaxValue;
            Filter = Order.EMPTY_STR;
            FillWatchlist = false;
            FillPortfolio = false;
            FillCompetitors = false;
            StartDate = Order.EMPTY_STR;
            EndDate = Order.EMPTY_STR;
            TotalLimit = int.MaxValue;
        }

        public WshEventData(int conId, bool fillWatchlist, bool fillPortfolio, bool fillCompetitors, string startDate, string endDate, int totalLimit)
        {
            ConId = conId;
            Filter = Order.EMPTY_STR;
            FillWatchlist = fillWatchlist;
            FillPortfolio = fillPortfolio;
            FillCompetitors = fillCompetitors;
            StartDate = startDate;
            EndDate = endDate;
            TotalLimit = totalLimit;
        }

        public WshEventData(string filter, bool fillWatchlist, bool fillPortfolio, bool fillCompetitors, string startDate, string endDate, int totalLimit)
        {
            ConId = int.MaxValue;
            Filter = filter;
            FillWatchlist = fillWatchlist;
            FillPortfolio = fillPortfolio;
            FillCompetitors = fillCompetitors;
            StartDate = startDate;
            EndDate = endDate;
            TotalLimit = totalLimit;
        }
    }
}
