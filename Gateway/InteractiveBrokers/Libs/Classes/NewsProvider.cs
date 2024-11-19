/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
     * @class NewsProvider
     * @brief Class describing news provider
     * @sa EClient::reqNewsProviders, EWrapper::newsProviders
     */
    public class NewsProvider
    {
        /**
         * @brief The API news provider code
         */
        public string ProviderCode { get; set; }

        /**
         * @brief The API news provider name
         */
        public string ProviderName { get; set; }

        public NewsProvider() { }

        public NewsProvider(string providerCode, string providerName)
        {
            ProviderCode = providerCode;
            ProviderName = providerName;
        }
    }
}
