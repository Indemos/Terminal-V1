/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace IBApi
{
    /**
     * @class OrderState
     * @brief Provides an active order's current state
     * @sa Order
     */
    public class OrderState
    {
        /**
         * @brief The order's current status
         */
        public string Status { get; set; }

        /**
         * @brief The account's current initial margin.
         */
        public string InitMarginBefore { get; set; }

        /**
        * @brief The account's current maintenance margin
        */
        public string MaintMarginBefore { get; set; }

        /**
        * @brief The account's current equity with loan
        */
        public string EquityWithLoanBefore { get; set; }

        /**
         * @brief The change of the account's initial margin.
         */
        public string InitMarginChange { get; set; }

        /**
        * @brief The change of the account's maintenance margin
        */
        public string MaintMarginChange { get; set; }

        /**
        * @brief The change of the account's equity with loan
        */
        public string EquityWithLoanChange { get; set; }

        /**
         * @brief The order's impact on the account's initial margin.
         */
        public string InitMarginAfter { get; set; }

        /**
        * @brief The order's impact on the account's maintenance margin
        */
        public string MaintMarginAfter { get; set; }

        /**
        * @brief Shows the impact the order would have on the account's equity with loan
        */
        public string EquityWithLoanAfter { get; set; }

        /**
          * @brief The order's generated commission.
          */
        public double Commission { get; set; }

        /**
        * @brief The execution's minimum commission.
        */
        public double MinCommission { get; set; }

        /**
        * @brief The executions maximum commission.
        */
        public double MaxCommission { get; set; }

        /**
         * @brief The generated commission currency
         * @sa CommissionReport
         */
        public string CommissionCurrency { get; set; }

        /**
         * @brief If the order is warranted, a descriptive message will be provided.
         */
        public string WarningText { get; set; }

        public string CompletedTime { get; set; }

        public string CompletedStatus { get; set; }

        public OrderState()
        {
            Status = null;
            InitMarginBefore = null;
            MaintMarginBefore = null;
            EquityWithLoanBefore = null;
            InitMarginChange = null;
            MaintMarginChange = null;
            EquityWithLoanChange = null;
            InitMarginAfter = null;
            MaintMarginAfter = null;
            EquityWithLoanAfter = null;
            Commission = 0.0;
            MinCommission = 0.0;
            MaxCommission = 0.0;
            CommissionCurrency = null;
            WarningText = null;
            CompletedTime = null;
            CompletedStatus = null;
        }

        public OrderState(string status,
                string initMarginBefore, string maintMarginBefore, string equityWithLoanBefore,
                string initMarginChange, string maintMarginChange, string equityWithLoanChange,
                string initMarginAfter, string maintMarginAfter, string equityWithLoanAfter,
                double commission, double minCommission,
                double maxCommission, string commissionCurrency, string warningText,
                string completedTime, string completedStatus)
        {
            Status = status;
            InitMarginBefore = initMarginBefore;
            MaintMarginBefore = maintMarginBefore;
            EquityWithLoanBefore = equityWithLoanBefore;
            InitMarginChange = initMarginChange;
            MaintMarginChange = maintMarginChange;
            EquityWithLoanChange = equityWithLoanChange;
            InitMarginAfter = initMarginAfter;
            MaintMarginAfter = maintMarginAfter;
            EquityWithLoanAfter = equityWithLoanAfter;
            Commission = commission;
            MinCommission = minCommission;
            MaxCommission = maxCommission;
            CommissionCurrency = commissionCurrency;
            WarningText = warningText;
            CompletedTime = completedTime;
            CompletedStatus = completedStatus;
        }

        public override bool Equals(object other)
        {
            if (this == other)
                return true;


            if (!(other is OrderState state))
                return false;

            if (Commission != state.Commission ||
                MinCommission != state.MinCommission ||
                MaxCommission != state.MaxCommission)
            {
                return false;
            }

            if (Util.StringCompare(Status, state.Status) != 0 ||
                Util.StringCompare(InitMarginBefore, state.InitMarginBefore) != 0 ||
                Util.StringCompare(MaintMarginBefore, state.MaintMarginBefore) != 0 ||
                Util.StringCompare(EquityWithLoanBefore, state.EquityWithLoanBefore) != 0 ||
                Util.StringCompare(InitMarginChange, state.InitMarginChange) != 0 ||
                Util.StringCompare(MaintMarginChange, state.MaintMarginChange) != 0 ||
                Util.StringCompare(EquityWithLoanChange, state.EquityWithLoanChange) != 0 ||
                Util.StringCompare(InitMarginAfter, state.InitMarginAfter) != 0 ||
                Util.StringCompare(MaintMarginAfter, state.MaintMarginAfter) != 0 ||
                Util.StringCompare(EquityWithLoanAfter, state.EquityWithLoanAfter) != 0 ||
                Util.StringCompare(CommissionCurrency, state.CommissionCurrency) != 0 ||
                Util.StringCompare(CompletedTime, state.CompletedTime) != 0 ||
                Util.StringCompare(CompletedStatus, state.CompletedStatus) != 0)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 1754944475;
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Status);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(InitMarginBefore);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(MaintMarginBefore);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(EquityWithLoanBefore);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(InitMarginChange);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(MaintMarginChange);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(EquityWithLoanChange);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(InitMarginAfter);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(MaintMarginAfter);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(EquityWithLoanAfter);
            hashCode *= -1521134295 + Commission.GetHashCode();
            hashCode *= -1521134295 + MinCommission.GetHashCode();
            hashCode *= -1521134295 + MaxCommission.GetHashCode();
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(CommissionCurrency);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(WarningText);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(CompletedTime);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(CompletedStatus);
            return hashCode;
        }
    }
}
