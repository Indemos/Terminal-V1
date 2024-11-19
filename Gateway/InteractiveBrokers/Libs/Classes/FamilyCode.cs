/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class FamilyCode
     * @brief Class describing family code
     * @sa EClient::reqFamilyCodes, EWrapper::familyCodes
     */
    public class FamilyCode
    {
        /**
         * @brief The API account id
         */
        public string AccountID { get; set; }

        /**
         * @brief The API family code
         */
        public string FamilyCodeStr { get; set; }

        public FamilyCode() { }

        public FamilyCode(string accountID, string familyCodeStr)
        {
            AccountID = accountID;
            FamilyCodeStr = familyCodeStr;
        }
    }
}
