/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    public abstract class OperatorCondition : OrderCondition
    {
        protected abstract string Value { get; set; }
        public bool IsMore { get; set; }

        private const string header = " is ";

        public override string ToString() => header + (IsMore ? ">= " : "<= ") + Value;

        public override bool Equals(object obj)
        {
            if (!(obj is OperatorCondition other))
                return false;

            return base.Equals(obj)
                && Value.Equals(other.Value, System.StringComparison.Ordinal)
                && IsMore == other.IsMore;
        }

        public override int GetHashCode() => base.GetHashCode() + Value.GetHashCode() + IsMore.GetHashCode();

        protected override bool TryParse(string cond)
        {
            if (!cond.StartsWith(header))
                return false;

            try
            {
                cond = cond.Replace(header, "");

                if (!cond.StartsWith(">=") && !cond.StartsWith("<="))
                    return false;

                IsMore = cond.StartsWith(">=");

                if (base.TryParse(cond.Substring(cond.LastIndexOf(" "))))
                    cond = cond.Substring(0, cond.LastIndexOf(" "));

                Value = cond.Substring(3);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override void Deserialize(IDecoder inStream)
        {
            base.Deserialize(inStream);

            IsMore = inStream.ReadBoolFromInt();
            Value = inStream.ReadString();
        }

        public override void Serialize(System.IO.BinaryWriter outStream)
        {
            base.Serialize(outStream);
            outStream.AddParameter(IsMore);
            outStream.AddParameter(Value);
        }
    }
}
