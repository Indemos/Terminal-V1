/* Copyright (C) 2024 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace IBApi
{
    internal class EOrderDecoder
    {
        private readonly EDecoder eDecoder;
        private readonly Contract contract;
        private readonly Order order;
        private readonly OrderState orderState;
        private readonly int msgVersion;
        private readonly int serverVersion;

        public EOrderDecoder(EDecoder eDecoder, Contract contract, Order order, OrderState orderState, int msgVersion, int serverVersion)
        {
            this.eDecoder = eDecoder;
            this.contract = contract;
            this.order = order;
            this.orderState = orderState;
            this.msgVersion = msgVersion;
            this.serverVersion = serverVersion;
        }

        public void readOrderId() => order.OrderId = eDecoder.ReadInt();

        public void readAction() => order.Action = eDecoder.ReadString();

        public void readContractFields()
        {
            if (msgVersion >= 17)
            {
                contract.ConId = eDecoder.ReadInt();
            }
            contract.Symbol = eDecoder.ReadString();
            contract.SecType = eDecoder.ReadString();
            contract.LastTradeDateOrContractMonth = eDecoder.ReadString();
            contract.Strike = eDecoder.ReadDouble();
            contract.Right = eDecoder.ReadString();
            if (msgVersion >= 32)
            {
                contract.Multiplier = eDecoder.ReadString();
            }
            contract.Exchange = eDecoder.ReadString();
            contract.Currency = eDecoder.ReadString();
            if (msgVersion >= 2)
            {
                contract.LocalSymbol = eDecoder.ReadString();
            }
            if (msgVersion >= 32)
            {
                contract.TradingClass = eDecoder.ReadString();
            }
        }

        public void readTotalQuantity() => order.TotalQuantity = eDecoder.ReadDecimal();

        public void readOrderType() => order.OrderType = eDecoder.ReadString();

        public void readLmtPrice()
        {
            if (msgVersion < 29)
            {
                order.LmtPrice = eDecoder.ReadDouble();
            }
            else
            {
                order.LmtPrice = eDecoder.ReadDoubleMax();
            }
        }

        public void readAuxPrice()
        {
            if (msgVersion < 30)
            {
                order.AuxPrice = eDecoder.ReadDouble();
            }
            else
            {
                order.AuxPrice = eDecoder.ReadDoubleMax();
            }
        }

        public void readTIF() => order.Tif = eDecoder.ReadString();

        public void readOcaGroup() => order.OcaGroup = eDecoder.ReadString();

        public void readAccount() => order.Account = eDecoder.ReadString();

        public void readOpenClose() => order.OpenClose = eDecoder.ReadString();

        public void readOrigin() => order.Origin = eDecoder.ReadInt();

        public void readOrderRef() => order.OrderRef = eDecoder.ReadString();

        public void readClientId()
        {
            if (msgVersion >= 3)
            {
                order.ClientId = eDecoder.ReadInt();
            }
        }

        public void readPermId()
        {
            if (msgVersion >= 4)
            {
                order.PermId = eDecoder.ReadInt();
            }
        }

        public void readOutsideRth()
        {
            if (msgVersion >= 4)
            {
                if (msgVersion < 18)
                {
                    // will never happen
                    /* order.ignoreRth = */
                    eDecoder.ReadBoolFromInt();
                }
                else
                {
                    order.OutsideRth = eDecoder.ReadBoolFromInt();
                }
            }
        }

        public void readHidden()
        {
            if (msgVersion >= 4)
            {
                order.Hidden = eDecoder.ReadInt() == 1;
            }
        }

        public void readDiscretionaryAmount()
        {
            if (msgVersion >= 4)
            {
                order.DiscretionaryAmt = eDecoder.ReadDouble();
            }
        }

        public void readGoodAfterTime()
        {
            if (msgVersion >= 5)
            {
                order.GoodAfterTime = eDecoder.ReadString();
            }
        }

        public void skipSharesAllocation()
        {
            if (msgVersion >= 6)
            {
                // skip deprecated sharesAllocation field
                eDecoder.ReadString();
            }
        }


        public void readFAParams()
        {
            if (msgVersion >= 7)
            {
                order.FaGroup = eDecoder.ReadString();
                order.FaMethod = eDecoder.ReadString();
                order.FaPercentage = eDecoder.ReadString();
                if (serverVersion < MinServerVer.MIN_SERVER_VER_FA_PROFILE_DESUPPORT)
                {
                    eDecoder.ReadString(); // skip deprecated faProfile field
                }
            }
        }

        public void readModelCode()
        {
            if (serverVersion >= MinServerVer.MODELS_SUPPORT)
            {
                order.ModelCode = eDecoder.ReadString();
            }
        }

        public void readGoodTillDate()
        {
            if (msgVersion >= 8)
            {
                order.GoodTillDate = eDecoder.ReadString();
            }
        }

        public void readRule80A()
        {
            if (msgVersion >= 9)
            {
                order.Rule80A = eDecoder.ReadString();
            }
        }

        public void readPercentOffset()
        {
            if (msgVersion >= 9)
            {
                order.PercentOffset = eDecoder.ReadDoubleMax();
            }
        }

        public void readSettlingFirm()
        {
            if (msgVersion >= 9)
            {
                order.SettlingFirm = eDecoder.ReadString();
            }
        }

        public void readShortSaleParams()
        {
            if (msgVersion >= 9)
            {
                order.ShortSaleSlot = eDecoder.ReadInt();
                order.DesignatedLocation = eDecoder.ReadString();
                if (serverVersion == 51)
                {
                    eDecoder.ReadInt(); // exemptCode
                }
                else if (msgVersion >= 23)
                {
                    order.ExemptCode = eDecoder.ReadInt();
                }
            }
        }

        public void readAuctionStrategy()
        {
            if (msgVersion >= 9)
            {
                order.AuctionStrategy = eDecoder.ReadInt();
            }
        }

        public void readBoxOrderParams()
        {
            if (msgVersion >= 9)
            {
                order.StartingPrice = eDecoder.ReadDoubleMax();
                order.StockRefPrice = eDecoder.ReadDoubleMax();
                order.Delta = eDecoder.ReadDoubleMax();
            }
        }

        public void readPegToStkOrVolOrderParams()
        {
            if (msgVersion >= 9)
            {
                order.StockRangeLower = eDecoder.ReadDoubleMax();
                order.StockRangeUpper = eDecoder.ReadDoubleMax();
            }
        }

        public void readDisplaySize()
        {
            if (msgVersion >= 9)
            {
                order.DisplaySize = eDecoder.ReadIntMax();
            }
        }

        public void readOldStyleOutsideRth()
        {
            if (msgVersion >= 9)
            {
                if (msgVersion < 18)
                {
                    // will never happen
                    /* order.rthOnly = */
                    eDecoder.ReadBoolFromInt();
                }
            }
        }

        public void readBlockOrder()
        {
            if (msgVersion >= 9)
            {
                order.BlockOrder = eDecoder.ReadBoolFromInt();
            }
        }

        public void readSweepToFill()
        {
            if (msgVersion >= 9)
            {
                order.SweepToFill = eDecoder.ReadBoolFromInt();
            }
        }

        public void readAllOrNone()
        {
            if (msgVersion >= 9)
            {
                order.AllOrNone = eDecoder.ReadBoolFromInt();
            }
        }

        public void readMinQty()
        {
            if (msgVersion >= 9)
            {
                order.MinQty = eDecoder.ReadIntMax();
            }
        }

        public void readOcaType()
        {
            if (msgVersion >= 9)
            {
                order.OcaType = eDecoder.ReadInt();
            }
        }

        public void skipETradeOnly()
        {
            if (msgVersion >= 9)
            {
                eDecoder.ReadBoolFromInt();
            }
        }

        public void skipFirmQuoteOnly()
        {
            if (msgVersion >= 9)
            {
                eDecoder.ReadBoolFromInt();
            }
        }

        public void skipNbboPriceCap()
        {
            if (msgVersion >= 9)
            {
                eDecoder.ReadDoubleMax();
            }
        }

        public void readParentId()
        {
            if (msgVersion >= 10)
            {
                order.ParentId = eDecoder.ReadInt();
            }
        }

        public void readTriggerMethod()
        {
            if (msgVersion >= 10)
            {
                order.TriggerMethod = eDecoder.ReadInt();
            }
        }

        public void readVolOrderParams(bool readOpenOrderAttribs)
        {
            if (msgVersion >= 11)
            {
                order.Volatility = eDecoder.ReadDoubleMax();
                order.VolatilityType = eDecoder.ReadInt();
                if (msgVersion == 11)
                {
                    var receivedInt = eDecoder.ReadInt();
                    order.DeltaNeutralOrderType = receivedInt == 0 ? "NONE" : "MKT";
                }
                else
                { // msgVersion 12 and up
                    order.DeltaNeutralOrderType = eDecoder.ReadString();
                    order.DeltaNeutralAuxPrice = eDecoder.ReadDoubleMax();

                    if (msgVersion >= 27 && !Util.StringIsEmpty(order.DeltaNeutralOrderType))
                    {
                        order.DeltaNeutralConId = eDecoder.ReadInt();
                        if (readOpenOrderAttribs)
                        {
                            order.DeltaNeutralSettlingFirm = eDecoder.ReadString();
                            order.DeltaNeutralClearingAccount = eDecoder.ReadString();
                            order.DeltaNeutralClearingIntent = eDecoder.ReadString();
                        }
                    }

                    if (msgVersion >= 31 && !Util.StringIsEmpty(order.DeltaNeutralOrderType))
                    {
                        if (readOpenOrderAttribs)
                        {
                            order.DeltaNeutralOpenClose = eDecoder.ReadString();
                        }
                        order.DeltaNeutralShortSale = eDecoder.ReadBoolFromInt();
                        order.DeltaNeutralShortSaleSlot = eDecoder.ReadInt();
                        order.DeltaNeutralDesignatedLocation = eDecoder.ReadString();
                    }
                }
                order.ContinuousUpdate = eDecoder.ReadInt();
                if (serverVersion == 26)
                {
                    order.StockRangeLower = eDecoder.ReadDouble();
                    order.StockRangeUpper = eDecoder.ReadDouble();
                }
                order.ReferencePriceType = eDecoder.ReadInt();
            }
        }

        public void readTrailParams()
        {
            if (msgVersion >= 13)
            {
                order.TrailStopPrice = eDecoder.ReadDoubleMax();
            }
            if (msgVersion >= 30)
            {
                order.TrailingPercent = eDecoder.ReadDoubleMax();
            }
        }

        public void readBasisPoints()
        {
            if (msgVersion >= 14)
            {
                order.BasisPoints = eDecoder.ReadDoubleMax();
                order.BasisPointsType = eDecoder.ReadIntMax();
            }
        }

        public void readComboLegs()
        {
            if (msgVersion >= 14)
            {
                contract.ComboLegsDescription = eDecoder.ReadString();
            }

            if (msgVersion >= 29)
            {
                var comboLegsCount = eDecoder.ReadInt();
                if (comboLegsCount > 0)
                {
                    contract.ComboLegs = new List<ComboLeg>(comboLegsCount);
                    for (var i = 0; i < comboLegsCount; ++i)
                    {
                        var conId = eDecoder.ReadInt();
                        var ratio = eDecoder.ReadInt();
                        var action = eDecoder.ReadString();
                        var exchange = eDecoder.ReadString();
                        var openClose = eDecoder.ReadInt();
                        var shortSaleSlot = eDecoder.ReadInt();
                        var designatedLocation = eDecoder.ReadString();
                        var exemptCode = eDecoder.ReadInt();

                        var comboLeg = new ComboLeg(conId, ratio, action, exchange, openClose, shortSaleSlot, designatedLocation, exemptCode);
                        contract.ComboLegs.Add(comboLeg);
                    }
                }

                var orderComboLegsCount = eDecoder.ReadInt();
                if (orderComboLegsCount > 0)
                {
                    order.OrderComboLegs = new List<OrderComboLeg>(orderComboLegsCount);
                    for (var i = 0; i < orderComboLegsCount; ++i)
                    {
                        var price = eDecoder.ReadDoubleMax();

                        var orderComboLeg = new OrderComboLeg(price);
                        order.OrderComboLegs.Add(orderComboLeg);
                    }
                }
            }
        }

        public void readSmartComboRoutingParams()
        {
            if (msgVersion >= 26)
            {
                var smartComboRoutingParamsCount = eDecoder.ReadInt();
                if (smartComboRoutingParamsCount > 0)
                {
                    order.SmartComboRoutingParams = new List<TagValue>(smartComboRoutingParamsCount);
                    for (var i = 0; i < smartComboRoutingParamsCount; ++i)
                    {
                        var tagValue = new TagValue
                        {
                            Tag = eDecoder.ReadString(),
                            Value = eDecoder.ReadString()
                        };
                        order.SmartComboRoutingParams.Add(tagValue);
                    }
                }
            }
        }

        public void readScaleOrderParams()
        {
            if (msgVersion >= 15)
            {
                if (msgVersion >= 20)
                {
                    order.ScaleInitLevelSize = eDecoder.ReadIntMax();
                    order.ScaleSubsLevelSize = eDecoder.ReadIntMax();
                }
                else
                {
                    /* int notSuppScaleNumComponents = */
                    eDecoder.ReadIntMax();
                    order.ScaleInitLevelSize = eDecoder.ReadIntMax();
                }
                order.ScalePriceIncrement = eDecoder.ReadDoubleMax();
            }

            if (msgVersion >= 28 && order.ScalePriceIncrement > 0.0 && order.ScalePriceIncrement != double.MaxValue)
            {
                order.ScalePriceAdjustValue = eDecoder.ReadDoubleMax();
                order.ScalePriceAdjustInterval = eDecoder.ReadIntMax();
                order.ScaleProfitOffset = eDecoder.ReadDoubleMax();
                order.ScaleAutoReset = eDecoder.ReadBoolFromInt();
                order.ScaleInitPosition = eDecoder.ReadIntMax();
                order.ScaleInitFillQty = eDecoder.ReadIntMax();
                order.ScaleRandomPercent = eDecoder.ReadBoolFromInt();
            }
        }

        public void readHedgeParams()
        {
            if (msgVersion >= 24)
            {
                order.HedgeType = eDecoder.ReadString();
                if (!Util.StringIsEmpty(order.HedgeType))
                {
                    order.HedgeParam = eDecoder.ReadString();
                }
            }
        }

        public void readOptOutSmartRouting()
        {
            if (msgVersion >= 25)
            {
                order.OptOutSmartRouting = eDecoder.ReadBoolFromInt();
            }
        }

        public void readClearingParams()
        {
            if (msgVersion >= 19)
            {
                order.ClearingAccount = eDecoder.ReadString();
                order.ClearingIntent = eDecoder.ReadString();
            }
        }

        public void readNotHeld()
        {
            if (msgVersion >= 22)
            {
                order.NotHeld = eDecoder.ReadBoolFromInt();
            }
        }

        public void readDeltaNeutral()
        {
            if (msgVersion >= 20)
            {
                if (eDecoder.ReadBoolFromInt())
                {
                    var deltaNeutralContract = new DeltaNeutralContract
                    {
                        ConId = eDecoder.ReadInt(),
                        Delta = eDecoder.ReadDouble(),
                        Price = eDecoder.ReadDouble()
                    };
                    contract.DeltaNeutralContract = deltaNeutralContract;
                }
            }
        }

        public void readAlgoParams()
        {
            if (msgVersion >= 21)
            {
                order.AlgoStrategy = eDecoder.ReadString();
                if (!Util.StringIsEmpty(order.AlgoStrategy))
                {
                    var algoParamsCount = eDecoder.ReadInt();
                    if (algoParamsCount > 0)
                    {
                        order.AlgoParams = new List<TagValue>(algoParamsCount);
                        for (var i = 0; i < algoParamsCount; ++i)
                        {
                            var tagValue = new TagValue
                            {
                                Tag = eDecoder.ReadString(),
                                Value = eDecoder.ReadString()
                            };
                            order.AlgoParams.Add(tagValue);
                        }
                    }
                }
            }
        }

        public void readSolicited()
        {
            if (msgVersion >= 33)
            {
                order.Solicited = eDecoder.ReadBoolFromInt();
            }
        }

        public void readWhatIfInfoAndCommission()
        {
            if (msgVersion >= 16)
            {
                order.WhatIf = eDecoder.ReadBoolFromInt();
                readOrderStatus();
                if (serverVersion >= MinServerVer.WHAT_IF_EXT_FIELDS)
                {
                    orderState.InitMarginBefore = eDecoder.ReadString();
                    orderState.MaintMarginBefore = eDecoder.ReadString();
                    orderState.EquityWithLoanBefore = eDecoder.ReadString();
                    orderState.InitMarginChange = eDecoder.ReadString();
                    orderState.MaintMarginChange = eDecoder.ReadString();
                    orderState.EquityWithLoanChange = eDecoder.ReadString();
                }
                orderState.InitMarginAfter = eDecoder.ReadString();
                orderState.MaintMarginAfter = eDecoder.ReadString();
                orderState.EquityWithLoanAfter = eDecoder.ReadString();
                orderState.Commission = eDecoder.ReadDoubleMax();
                orderState.MinCommission = eDecoder.ReadDoubleMax();
                orderState.MaxCommission = eDecoder.ReadDoubleMax();
                orderState.CommissionCurrency = eDecoder.ReadString();
                orderState.WarningText = eDecoder.ReadString();
            }
        }

        public void readOrderStatus() => orderState.Status = eDecoder.ReadString();

        public void readVolRandomizeFlags()
        {
            if (msgVersion >= 34)
            {
                order.RandomizeSize = eDecoder.ReadBoolFromInt();
                order.RandomizePrice = eDecoder.ReadBoolFromInt();
            }
        }

        public void readPegToBenchParams()
        {
            if (serverVersion >= MinServerVer.PEGGED_TO_BENCHMARK)
            {
                if (Util.IsPegBenchOrder(order.OrderType))
                {
                    order.ReferenceContractId = eDecoder.ReadInt();
                    order.IsPeggedChangeAmountDecrease = eDecoder.ReadBoolFromInt();
                    order.PeggedChangeAmount = eDecoder.ReadDoubleMax();
                    order.ReferenceChangeAmount = eDecoder.ReadDoubleMax();
                    order.ReferenceExchange = eDecoder.ReadString();
                }
            }
        }

        public void readConditions()
        {
            if (serverVersion >= MinServerVer.PEGGED_TO_BENCHMARK)
            {
                var nConditions = eDecoder.ReadInt();

                if (nConditions > 0)
                {
                    for (var i = 0; i < nConditions; i++)
                    {
                        var orderConditionType = (OrderConditionType)eDecoder.ReadInt();
                        var condition = OrderCondition.Create(orderConditionType);

                        condition.Deserialize(eDecoder);
                        order.Conditions.Add(condition);
                    }

                    order.ConditionsIgnoreRth = eDecoder.ReadBoolFromInt();
                    order.ConditionsCancelOrder = eDecoder.ReadBoolFromInt();
                }
            }
        }

        public void readAdjustedOrderParams()
        {
            if (serverVersion >= MinServerVer.PEGGED_TO_BENCHMARK)
            {
                order.AdjustedOrderType = eDecoder.ReadString();
                order.TriggerPrice = eDecoder.ReadDoubleMax();
                readStopPriceAndLmtPriceOffset();
                order.AdjustedStopPrice = eDecoder.ReadDoubleMax();
                order.AdjustedStopLimitPrice = eDecoder.ReadDoubleMax();
                order.AdjustedTrailingAmount = eDecoder.ReadDoubleMax();
                order.AdjustableTrailingUnit = eDecoder.ReadInt();
            }
        }

        public void readStopPriceAndLmtPriceOffset()
        {
            order.TrailStopPrice = eDecoder.ReadDoubleMax();
            order.LmtPriceOffset = eDecoder.ReadDoubleMax();
        }

        public void readSoftDollarTier()
        {
            if (serverVersion >= MinServerVer.SOFT_DOLLAR_TIER)
            {
                order.Tier = new SoftDollarTier(eDecoder.ReadString(), eDecoder.ReadString(), eDecoder.ReadString());
            }
        }

        public void readCashQty()
        {
            if (serverVersion >= MinServerVer.CASH_QTY)
            {
                order.CashQty = eDecoder.ReadDoubleMax();
            }
        }

        public void readDontUseAutoPriceForHedge()
        {
            if (serverVersion >= MinServerVer.AUTO_PRICE_FOR_HEDGE)
            {
                order.DontUseAutoPriceForHedge = eDecoder.ReadBoolFromInt();
            }
        }

        public void readIsOmsContainer()
        {
            if (serverVersion >= MinServerVer.ORDER_CONTAINER)
            {
                order.IsOmsContainer = eDecoder.ReadBoolFromInt();
            }
        }

        public void readDiscretionaryUpToLimitPrice()
        {
            if (serverVersion >= MinServerVer.D_PEG_ORDERS)
            {
                order.DiscretionaryUpToLimitPrice = eDecoder.ReadBoolFromInt();
            }
        }

        public void readAutoCancelDate() => order.AutoCancelDate = eDecoder.ReadString();

        public void readFilledQuantity() => order.FilledQuantity = eDecoder.ReadDecimal();

        public void readRefFuturesConId() => order.RefFuturesConId = eDecoder.ReadInt();

        public void readAutoCancelParent() => readAutoCancelParent(Constants.MinVersion);

        public void readAutoCancelParent(int minVersionAutoCancelParent)
        {
            if (serverVersion >= minVersionAutoCancelParent)
            {
                order.AutoCancelParent = eDecoder.ReadBoolFromInt();
            }
        }

        public void readShareholder() => order.Shareholder = eDecoder.ReadString();

        public void readImbalanceOnly() => order.ImbalanceOnly = eDecoder.ReadBoolFromInt();

        public void readRouteMarketableToBbo() => order.RouteMarketableToBbo = eDecoder.ReadBoolFromInt();

        public void readParentPermId() => order.ParentPermId = eDecoder.ReadLong();

        public void readCompletedTime() => orderState.CompletedTime = eDecoder.ReadString();

        public void readCompletedStatus() => orderState.CompletedStatus = eDecoder.ReadString();

        public void readUsePriceMgmtAlgo()
        {
            if (serverVersion >= MinServerVer.PRICE_MGMT_ALGO)
            {
                order.UsePriceMgmtAlgo = eDecoder.ReadBoolFromInt();
            }
        }

        public void readDuration()
        {
            if (serverVersion >= MinServerVer.DURATION)
            {
                order.Duration = eDecoder.ReadIntMax();
            }
        }

        public void readPostToAts()
        {
            if (serverVersion >= MinServerVer.POST_TO_ATS)
            {
                order.PostToAts = eDecoder.ReadIntMax();
            }
        }

        public void readPegBestPegMidOrderAttributes()
        {
            if (serverVersion >= MinServerVer.PEGBEST_PEGMID_OFFSETS)
            {
                order.MinTradeQty = eDecoder.ReadIntMax();
                order.MinCompeteSize = eDecoder.ReadIntMax();
                order.CompeteAgainstBestOffset = eDecoder.ReadDoubleMax();
                order.MidOffsetAtWhole = eDecoder.ReadDoubleMax();
                order.MidOffsetAtHalf = eDecoder.ReadDoubleMax();
            }
        }

        public void readCustomerAccount()
        {
            if (serverVersion >= MinServerVer.MIN_SERVER_VER_CUSTOMER_ACCOUNT)
            {
                order.CustomerAccount = eDecoder.ReadString();
            }
        }

        public void readProfessionalCustomer()
        {
            if (serverVersion >= MinServerVer.MIN_SERVER_VER_PROFESSIONAL_CUSTOMER)
            {
                order.ProfessionalCustomer = eDecoder.ReadBoolFromInt();
            }
        }

        public void readBondAccruedInterest()
        {
            if (serverVersion >= MinServerVer.MIN_SERVER_VER_BOND_ACCRUED_INTEREST)
            {
                order.BondAccruedInterest = eDecoder.ReadString();
            }
        }

    }
}
