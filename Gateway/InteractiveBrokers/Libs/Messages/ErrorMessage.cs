/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using IBApi;

namespace InteractiveBrokers
{
    class ErrorMessage 
    {
        public ErrorMessage(int requestId, int errorCode, string message, string advancedOrderRejectJson)
        {
            AdvancedOrderRejectJson = advancedOrderRejectJson;
            Message = message;
            RequestId = requestId;
            ErrorCode = errorCode;
        }

        public string AdvancedOrderRejectJson { get; set; }

        public string Message { get; set; }

        public int ErrorCode { get; set; }


        public int RequestId { get; set; }

        public override string ToString()
        {
            string ret = "Error. Request: " + RequestId + ", Code: " + ErrorCode + " - " + Message;
            if (!Util.StringIsEmpty(AdvancedOrderRejectJson))
            {
                ret += (", AdvancedOrderRejectJson: " + AdvancedOrderRejectJson);
            }
            return ret;
        }
       
    }
}
