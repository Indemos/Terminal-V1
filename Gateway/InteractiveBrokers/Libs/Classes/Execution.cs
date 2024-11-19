/* Copyright (C) 2023 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace IBApi
{

    /**
     * @class Liquidity
     * @brief Class describing the liquidity type of an execution.
     * @sa Execution
     */
    public class Liquidity
    {
        /**
         * @brief The enum of available liquidity flag types.
         *       0 = Unknown
         *       1 = Added liquidity
         *       2 = Removed liquidity
         *       3 = Liquidity routed out
         */
        private static readonly Dictionary<int, string> Values = new Dictionary<int, string>
        {
            {0, "None"},
            {1, "Added Liquidity"},
            {2, "Removed Liquidity"},
            {3, "Liquidity Routed Out" }
        };

        public Liquidity(int p) => Value = Values.ContainsKey(p) ? p : 0;

        /**
         * @brief The value of the liquidity type.
         */
        public int Value { get; set; }

        public override string ToString() => Values[Value];
    }

    /**
     * @class Execution
     * @brief Class describing an order's execution.
     * @sa ExecutionFilter, CommissionReport
     */
    public class Execution
    {
        /**
         * @brief The API client's order Id. May not be unique to an account.
         */
        public int OrderId { get; set; }

        /**
         * @brief The API client identifier which placed the order which originated this execution.
         */
        public int ClientId { get; set; }

        /**
         * @brief The execution's identifier. Each partial fill has a separate ExecId. 
         * A correction is indicated by an ExecId which differs from a previous ExecId in only the digits after the final period,
         * e.g. an ExecId ending in ".02" would be a correction of a previous execution with an ExecId ending in ".01"
         */
        public string ExecId { get; set; }

        /**
         * @brief The execution's server time.
         */
        public string Time { get; set; }

        /**
         * @brief The account to which the order was allocated.
         */
        public string AcctNumber { get; set; }

        /**
         * @brief The exchange where the execution took place.
         */
        public string Exchange { get; set; }

        /**
         * @brief Specifies if the transaction was buy or sale
         * BOT for bought, SLD for sold
         */
        public string Side { get; set; }

        /**
         * @brief The number of shares filled.
         */
        public decimal Shares { get; set; }

        /**
         * @brief The order's execution price excluding commissions.
         */
        public double Price { get; set; }

        /**
         * @brief The TWS order identifier. The PermId can be 0 for trades originating outside IB. 
         */
        public int PermId { get; set; }

        /**
         * @brief Identifies whether an execution occurred because of an IB-initiated liquidation. 
         */
        public int Liquidation { get; set; }

        /**
         * @brief Cumulative quantity. 
         * Used in regular trades, combo trades and legs of the combo.
         */
        public decimal CumQty { get; set; }

        /**
         * @brief Average price. 
         * Used in regular trades, combo trades and legs of the combo. Does not include commissions.
         */
        public double AvgPrice { get; set; }

        /**
         * @brief The OrderRef is a user-customizable string that can be set from the API or TWS and will be associated with an order for its lifetime.
         */
        public string OrderRef { get; set; }

        /**
         * @brief The Economic Value Rule name and the respective optional argument.
         * The two values should be separated by a colon. For example, aussieBond:YearsToExpiration=3. When the optional argument is not present, the first value will be followed by a colon.
         */
        public string EvRule { get; set; }

        /**
         * @brief Tells you approximately how much the market value of a contract would change if the price were to change by 1.
         * It cannot be used to get market value by multiplying the price by the approximate multiplier.
         */
        public double EvMultiplier { get; set; }

        /**
         * @brief model code
         */
        public string ModelCode { get; set; }

        /**
         * @brief The liquidity type of the execution. Requires TWS 968+ and API v973.05+. Python API specifically requires API v973.06+.
         */
        public Liquidity LastLiquidity { get; set; }

        /**
         * @brief pending price revision
         */
        public bool PendingPriceRevision { get; set; }

        public Execution()
        {
            OrderId = 0;
            ClientId = 0;
            Shares = 0;
            Price = 0;
            PermId = 0;
            Liquidation = 0;
            CumQty = 0;
            AvgPrice = 0;
            EvMultiplier = 0;
            LastLiquidity = new Liquidity(0);
            PendingPriceRevision = false;
        }

        public Execution(int orderId, int clientId, string execId, string time,
                          string acctNumber, string exchange, string side, decimal shares,
                          double price, int permId, int liquidation, decimal cumQty,
                          double avgPrice, string orderRef, string evRule, double evMultiplier,
                          string modelCode, Liquidity lastLiquidity, bool pendingPriceRevision)
        {
            OrderId = orderId;
            ClientId = clientId;
            ExecId = execId;
            Time = time;
            AcctNumber = acctNumber;
            Exchange = exchange;
            Side = side;
            Shares = shares;
            Price = price;
            PermId = permId;
            Liquidation = liquidation;
            CumQty = cumQty;
            AvgPrice = avgPrice;
            OrderRef = orderRef;
            EvRule = evRule;
            EvMultiplier = evMultiplier;
            ModelCode = modelCode;
            LastLiquidity = lastLiquidity;
            PendingPriceRevision = pendingPriceRevision;
        }

        public override bool Equals(object p_other)
        {
            bool l_bRetVal;
            if (!(p_other is Execution l_theOther))
            {
                l_bRetVal = false;
            }
            else if (this == p_other)
            {
                l_bRetVal = true;
            }
            else
            {
                l_bRetVal = string.Equals(ExecId, l_theOther.ExecId, System.StringComparison.OrdinalIgnoreCase);
            }
            return l_bRetVal;
        }

        public override int GetHashCode()
        {
            var hashCode = 926796717;
            hashCode *= -1521134295 + OrderId.GetHashCode();
            hashCode *= -1521134295 + ClientId.GetHashCode();
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExecId);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Time);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(AcctNumber);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Exchange);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Side);
            hashCode *= -1521134295 + Shares.GetHashCode();
            hashCode *= -1521134295 + Price.GetHashCode();
            hashCode *= -1521134295 + PermId.GetHashCode();
            hashCode *= -1521134295 + Liquidation.GetHashCode();
            hashCode *= -1521134295 + CumQty.GetHashCode();
            hashCode *= -1521134295 + AvgPrice.GetHashCode();
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(OrderRef);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(EvRule);
            hashCode *= -1521134295 + EvMultiplier.GetHashCode();
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(ModelCode);
            hashCode *= -1521134295 + EqualityComparer<Liquidity>.Default.GetHashCode(LastLiquidity);
            hashCode *= -1521134295 + PendingPriceRevision.GetHashCode();
            return hashCode;
        }
    }
}
