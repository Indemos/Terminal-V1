/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @brief Used with conditional orders to submit or cancel an order based on a specified volume change in a security.
     */
    public class VolumeCondition : ContractCondition
    {
        protected override string Value
        {
            get => Volume.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            set => Volume = int.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public int Volume { get; set; }
    }
}
