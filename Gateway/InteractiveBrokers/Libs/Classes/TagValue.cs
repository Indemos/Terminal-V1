/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace IBApi
{
    /**
    * @class TagValue
    * @brief Convenience class to define key-value pairs
    */
    public class TagValue
    {
        public string Tag { get; set; }


        public string Value { get; set; }

        public TagValue() { }

        public TagValue(string p_tag, string p_value)
        {
            Tag = p_tag;
            Value = p_value;
        }

        public override bool Equals(object other)
        {
            if (this == other) return true;
            if (!(other is TagValue l_theOther)) return false;
            if (Util.StringCompare(Tag, l_theOther.Tag) != 0) return false;
            if (Util.StringCompare(Value, l_theOther.Value) != 0) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 221537429;
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Tag);
            hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
