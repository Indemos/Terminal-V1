/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;

namespace IBApi
{
    public abstract class ContractCondition : OperatorCondition
    {
        public int ConId { get; set; }
        public string Exchange { get; set; }

        private const string delimiter = " of ";

        public Func<int, string, string> ContractResolver { get; set; }

        public ContractCondition() => ContractResolver = (conid, exch) => $"{conid}({exch})";

        public override string ToString() => Type + delimiter + ContractResolver(ConId, Exchange) + base.ToString();

        public override bool Equals(object obj)
        {
            if (!(obj is ContractCondition other))
                return false;

            return base.Equals(obj)
                && ConId == other.ConId
                && Exchange.Equals(other.Exchange, StringComparison.Ordinal);
        }

        public override int GetHashCode() => base.GetHashCode() + ConId.GetHashCode() + Exchange.GetHashCode();

        protected override bool TryParse(string cond)
        {
            try
            {
                if (cond.Substring(0, cond.IndexOf(delimiter)) != Type.ToString())
                    return false;

                cond = cond.Substring(cond.IndexOf(delimiter) + delimiter.Length);

                if (!int.TryParse(cond.Substring(0, cond.IndexOf("(")), out var conid))
                    return false;

                ConId = conid;
                cond = cond.Substring(cond.IndexOf("(") + 1);
                Exchange = cond.Substring(0, cond.IndexOf(")"));
                cond = cond.Substring(cond.IndexOf(")") + 1);

                return base.TryParse(cond);
            }
            catch
            {
                return false;
            }
        }

        public override void Deserialize(IDecoder inStream)
        {
            base.Deserialize(inStream);

            ConId = inStream.ReadInt();
            Exchange = inStream.ReadString();
        }

        public override void Serialize(System.IO.BinaryWriter outStream)
        {
            base.Serialize(outStream);
            outStream.AddParameter(ConId);
            outStream.AddParameter(Exchange);
        }
    }
}
