/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Runtime.InteropServices;

namespace IBApi
{
    /**
     * @class HistoricalTickLast
     * @brief The historical last tick's description. Used when requesting historical tick data with whatToShow = TRADES
     * @sa EClient, EWrapper
     */
    [ComVisible(true)]
    public class HistoricalTickLast
    {
        public HistoricalTickLast() { }

        public HistoricalTickLast(long time, TickAttribLast tickAttribLast, double price, decimal size, string exchange, string specialConditions)
        {
            Time = time;
            TickAttribLast = tickAttribLast;
            Price = price;
            Size = size;
            Exchange = exchange;
            SpecialConditions = specialConditions;
        }

        /**
         * @brief The UNIX timestamp of the historical tick 
         */
        public long Time
        {
            [return: MarshalAs(UnmanagedType.I8)]
            get;
            [param: MarshalAs(UnmanagedType.I8)]
            private set;
        }

        /**
         * @brief Tick attribs of historical last tick
         */
        public TickAttribLast TickAttribLast { get; private set; }

        /**
         * @brief The last price of the historical tick 
         */
        public double Price { get; private set; }

        /**
         * @brief The last size of the historical tick 
         */
        public decimal Size
        {
            [return: MarshalAs(UnmanagedType.I8)]
            get;
            [param: MarshalAs(UnmanagedType.I8)]
            private set;
        }

        /**
         * @brief The source exchange of the historical tick 
         */
        public string Exchange { get; private set; }

        /**
         * @brief The conditions of the historical tick. Refer to Trade Conditions page for more details: https://www.interactivebrokers.com/en/index.php?f=7235
         */
        public string SpecialConditions { get; private set; }
    }
}
