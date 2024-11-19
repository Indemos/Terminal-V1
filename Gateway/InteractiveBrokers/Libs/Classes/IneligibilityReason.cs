/* Copyright (C) 2024 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace IBApi
{
    /**
    * @class IneligibilityReason
    * @brief Convenience class to define ineligibility reason
    */
    public class IneligibilityReason
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public IneligibilityReason() { }

        public IneligibilityReason(string p_id, string p_description)
        {
            Id = p_id;
            Description = p_description;
        }
    }
}
