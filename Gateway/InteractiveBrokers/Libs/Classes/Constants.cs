/* Copyright (C) 2024 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace IBApi
{
    public static class Constants
    {
        public const int ClientVersion = 66; //API v. 9.71
        public const byte EOL = 0;
        public const string BagSecType = "BAG";
        public const int REDIRECT_COUNT_MAX = 2;
        public const string INFINITY_STR = "Infinity";

        public const int FaGroups = 1;
        public const int FaAliases = 3;
        public const int MinVersion = 100;
        public const int MaxVersion = MinServerVer.MIN_SERVER_VER_INELIGIBILITY_REASONS;
        public const int MaxMsgSize = 0x00FFFFFF;
    }
}
