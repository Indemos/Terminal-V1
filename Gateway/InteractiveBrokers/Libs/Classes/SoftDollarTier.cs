/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class SoftDollarTier
     * @brief A container for storing Soft Dollar Tier information
     */
    public class SoftDollarTier
    {
        /**
         * @brief The name of the Soft Dollar Tier
         */
        public string Name { get; set; }

        /**
         * @brief The value of the Soft Dollar Tier
         */
        public string Value { get; set; }

        /**
         * @brief The display name of the Soft Dollar Tier
         */
        public string DisplayName { get; set; }

        public SoftDollarTier(string name, string value, string displayName)
        {
            Name = name;
            Value = value;
            DisplayName = displayName;
        }

        public SoftDollarTier() : this(null, null, null) { }

        public override bool Equals(object obj)
        {
            var b = obj as SoftDollarTier;

            if (Equals(b, null)) return false;

            return string.Equals(Name, b.Name, System.StringComparison.OrdinalIgnoreCase) && string.Equals(Value, b.Value, System.StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => (Name ?? "").GetHashCode() + (Value ?? "").GetHashCode();

        public static bool operator ==(SoftDollarTier left, SoftDollarTier right) => left.Equals(right);

        public static bool operator !=(SoftDollarTier left, SoftDollarTier right) => !left.Equals(right);

        public override string ToString() => DisplayName;
    }
}
