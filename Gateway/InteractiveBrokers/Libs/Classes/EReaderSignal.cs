/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    /**
    * @brief Notifies the thread reading information from the TWS whenever there are messages ready to be consumed. Not currently used in Python API.
    */
    public interface EReaderSignal
    {
        /**
         * @brief Issues a signal to the consuming thread when there are things to be consumed.
         */
        void issueSignal();

        /**
         * @brief Makes the consuming thread waiting until a signal is issued.
         */
        void waitForSignal();
    }
}
