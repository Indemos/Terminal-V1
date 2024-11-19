/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class ComboLeg
     * @brief Class representing a leg within combo orders.
     * @sa Order
     */
    public class ComboLeg
    {
        public static int SAME = 0;
        public static int OPEN = 1;
        public static int CLOSE = 2;
        public static int UNKNOWN = 3;


        /**
         * @brief The Contract's IB's unique id
         */
        public int ConId { get; set; }

        /**
         * @brief Select the relative number of contracts for the leg you are constructing. To help determine the ratio for a specific combination order, refer to the Interactive Analytics section of the User's Guide.
         */
        public int Ratio { get; set; }

        /**
         * @brief The side (buy or sell) of the leg:\n
         *      - For individual accounts, only BUY and SELL are available. SSHORT is for institutions.
         */
        public string Action { get; set; }

        /**
         * @brief The destination exchange to which the order will be routed.
         */
        public string Exchange { get; set; }

        /**
         * @brief Specifies whether an order is an open or closing order.
         * For institutional customers to determine if this order is to open or close a position.
         *      0 - Same as the parent security. This is the only option for retail customers.\n
         *      1 - Open. This value is only valid for institutional customers.\n
         *      2 - Close. This value is only valid for institutional customers.\n
         *      3 - Unknown
         */
        public int OpenClose { get; set; }

        /**
         * @brief For stock legs when doing short selling.
         * Set to 1 = clearing broker, 2 = third party
         */
        public int ShortSaleSlot { get; set; }

        /**
         * @brief When ShortSaleSlot is 2, this field shall contain the designated location.
         */
        public string DesignatedLocation { get; set; }

        /**
         * @brief Mark order as exempt from short sale uptick rule.\n
         * Possible values:\n
         *      0 - Does not apply the rule.\n
         *      -1 - Applies the short sale uptick rule.
         */
        public int ExemptCode { get; set; }

        public ComboLeg() { }

        public ComboLeg(int conId, int ratio, string action, string exchange, int openClose, int shortSaleSlot, string designatedLocation, int exemptCode)
        {
            ConId = conId;
            Ratio = ratio;
            Action = action;
            Exchange = exchange;
            OpenClose = openClose;
            ShortSaleSlot = shortSaleSlot;
            DesignatedLocation = designatedLocation;
            ExemptCode = exemptCode;
        }
    }
}
