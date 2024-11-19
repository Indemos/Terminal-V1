/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace IBApi
{
    public enum TriggerMethod
    {
        Default = 0,
        DoubleBidAsk,
        Last,
        DoubleLast,
        BidAsk,
        LastOfBidAsk = 7,
        MidPoint
    }

    public static class CTriggerMethod
    {
        public static readonly string[] friendlyNames = { "default", "double bid/ask", "last", "double last", "bid/ask", "", "", "last of bid/ask", "mid-point" };


        public static string ToFriendlyString(this TriggerMethod th) => friendlyNames[(int)th];

        public static TriggerMethod FromFriendlyString(string friendlyName) => (TriggerMethod)Array.IndexOf(friendlyNames, friendlyName);
    }

    /**
     *  @brief Used with conditional orders to cancel or submit order based on price of an instrument. 
     */
    public class PriceCondition : ContractCondition
    {
        protected override string Value
        {
            get => Price.ToString(NumberFormatInfo.InvariantInfo);
            set => Price = double.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        public override string ToString() => $"{TriggerMethod.ToFriendlyString()} {base.ToString()}";

        public override bool Equals(object obj)
        {
            if (!(obj is PriceCondition other)) return false;

            return base.Equals(obj) && TriggerMethod == other.TriggerMethod;
        }

        public override int GetHashCode() => base.GetHashCode() + TriggerMethod.GetHashCode();

        public double Price { get; set; }
        public TriggerMethod TriggerMethod { get; set; }

        public override void Deserialize(IDecoder inStream)
        {
            base.Deserialize(inStream);

            TriggerMethod = (TriggerMethod)inStream.ReadInt();
        }

        public override void Serialize(BinaryWriter outStream)
        {
            base.Serialize(outStream);
            outStream.AddParameter((int)TriggerMethod);
        }

        protected override bool TryParse(string cond)
        {
            var fName = CTriggerMethod.friendlyNames.Where(n => cond.StartsWith(n)).OrderByDescending(n => n.Length).FirstOrDefault();

            if (fName == null) return false;

            try
            {
                TriggerMethod = CTriggerMethod.FromFriendlyString(fName);
                cond = cond.Substring(cond.IndexOf(fName) + fName.Length + 1);

                return base.TryParse(cond);
            }
            catch
            {
                return false;
            }
        }
    }
}
