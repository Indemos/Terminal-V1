/* Copyright (C) 2024 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace IBApi
{
  /**
   * @class Order
   * @brief The order's description.
   * @sa Contract, OrderComboLeg, OrderState
   */
  public class Order
  {
    public static int CUSTOMER = 0;
    public static int FIRM = 1;
    public static char OPT_UNKNOWN = '?';
    public static char OPT_BROKER_DEALER = 'b';
    public static char OPT_CUSTOMER = 'c';
    public static char OPT_FIRM = 'f';
    public static char OPT_ISEMM = 'm';
    public static char OPT_FARMM = 'n';
    public static char OPT_SPECIALIST = 'y';
    public static int AUCTION_MATCH = 1;
    public static int AUCTION_IMPROVEMENT = 2;
    public static int AUCTION_TRANSPARENT = 3;
    public static string EMPTY_STR = "";
    public static double COMPETE_AGAINST_BEST_OFFSET_UP_TO_MID = double.PositiveInfinity;

    // main order fields
    // extended order fields
    // "Time in Force" - DAY, GTC, etc.
    // GTC orders
    // one cancels all group name
    // 1 = CANCEL_WITH_BLOCK, 2 = REDUCE_WITH_BLOCK, 3 = REDUCE_NON_BLOCK
    // if false, order will be created but not transmitted
    // Parent order Id, to associate Auto STP or TRAIL orders with the original order.
    // 0=Default, 1=Double_Bid_Ask, 2=Last, 3=Double_Last, 4=Bid_Ask, 7=Last_or_Bid_Ask, 8=Mid-point
    // FORMAT: 20060505 08:00:00 {time zone}
    // FORMAT: 20060505 08:00:00 {time zone}
    // Individual = 'I', Agency = 'A', AgentOtherMember = 'W', IndividualPTIA = 'J', AgencyPTIA = 'U', AgentOtherMemberPTIA = 'M', IndividualPT = 'K', AgencyPT = 'Y', AgentOtherMemberPT = 'N'
    // REL orders only
    // for TRAILLIMIT orders only
    // Financial advisors only
    // Institutional orders only
    // O=Open, C=Close
    // 0=Customer, 1=Firm
    // 1 if you hold the shares, 2 if they will be delivered from elsewhere.  Only for Action="SSHORT
    // set when slot=2 only.
    // SMART routing only
    // BOX or VOL ORDERS ONLY
    // 1=AUCTION_MATCH, 2=AUCTION_IMPROVEMENT, 3=AUCTION_TRANSPARENT
    // BOX ORDERS ONLY
    // pegged to stock or VOL orders
    // VOLATILITY ORDERS ONLY
    // 1=daily, 2=annual
    // 1=Average, 2 = BidOrAsk

    // COMBO ORDERS ONLY
    // EFP orders only
    // EFP orders only
    // SCALE ORDERS ONLY
    // HEDGE ORDERS ONLY
    // 'D' - delta, 'B' - beta, 'F' - FX, 'P' - pair
    // beta value for beta hedge, ratio for pair hedge
    // Clearing info
    // True beneficiary of the order
    // "" (Default), "IB", "Away", "PTA" (PostTrade)
    // ALGO ORDERS ONLY
    // What-if
    //algoId
    // Not Held

    // Smart combo routing params
    // order combo legs
    // native cash quantity

    // don't use auto price for hedge

    /**
     * @brief The API client's order id.
     */
    public int OrderId { get; set; }

    /**
     * @brief The Solicited field should be used for orders initiated or recommended by the broker or adviser that were approved by the client (by phone, email, chat, verbally, etc.) prior to entry. Please note that orders that the adviser or broker placed without specifically discussing with the client are discretionary orders, not solicited.
     */
    public bool Solicited { get; set; }

    /**
     * @brief The API client id which placed the order.
     */
    public int ClientId { get; set; }

    /**
     * @brief The Host order identifier.
     */
    public int PermId { get; set; }

    /**
     * @brief Identifies the side. \n
     * Generally available values are <b>BUY</b> and <b>SELL</b>. \n
     * Additionally, <b>SSHORT</b> and <b>SLONG</b> are available in some institutional-accounts only. \n
     * For general account types, a <b>SELL</b> order will be able to enter a short position automatically if the order quantity is larger than your current long position. \n
     * <b>SSHORT</b> is only supported for institutional account configured with Long/Short account segments or clearing with a separate account. \n
     * <b>SLONG</b> is available in specially-configured institutional accounts to indicate that long position not yet delivered is being sold.
     */
    public string Action { get; set; }

    /**
     * @brief The number of positions being bought/sold.
     */
    public decimal TotalQuantity { get; set; }

    /**
     * @brief The order's type.
     */
    public string OrderType { get; set; }

    /**
     * @brief The LIMIT price. \n
     * <i>Used for limit, stop-limit and relative orders. In all other cases specify zero. For relative orders with no limit price, also specify zero.</i>
     */
    public double LmtPrice { get; set; }

    /**
         * @brief Generic field to contain the stop price for <b>STP LMT</b> orders, trailing amount, etc.
         */
    public double AuxPrice { get; set; }

    /**
     * @brief The time in force.\n
     * Valid values are: \n
     *      <b>DAY</b> - Valid for the day only.\n
     *      <b>GTC</b> - Good until canceled. The order will continue to work within the system and in the marketplace until it executes or is canceled. GTC orders will be automatically be cancelled under the following conditions: \n
     *          \t\t If a corporate action on a security results in a stock split (forward or reverse), exchange for shares, or distribution of shares.
     *          \t\t If you do not log into your IB account for 90 days. \n
     *          \t\t At the end of the calendar quarter following the current quarter. For example, an order placed during the third quarter of 2011 will be canceled at the end of the first quarter of 2012. If the last day is a non-trading day, the cancellation will occur at the close of the final trading day of that quarter. For example, if the last day of the quarter is Sunday, the orders will be cancelled on the preceding Friday.\n
     *          \t\t Orders that are modified will be assigned a new “Auto Expire” date consistent with the end of the calendar quarter following the current quarter.\n
     *          \t\t Orders submitted to IB that remain in force for more than one day will not be reduced for dividends. To allow adjustment to your order price on ex-dividend date, consider using a Good-Til-Date/Time (GTD) or Good-after-Time/Date (GAT) order type, or a combination of the two.\n
     *      <b>IOC</b> - Immediate or Cancel. Any portion that is not filled as soon as it becomes available in the market is canceled.\n
     *      <b>GTD</b> - Good until Date. It will remain working within the system and in the marketplace until it executes or until the close of the market on the date specified\n
     *      <b>OPG</b> - Use OPG to send a market-on-open (MOO) or limit-on-open (LOO) order.\n
     *      <b>FOK</b> - If the entire Fill-or-Kill order does not execute as soon as it becomes available, the entire order is canceled.\n
     *      <b>DTC</b> - Day until Canceled.
     */
    public string Tif { get; set; }


    /**
     * @brief One-Cancels-All group identifier.
     */
    public string OcaGroup { get; set; }

    /**
     * @brief Tells how to handle remaining orders in an OCA group when one order or part of an order executes.\n
     * Valid values are:\n
     *      \t\t <b>1</b> - Cancel all remaining orders with block.\n
     *      \t\t <b>2</b> - Remaining orders are proportionately reduced in size with block.\n
     *      \t\t <b>3</b> - Remaining orders are proportionately reduced in size with no block.\n
     * If you use a value "with block" it gives the order overfill protection. This means that only one order in the group will be routed at a time to remove the possibility of an overfill.
     */
    public int OcaType { get; set; }

    /**
     * @brief The order reference. \n
     * <i>Intended for institutional customers only, although all customers may use it to identify the API client that sent the order when multiple API clients are running.</i>
     */
    public string OrderRef { get; set; }

    /**
     * @brief Specifies whether the order will be transmitted by TWS. If set to false, the order will be created at TWS but will not be sent.
     */
    public bool Transmit { get; set; }

    /**
     * @brief The order ID of the parent order, used for bracket and auto trailing stop orders.
     */
    public int ParentId { get; set; }

    /**
     * @brief If set to true, specifies that the order is an ISE Block order.
     */
    public bool BlockOrder { get; set; }

    /**
     * @brief If set to true, specifies that the order is a Sweep-to-Fill order.
     */
    public bool SweepToFill { get; set; }

    /**
     * @brief The publicly disclosed order size, used when placing Iceberg orders.
     */
    public int DisplaySize { get; set; }

    /**
     * @brief Specifies how Simulated Stop, Stop-Limit and Trailing Stop orders are triggered.\n
     * Valid values are:\n
     *  <b>0</b> - The default value. The "double bid/ask" function will be used for orders for OTC stocks and US options. All other orders will used the "last" function.\n
     *  <b>1</b> - use "double bid/ask" function, where stop orders are triggered based on two consecutive bid or ask prices.\n
     *  <b>2</b> - "last" function, where stop orders are triggered based on the last price.\n
     *  <b>3</b> - double last function.\n
     *  <b>4</b> - bid/ask function.\n
     *  <b>7</b> - last or bid/ask function.\n
     *  <b>8</b> - mid-point function.
     */
    public int TriggerMethod { get; set; }

    /**
     * @brief If set to true, allows orders to also trigger or fill outside of regular trading hours.
     */
    public bool OutsideRth { get; set; }

    /**
     * @brief If set to true, the order will not be visible when viewing the market depth. This option only applies to orders routed to the NASDAQ exchange.
     */
    public bool Hidden { get; set; }

    /**
     * @brief Specifies the date and time after which the order will be active.\n
     * Format: yyyymmdd hh:mm:ss {optional Timezone}
     */
    public string GoodAfterTime { get; set; }

    /**
     * @brief The date and time until the order will be active.\n
     * You must enter GTD as the time in force to use this string. The trade's "Good Till Date," format "yyyyMMdd HH:mm:ss (optional time zone)" or UTC "yyyyMMdd-HH:mm:ss"
     */
    public string GoodTillDate { get; set; }

    /**
     * @brief Overrides TWS constraints.\n
     * Precautionary constraints are defined on the TWS Presets page, and help ensure tha tyour price and size order values are reasonable. Orders sent from the API are also validated against these safety constraints, and may be rejected if any constraint is violated. To override validation, set this parameter’s value to True.
     */
    public bool OverridePercentageConstraints { get; set; }

    /**
     * @brief
     * Individual = 'I'\n
     * Agency = 'A'\n
     * AgentOtherMember = 'W'\n
     * IndividualPTIA = 'J'\n
     * AgencyPTIA = 'U'\n
     * AgentOtherMemberPTIA = 'M'\n
     * IndividualPT = 'K'\n
     * AgencyPT = 'Y'\n
     * AgentOtherMemberPT = 'N'
     */
    public string Rule80A { get; set; }

    /**
     * @brief Indicates whether or not all the order has to be filled on a single execution.
     */
    public bool AllOrNone { get; set; }

    /**
     * @brief Identifies a minimum quantity order type.
     */
    public int MinQty { get; set; }

    /**
     * @brief The percent offset amount for relative orders.
     */
    public double PercentOffset { get; set; }

    /**
     * @brief Trail stop price for TRAIL LIMIT orders.
     */
    public double TrailStopPrice { get; set; }

    /**
     * @brief Specifies the trailing amount of a trailing stop order as a percentage.\n
     * Observe the following guidelines when using the trailingPercent field:
     *    - This field is mutually exclusive with the existing trailing amount. That is, the API client can send one or the other but not both.\n
     *    - This field is read AFTER the stop price (barrier price) as follows: deltaNeutralAuxPrice stopPrice, trailingPercent, scale order attributes\n
     *    - The field will also be sent to the API in the openOrder message if the API client version is >= 56. It is sent after the stopPrice field as follows: stopPrice, trailingPct, basisPoint.
     */
    public double TrailingPercent { get; set; }

    /**
     * @brief The Financial Advisor group the trade will be allocated to. <i>Use an empty string if not applicable.</i>
     */
    public string FaGroup { get; set; }

    /**
     * @brief The Financial Advisor allocation method the trade will be allocated to. <i>Use an empty string if not applicable.</i>
     */
    public string FaMethod { get; set; }

    /**
     * @brief The Financial Advisor percentage concerning the trade's allocation. <i>Use an empty string if not applicable.</i>
     */
    public string FaPercentage { get; set; }


    /**
     * @brief For institutional customers only. Valid values are <b>O (open) and C (close).</b>\n
     * Available for institutional clients to determine if this order is to open or close a position.\n
     * When Action = "BUY" and OpenClose = "O" this will open a new position.\n
     * When Action = "BUY" and OpenClose = "C" this will close and existing short position.
     */
    public string OpenClose { get; set; }


    /**
     * @brief The order's origin. Same as TWS "Origin" column. Identifies the type of customer from which the order originated. \n
     * Valid values are: \n
     * <b>0</b> - Customer \n
     * <b>1</b> - Firm
     */
    public int Origin { get; set; }

    /**
     * @brief For institutions only. \n
     * Valid values are: \n
     * <b>1</b> - Broker holds shares \n
     * <b>2</b> - Shares come from elsewhere
     */
    public int ShortSaleSlot { get; set; }

    /**
     * For institutions only. Indicates the location where the shares to short come from. Used only when short sale slot is set to 2 (which means that the shares to short are held elsewhere and not with IB).
     */
    public string DesignatedLocation { get; set; }

    /**
     * @brief Only available with IB Execution-Only accounts with applicable securities. \n
     * Mark order as exempt from short sale uptick rule
     */
    public int ExemptCode { get; set; }

    /**
     * @brief The amount off the limit price allowed for discretionary orders.
     */
    public double DiscretionaryAmt { get; set; }

    /**
     * @brief Use to opt out of default SmartRouting for orders routed directly to ASX. \n
     * This attribute defaults to false unless explicitly set to true. \n
     * When set to false, orders routed directly to ASX will NOT use SmartRouting. \n
     * When set to true, orders routed directly to ASX orders WILL use SmartRouting
     */

    public bool OptOutSmartRouting { get; set; }

    /**
     * @brief For BOX orders only. \n
     * Values include: \n
     * <b>1</b> - Match \n
     * <b>2</b> - Improvement \n
     * <b>3</b> - Transparent
     */
    public int AuctionStrategy { get; set; }

    /**
     * @brief The auction's starting price. <i>For BOX orders only.</i>
     */
    public double StartingPrice { get; set; }

    /**
     * @brief The stock's reference price.\n
     * <i>The reference price is used for VOL orders to compute the limit price sent to an exchange (whether or not Continuous Update is selected), and for price range monitoring.</i>
     */
    public double StockRefPrice { get; set; }

    /**
     * @brief The stock's Delta. <i>For orders on BOX only.</i>
     */
    public double Delta { get; set; }

    /**
     * @brief The lower value for the acceptable underlying stock price range.\n
     * <i>For price improvement option orders on BOX and VOL orders with dynamic management.</i>
     */
    public double StockRangeLower { get; set; }

    /**
     * @brief The upper value for the acceptable underlying stock price range.\n
     * <i>For price improvement option orders on BOX and VOL orders with dynamic management.</i>
     */
    public double StockRangeUpper { get; set; }

    /**
     * @brief The option price in volatility, as calculated by TWS' Option Analytics.\n
     * This value is expressed as a percent and is used to calculate the limit price sent to the exchange.
     */
    public double Volatility { get; set; }

    /**
     * @brief Values include: \n
     * <b>1</b> - Daily Volatility \n
     * <b>2</b> - Annual Volatility
     */
    public int VolatilityType { get; set; }

    /**
     * @brief Specifies whether TWS will automatically update the limit price of the order as the underlying price moves. <i>VOL orders only.</i>
     */
    public int ContinuousUpdate { get; set; }

    /**
     * @brief Specifies how you want TWS to calculate the limit price for options, and for stock range price monitoring.\n
     * <i>VOL orders only. </i>\n
     * Valid values include: \n
     * <b>1</b> - Average of NBBO \n
     * <b>2</b> - NBB or the NBO depending on the action and right.
     */
    public int ReferencePriceType { get; set; }

    /**
     * @brief Enter an order type to instruct TWS to submit a delta neutral trade on full or partial execution of the VOL order. <i>VOL orders only. For no hedge delta order to be sent, specify NONE.</i>
     */
    public string DeltaNeutralOrderType { get; set; }

    /**
     * @brief Use this field to enter a value if the value in the deltaNeutralOrderType field is an order type that requires an Aux price, such as a REL order. <i>VOL orders only.</i>
     */
    public double DeltaNeutralAuxPrice { get; set; }

    /**
     * @brief The unique contract identifier specifying the security in Delta Neutral order.
     */
    public int DeltaNeutralConId { get; set; }

    /**
     * @brief Indicates the firm which will settle the Delta Neutral trade. <i>Institutions only.</i>
     */
    public string DeltaNeutralSettlingFirm { get; set; }

    /**
     * @brief Specifies the beneficiary of the Delta Neutral order.
     */
    public string DeltaNeutralClearingAccount { get; set; }

    /**
     * @brief Specifies where the clients want their shares to be cleared at. <i>Must be specified by execution-only clients.</i>\n
     * Valid values are:\n
     * <b>IB</b>, <b>Away</b>, and <b>PTA</b> (post trade allocation).
     */
    public string DeltaNeutralClearingIntent { get; set; }

    /**
     * @brief Specifies whether the order is an Open or a Close order and is used when the hedge involves a CFD and and the order is clearing away.
     */
    public string DeltaNeutralOpenClose { get; set; }

    /**
     * @brief Used when the hedge involves a stock and indicates whether or not it is sold short.
     */
    public bool DeltaNeutralShortSale { get; set; }

    /**
     * @brief Indicates a short sale Delta Neutral order. Has a value of 1 (the clearing broker holds shares) or 2 (delivered from a third party). If you use 2, then you must specify a deltaNeutralDesignatedLocation.
     */
    public int DeltaNeutralShortSaleSlot { get; set; }

    /**
     * @brief Identifies third party order origin. Used only when deltaNeutralShortSaleSlot = 2.
     */
    public string DeltaNeutralDesignatedLocation { get; set; }

    /**
     * @brief Specifies Basis Points for EFP order. The values increment in 0.01% = 1 basis point. <i>For EFP orders only.</i>
     */
    public double BasisPoints { get; set; }

    /**
     * @brief Specifies the increment of the Basis Points. <i>For EFP orders only.</i>
     */
    public int BasisPointsType { get; set; }

    /**
     * @brief Defines the size of the first, or initial, order component. <i>For Scale orders only.</i>
     */
    public int ScaleInitLevelSize { get; set; }

    /**
     * @brief Defines the order size of the subsequent scale order components. <i>For Scale orders only. Used in conjunction with scaleInitLevelSize().</i>
     */
    public int ScaleSubsLevelSize { get; set; }

    /**
     * @brief Defines the price increment between scale components. <i>For Scale orders only. This value is compulsory.</i>
     */
    public double ScalePriceIncrement { get; set; }

    /**
     * @brief Modifies the value of the Scale order. <i>For extended Scale orders.</i>
     */
    public double ScalePriceAdjustValue { get; set; }

    /**
     * @brief Specifies the interval when the price is adjusted. <i>For extended Scale orders.</i>
     */
    public int ScalePriceAdjustInterval { get; set; }

    /**
     * @brief Specifies the offset when to adjust profit. <i>For extended scale orders.</i>
     */
    public double ScaleProfitOffset { get; set; }

    /**
     * @brief Restarts the Scale series if the order is cancelled. <i>For extended scale orders.</i>
     */
    public bool ScaleAutoReset { get; set; }

    /**
     * @brief The initial position of the Scale order. <i>For extended scale orders.</i>
     */
    public int ScaleInitPosition { get; set; }

    /**
     * @brief Specifies the initial quantity to be filled. <i>For extended scale orders.</i>
     */
    public int ScaleInitFillQty { get; set; }

    /**
     * @brief Defines the random percent by which to adjust the position. <i>For extended scale orders.</i>
     */
    public bool ScaleRandomPercent { get; set; }

    /**
     * @brief <i>For hedge orders.</i>\n
     * Possible values include:\n
     *      <b>D</b> - Delta \n
     *      <b>B</b> - Beta \n
     *      <b>F</b> - FX \n
     *      <b>P</b> - Pair
     */
    public string HedgeType { get; set; }

    /**
     * @brief <i>For hedge orders.</i>\n
     * Beta = x for Beta hedge orders, ratio = y for Pair hedge order
     */
    public string HedgeParam { get; set; }

    /**
     * @brief The account the trade will be allocated to.
     */
    public string Account { get; set; }

    /**
     * @brief Indicates the firm which will settle the trade. <i>Institutions only.</i>
     */
    public string SettlingFirm { get; set; }

    /**
     * @brief Specifies the true beneficiary of the order.\n
     * <i>For IBExecution customers. This value is required for FUT/FOP orders for reporting to the exchange.</i>
     */
    public string ClearingAccount { get; set; }

    /**
     * @brief For execution-only clients to know where do they want their shares to be cleared at.\n
     * Valid values are:\n
     * <b>IB</b>, <b>Away</b>, and <b>PTA</b> (post trade allocation).
     */
    public string ClearingIntent { get; set; }

    /**
     * @brief The algorithm strategy.\n
     * As of API verion 9.6, the following algorithms are supported:\n
     *      <b>ArrivalPx</b> - Arrival Price \n
     *      <b>DarkIce</b> - Dark Ice \n
     *      <b>PctVol</b> - Percentage of Volume \n
     *      <b>Twap</b> - TWAP (Time Weighted Average Price) \n
     *      <b>Vwap</b> - VWAP (Volume Weighted Average Price) \n
     * <b>For more information about IB's API algorithms, refer to https://interactivebrokers.github.io/tws-api/ibalgos.html</b>
     */
    public string AlgoStrategy { get; set; }

    /**
     * @brief The list of parameters for the IB algorithm.\n
     * <b>For more information about IB's API algorithms, refer to https://interactivebrokers.github.io/tws-api/ibalgos.html</b>
     */
    public List<TagValue> AlgoParams { get; set; }

    /**
     * @brief Allows to retrieve the commissions and margin information.\n
     * When placing an order with this attribute set to true, the order will not be placed as such. Instead it will used to request the commissions and margin information that would result from this order.
     */
    public bool WhatIf { get; set; }

    /**
     * @brief Identifies orders generated by algorithmic trading.
     */
    public string AlgoId { get; set; }

    /**
     * @brief Orders routed to IBDARK are tagged as “post only” and are held in IB's order book, where incoming SmartRouted orders from other IB customers are eligible to trade against them.\n
     * <i>For IBDARK orders only.</i>
     */
    public bool NotHeld { get; set; }

    /**
     * @brief Advanced parameters for Smart combo routing. \n
     * These features are for both guaranteed and nonguaranteed combination orders routed to Smart, and are available based on combo type and order type.
     * SmartComboRoutingParams is similar to AlgoParams in that it makes use of tag/value pairs to add parameters to combo orders. \n
     * Make sure that you fully understand how Advanced Combo Routing works in TWS itself first: https://www.interactivebrokers.com/en/software/tws/usersguidebook/specializedorderentry/advanced_combo_routing.htm \n
     * The parameters cover the following capabilities:
     *  - Non-Guaranteed - Determine if the combo order is Guaranteed or Non-Guaranteed. \n
     *    Tag = NonGuaranteed \n
     *    Value = 0: The order is guaranteed \n
     *    Value = 1: The order is non-guaranteed \n
     * \n
     *  - Select Leg to Fill First - User can specify which leg to be executed first. \n
     *    Tag = LeginPrio \n
     *    Value = -1: No priority is assigned to either combo leg \n
     *    Value = 0: Priority is assigned to the first leg being added to the comboLeg \n
     *    Value = 1: Priority is assigned to the second leg being added to the comboLeg \n
     *    Note: The LeginPrio parameter can only be applied to two-legged combo. \n
     * \n
     *  - Maximum Leg-In Combo Size - Specify the maximum allowed leg-in size per segment \n
     *    Tag = MaxSegSize \n
     *    Value = Unit of combo size \n
     * \n
     *  - Do Not Start Next Leg-In if Previous Leg-In Did Not Finish - Specify whether or not the system should attempt to fill the next segment before the current segment fills. \n
     *    Tag = DontLeginNext \n
     *    Value = 0: Start next leg-in even if previous leg-in did not finish \n
     *    Value = 1: Do not start next leg-in if previous leg-in did not finish \n
     * \n
     *  - Price Condition - Combo order will be rejected or cancelled if the leg market price is outside of the specified price range [CondPriceMin, CondPriceMax] \n
     *    Tag = PriceCondConid: The ContractID of the combo leg to specify price condition on \n
     *    Value = The ContractID \n
     *    Tag = CondPriceMin: The lower price range of the price condition \n
     *    Value = The lower price \n
     *    Tag = CondPriceMax: The upper price range of the price condition \n
     *    Value = The upper price \n
     * \n
     */
    public List<TagValue> SmartComboRoutingParams { get; set; }

    /**
     * @brief List of Per-leg price following the same sequence combo legs are added. The combo price must be left unspecified when using per-leg prices.
     */
    public List<OrderComboLeg> OrderComboLegs { get; set; } = new List<OrderComboLeg>();

    /**
     * @brief <i>For internal use only. Use the default value XYZ.</i>
     */
    public List<TagValue> OrderMiscOptions { get; set; } = new List<TagValue>();

    /**
     * @brief Defines the start time of GTC orders.
     */
    public string ActiveStartTime { get; set; }

    /**
     * @brief Defines the stop time of GTC orders.
     */
    public string ActiveStopTime { get; set; }

    /**
     * @brief The list of scale orders. <i>Used for scale orders.</i>
     */
    public string ScaleTable { get; set; }

    /**
     * @brief Is used to place an order to a model. For example, "Technology" model can be used for tech stocks first created in TWS.
     */
    public string ModelCode { get; set; }

    /**
     * @brief This is a regulartory attribute that applies to all US Commodity (Futures) Exchanges, provided to allow client to comply with CFTC Tag 50 Rules
     */
    public string ExtOperator { get; set; }

    /**
     * @brief The native cash quantity.
     */
    public double CashQty { get; set; }

    /**
     * @brief Identifies a person as the responsible party for investment decisions within the firm. Orders covered by MiFID 2 (Markets in Financial Instruments Directive 2) must include either Mifid2DecisionMaker or Mifid2DecisionAlgo field (but not both). <i>Requires TWS 969+.</i>
     */
    public string Mifid2DecisionMaker { get; set; }

    /**
     * @brief Identifies the algorithm responsible for investment decisions within the firm. Orders covered under MiFID 2 must include either Mifid2DecisionMaker or Mifid2DecisionAlgo, but cannot have both. <i>Requires TWS 969+.</i>
     */
    public string Mifid2DecisionAlgo { get; set; }

    /**
     * @brief For MiFID 2 reporting; identifies a person as the responsible party for the execution of a transaction within the firm. <i>Requires TWS 969+.</i>
     */
    public string Mifid2ExecutionTrader { get; set; }

    /**
     * @brief For MiFID 2 reporting; identifies the algorithm responsible for the execution of a transaction within the firm. <i>Requires TWS 969+.</i>
     */
    public string Mifid2ExecutionAlgo { get; set; }

    /**
     * @brief Don't use auto price for hedge
     */
    public bool DontUseAutoPriceForHedge { get; set; }

    /**
     * @brief Specifies the date to auto cancel the order.
     */
    public string AutoCancelDate { get; set; }

    /**
     * @brief Specifies the initial order quantity to be filled.
     */
    public decimal FilledQuantity { get; set; }

    /**
     * @brief Identifies the reference future conId.
     */
    public int RefFuturesConId { get; set; }

    /**
     * @brief Cancels the parent order if child order was cancelled.
     */
    public bool AutoCancelParent { get; set; }

    /**
     * @brief Identifies the Shareholder.
     */
    public string Shareholder { get; set; }

    /**
     * @brief Used to specify <i>"imbalance only open orders"</i> or <i>"imbalance only closing orders".</i>
     */
    public bool ImbalanceOnly { get; set; }

    /**
     * @brief Routes market order to Best Bid Offer.
     */
    public bool RouteMarketableToBbo { get; set; }

    /**
     * @brief Parent order Id.
     */
    public long ParentPermId { get; set; }

    /**
     * @brief Accepts a list with parameters obtained from advancedOrderRejectJson.
     */
    public string AdvancedErrorOverride { get; set; }

    /**
     * @brief Used by brokers and advisors when manually entering, modifying or cancelling orders at the direction of a client.
     * <i>Only used when allocating orders to specific groups or accounts. Excluding "All" group.</i>
     */
    public string ManualOrderTime { get; set; }

    /**
     * @brief Defines the minimum trade quantity to fill. <i>For IBKRATS orders.</i>
     */
    public int MinTradeQty { get; set; }

    /**
     * @brief Defines the minimum size to compete. <i>For IBKRATS orders.</i>
     */
    public int MinCompeteSize { get; set; }

    /**
     * @brief Dpecifies the offset Off The Midpoint that will be applied to the order. <i>For IBKRATS orders.</i>
     */
    public double CompeteAgainstBestOffset { get; set; }

    /**
     * @brief This offset is applied when the spread is an even number of cents wide. This offset must be in whole-penny increments or zero. <i>For IBKRATS orders.</i>
     */
    public double MidOffsetAtWhole { get; set; }

    /**
     * @brief This offset is applied when the spread is an odd number of cents wide. This offset must be in half-penny increments. <i>For IBKRATS orders.</i>
     */
    public double MidOffsetAtHalf { get; set; }

    /**
     * @brief Customer account
     */
    public string CustomerAccount { get; set; }

    /**
     * @brief Professional customer
     */
    public bool ProfessionalCustomer { get; set; }

    /**
     * @brief Bond accrued interest
     */
    public string BondAccruedInterest { get; set; }

    public Order()
    {
      LmtPrice = double.MaxValue;
      AuxPrice = double.MaxValue;
      ActiveStartTime = EMPTY_STR;
      ActiveStopTime = EMPTY_STR;
      OutsideRth = false;
      OpenClose = EMPTY_STR;
      Origin = CUSTOMER;
      Transmit = true;
      DesignatedLocation = EMPTY_STR;
      ExemptCode = -1;
      MinQty = int.MaxValue;
      PercentOffset = double.MaxValue;
      OptOutSmartRouting = false;
      StartingPrice = double.MaxValue;
      StockRefPrice = double.MaxValue;
      Delta = double.MaxValue;
      StockRangeLower = double.MaxValue;
      StockRangeUpper = double.MaxValue;
      Volatility = double.MaxValue;
      VolatilityType = int.MaxValue;
      DeltaNeutralOrderType = EMPTY_STR;
      DeltaNeutralAuxPrice = double.MaxValue;
      DeltaNeutralConId = 0;
      DeltaNeutralSettlingFirm = EMPTY_STR;
      DeltaNeutralClearingAccount = EMPTY_STR;
      DeltaNeutralClearingIntent = EMPTY_STR;
      DeltaNeutralOpenClose = EMPTY_STR;
      DeltaNeutralShortSale = false;
      DeltaNeutralShortSaleSlot = 0;
      DeltaNeutralDesignatedLocation = EMPTY_STR;
      ReferencePriceType = int.MaxValue;
      TrailStopPrice = double.MaxValue;
      TrailingPercent = double.MaxValue;
      BasisPoints = double.MaxValue;
      BasisPointsType = int.MaxValue;
      ScaleInitLevelSize = int.MaxValue;
      ScaleSubsLevelSize = int.MaxValue;
      ScalePriceIncrement = double.MaxValue;
      ScalePriceAdjustValue = double.MaxValue;
      ScalePriceAdjustInterval = int.MaxValue;
      ScaleProfitOffset = double.MaxValue;
      ScaleAutoReset = false;
      ScaleInitPosition = int.MaxValue;
      ScaleInitFillQty = int.MaxValue;
      ScaleRandomPercent = false;
      ScaleTable = EMPTY_STR;
      WhatIf = false;
      NotHeld = false;
      Conditions = new List<OrderCondition>();
      TriggerPrice = double.MaxValue;
      LmtPriceOffset = double.MaxValue;
      AdjustedStopPrice = double.MaxValue;
      AdjustedStopLimitPrice = double.MaxValue;
      AdjustedTrailingAmount = double.MaxValue;
      ExtOperator = EMPTY_STR;
      Tier = new SoftDollarTier(EMPTY_STR, EMPTY_STR, EMPTY_STR);
      CashQty = double.MaxValue;
      Mifid2DecisionMaker = EMPTY_STR;
      Mifid2DecisionAlgo = EMPTY_STR;
      Mifid2ExecutionTrader = EMPTY_STR;
      Mifid2ExecutionAlgo = EMPTY_STR;
      DontUseAutoPriceForHedge = false;
      AutoCancelDate = EMPTY_STR;
      FilledQuantity = decimal.MaxValue;
      RefFuturesConId = int.MaxValue;
      AutoCancelParent = false;
      Shareholder = EMPTY_STR;
      ImbalanceOnly = false;
      RouteMarketableToBbo = false;
      ParentPermId = long.MaxValue;
      UsePriceMgmtAlgo = null;
      Duration = int.MaxValue;
      PostToAts = int.MaxValue;
      AdvancedErrorOverride = EMPTY_STR;
      ManualOrderTime = EMPTY_STR;
      MinTradeQty = int.MaxValue;
      MinCompeteSize = int.MaxValue;
      CompeteAgainstBestOffset = double.MaxValue;
      MidOffsetAtWhole = double.MaxValue;
      MidOffsetAtHalf = double.MaxValue;
      CustomerAccount = EMPTY_STR;
      ProfessionalCustomer = false;
      BondAccruedInterest = EMPTY_STR;
    }

    // Note: Two orders can be 'equivalent' even if all fields do not match. This function is not intended to be used with Order objects returned from TWS.
    public override bool Equals(object p_other)
    {
      if (this == p_other)
        return true;


      if (!(p_other is Order l_theOther))
        return false;

      if (PermId == l_theOther.PermId)
      {
        return true;
      }

      if (OrderId != l_theOther.OrderId ||
          ClientId != l_theOther.ClientId ||
          TotalQuantity != l_theOther.TotalQuantity ||
          LmtPrice != l_theOther.LmtPrice ||
          AuxPrice != l_theOther.AuxPrice ||
          OcaType != l_theOther.OcaType ||
          Transmit != l_theOther.Transmit ||
          ParentId != l_theOther.ParentId ||
          BlockOrder != l_theOther.BlockOrder ||
          SweepToFill != l_theOther.SweepToFill ||
          DisplaySize != l_theOther.DisplaySize ||
          TriggerMethod != l_theOther.TriggerMethod ||
          OutsideRth != l_theOther.OutsideRth ||
          Hidden != l_theOther.Hidden ||
          OverridePercentageConstraints != l_theOther.OverridePercentageConstraints ||
          AllOrNone != l_theOther.AllOrNone ||
          MinQty != l_theOther.MinQty ||
          PercentOffset != l_theOther.PercentOffset ||
          TrailStopPrice != l_theOther.TrailStopPrice ||
          TrailingPercent != l_theOther.TrailingPercent ||
          Origin != l_theOther.Origin ||
          ShortSaleSlot != l_theOther.ShortSaleSlot ||
          DiscretionaryAmt != l_theOther.DiscretionaryAmt ||
          OptOutSmartRouting != l_theOther.OptOutSmartRouting ||
          AuctionStrategy != l_theOther.AuctionStrategy ||
          StartingPrice != l_theOther.StartingPrice ||
          StockRefPrice != l_theOther.StockRefPrice ||
          Delta != l_theOther.Delta ||
          StockRangeLower != l_theOther.StockRangeLower ||
          StockRangeUpper != l_theOther.StockRangeUpper ||
          Volatility != l_theOther.Volatility ||
          VolatilityType != l_theOther.VolatilityType ||
          ContinuousUpdate != l_theOther.ContinuousUpdate ||
          ReferencePriceType != l_theOther.ReferencePriceType ||
          DeltaNeutralAuxPrice != l_theOther.DeltaNeutralAuxPrice ||
          DeltaNeutralConId != l_theOther.DeltaNeutralConId ||
          DeltaNeutralShortSale != l_theOther.DeltaNeutralShortSale ||
          DeltaNeutralShortSaleSlot != l_theOther.DeltaNeutralShortSaleSlot ||
          BasisPoints != l_theOther.BasisPoints ||
          BasisPointsType != l_theOther.BasisPointsType ||
          ScaleInitLevelSize != l_theOther.ScaleInitLevelSize ||
          ScaleSubsLevelSize != l_theOther.ScaleSubsLevelSize ||
          ScalePriceIncrement != l_theOther.ScalePriceIncrement ||
          ScalePriceAdjustValue != l_theOther.ScalePriceAdjustValue ||
          ScalePriceAdjustInterval != l_theOther.ScalePriceAdjustInterval ||
          ScaleProfitOffset != l_theOther.ScaleProfitOffset ||
          ScaleAutoReset != l_theOther.ScaleAutoReset ||
          ScaleInitPosition != l_theOther.ScaleInitPosition ||
          ScaleInitFillQty != l_theOther.ScaleInitFillQty ||
          ScaleRandomPercent != l_theOther.ScaleRandomPercent ||
          WhatIf != l_theOther.WhatIf ||
          NotHeld != l_theOther.NotHeld ||
          ExemptCode != l_theOther.ExemptCode ||
          RandomizePrice != l_theOther.RandomizePrice ||
          RandomizeSize != l_theOther.RandomizeSize ||
          Solicited != l_theOther.Solicited ||
          ConditionsIgnoreRth != l_theOther.ConditionsIgnoreRth ||
          ConditionsCancelOrder != l_theOther.ConditionsCancelOrder ||
          Tier != l_theOther.Tier ||
          CashQty != l_theOther.CashQty ||
          DontUseAutoPriceForHedge != l_theOther.DontUseAutoPriceForHedge ||
          IsOmsContainer != l_theOther.IsOmsContainer ||
          UsePriceMgmtAlgo != l_theOther.UsePriceMgmtAlgo ||
          FilledQuantity != l_theOther.FilledQuantity ||
          RefFuturesConId != l_theOther.RefFuturesConId ||
          AutoCancelParent != l_theOther.AutoCancelParent ||
          ImbalanceOnly != l_theOther.ImbalanceOnly ||
          RouteMarketableToBbo != l_theOther.RouteMarketableToBbo ||
          ParentPermId != l_theOther.ParentPermId ||
          Duration != l_theOther.Duration ||
          PostToAts != l_theOther.PostToAts ||
          MinTradeQty != l_theOther.MinTradeQty ||
          MinCompeteSize != l_theOther.MinCompeteSize ||
          CompeteAgainstBestOffset != l_theOther.CompeteAgainstBestOffset ||
          MidOffsetAtWhole != l_theOther.MidOffsetAtWhole ||
          MidOffsetAtHalf != l_theOther.MidOffsetAtHalf ||
          ProfessionalCustomer != l_theOther.ProfessionalCustomer)
      {
        return false;
      }

      if (Util.StringCompare(Action, l_theOther.Action) != 0 ||
          Util.StringCompare(OrderType, l_theOther.OrderType) != 0 ||
          Util.StringCompare(Tif, l_theOther.Tif) != 0 ||
          Util.StringCompare(ActiveStartTime, l_theOther.ActiveStartTime) != 0 ||
          Util.StringCompare(ActiveStopTime, l_theOther.ActiveStopTime) != 0 ||
          Util.StringCompare(OcaGroup, l_theOther.OcaGroup) != 0 ||
          Util.StringCompare(OrderRef, l_theOther.OrderRef) != 0 ||
          Util.StringCompare(GoodAfterTime, l_theOther.GoodAfterTime) != 0 ||
          Util.StringCompare(GoodTillDate, l_theOther.GoodTillDate) != 0 ||
          Util.StringCompare(Rule80A, l_theOther.Rule80A) != 0 ||
          Util.StringCompare(FaGroup, l_theOther.FaGroup) != 0 ||
          Util.StringCompare(FaMethod, l_theOther.FaMethod) != 0 ||
          Util.StringCompare(FaPercentage, l_theOther.FaPercentage) != 0 ||
          Util.StringCompare(OpenClose, l_theOther.OpenClose) != 0 ||
          Util.StringCompare(DesignatedLocation, l_theOther.DesignatedLocation) != 0 ||
          Util.StringCompare(DeltaNeutralOrderType, l_theOther.DeltaNeutralOrderType) != 0 ||
          Util.StringCompare(DeltaNeutralSettlingFirm, l_theOther.DeltaNeutralSettlingFirm) != 0 ||
          Util.StringCompare(DeltaNeutralClearingAccount, l_theOther.DeltaNeutralClearingAccount) != 0 ||
          Util.StringCompare(DeltaNeutralClearingIntent, l_theOther.DeltaNeutralClearingIntent) != 0 ||
          Util.StringCompare(DeltaNeutralOpenClose, l_theOther.DeltaNeutralOpenClose) != 0 ||
          Util.StringCompare(DeltaNeutralDesignatedLocation, l_theOther.DeltaNeutralDesignatedLocation) != 0 ||
          Util.StringCompare(HedgeType, l_theOther.HedgeType) != 0 ||
          Util.StringCompare(HedgeParam, l_theOther.HedgeParam) != 0 ||
          Util.StringCompare(Account, l_theOther.Account) != 0 ||
          Util.StringCompare(SettlingFirm, l_theOther.SettlingFirm) != 0 ||
          Util.StringCompare(ClearingAccount, l_theOther.ClearingAccount) != 0 ||
          Util.StringCompare(ClearingIntent, l_theOther.ClearingIntent) != 0 ||
          Util.StringCompare(AlgoStrategy, l_theOther.AlgoStrategy) != 0 ||
          Util.StringCompare(AlgoId, l_theOther.AlgoId) != 0 ||
          Util.StringCompare(ScaleTable, l_theOther.ScaleTable) != 0 ||
          Util.StringCompare(ModelCode, l_theOther.ModelCode) != 0 ||
          Util.StringCompare(ExtOperator, l_theOther.ExtOperator) != 0 ||
          Util.StringCompare(AutoCancelDate, l_theOther.AutoCancelDate) != 0 ||
          Util.StringCompare(Shareholder, l_theOther.Shareholder) != 0 ||
          Util.StringCompare(AdvancedErrorOverride, l_theOther.AdvancedErrorOverride) != 0 ||
          Util.StringCompare(ManualOrderTime, l_theOther.ManualOrderTime) != 0 ||
          Util.StringCompare(CustomerAccount, l_theOther.CustomerAccount) != 0 ||
          Util.StringCompare(BondAccruedInterest, l_theOther.BondAccruedInterest) != 0)
      {
        return false;
      }

      if (!Util.VectorEqualsUnordered(AlgoParams, l_theOther.AlgoParams))
      {
        return false;
      }

      if (!Util.VectorEqualsUnordered(SmartComboRoutingParams, l_theOther.SmartComboRoutingParams))
      {
        return false;
      }

      // compare order combo legs
      if (!Util.VectorEqualsUnordered(OrderComboLegs, l_theOther.OrderComboLegs))
      {
        return false;
      }

      return true;
    }

    public override int GetHashCode()
    {
      var hashCode = 1040337091;
      hashCode *= -1521134295 + OrderId.GetHashCode();
      hashCode *= -1521134295 + Solicited.GetHashCode();
      hashCode *= -1521134295 + ClientId.GetHashCode();
      hashCode *= -1521134295 + PermId.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Action);
      hashCode *= -1521134295 + TotalQuantity.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(OrderType);
      hashCode *= -1521134295 + LmtPrice.GetHashCode();
      hashCode *= -1521134295 + AuxPrice.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Tif);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(OcaGroup);
      hashCode *= -1521134295 + OcaType.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(OrderRef);
      hashCode *= -1521134295 + Transmit.GetHashCode();
      hashCode *= -1521134295 + ParentId.GetHashCode();
      hashCode *= -1521134295 + BlockOrder.GetHashCode();
      hashCode *= -1521134295 + SweepToFill.GetHashCode();
      hashCode *= -1521134295 + DisplaySize.GetHashCode();
      hashCode *= -1521134295 + TriggerMethod.GetHashCode();
      hashCode *= -1521134295 + OutsideRth.GetHashCode();
      hashCode *= -1521134295 + Hidden.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(GoodAfterTime);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(GoodTillDate);
      hashCode *= -1521134295 + OverridePercentageConstraints.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Rule80A);
      hashCode *= -1521134295 + AllOrNone.GetHashCode();
      hashCode *= -1521134295 + MinQty.GetHashCode();
      hashCode *= -1521134295 + PercentOffset.GetHashCode();
      hashCode *= -1521134295 + TrailStopPrice.GetHashCode();
      hashCode *= -1521134295 + TrailingPercent.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(FaGroup);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(FaMethod);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(FaPercentage);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(OpenClose);
      hashCode *= -1521134295 + Origin.GetHashCode();
      hashCode *= -1521134295 + ShortSaleSlot.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(DesignatedLocation);
      hashCode *= -1521134295 + ExemptCode.GetHashCode();
      hashCode *= -1521134295 + DiscretionaryAmt.GetHashCode();
      hashCode *= -1521134295 + OptOutSmartRouting.GetHashCode();
      hashCode *= -1521134295 + AuctionStrategy.GetHashCode();
      hashCode *= -1521134295 + StartingPrice.GetHashCode();
      hashCode *= -1521134295 + StockRefPrice.GetHashCode();
      hashCode *= -1521134295 + Delta.GetHashCode();
      hashCode *= -1521134295 + StockRangeLower.GetHashCode();
      hashCode *= -1521134295 + StockRangeUpper.GetHashCode();
      hashCode *= -1521134295 + Volatility.GetHashCode();
      hashCode *= -1521134295 + VolatilityType.GetHashCode();
      hashCode *= -1521134295 + ContinuousUpdate.GetHashCode();
      hashCode *= -1521134295 + ReferencePriceType.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeltaNeutralOrderType);
      hashCode *= -1521134295 + DeltaNeutralAuxPrice.GetHashCode();
      hashCode *= -1521134295 + DeltaNeutralConId.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeltaNeutralSettlingFirm);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeltaNeutralClearingAccount);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeltaNeutralClearingIntent);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeltaNeutralOpenClose);
      hashCode *= -1521134295 + DeltaNeutralShortSale.GetHashCode();
      hashCode *= -1521134295 + DeltaNeutralShortSaleSlot.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeltaNeutralDesignatedLocation);
      hashCode *= -1521134295 + BasisPoints.GetHashCode();
      hashCode *= -1521134295 + BasisPointsType.GetHashCode();
      hashCode *= -1521134295 + ScaleInitLevelSize.GetHashCode();
      hashCode *= -1521134295 + ScaleSubsLevelSize.GetHashCode();
      hashCode *= -1521134295 + ScalePriceIncrement.GetHashCode();
      hashCode *= -1521134295 + ScalePriceAdjustValue.GetHashCode();
      hashCode *= -1521134295 + ScalePriceAdjustInterval.GetHashCode();
      hashCode *= -1521134295 + ScaleProfitOffset.GetHashCode();
      hashCode *= -1521134295 + ScaleAutoReset.GetHashCode();
      hashCode *= -1521134295 + ScaleInitPosition.GetHashCode();
      hashCode *= -1521134295 + ScaleInitFillQty.GetHashCode();
      hashCode *= -1521134295 + ScaleRandomPercent.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(HedgeType);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(HedgeParam);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Account);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(SettlingFirm);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClearingAccount);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClearingIntent);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlgoStrategy);
      hashCode *= -1521134295 + EqualityComparer<List<TagValue>>.Default.GetHashCode(AlgoParams);
      hashCode *= -1521134295 + WhatIf.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlgoId);
      hashCode *= -1521134295 + NotHeld.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<List<TagValue>>.Default.GetHashCode(SmartComboRoutingParams);
      hashCode *= -1521134295 + EqualityComparer<List<OrderComboLeg>>.Default.GetHashCode(OrderComboLegs);
      hashCode *= -1521134295 + EqualityComparer<List<TagValue>>.Default.GetHashCode(OrderMiscOptions);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ActiveStartTime);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ActiveStopTime);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ScaleTable);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ModelCode);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExtOperator);
      hashCode *= -1521134295 + CashQty.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Mifid2DecisionMaker);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Mifid2DecisionAlgo);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Mifid2ExecutionTrader);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Mifid2ExecutionAlgo);
      hashCode *= -1521134295 + DontUseAutoPriceForHedge.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(AutoCancelDate);
      hashCode *= -1521134295 + FilledQuantity.GetHashCode();
      hashCode *= -1521134295 + RefFuturesConId.GetHashCode();
      hashCode *= -1521134295 + AutoCancelParent.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Shareholder);
      hashCode *= -1521134295 + ImbalanceOnly.GetHashCode();
      hashCode *= -1521134295 + RouteMarketableToBbo.GetHashCode();
      hashCode *= -1521134295 + ParentPermId.GetHashCode();
      hashCode *= -1521134295 + RandomizeSize.GetHashCode();
      hashCode *= -1521134295 + RandomizePrice.GetHashCode();
      hashCode *= -1521134295 + ReferenceContractId.GetHashCode();
      hashCode *= -1521134295 + IsPeggedChangeAmountDecrease.GetHashCode();
      hashCode *= -1521134295 + PeggedChangeAmount.GetHashCode();
      hashCode *= -1521134295 + ReferenceChangeAmount.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReferenceExchange);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(AdjustedOrderType);
      hashCode *= -1521134295 + TriggerPrice.GetHashCode();
      hashCode *= -1521134295 + LmtPriceOffset.GetHashCode();
      hashCode *= -1521134295 + AdjustedStopPrice.GetHashCode();
      hashCode *= -1521134295 + AdjustedStopLimitPrice.GetHashCode();
      hashCode *= -1521134295 + AdjustedTrailingAmount.GetHashCode();
      hashCode *= -1521134295 + AdjustableTrailingUnit.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<List<OrderCondition>>.Default.GetHashCode(Conditions);
      hashCode *= -1521134295 + ConditionsIgnoreRth.GetHashCode();
      hashCode *= -1521134295 + ConditionsCancelOrder.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<SoftDollarTier>.Default.GetHashCode(Tier);
      hashCode *= -1521134295 + IsOmsContainer.GetHashCode();
      hashCode *= -1521134295 + DiscretionaryUpToLimitPrice.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(UsePriceMgmtAlgo);
      hashCode *= -1521134295 + Duration.GetHashCode();
      hashCode *= -1521134295 + PostToAts.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(AdvancedErrorOverride);
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ManualOrderTime);
      hashCode *= -1521134295 + MinTradeQty.GetHashCode();
      hashCode *= -1521134295 + MinCompeteSize.GetHashCode();
      hashCode *= -1521134295 + CompeteAgainstBestOffset.GetHashCode();
      hashCode *= -1521134295 + MidOffsetAtWhole.GetHashCode();
      hashCode *= -1521134295 + MidOffsetAtHalf.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(CustomerAccount);
      hashCode *= -1521134295 + ProfessionalCustomer.GetHashCode();
      hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(BondAccruedInterest);

      return hashCode;
    }

    /**
     * @brief Randomizes the order's size. <i>Only for Volatility and Pegged to Volatility orders.</i>
     */
    public bool RandomizeSize { get; set; }

    /**
     * @brief Randomizes the order's price. <i>Only for Volatility and Pegged to Volatility orders.</i>
     */
    public bool RandomizePrice { get; set; }

    /**
     * @brief Pegged-to-benchmark orders: this attribute will contain the conId of the contract against which the order will be pegged.
     */
    public int ReferenceContractId { get; set; }

    /**
     * @brief Pegged-to-benchmark orders: indicates whether the order's pegged price should increase or decreases.
     */
    public bool IsPeggedChangeAmountDecrease { get; set; }

    /**
     * @brief Pegged-to-benchmark orders: amount by which the order's pegged price should move.
     */
    public double PeggedChangeAmount { get; set; }

    /**
     * @brief Pegged-to-benchmark orders: the amount the reference contract needs to move to adjust the pegged order.
     */
    public double ReferenceChangeAmount { get; set; }

    /**
     * @brief Pegged-to-benchmark orders: the exchange against which we want to observe the reference contract.
     */
    public string ReferenceExchange { get; set; }

    /**
     * @brief Adjusted Stop orders: the parent order will be adjusted to the given type when the adjusted trigger price is penetrated.
     */
    public string AdjustedOrderType { get; set; }

    /**
     * @brief Adjusted Stop orders: specifies the trigger price to execute.
     */
    public double TriggerPrice { get; set; }

    /**
     * @brief Adjusted Stop orders: specifies the price offset for the stop to move in increments.
     */
    public double LmtPriceOffset { get; set; }

    /**
     * @brief Adjusted Stop orders: specifies the stop price of the adjusted (STP) parent
     */
    public double AdjustedStopPrice { get; set; }

    /**
     * @brief Adjusted Stop orders: specifies the stop limit price of the adjusted (STPL LMT) parent
     */
    public double AdjustedStopLimitPrice { get; set; }

    /**
     * @brief Adjusted Stop orders: specifies the trailing amount of the adjusted (TRAIL) parent
     */
    public double AdjustedTrailingAmount { get; set; }

    /**
     * @brief Adjusted Stop orders: specifies where the trailing unit is an amount (set to 0) or a percentage (set to 1)
     */
    public int AdjustableTrailingUnit { get; set; }

    /**
     * @brief Conditions determining when the order will be activated or canceled
     */
    public List<OrderCondition> Conditions { get; set; }
    /**
     * @brief Indicates whether or not conditions will also be valid outside Regular Trading Hours
     */
    public bool ConditionsIgnoreRth { get; set; }

    /**
     * @brief Conditions can determine if an order should become active or canceled.
     */
    public bool ConditionsCancelOrder { get; set; }

    /**
    * @brief Define the Soft Dollar Tier used for the order. Only provided for registered professional advisors and hedge and mutual funds.
    */
    public SoftDollarTier Tier { get; set; }

    /**
     * @brief Set to true to create tickets from API orders when TWS is used as an OMS
     */
    public bool IsOmsContainer { get; set; }

    /**
     * @brief Set to true to convert order of type 'Primary Peg' to 'D-Peg'
     */
    public bool DiscretionaryUpToLimitPrice { get; set; }

    /**
     * @brief Specifies wether to use Price Management Algo. <i>CTCI users only.</i>
     */
    public bool? UsePriceMgmtAlgo { get; set; }

    /**
     * @brief Specifies the duration of the order. Format: yyyymmdd hh:mm:ss TZ. <i>For GTD orders.</i>
     */
    public int Duration { get; set; }

    /**
     * @brief Value must be positive, and it is number of seconds that SMART order would be parked for at IBKRATS before being routed to exchange.
     */
    public int PostToAts { get; set; }
  }
}
